﻿namespace SideEffects
{
    public class AddTempCardToDeck : TargetSideEffect, ICardDeckLinked, IPositive
    {
        public AddTempCardToDeck()
        {
        }

        protected override void InitSideEffectParam()
        {
            base.InitSideEffectParam();
            M_SideEffectParam.SetParam_MultipliedInt("CardCount", 0);
            M_SideEffectParam.SetParam_ConstInt("CardID", (int) AllCards.EmptyCardTypes.EmptyCard, typeof(CardDeck));
        }

        public override TargetSelector.TargetSelectorTypes TargetSelectorType => TargetSelector.TargetSelectorTypes.DeckBased;

        public override string GenerateDesc()
        {
            int cardID = M_SideEffectParam.GetParam_ConstInt("CardID");
            if (cardID == (int) AllCards.EmptyCardTypes.NoCard || cardID == (int) AllCards.EmptyCardTypes.EmptyCard)
            {
                return "Error!!!";
            }

            BaseInfo bi = AllCards.GetCard(cardID).BaseInfo;
            return HighlightStringFormat(
                DescRaws[LanguageManager_Common.GetCurrentLanguage()],
                GetDescOfTargetRange(),
                M_SideEffectParam.GetParam_MultipliedInt("CardCount"),
                "[" + bi.CardNames[LanguageManager_Common.GetCurrentLanguage()] + "]");
        }

        public override void Execute(ExecutorInfo executorInfo)
        {
            BattlePlayer player = (BattlePlayer) Player;
            player.GameManager.SideEffect_ShipAction(
                delegate(BattlePlayer sp) { sp.CardDeckManager.RandomInsertTempCard(M_SideEffectParam.GetParam_ConstInt("CardID"), M_SideEffectParam.GetParam_MultipliedInt("CardCount")); },
                player,
                ChoiceCount,
                TargetRange,
                TargetSelect,
                executorInfo.TargetClientIds);
        }

        public SideEffectValue_ConstInt GetCardIDSideEffectValue()
        {
            return (SideEffectValue_ConstInt) M_SideEffectParam.GetParam("CardID");
        }

        public int GetSideEffectFunctionBias()
        {
            CardInfo_Base card = AllCards.GetCard(M_SideEffectParam.GetParam_ConstInt("CardID"));
            if (card != null)
            {
                return card.GetCardUseBias() * M_SideEffectParam.GetParam_MultipliedInt("CardCount") * 3;
            }
            else
            {
                return 0;
            }
        }
    }
}