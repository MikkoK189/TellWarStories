using System;
using System.Linq;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace TellWarStories
{

    public class TellStories : CampaignBehaviorBase
    {       
        Dictionary<Village, ToldStoriesTo> _villagesToldTo = new Dictionary<Village, ToldStoriesTo>();
        int _notableBattlesWon = 0;
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
            CampaignEvents.MapEventEnded.AddNonSerializedListener(this, new Action<MapEvent>(OnMapEvent));
        }

        private void game_menu_tellstories_village_on_consequence(MenuCallbackArgs args)
        {
            Village village = Settlement.CurrentSettlement.Village;
            if(!_villagesToldTo.ContainsKey(village))
            {
                _villagesToldTo.Add(village, new ToldStoriesTo());              
            }
            if (_villagesToldTo[village]._hasToldStories == false)
            {
                DoTellStories(_villagesToldTo[village]);
            }
            else if (_villagesToldTo[village]._hasToldStories)
            {
                if (CampaignTime.Now >= _villagesToldTo[village]._daysToResetStories)
                {
                    DoTellStories(_villagesToldTo[village]);
                }
                else
                {
                    InformationManager.DisplayMessage(new InformationMessage("You have already told war stories to these villagers today, come back on " + _villagesToldTo[village]._daysToResetStories));
                }
            }
        }

        private void OnMapEvent(MapEvent obj)
        {
            switch (obj.BattleState)
            {
                case BattleState.None:
                    break;
                case BattleState.DefenderVictory:
                case BattleState.AttackerVictory:
                    var winnerSide = obj.BattleState == BattleState.AttackerVictory ? obj.AttackerSide : obj.DefenderSide;
                    var winnerParties = winnerSide.PartiesOnThisSide;
                    int enemyAmountWonAgainst = obj.BattleState == BattleState.AttackerVictory ? obj.DefenderSide.Casualties : obj.AttackerSide.Casualties;
                    foreach (var VARIABLE in winnerParties)
                    {
                        if (VARIABLE.Owner == null) continue;
                        if (VARIABLE.Owner == Hero.MainHero)
                        {                         
                            if(enemyAmountWonAgainst >= 15)
                            {
                                InformationManager.DisplayMessage(new InformationMessage("You have gained a story about a notable battle"));
                                _notableBattlesWon++;
                            }                           
                        }
                    }
                    break;
                case BattleState.Dispersed:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DoTellStories(ToldStoriesTo village)
        {
            if (village._battleStoriesTold < _notableBattlesWon)
            {
                float _renownToGive = CalculateRenownToGive();
                Hero.MainHero.Clan.AddRenown(_renownToGive, true);
                InformationManager.DisplayMessage(new InformationMessage("You told the villagers a story about a notable battle, gained " + _renownToGive + " renown."));
                village._daysToResetStories = CampaignTime.DaysFromNow(1f);
                village._hasToldStories = true;
                village._battleStoriesTold++;
            }
            else
            {
                InformationManager.DisplayMessage(new InformationMessage("You do not have new stories to tell to these villagers."));
            }
        }

        private float CalculateRenownToGive()
        {
            Random rnd = new Random();
            int _rAmount = rnd.Next(1, 10);
            float _givenAmount = _rAmount * 0.1f;
            return _givenAmount;
        }
        private bool game_menu_tellstories_here_on_condition(MenuCallbackArgs args)
        {
            return true;
        }
        private void OnSessionLaunched(CampaignGameStarter obj)
        {
            obj.AddGameMenuOption("village", "village_tellstories", "Tell war stories", game_menu_tellstories_here_on_condition, this.game_menu_tellstories_village_on_consequence, false, 3);
        }
        public class ToldStoriesTo
        {
            [SaveableField(1)]
            public bool _hasToldStories = false;
            [SaveableField(2)]
            public CampaignTime _daysToResetStories = CampaignTime.Now;
            [SaveableField(3)]
            public int _battleStoriesTold = 0;

        }
        public class TellWarStoriesSaveDefiner : SaveableTypeDefiner
        {
            public TellWarStoriesSaveDefiner() : base(18401685)
            {
            }

            protected override void DefineClassTypes()
            {
                AddClassDefinition(typeof(ToldStoriesTo), 1);
            }

            protected override void DefineContainerDefinitions()
            {
                ConstructContainerDefinition(typeof(Dictionary<Village, ToldStoriesTo>));
            }
        }
        public override void SyncData(IDataStore dataStore)
        {
            try
            {
                dataStore.SyncData("_villagesToldTo", ref _villagesToldTo);
                dataStore.SyncData("_notableBattlesWon", ref _notableBattlesWon);
            }
            catch (NullReferenceException doesntExist)
            {

            }
        }
    }





}

