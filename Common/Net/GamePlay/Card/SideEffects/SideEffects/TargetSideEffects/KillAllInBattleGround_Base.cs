﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class KillAllInBattleGround_Base : TargetSideEffect
{
    public override string GenerateDesc()
    {
        return HightlightStringFormat(DescRaw, GetChineseDescOfTargetRange(M_TargetRange));
    }

    public override void Serialze(DataStream writer)
    {
        base.Serialze(writer);
    }

    protected override void Deserialze(DataStream reader)
    {
        base.Deserialze(reader);
    }
}