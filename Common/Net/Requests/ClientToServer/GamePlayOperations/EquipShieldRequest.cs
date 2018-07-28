﻿using System.Collections;
using System.Collections.Generic;

public class EquipShieldRequest : ClientRequestBase
{
    public int handCardInstanceId;
    public int retinueID;
    public int shieldPlaceIndex;

    public EquipShieldRequest()
    {
    }

    public EquipShieldRequest(int clientId, int handCardInstanceId, int retinueId, int shieldPlaceIndex) :base(clientId)
    {
        this.handCardInstanceId = handCardInstanceId;
        this.retinueID = retinueId;
        this.shieldPlaceIndex = shieldPlaceIndex;
    }

    public override int GetProtocol()
    {
        return NetProtocols.EQUIP_SHIELD_REQUEST;
    }

    public override string GetProtocolName()
    {
        return "EQUIP_SHIELD_REQUEST";
    }

    public override void Serialize(DataStream writer)
    {
        base.Serialize(writer);
        writer.WriteSInt32(handCardInstanceId);
        writer.WriteSInt32(retinueID);
        writer.WriteSInt32(shieldPlaceIndex);
    }

    public override void Deserialize(DataStream reader)
    {
        base.Deserialize(reader);
        handCardInstanceId = reader.ReadSInt32();
        retinueID = reader.ReadSInt32();
        shieldPlaceIndex = reader.ReadSInt32();
    }

    public override string DeserializeLog()
    {
        string log = "";
        log += " [handCardInstanceId]=" + handCardInstanceId;
        log += " [retinueID]=" + retinueID;
        log += " [shieldPlaceIndex]=" + shieldPlaceIndex;
        return log;
    }
}