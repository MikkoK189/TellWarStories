using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace TellWarStories
{
    class BuyDrinks : CampaignBehaviorBase
    {
        Dictionary<Settlement, int> _drinkPlacesPrices = new Dictionary<Settlement, int>();
        List<Settlement> _boughtDrinksIn = new List<Settlement>();
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, new Action(this.DailyTick));
        }

        private void OnSessionLaunched(CampaignGameStarter obj)
        {
            obj.AddPlayerLine("tavernkeeper_talk_to_buy_drinks", "tavernkeeper_talk", "tavernkeeper_buy_drinks_condition", "I would like to buy drinks to every customer today.", new ConversationSentence.OnConditionDelegate(this.conversation_tavernkeep_offers_drinks_on_condition), null, 100, null, null);
            obj.AddDialogLine("tavernkeeper_buy_drinks", "tavernkeeper_buy_drinks_condition", "tavernkeeper_offer_buy_drinks", "That would cost you {DRINKSCOST}{GOLD_ICON}", null, null, 100, null);
            obj.AddPlayerLine("tavernkeeper_confirm_buy_drinks", "tavernkeeper_offer_buy_drinks", "tavernkeep_confirm_buy_drinks", "Yes, I can pay that.", new ConversationSentence.OnConditionDelegate(this.conversation_tavernkeep_can_afford_drinks), new ConversationSentence.OnConsequenceDelegate(this.conversation_tavernkeep_bought_drinks), 100, null, null);
            obj.AddPlayerLine("tavernkeeper_deny_buy_drinks", "tavernkeeper_offer_buy_drinks", "tavernkeeper_pretalk", "Nevermind.", null, null, 100, null, null);
            obj.AddDialogLine("tavernkeeper_thank_for_drinks", "tavernkeep_confirm_buy_drinks", "tavernkeeper_pretalk", "Thank you.", null, null, 100, null);
        }

        private void conversation_tavernkeep_bought_drinks()
        {
            int _curDrinkPrice = CalculateDrinksPrice();
            GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, _curDrinkPrice, false);
            if(_curDrinkPrice >= 500 && _curDrinkPrice < 1000)
            {
                GainRenownAction.Apply(Hero.MainHero, MBRandom.RandomFloatRanged(1.0f, 2.0f));
                Hero.MainHero.AddSkillXp(DefaultSkills.Charm, MBRandom.RandomInt(2, 4));
            }
            else if(_curDrinkPrice >= 1000 && _curDrinkPrice < 1500)
            {
                GainRenownAction.Apply(Hero.MainHero, MBRandom.RandomFloatRanged(2.0f, 3.5f));
                Hero.MainHero.AddSkillXp(DefaultSkills.Charm, MBRandom.RandomInt(4, 8));
            }
            else if (_curDrinkPrice >= 1500)
            {
                GainRenownAction.Apply(Hero.MainHero, MBRandom.RandomFloatRanged(3.5f, 5.0f));
                Hero.MainHero.AddSkillXp(DefaultSkills.Charm, MBRandom.RandomInt(5, 10));
            }           
            InformationManager.DisplayMessage(new InformationMessage("Gained some renown and charm for buying drinks."));
            _boughtDrinksIn.Add(MobileParty.MainParty.CurrentSettlement);
        }

        private bool conversation_tavernkeep_can_afford_drinks()
        {
            if(Hero.MainHero.Gold >= CalculateDrinksPrice())
            {
                return true;
            }
            return false;
        }

        private bool conversation_tavernkeep_offers_drinks_on_condition()
        {
            Settlement _settlementPlayerIsIn = MobileParty.MainParty.CurrentSettlement;
            if (_boughtDrinksIn.Contains(_settlementPlayerIsIn))
            {
                return false;
            }
            else
            {
                int drinks_price = CalculateDrinksPrice();
                MBTextManager.SetTextVariable("DRINKSCOST", drinks_price, false);
                return true;
            }
        }

        private int CalculateDrinksPrice()
        {
            Settlement _settlementPlayerIsIn = MobileParty.MainParty.CurrentSettlement;
            if (_drinkPlacesPrices.ContainsKey(_settlementPlayerIsIn))
            {
                return _drinkPlacesPrices[_settlementPlayerIsIn];
            }
            else
            {
                int drinkCost = MBRandom.RandomInt(500, 2500);
                _drinkPlacesPrices.Add(_settlementPlayerIsIn, drinkCost);
                return drinkCost;
            }
        }
        public class BuyDrinksSaveDefiner : SaveableTypeDefiner
        {
            public BuyDrinksSaveDefiner() : base(18401685)
            {
            }

            protected override void DefineClassTypes()
            {
            }

            protected override void DefineContainerDefinitions()
            {
                ConstructContainerDefinition(typeof(Dictionary<Settlement, int>));
            }
        }
        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData<List<Settlement>>("_boughtDrinksIn", ref this._boughtDrinksIn);
            dataStore.SyncData("_drinkPlacesPrices", ref _drinkPlacesPrices);
        }
        public void DailyTick()
        {
            this._drinkPlacesPrices.Clear();
            this._boughtDrinksIn.Clear();
        }
    }
}
