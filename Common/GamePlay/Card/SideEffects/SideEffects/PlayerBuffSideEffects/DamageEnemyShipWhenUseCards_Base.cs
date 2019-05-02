﻿public class DamageEnemyShipWhenUseCards_Base : PlayerBuffSideEffects
{
    protected override void InitSideEffectParam()
    {
        M_SideEffectParam.SetParam_MultipliedInt("Damage", 0);
        base.InitSideEffectParam();
    }

    public override string GenerateDesc()
    {
        return HighlightStringFormat(DescRaws[LanguageManager_Common.GetCurrentLanguage()], ((Damage_Base) Sub_SideEffect[0]).M_SideEffectParam.GetParam_MultipliedInt("Damage"), M_SideEffectParam.GetParam_ConstInt("RemoveTriggerTimes"));
    }
}