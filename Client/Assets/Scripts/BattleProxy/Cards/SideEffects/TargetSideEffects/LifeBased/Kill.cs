﻿namespace SideEffects
{
    public class Kill : TargetSideEffect, INegative, IPriorUsed
    {
        public Kill()
        {
        }

        public override TargetSelector.TargetSelectorTypes TargetSelectorType => TargetSelector.TargetSelectorTypes.MechBased;

        public override string GenerateDesc()
        {
            return HighlightStringFormat(DescRaws[LanguageManager_Common.GetCurrentLanguage()], GetDescOfTargetRange());
        }

        public override void Execute(ExecutorInfo executorInfo)
        {
            BattlePlayer player = (BattlePlayer) Player;
            player.GameManager.KillMechs(ChoiceCount, executorInfo.TargetMechIds, player, TargetRange, TargetSelect, -1);
        }

        public int GetSideEffectFunctionBias()
        {
            return -5;
        }
    }
}