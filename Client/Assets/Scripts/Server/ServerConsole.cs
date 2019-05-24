﻿using System;
using System.IO;

internal class ServerConsole
{
    public static string ServerRoot
    {
        get
        {
            if (Platform == DEVELOP.FORMAL)
            {
                return "./MyFirstCardGame/ServerBuild/";
            }
            else
            {
                return "./";
            }
        }
    }

    public static DEVELOP Platform = DEVELOP.DEVELOP;

    public enum DEVELOP
    {
        DEVELOP,
        TEST,
        FORMAL
    }

    static void Main(string[] args)
    {
        ServerLog.Instance.ServerLogVerbosity = ServerLogVerbosity.All;
        StreamReader sr;
        FileInfo fi_formal = new FileInfo(ServerRoot + "Config/ServerSwitch.txt");
        FileInfo fi_develop = new FileInfo("./Config/ServerSwitch.txt");
        if (fi_formal.Exists)
        {
            sr = new StreamReader(fi_formal.FullName);
        }
        else
        {
            sr = new StreamReader(fi_develop.FullName);
        }

        string platform = sr.ReadToEnd().TrimEnd("\n ".ToCharArray());
        sr.Close();
        Platform = (DEVELOP) Enum.Parse(typeof(DEVELOP), platform);

        switch (Platform)
        {
            case DEVELOP.DEVELOP:
            {
                ServerLog.Instance.ServerLogVerbosity = ServerLogVerbosity.All;
                Server.SV = new Server("127.0.0.1", 9999);

                if (Directory.Exists("./Config"))
                {
                    Directory.Delete("./Config", true);
                }

                Directory.CreateDirectory("./Config");

                Utils.CopyDirectory("../../../Client/Assets/StreamingAssets/Config", "./Config/");
                Utils.CopyDirectory("../../Config", "./Config/");

                ServerLog.Instance.Print("DEVELOP SERVER");
                break;
            }
            case DEVELOP.TEST:
            {
                ServerLog.Instance.ServerLogVerbosity = ServerLogVerbosity.All;
                ServerLog.Instance.Print("TEST SERVER");
                Server.SV = new Server("127.0.0.1", 9999);
                break;
            }
            case DEVELOP.FORMAL:
            {
                ServerLog.Instance.ServerLogVerbosity = ServerLogVerbosity.States;
                ServerLog.Instance.Print("FORMAL SERVER");
                Server.SV = new Server("95.169.26.10", 9999);
                break;
            }
        }

        ServerLog.Instance.Print("ServerVersion: " + Server.ServerVersion);
        Server.SV.Start();
        while (Console.ReadLine() != "Exit")
        {
        }

        Server.SV.Stop();
    }
}