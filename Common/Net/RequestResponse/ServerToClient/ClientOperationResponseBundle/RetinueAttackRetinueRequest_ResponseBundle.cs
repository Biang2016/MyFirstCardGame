﻿using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;

public class RetinueAttackRetinueRequest_ResponseBundle : ResponseBundleBase
{
    public RetinueAttackRetinueRequest_ResponseBundle()
    {
    }

    public override int GetProtocol()
    {
        return NetProtocols.RETINUE_ATTACK_RETINUE_REQUEST_RESPONSE;
    }

    public override string GetProtocolName()
    {
        return "RETINUE_ATTACK_RETINUE_REQUEST_RESPONSE";
    }

    public override void Serialize(DataStream writer)
    {
        base.Serialize(writer);
    }

    public override void Deserialize(DataStream reader)
    {
        base.Deserialize(reader);
    }

    public override string DeserializeLog()
    {
        string log = base.DeserializeLog();
        return log;
    }
}