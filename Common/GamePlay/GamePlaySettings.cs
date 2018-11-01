﻿public class GamePlaySettings
{
    public static GamePlaySettings OnlineGamePlaySettings = new GamePlaySettings(
        drawCardPerRound: 2,
        defaultCoin: 15000,
        defaultLife: 100,
        defaultLifeMax: 200,
        defaultLifeMin: 50,
        defaultEnergy: 10,
        defaultEnergyMax: 50,
        defaultDrawCardNum: 2,
        minDrawCardNum: 1,
        maxDrawCardNum: 5
    );

    public static GamePlaySettings ServerGamePlaySettings = new GamePlaySettings(
        drawCardPerRound: 2,
        defaultCoin: 30000,
        defaultLife: 100,
        defaultLifeMax: 999,
        defaultLifeMin: 10,
        defaultEnergy: 10,
        defaultEnergyMax: 50,
        defaultDrawCardNum: 2,
        minDrawCardNum: 1,
        maxDrawCardNum: 5
    );

    public static int MaxHandCard = 30;
    public static int FirstDrawCard = 0;
    public static int SecondDrawCard = 0;
    public static int MaxRetinueNumber = 6;

    public static int MaxHeroNumber = 4;

    public static int BeginMetal = 0;
    public static int MaxMetal = 10;
    public static int MetalIncrease = 1;

    public static int LifeToCoin = 50;
    public static int EnergyToCoin = 50;
    public static int[] DrawCardNumToCoin = new[] {0, 10000, 15000, 18000, 23000, 26000};

    public int DrawCardPerRound = 2;

    public int DefaultCoin = 15000;
    public int DefaultLife = 100;
    public int DefaultLifeMax = 200;
    public int DefaultLifeMin = 50;
    public int DefaultEnergy = 10;
    public int DefaultEnergyMax = 50;

    public int DefaultDrawCardNum = 2;
    public int MinDrawCardNum = 1;
    public int MaxDrawCardNum = 5;

    public GamePlaySettings() { }

    public GamePlaySettings(int drawCardPerRound, int defaultCoin, int defaultLife, int defaultLifeMax, int defaultLifeMin, int defaultEnergy, int defaultEnergyMax, int defaultDrawCardNum, int minDrawCardNum, int maxDrawCardNum)
    {
        DrawCardPerRound = drawCardPerRound;
        DefaultCoin = defaultCoin;
        DefaultLife = defaultLife;
        DefaultLifeMax = defaultLifeMax;
        DefaultLifeMin = defaultLifeMin;
        DefaultEnergy = defaultEnergy;
        DefaultEnergyMax = defaultEnergyMax;
        DefaultDrawCardNum = defaultDrawCardNum;
        MinDrawCardNum = minDrawCardNum;
        MaxDrawCardNum = maxDrawCardNum;
    }

    public int DefaultMaxCoin
    {
        get { return DefaultCoin + (DefaultLife - DefaultLifeMin) * LifeToCoin + DefaultEnergy * EnergyToCoin + DrawCardNumToCoin[DefaultDrawCardNum] - DrawCardNumToCoin[MinDrawCardNum]; }
    }

    public GamePlaySettings Clone()
    {
        GamePlaySettings gps = new GamePlaySettings(
            drawCardPerRound: DrawCardPerRound,
            defaultCoin: DefaultCoin,
            defaultLife: DefaultLife,
            defaultLifeMax: DefaultLifeMax,
            defaultLifeMin: DefaultLifeMin,
            defaultEnergy: DefaultEnergy,
            defaultEnergyMax: DefaultEnergyMax,
            defaultDrawCardNum: DefaultDrawCardNum,
            minDrawCardNum: MinDrawCardNum,
            maxDrawCardNum: MaxDrawCardNum
        );
        return gps;
    }

    public void Serialize(DataStream writer)
    {
        writer.WriteSInt32(DrawCardPerRound);

        writer.WriteSInt32(DefaultCoin);
        writer.WriteSInt32(DefaultLife);
        writer.WriteSInt32(DefaultLifeMax);
        writer.WriteSInt32(DefaultLifeMin);
        writer.WriteSInt32(DefaultEnergy);
        writer.WriteSInt32(DefaultEnergyMax);

        writer.WriteSInt32(DefaultDrawCardNum);
        writer.WriteSInt32(MinDrawCardNum);
        writer.WriteSInt32(MaxDrawCardNum);
    }

    public static GamePlaySettings Deserialize(DataStream reader)
    {
        GamePlaySettings res = new GamePlaySettings(
            drawCardPerRound: reader.ReadSInt32(),
            defaultCoin: reader.ReadSInt32(),
            defaultLife: reader.ReadSInt32(),
            defaultLifeMax: reader.ReadSInt32(),
            defaultLifeMin: reader.ReadSInt32(),
            defaultEnergy: reader.ReadSInt32(),
            defaultEnergyMax: reader.ReadSInt32(),
            defaultDrawCardNum: reader.ReadSInt32(),
            minDrawCardNum: reader.ReadSInt32(),
            maxDrawCardNum: reader.ReadSInt32());
        return res;
    }

    public string DeserializeLog()
    {
        string log = " <GamePlaySettings>";
        log += " [DrawCardPerRound]=" + DrawCardPerRound;

        log += " [DefaultCoin]=" + DefaultCoin;
        log += " [DefaultLifeMax]=" + DefaultLifeMax;
        log += " [DefaultLifeMin]=" + DefaultLifeMin;
        log += " [DefaultEnergy]=" + DefaultEnergy;
        log += " [DefaultEnergyMax]=" + DefaultEnergyMax;

        log += " [DefaultDrawCardNum]=" + DefaultDrawCardNum;
        log += " [MinDrawCardNum]=" + MinDrawCardNum;
        log += " [MaxDrawCardNum]=" + MaxDrawCardNum;

        return log;
    }
}