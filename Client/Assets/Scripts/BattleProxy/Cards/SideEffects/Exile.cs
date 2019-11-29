﻿namespace SideEffects
{
    public class Exile : Exile_Base
    {
        public Exile()
        {
        }

        protected override void InitSideEffectParam()
        {
        }

        public override string GenerateDesc()
        {
            return HighlightStringFormat(DescRaws[LanguageManager_Common.GetCurrentLanguage()]);
        }

        public override void Execute(ExecutorInfo executorInfo)
        {
            BattlePlayer player = (BattlePlayer) Player;
            CardBase card = player.HandManager.GetCardByCardInstanceId(executorInfo.CardInstanceId);
            if (card != null)
            {
                card.CardInfo.BaseInfo.IsTemp = true;
            }
        }
    }
}