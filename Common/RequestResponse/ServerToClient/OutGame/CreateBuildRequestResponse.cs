﻿using System.Collections.Generic;

public class CreateBuildRequestResponse : ServerRequestBase
{
    public int buildId;

    public CreateBuildRequestResponse()
    {
    }


    public CreateBuildRequestResponse( int buildId)
    {
        this.buildId = buildId;
    }

    public override int GetProtocol()
    {
        return NetProtocols.CREATE_BUILD_REQUEST_RESPONSE;
    }

    public override string GetProtocolName()
    {
        return "CREATE_BUILD_REQUEST_RESPONSE";
    }

    public override void Serialize(DataStream writer)
    {
        base.Serialize(writer);
        writer.WriteSInt32(buildId);
    }

    public override void Deserialize(DataStream reader)
    {
        base.Deserialize(reader);
        buildId = reader.ReadSInt32();
    }

    public override string DeserializeLog()
    {
        string log = base.DeserializeLog();
        log += " [buildId]=" + buildId;
        return log;
    }
}