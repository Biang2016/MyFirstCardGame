﻿using System.Collections;
using System.Collections.Generic;

public class EquipWeaponRequest_Response : ClientOperationResponseBase
{
    public EquipWeaponRequest_Response()
    {
    }

    public override int GetProtocol()
    {
        return NetProtocols.EQUIP_WEAPON_REQUEST_RESPONSE;
    }

    public override string GetProtocolName()
    {
        return "EQUIP_WEAPON_REQUEST_RESPONSE";
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