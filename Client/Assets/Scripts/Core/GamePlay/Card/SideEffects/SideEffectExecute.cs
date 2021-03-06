﻿using System;
using System.Collections.Generic;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// Encapsulate side effect and its TriggerTime, TriggerRange, number of trigger times, and other attributes. 
/// </summary>
public sealed class SideEffectExecute : IClone<SideEffectExecute>
{
    private static int idGenerator = 5000;

    public static int GenerateID()
    {
        return idGenerator++;
    }

    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SideEffectFrom
    {
        Unknown = 0,
        Buff = 1,
        SpellCard = 2,
        EnergyCard = 4,
        MechSideEffect = 8,
        EquipSideEffect = 16,
    }

    public static SideEffectFrom GetSideEffectFromByCardType(CardTypes cardType)
    {
        switch (cardType)
        {
            case CardTypes.Mech: return SideEffectFrom.MechSideEffect;
            case CardTypes.Equip: return SideEffectFrom.EquipSideEffect;
            case CardTypes.Energy: return SideEffectFrom.EnergyCard;
            case CardTypes.Spell: return SideEffectFrom.SpellCard;
        }

        return SideEffectFrom.Unknown;
    }

    public SideEffectFrom M_SideEffectFrom;

    public int ID;
    public ExecuteSetting M_ExecuteSetting;
    public ExecutorInfo M_ExecutorInfo;
    public List<SideEffectBase> SideEffectBases = new List<SideEffectBase>();

    /// <summary>
    /// 对于不同的Type，有一些trigger参数的预设值，防止设置错误
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ExecuteSettingTypes
    {
        PlayOutEffect,
        BattleCry,
        EquipBattleCry,
        SelfMechDie,
        AttachedMechDie,
        SelfEquipDie,
        AttachedEquipDie,
        WhenSelfMechAttack,
        WhenSelfMechKillOther,
        WhenAttachedMechAttack,
        WhenAttachedMechKillOther,
        Others, // 高级自定义
        Scripts // 源于脚本
    }

    public ExecuteSettingTypes ExecuteSettingType
    {
        get
        {
            foreach (KeyValuePair<ExecuteSettingTypes, ExecuteSetting> kv in ExecuteSetting_Presets)
            {
                if (M_ExecuteSetting.IsEqual(kv.Value))
                {
                    return kv.Key;
                }
            }

            if (M_ExecuteSetting is ScriptExecuteSettingBase)
            {
                return ExecuteSettingTypes.Scripts;
            }
            else
            {
                return ExecuteSettingTypes.Others;
            }
        }
    }

    public class ExecuteSetting
    {
        public TriggerTime TriggerTime; //when to trigger
        public TriggerRange TriggerRange; //which range of events can trigger this effect
        public int TriggerTimes; // the max times we can trigger it.
        public int TriggerDelayTimes; //how many times we need to trigger it before it can really trigger
        public TriggerTime RemoveTriggerTime; //when to remove this effect/decrease the remove time of this effect
        public TriggerRange RemoveTriggerRange; //which range of events can remove this effect
        public int RemoveTriggerTimes; //the max times we can remove it.
        public int RemoveTriggerDelayTimes; //how many times of remove before we can remove the effect permanently. (usually used in buffs)

        public ExecuteSetting()
        {
        }

        public ExecuteSetting(TriggerTime triggerTime, TriggerRange triggerRange, int triggerTimes, int triggerDelayTimes, TriggerTime removeTriggerTime, TriggerRange removeTriggerRange, int removeTriggerTimes, int removeTriggerDelayTimes)
        {
            TriggerTime = triggerTime;
            TriggerRange = triggerRange;
            TriggerTimes = triggerTimes;
            TriggerDelayTimes = triggerDelayTimes;
            RemoveTriggerTime = removeTriggerTime;
            RemoveTriggerRange = removeTriggerRange;
            RemoveTriggerTimes = removeTriggerTimes;
            RemoveTriggerDelayTimes = removeTriggerDelayTimes;
        }

        public static ExecuteSetting GenerateFromXMLNode(XmlNode node)
        {
            ExecuteSettingTypes est = ExecuteSettingTypes.Others;
            if (node.Attributes["ExecuteSettingTypes"] != null)
            {
                est = (ExecuteSettingTypes) Enum.Parse(typeof(ExecuteSettingTypes), node.Attributes["ExecuteSettingTypes"].Value);
            }

            if (est == ExecuteSettingTypes.Others || est == ExecuteSettingTypes.Scripts)
            {
                TriggerTime triggerTime = (TriggerTime) Enum.Parse(typeof(TriggerTime), node.Attributes["triggerTime"].Value);
                TriggerRange triggerRange = (TriggerRange) Enum.Parse(typeof(TriggerRange), node.Attributes["triggerRange"].Value);
                int triggerDelayTimes = int.Parse(node.Attributes["triggerDelayTimes"].Value);
                int triggerTimes = int.Parse(node.Attributes["triggerTimes"].Value);
                TriggerTime removeTriggerTime = (TriggerTime) Enum.Parse(typeof(TriggerTime), node.Attributes["removeTriggerTime"].Value);
                TriggerRange removeTriggerRange = (TriggerRange) Enum.Parse(typeof(TriggerRange), node.Attributes["removeTriggerRange"].Value);
                int removeTriggerTimes = int.Parse(node.Attributes["removeTriggerTimes"].Value);
                int removeTriggerDelayTimes = int.Parse(node.Attributes["removeTriggerDelayTimes"].Value);

                if (est == ExecuteSettingTypes.Others)
                {
                    ExecuteSetting newExecuteSetting = new ExecuteSetting(triggerTime, triggerRange, triggerTimes, triggerDelayTimes, removeTriggerTime, removeTriggerRange, removeTriggerTimes, removeTriggerDelayTimes);
                    return newExecuteSetting;
                }
                else
                {
                    ScriptExecuteSettingBase copy = AllScriptExecuteSettings.GetScriptExecuteSetting(node.Attributes["ScriptName"].Value);
                    GetScriptExecuteSettingParamsFromXMLNode(copy.M_SideEffectParam, node);
                    copy.TriggerTime = triggerTime;
                    copy.TriggerRange = triggerRange;
                    copy.TriggerDelayTimes = triggerDelayTimes;
                    copy.TriggerTimes = triggerTimes;
                    copy.RemoveTriggerTime = removeTriggerTime;
                    copy.RemoveTriggerRange = removeTriggerRange;
                    copy.RemoveTriggerTimes = removeTriggerTimes;
                    copy.RemoveTriggerDelayTimes = removeTriggerDelayTimes;
                    return copy;
                }
            }
            else
            {
                return ExecuteSetting_Presets[est];
            }
        }

        public static void GetScriptExecuteSettingParamsFromXMLNode(SideEffectParam sep, XmlNode node)
        {
            List<XmlAttribute> notMatchAttrs = new List<XmlAttribute>();
            for (int i = 0; i < node.Attributes.Count; i++)
            {
                XmlAttribute attr = node.Attributes[i];
                if (!attr.Name.Equals("ExecuteSettingTypes") &&
                    !attr.Name.Equals("ScriptName") &&
                    !attr.Name.Equals("triggerTime") &&
                    !attr.Name.Equals("triggerRange") &&
                    !attr.Name.Equals("triggerDelayTimes") &&
                    !attr.Name.Equals("triggerTimes") &&
                    !attr.Name.Equals("removeTriggerTime") &&
                    !attr.Name.Equals("removeTriggerRange") &&
                    !attr.Name.Equals("removeTriggerTimes") &&
                    !attr.Name.Equals("removeTriggerDelayTimes"))
                {
                    SideEffectValue sev = sep.GetParam(attr.Name);
                    if (sev != null)
                    {
                        sev.SetValue(attr.Value);
                    }
                    else
                    {
                        notMatchAttrs.Add(attr);
                    }
                }
            }
        }

        public virtual bool IsEqual(ExecuteSetting target)
        {
            if (GetType() != target.GetType()) return false;
            if (TriggerTime != target.TriggerTime) return false;
            if (TriggerRange != target.TriggerRange) return false;
            if (TriggerDelayTimes != target.TriggerDelayTimes) return false;
            if (TriggerTimes != target.TriggerTimes) return false;
            if (RemoveTriggerTime != target.RemoveTriggerTime) return false;
            if (RemoveTriggerRange != target.RemoveTriggerRange) return false;
            if (RemoveTriggerTimes != target.RemoveTriggerTimes) return false;
            if (RemoveTriggerDelayTimes != target.RemoveTriggerDelayTimes) return false;
            return true;
        }

        public virtual void ExportToXML(XmlElement ele)
        {
            ele.SetAttribute("triggerTime", TriggerTime.ToString());
            ele.SetAttribute("triggerRange", TriggerRange.ToString());
            ele.SetAttribute("triggerDelayTimes", TriggerDelayTimes.ToString());
            ele.SetAttribute("triggerTimes", TriggerTimes.ToString());
            ele.SetAttribute("removeTriggerTime", RemoveTriggerTime.ToString());
            ele.SetAttribute("removeTriggerRange", RemoveTriggerRange.ToString());
            ele.SetAttribute("removeTriggerTimes", RemoveTriggerTimes.ToString());
            ele.SetAttribute("removeTriggerDelayTimes", RemoveTriggerDelayTimes.ToString());
        }

        public virtual void Serialize(DataStream writer)
        {
            writer.WriteString8(GetType().ToString());
            writer.WriteSInt32((int) TriggerTime);
            writer.WriteSInt32((int) TriggerRange);
            writer.WriteSInt32(TriggerDelayTimes);
            writer.WriteSInt32(TriggerTimes);
            writer.WriteSInt32((int) RemoveTriggerTime);
            writer.WriteSInt32((int) RemoveTriggerRange);
            writer.WriteSInt32(RemoveTriggerTimes);
            writer.WriteSInt32(RemoveTriggerDelayTimes);
        }

        public static ExecuteSetting Deserialize(DataStream reader)
        {
            string type = reader.ReadString8();
            ExecuteSetting es;
            if (type != typeof(ExecuteSetting).ToString())
            {
                es = ScriptExecuteSettingManager.GetNewScriptExecuteSetting(type);
            }
            else
            {
                es = new ExecuteSetting();
            }

            es.TriggerTime = (TriggerTime) reader.ReadSInt32();
            es.TriggerRange = (TriggerRange) reader.ReadSInt32();
            es.TriggerDelayTimes = reader.ReadSInt32();
            es.TriggerTimes = reader.ReadSInt32();
            es.RemoveTriggerTime = (TriggerTime) reader.ReadSInt32();
            es.RemoveTriggerRange = (TriggerRange) reader.ReadSInt32();
            es.RemoveTriggerTimes = reader.ReadSInt32();
            es.RemoveTriggerDelayTimes = reader.ReadSInt32();
            es.Child_Deserialize(reader);
            return es;
        }

        public virtual void Child_Deserialize(DataStream reader)
        {
        }

        public virtual ExecuteSetting Clone()
        {
            return new ExecuteSetting(TriggerTime, TriggerRange, TriggerTimes, TriggerDelayTimes, RemoveTriggerTime, RemoveTriggerRange, RemoveTriggerTimes, RemoveTriggerDelayTimes);
        }
    }

    /// <summary>
    /// 对于不同的Type，有一些trigger参数的预设值，防止设置错误
    /// </summary>
    public static Dictionary<ExecuteSettingTypes, ExecuteSetting> ExecuteSetting_Presets = new Dictionary<ExecuteSettingTypes, ExecuteSetting>
    {
        {
            ExecuteSettingTypes.PlayOutEffect, new ExecuteSetting(
                triggerTime: TriggerTime.OnPlayCard,
                triggerRange: TriggerRange.Self,
                triggerDelayTimes: 0,
                triggerTimes: 1,
                removeTriggerTime: TriggerTime.None,
                removeTriggerRange: TriggerRange.None,
                removeTriggerTimes: 1,
                removeTriggerDelayTimes: 0)
        },
        {
            ExecuteSettingTypes.BattleCry, new ExecuteSetting(
                triggerTime: TriggerTime.OnMechSummon,
                triggerRange: TriggerRange.Self,
                triggerDelayTimes: 0,
                triggerTimes: 1,
                removeTriggerTime: TriggerTime.None,
                removeTriggerRange: TriggerRange.None,
                removeTriggerTimes: 1,
                removeTriggerDelayTimes: 0)
        },
        {
            ExecuteSettingTypes.EquipBattleCry, new ExecuteSetting(
                triggerTime: TriggerTime.OnEquipEquiped,
                triggerRange: TriggerRange.Self,
                triggerDelayTimes: 0,
                triggerTimes: 1,
                removeTriggerTime: TriggerTime.None,
                removeTriggerRange: TriggerRange.None,
                removeTriggerTimes: 1,
                removeTriggerDelayTimes: 0)
        },
        {
            ExecuteSettingTypes.SelfMechDie, new ExecuteSetting(
                triggerTime: TriggerTime.OnMechDie,
                triggerRange: TriggerRange.Self,
                triggerDelayTimes: 0,
                triggerTimes: 1,
                removeTriggerTime: TriggerTime.None,
                removeTriggerRange: TriggerRange.None,
                removeTriggerTimes: 1,
                removeTriggerDelayTimes: 0)
        },
        {
            ExecuteSettingTypes.AttachedMechDie, new ExecuteSetting(
                triggerTime: TriggerTime.OnMechDie,
                triggerRange: TriggerRange.AttachedMech,
                triggerDelayTimes: 0,
                triggerTimes: 1,
                removeTriggerTime: TriggerTime.None,
                removeTriggerRange: TriggerRange.None,
                removeTriggerTimes: 1,
                removeTriggerDelayTimes: 0)
        },
        {
            ExecuteSettingTypes.SelfEquipDie, new ExecuteSetting(
                triggerTime: TriggerTime.OnEquipDie,
                triggerRange: TriggerRange.Self,
                triggerDelayTimes: 0,
                triggerTimes: 99999,
                removeTriggerTime: TriggerTime.None,
                removeTriggerRange: TriggerRange.None,
                removeTriggerTimes: 1,
                removeTriggerDelayTimes: 0)
        },
        {
            ExecuteSettingTypes.AttachedEquipDie, new ExecuteSetting(
                triggerTime: TriggerTime.OnEquipDie,
                triggerRange: TriggerRange.AttachedEquip,
                triggerDelayTimes: 0,
                triggerTimes: 1,
                removeTriggerTime: TriggerTime.None,
                removeTriggerRange: TriggerRange.None,
                removeTriggerTimes: 1,
                removeTriggerDelayTimes: 0)
        },
        {
            ExecuteSettingTypes.WhenSelfMechAttack, new ExecuteSetting(
                triggerTime: TriggerTime.OnMechAttack,
                triggerRange: TriggerRange.Self,
                triggerDelayTimes: 0,
                triggerTimes: 99999,
                removeTriggerTime: TriggerTime.None,
                removeTriggerRange: TriggerRange.None,
                removeTriggerTimes: 1,
                removeTriggerDelayTimes: 0)
        },
        {
            ExecuteSettingTypes.WhenSelfMechKillOther, new ExecuteSetting(
                triggerTime: TriggerTime.OnMechKill,
                triggerRange: TriggerRange.Self,
                triggerDelayTimes: 0,
                triggerTimes: 99999,
                removeTriggerTime: TriggerTime.None,
                removeTriggerRange: TriggerRange.None,
                removeTriggerTimes: 1,
                removeTriggerDelayTimes: 0)
        },
        {
            ExecuteSettingTypes.WhenAttachedMechAttack, new ExecuteSetting(
                triggerTime: TriggerTime.OnMechAttack,
                triggerRange: TriggerRange.AttachedMech,
                triggerDelayTimes: 0,
                triggerTimes: 99999,
                removeTriggerTime: TriggerTime.None,
                removeTriggerRange: TriggerRange.None,
                removeTriggerTimes: 1,
                removeTriggerDelayTimes: 0)
        },
        {
            ExecuteSettingTypes.WhenAttachedMechKillOther, new ExecuteSetting(
                triggerTime: TriggerTime.OnMechKill,
                triggerRange: TriggerRange.AttachedMech,
                triggerDelayTimes: 0,
                triggerTimes: 99999,
                removeTriggerTime: TriggerTime.None,
                removeTriggerRange: TriggerRange.None,
                removeTriggerTimes: 1,
                removeTriggerDelayTimes: 0)
        },
        {
            ExecuteSettingTypes.Others, new ExecuteSetting(
                triggerTime: TriggerTime.None,
                triggerRange: TriggerRange.None,
                triggerDelayTimes: 0,
                triggerTimes: 1,
                removeTriggerTime: TriggerTime.None,
                removeTriggerRange: TriggerRange.None,
                removeTriggerTimes: 1,
                removeTriggerDelayTimes: 0)
        },
        {
            ExecuteSettingTypes.Scripts, new ScriptExecuteSettingBase(
                name: "OnPlaySpecifiedCardByID",
                descRaws: new SortedDictionary<string, string>
                {
                    {"en", "When play card ID"},
                    {"zh", "当打出[ID]牌时"},
                },
                triggerTime: TriggerTime.None,
                triggerRange: TriggerRange.None,
                triggerDelayTimes: 0,
                triggerTimes: 1,
                removeTriggerTime: TriggerTime.None,
                removeTriggerRange: TriggerRange.None,
                removeTriggerTimes: 1,
                removeTriggerDelayTimes: 0)
        },
    };

    public static Dictionary<SideEffectFrom, List<ExecuteSettingTypes>> ValidExecuteSettingTypesForSideEffectFrom = new Dictionary<SideEffectFrom, List<ExecuteSettingTypes>>
    {
        {
            SideEffectFrom.MechSideEffect,
            new List<ExecuteSettingTypes>
            {
                ExecuteSettingTypes.BattleCry,
                ExecuteSettingTypes.SelfMechDie,
                ExecuteSettingTypes.AttachedEquipDie,
                ExecuteSettingTypes.WhenSelfMechAttack,
                ExecuteSettingTypes.WhenSelfMechKillOther,
                ExecuteSettingTypes.Others,
                ExecuteSettingTypes.Scripts
            }
        },
        {
            SideEffectFrom.EquipSideEffect,
            new List<ExecuteSettingTypes>
            {
                ExecuteSettingTypes.EquipBattleCry,
                ExecuteSettingTypes.AttachedMechDie,
                ExecuteSettingTypes.SelfEquipDie,
                ExecuteSettingTypes.WhenAttachedMechAttack,
                ExecuteSettingTypes.WhenAttachedMechKillOther,
                ExecuteSettingTypes.Others,
                ExecuteSettingTypes.Scripts
            }
        },
        {
            SideEffectFrom.SpellCard,
            new List<ExecuteSettingTypes>
            {
                ExecuteSettingTypes.PlayOutEffect,
                ExecuteSettingTypes.Others,
                ExecuteSettingTypes.Scripts
            }
        },
        {
            SideEffectFrom.EnergyCard,
            new List<ExecuteSettingTypes>
            {
                ExecuteSettingTypes.PlayOutEffect,
                ExecuteSettingTypes.Others,
                ExecuteSettingTypes.Scripts
            }
        },
        {
            SideEffectFrom.Buff,
            new List<ExecuteSettingTypes>
            {
                ExecuteSettingTypes.Others,
                ExecuteSettingTypes.Scripts
            }
        },
    };

    private SideEffectExecute()
    {
    }

    public SideEffectExecute(SideEffectFrom sideEffectFrom, List<SideEffectBase> sideEffectBases, ExecuteSetting executeSetting)
    {
        M_SideEffectFrom = sideEffectFrom;
        ID = GenerateID();
        SideEffectBases = sideEffectBases;
        M_ExecuteSetting = executeSetting.Clone();
    }

    public SideEffectExecute Clone()
    {
        return new SideEffectExecute(M_SideEffectFrom, CloneVariantUtils.List(SideEffectBases), M_ExecuteSetting.Clone());
    }

    public void Serialize(DataStream writer)
    {
        writer.WriteSInt32((int) M_SideEffectFrom);
        writer.WriteSInt32(ID);
        writer.WriteSInt32(SideEffectBases.Count);
        foreach (SideEffectBase se in SideEffectBases)
        {
            se.Serialize(writer);
        }

        M_ExecuteSetting.Serialize(writer);
    }

    public static SideEffectExecute Deserialize(DataStream reader)
    {
        SideEffectExecute see = new SideEffectExecute();
        see.M_SideEffectFrom = (SideEffectFrom) reader.ReadSInt32();
        see.ID = reader.ReadSInt32();
        int count = reader.ReadSInt32();
        for (int i = 0; i < count; i++)
        {
            SideEffectBase se = SideEffectBase.BaseDeserialize(reader);
            see.SideEffectBases.Add(se);
        }

        see.M_ExecuteSetting = ExecuteSetting.Deserialize(reader);
        return see;
    }

    public string GenerateDesc()
    {
        string res = "";
        if (ExecuteSettingType == ExecuteSettingTypes.Scripts)
        {
            ScriptExecuteSettingBase sesb = (ScriptExecuteSettingBase) M_ExecuteSetting;
            res += sesb.GenerateDesc();
        }
        else if (ExecuteSettingType == ExecuteSettingTypes.BattleCry || ExecuteSettingType == ExecuteSettingTypes.EquipBattleCry) res += BaseInfo.AddImportantColorToText(LanguageManager_Common.GetText("TriggerTime_BattleCry") + ": ");
        else if (ExecuteSettingType == ExecuteSettingTypes.SelfMechDie || ExecuteSettingType == ExecuteSettingTypes.SelfEquipDie) res += BaseInfo.AddImportantColorToText(LanguageManager_Common.GetText("TriggerTime_Die") + ": ");
        else if (ExecuteSettingType == ExecuteSettingTypes.WhenSelfMechAttack) res += LanguageManager_Common.GetText("TriggerTime_WhenAttack");
        else if (ExecuteSettingType == ExecuteSettingTypes.WhenSelfMechKillOther) res += LanguageManager_Common.GetText("TriggerTime_WhenKill");
        else if (ExecuteSettingType == ExecuteSettingTypes.PlayOutEffect)
        {
            foreach (SideEffectBase se in SideEffectBases)
            {
                if (se is Exile_Base)
                {
                    res = BaseInfo.AddImportantColorToText(LanguageManager_Common.GetText("TriggerTime_Disposable")) + "." + res;
                }
            }
        }
        else
        {
            res += string.Format(TriggerTimeDesc[LanguageManager_Common.GetCurrentLanguage()][M_ExecuteSetting.TriggerTime], BaseInfo.AddHighLightColorToText(TriggerRangeDesc[LanguageManager_Common.GetCurrentLanguage()][M_ExecuteSetting.TriggerRange]));
        }

        foreach (SideEffectBase se in SideEffectBases)
        {
            if (se is Exile_Base) continue;
            string sebDesc = se.GenerateDesc();
            sebDesc = sebDesc.TrimEnd("，。;,.;/n ".ToCharArray());
            if (sebDesc.EndsWith("</color>"))
            {
                sebDesc = sebDesc.Remove(sebDesc.LastIndexOf("</color>"));
                sebDesc = sebDesc.TrimEnd("，。;,.;/n ".ToCharArray());
                sebDesc += "</color>";
            }

            sebDesc = sebDesc.TrimEnd("，。;,.;/n ".ToCharArray());
            res += sebDesc + ". ";
        }

        res = res.TrimEnd("，。;,.;/n ".ToCharArray());

        if (res.EndsWith("</color>"))
        {
            res = res.Remove(res.LastIndexOf("</color>"));
            res = res.TrimEnd("，。;,.;/n ".ToCharArray());
            res += "</color>";
        }

        res = res.TrimEnd("，。;,.;/n ".ToCharArray());

        return res;
    }

    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TriggerTime
    {
        None = 0,

        /// <summary>
        /// e.g. a certain buff (double next damage) need to be triggered before next damage skill effect is triggered and double the skill effect number.
        /// So we need a super trigger to monitor all common triggers.
        /// </summary>
        OnTrigger = 1 << 0,

        OnBeginRound = 1 << 1,
        OnDrawCard = 1 << 2,
        OnPlayCard = OnPlayMechCard | OnPlayEquipCard | OnPlaySpellCard | OnPlayEnergyCard,
        OnPlayMechCard = 1 << 3,
        OnPlayEquipCard = 1 << 4,
        OnPlaySpellCard = 1 << 5,
        OnPlayEnergyCard = 1 << 6,

        /// <summary>
        /// Don't Invoke，otherwise it'll trigger multiple times.
        /// </summary>
        OnMechSummon = OnHeroSummon | OnSoldierSummon,
        OnHeroSummon = 1 << 7,
        OnSoldierSummon = 1 << 8,

        /// <summary>
        /// Don't Invoke，otherwise it'll trigger multiple times.
        /// </summary>
        OnMechAttack = OnHeroAttack | OnSoldierAttack,
        OnHeroAttack = 1 << 9,
        OnSoldierAttack = 1 << 10,

        /// <summary>
        /// Don't Invoke，otherwise it'll trigger multiple times.
        /// </summary>
        OnMechInjured = OnHeroInjured | OnSoldierInjured,
        OnHeroInjured = 1 << 11,
        OnSoldierInjured = 1 << 12,

        /// <summary>
        /// Don't Invoke，otherwise it'll trigger multiple times.
        /// </summary>
        OnMechKill = OnHeroKill | OnSoldierKill,
        OnHeroKill = 1 << 13,
        OnSoldierKill = 1 << 14,

        /// <summary>
        /// Don't Invoke，otherwise it'll trigger multiple times.
        /// </summary>
        OnMechMakeDamage = OnHeroMakeDamage | OnSoldierMakeDamage,
        OnHeroMakeDamage = 1 << 15,
        OnSoldierMakeDamage = 1 << 16,

        /// <summary>
        /// Don't Invoke，otherwise it'll trigger multiple times.
        /// </summary>
        OnMechBeHealed = OnHeroBeHealed | OnSoldierBeHealed,
        OnHeroBeHealed = 1 << 17,
        OnSoldierBeHealed = 1 << 18,

        /// <summary>
        /// Don't Invoke，otherwise it'll trigger multiple times.
        /// </summary>
        OnMechDie = OnHeroDie | OnSoldierDie,
        OnHeroDie = 1 << 19,
        OnSoldierDie = 1 << 20,

        OnEquipDie = 1 << 21,
        OnEquipEquiped = 1 << 22,

        /// <summary>
        /// Don't Invoke，otherwise it'll trigger multiple times.
        /// </summary>
        OnMakeDamage = OnMakeSpellDamage | OnMechMakeDamage,
        OnMakeSpellDamage = 1 << 23,

        OnPlayerGetEnergy = 1 << 24,
        OnPlayerEnergyFull = 1 << 25,
        OnPlayerEnergyEmpty = 1 << 26,
        OnPlayerUseEnergy = 1 << 27,
        OnPlayerAddLife = 1 << 28,
        OnPlayerLostLife = 1 << 29,
        OnEndRound = 1 << 30,

        OnUseMetal = 1 << 31,
    }

    private static SortedDictionary<TriggerTime, HashSet<TriggerRange>> TriggerTimeToTriggerRangeMap = new SortedDictionary<TriggerTime, HashSet<TriggerRange>>
    {
        {TriggerTime.None, new HashSet<TriggerRange> {TriggerRange.None}},
        {TriggerTime.OnTrigger, new HashSet<TriggerRange> {TriggerRange.None}},
        {TriggerTime.OnBeginRound, new HashSet<TriggerRange> {TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.OnePlayer}},
        {TriggerTime.OnDrawCard, new HashSet<TriggerRange> {TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.OnePlayer}},
        {TriggerTime.OnPlayCard, new HashSet<TriggerRange> {TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.OnePlayer}},
        {TriggerTime.OnPlayMechCard, new HashSet<TriggerRange> {TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.OnePlayer}},
        {TriggerTime.OnPlayEquipCard, new HashSet<TriggerRange> {TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.OnePlayer}},
        {TriggerTime.OnPlaySpellCard, new HashSet<TriggerRange> {TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.OnePlayer}},
        {TriggerTime.OnPlayEnergyCard, new HashSet<TriggerRange> {TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.OnePlayer}},
        {TriggerTime.OnMechSummon, new HashSet<TriggerRange> {TriggerRange.Self, TriggerRange.SelfAnother, TriggerRange.Another, TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer}},
        {TriggerTime.OnHeroSummon, new HashSet<TriggerRange> {TriggerRange.SelfAnother, TriggerRange.Another, TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer}},
        {TriggerTime.OnSoldierSummon, new HashSet<TriggerRange> {TriggerRange.SelfAnother, TriggerRange.Another, TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer}},
        {TriggerTime.OnMechAttack, new HashSet<TriggerRange> {TriggerRange.Self, TriggerRange.SelfAnother, TriggerRange.Another, TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.AttachedMech}},
        {TriggerTime.OnHeroAttack, new HashSet<TriggerRange> {TriggerRange.SelfAnother, TriggerRange.Another, TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.AttachedMech}},
        {TriggerTime.OnSoldierAttack, new HashSet<TriggerRange> {TriggerRange.SelfAnother, TriggerRange.Another, TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.AttachedMech}},
        {TriggerTime.OnMechInjured, new HashSet<TriggerRange> {TriggerRange.Self, TriggerRange.SelfAnother, TriggerRange.Another, TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.AttachedMech}},
        {TriggerTime.OnHeroInjured, new HashSet<TriggerRange> {TriggerRange.SelfAnother, TriggerRange.Another, TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.AttachedMech}},
        {TriggerTime.OnSoldierInjured, new HashSet<TriggerRange> {TriggerRange.SelfAnother, TriggerRange.Another, TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.AttachedMech}},
        {TriggerTime.OnMechKill, new HashSet<TriggerRange> {TriggerRange.Self, TriggerRange.SelfAnother, TriggerRange.Another, TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.AttachedMech}},
        {TriggerTime.OnHeroKill, new HashSet<TriggerRange> {TriggerRange.SelfAnother, TriggerRange.Another, TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.AttachedMech}},
        {TriggerTime.OnSoldierKill, new HashSet<TriggerRange> {TriggerRange.SelfAnother, TriggerRange.Another, TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.AttachedMech}},
        {TriggerTime.OnMechMakeDamage, new HashSet<TriggerRange> {TriggerRange.Self, TriggerRange.SelfAnother, TriggerRange.Another, TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.AttachedMech}},
        {TriggerTime.OnHeroMakeDamage, new HashSet<TriggerRange> {TriggerRange.SelfAnother, TriggerRange.Another, TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.AttachedMech}},
        {TriggerTime.OnSoldierMakeDamage, new HashSet<TriggerRange> {TriggerRange.SelfAnother, TriggerRange.Another, TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.AttachedMech}},
        {TriggerTime.OnMechBeHealed, new HashSet<TriggerRange> {TriggerRange.Self, TriggerRange.SelfAnother, TriggerRange.Another, TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.AttachedMech}},
        {TriggerTime.OnHeroBeHealed, new HashSet<TriggerRange> {TriggerRange.SelfAnother, TriggerRange.Another, TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.AttachedMech}},
        {TriggerTime.OnSoldierBeHealed, new HashSet<TriggerRange> {TriggerRange.SelfAnother, TriggerRange.Another, TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.AttachedMech}},
        {TriggerTime.OnMechDie, new HashSet<TriggerRange> {TriggerRange.Self, TriggerRange.SelfAnother, TriggerRange.Another, TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.AttachedMech}},
        {TriggerTime.OnHeroDie, new HashSet<TriggerRange> {TriggerRange.SelfAnother, TriggerRange.Another, TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.AttachedMech}},
        {TriggerTime.OnSoldierDie, new HashSet<TriggerRange> {TriggerRange.SelfAnother, TriggerRange.Another, TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.AttachedMech}},
        {TriggerTime.OnEquipDie, new HashSet<TriggerRange> {TriggerRange.Self, TriggerRange.AttachedEquip, TriggerRange.SelfAnother, TriggerRange.Another, TriggerRange.EnemyPlayer}},
        {TriggerTime.OnEquipEquiped, new HashSet<TriggerRange> {TriggerRange.Self, TriggerRange.AttachedEquip, TriggerRange.SelfAnother, TriggerRange.Another, TriggerRange.EnemyPlayer}},
        {TriggerTime.OnMakeDamage, new HashSet<TriggerRange> {TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.OnePlayer}},
        {TriggerTime.OnMakeSpellDamage, new HashSet<TriggerRange> {TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.OnePlayer}},
        {TriggerTime.OnPlayerGetEnergy, new HashSet<TriggerRange> {TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.OnePlayer}},
        {TriggerTime.OnPlayerEnergyFull, new HashSet<TriggerRange> {TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.OnePlayer}},
        {TriggerTime.OnPlayerEnergyEmpty, new HashSet<TriggerRange> {TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.OnePlayer}},
        {TriggerTime.OnPlayerUseEnergy, new HashSet<TriggerRange> {TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.OnePlayer}},
        {TriggerTime.OnPlayerAddLife, new HashSet<TriggerRange> {TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.OnePlayer}},
        {TriggerTime.OnPlayerLostLife, new HashSet<TriggerRange> {TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.OnePlayer}},
        {TriggerTime.OnEndRound, new HashSet<TriggerRange> {TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.OnePlayer}},
        {TriggerTime.OnUseMetal, new HashSet<TriggerRange> {TriggerRange.SelfPlayer, TriggerRange.EnemyPlayer, TriggerRange.OnePlayer}},
    };

    public static List<string> GetTriggerRangeListByTriggerTime(TriggerTime tt)
    {
        List<string> res = new List<string>();
        foreach (TriggerRange triggerRange in TriggerTimeToTriggerRangeMap[tt])
        {
            res.Add(triggerRange.ToString());
        }

        return res;
    }

    public static HashSet<TriggerTime> GetAllTriggerTimeSet()
    {
        HashSet<TriggerTime> res = new HashSet<TriggerTime>();
        foreach (TriggerTime tr in Enum.GetValues(typeof(TriggerTime)))
        {
            res.Add(tr);
        }

        return res;
    }

    public static TriggerTime GetTriggerTimeByCardType(CardTypes cardTypes)
    {
        switch (cardTypes)
        {
            case CardTypes.Mech:
            {
                return TriggerTime.OnPlayMechCard;
            }
            case CardTypes.Equip:
            {
                return TriggerTime.OnPlayEquipCard;
            }
            case CardTypes.Spell:
            {
                return TriggerTime.OnPlaySpellCard;
            }
            case CardTypes.Energy:
            {
                return TriggerTime.OnPlayEnergyCard;
            }
        }

        return TriggerTime.OnPlayMechCard;
    }

    public static SortedDictionary<string, SortedDictionary<TriggerTime, string>> TriggerTimeDesc = new SortedDictionary<string, SortedDictionary<TriggerTime, string>>
    {
        {
            "zh", new SortedDictionary<TriggerTime, string>
            {
                {TriggerTime.None, ""},
                {TriggerTime.OnTrigger, "{0}"},

                {TriggerTime.OnBeginRound, "每当{0}回合开始时, "},
                {TriggerTime.OnDrawCard, "每当{0}抽牌时, "},
                {TriggerTime.OnPlayCard, "每当{0}出牌时, "},
                {TriggerTime.OnPlayMechCard, "每当{0}打出机甲牌时, "},
                {TriggerTime.OnPlayEquipCard, "每当{0}打出装备牌时, "},
                {TriggerTime.OnPlaySpellCard, "每当{0}使用法术牌时, "},
                {TriggerTime.OnPlayEnergyCard, "每当{0}使用法术牌时, "},

                {TriggerTime.OnMechSummon, "每当{0}召唤机甲时, "},
                {TriggerTime.OnHeroSummon, "每当{0}召唤英雄时, "},
                {TriggerTime.OnSoldierSummon, "每当{0}召唤士兵时, "},

                {TriggerTime.OnMechAttack, "每当{0}机甲进攻时, "},
                {TriggerTime.OnHeroAttack, "每当{0}英雄进攻时, "},
                {TriggerTime.OnSoldierAttack, "每当{0}士兵进攻时, "},

                {TriggerTime.OnMechInjured, "每当{0}机甲受损时, "},
                {TriggerTime.OnHeroInjured, "每当{0}英雄受损时, "},
                {TriggerTime.OnSoldierInjured, "每当{0}士兵受损时, "},

                {TriggerTime.OnMechKill, "每当{0}机甲成功击杀, "},
                {TriggerTime.OnHeroKill, "每当{0}英雄成功击杀, "},
                {TriggerTime.OnSoldierKill, "每当{0}士兵成功击杀, "},

                {TriggerTime.OnMechMakeDamage, "每当{0}机甲造成伤害时, "},
                {TriggerTime.OnHeroMakeDamage, "每当{0}英雄造成伤害时, "},
                {TriggerTime.OnSoldierMakeDamage, "每当{0}士兵造成伤害时, "},

                {TriggerTime.OnMechBeHealed, "每当{0}机甲得到修复时, "},
                {TriggerTime.OnHeroBeHealed, "每当{0}英雄得到修复时, "},
                {TriggerTime.OnSoldierBeHealed, "每当{0}士兵得到修复时, "},

                {TriggerTime.OnMechDie, "每当{0}机甲死亡时, "},
                {TriggerTime.OnHeroDie, "每当{0}英雄死亡时, "},
                {TriggerTime.OnSoldierDie, "每当{0}士兵死亡时, "},

                {TriggerTime.OnEquipDie, "每当{0}装备破坏时, "},
                {TriggerTime.OnEquipEquiped, "每当{0}穿戴装备时, "},

                {TriggerTime.OnMakeDamage, "每当{0}造成伤害时, "},
                {TriggerTime.OnMakeSpellDamage, "每当{0}造成法术伤害时, "},

                {TriggerTime.OnPlayerGetEnergy, "每当{0}获得能量时, "},
                {TriggerTime.OnPlayerEnergyFull, "每当{0}能量满槽时, "},
                {TriggerTime.OnPlayerEnergyEmpty, "每当{0}能像耗尽时, "},
                {TriggerTime.OnPlayerUseEnergy, "每当{0}消耗能量时, "},
                {TriggerTime.OnPlayerAddLife, "每当{0}获得生命时, "},
                {TriggerTime.OnPlayerLostLife, "每当{0}生命减少时, "},
                {TriggerTime.OnEndRound, "每当{0}回合结束时, "},

                {TriggerTime.OnUseMetal, "每当{0}消耗金属时, "},
            }
        },
        {
            "en", new SortedDictionary<TriggerTime, string>
            {
                {TriggerTime.None, ""},
                {TriggerTime.OnTrigger, "{0}"},

                {TriggerTime.OnBeginRound, "each time when {0} turn starts, "},
                {TriggerTime.OnDrawCard, "each time when {0} draws, "},
                {TriggerTime.OnPlayCard, "each time when {0} plays a card, "},
                {TriggerTime.OnPlayMechCard, "each time when {0} plays a Mech card, "},
                {TriggerTime.OnPlayEquipCard, "each time when {0} plays an Equip card, "},
                {TriggerTime.OnPlaySpellCard, "each time when {0} plays a Spell card, "},
                {TriggerTime.OnPlayEnergyCard, "each time when {0} plays an Energy card, "},

                {TriggerTime.OnMechSummon, "each time when {0} mech summoned, "},
                {TriggerTime.OnHeroSummon, "each time when {0} hero-mech summoned, "},
                {TriggerTime.OnSoldierSummon, "each time when {0} soldier-mech summoned, "},

                {TriggerTime.OnMechAttack, "each time when {0} mech attacks, "},
                {TriggerTime.OnHeroAttack, "each time when {0} hero-mech attacks, "},
                {TriggerTime.OnSoldierAttack, "each time when {0} soldier-mech attacks, "},

                {TriggerTime.OnMechInjured, "each time when {0} mech damaged, "},
                {TriggerTime.OnHeroInjured, "each time when {0} hero-mech damaged, "},
                {TriggerTime.OnSoldierInjured, "each time when {0} soldier-mech damaged, "},

                {TriggerTime.OnMechKill, "each time when {0} mech kill enemy, "},
                {TriggerTime.OnHeroKill, "each time when {0} hero-mech kill enemy, "},
                {TriggerTime.OnSoldierKill, "each time when {0} soldier-mech kill enemy, "},

                {TriggerTime.OnMechMakeDamage, "each time when {0} mech make damage, "},
                {TriggerTime.OnHeroMakeDamage, "each time when {0} hero-mech make damage, "},
                {TriggerTime.OnSoldierMakeDamage, "each time when {0} soldier-mech make damage, "},

                {TriggerTime.OnMechBeHealed, "each time when {0} mech is healed, "},
                {TriggerTime.OnHeroBeHealed, "each time when {0} hero-mech is healed, "},
                {TriggerTime.OnSoldierBeHealed, "each time when {0} soldier-mech is healed, "},

                {TriggerTime.OnMechDie, "each time when {0} mech died, "},
                {TriggerTime.OnHeroDie, "each time when {0} hero-mech died, "},
                {TriggerTime.OnSoldierDie, "each time when {0} soldier-mech died, "},

                {TriggerTime.OnEquipDie, "each time when {0} equipment broken, "},
                {TriggerTime.OnEquipEquiped, "each time when {0} equipment equipped, "},

                {TriggerTime.OnMakeDamage, "each time when {0} deal damage, "},
                {TriggerTime.OnMakeSpellDamage, "each time when {0} deal spell damage, "},

                {TriggerTime.OnPlayerGetEnergy, "each time when {0} get energy, "},
                {TriggerTime.OnPlayerEnergyFull, "each time when {0}'s energy is full, "},
                {TriggerTime.OnPlayerEnergyEmpty, "each time when {0}'s energy is empty, "},
                {TriggerTime.OnPlayerUseEnergy, "each time when {0} consume energy, "},
                {TriggerTime.OnPlayerAddLife, "each time when {0} get healed, "},
                {TriggerTime.OnPlayerLostLife, "each time when {0} lost life, "},
                {TriggerTime.OnEndRound, "each time when {0} turn ends, "},

                {TriggerTime.OnUseMetal, "each time when {0} consume metal, "},
            }
        }
    };

    public static SortedDictionary<string, SortedDictionary<TriggerTime, string>> TriggerTimeDesc_ForRemoveTriggerTime = new SortedDictionary<string, SortedDictionary<TriggerTime, string>>
    {
        {
            "zh", new SortedDictionary<TriggerTime, string>
            {
                {TriggerTime.None, ""},
                {TriggerTime.OnTrigger, "{0}{1}"},

                {TriggerTime.OnBeginRound, "在此后的第{0}个{1}回合开始前, "},
                {TriggerTime.OnDrawCard, "在此后的第{0}次{1}抽牌前, "},
                {TriggerTime.OnPlayCard, "在此后的第{0}次{1}出牌前, "},
                {TriggerTime.OnPlayMechCard, "在此后的第{0}次{1}打出机甲牌前, "},
                {TriggerTime.OnPlayEquipCard, "在此后的第{0}次{1}打出装备牌前, "},
                {TriggerTime.OnPlaySpellCard, "在此后的第{0}次{1}使用法术牌前, "},
                {TriggerTime.OnPlayEnergyCard, "在此后的第{0}次{1}使用能量牌前, "},

                {TriggerTime.OnMechSummon, "在此后的第{0}次{1}召唤机甲前, "},
                {TriggerTime.OnHeroSummon, "在此后的第{0}次{1}召唤英雄前, "},
                {TriggerTime.OnSoldierSummon, "在此后的第{0}次{1}召唤士兵前, "},

                {TriggerTime.OnMechAttack, "在此后的第{0}次{1}机甲进攻前, "},
                {TriggerTime.OnHeroAttack, "在此后的第{0}次{1}英雄进攻前, "},
                {TriggerTime.OnSoldierAttack, "在此后的第{0}次{1}士兵进攻前, "},

                {TriggerTime.OnMechInjured, "在此后的第{0}次{1}机甲受损前, "},
                {TriggerTime.OnHeroInjured, "在此后的第{0}次{1}英雄受损前, "},
                {TriggerTime.OnSoldierInjured, "在此后的第{0}次{1}士兵受损前, "},

                {TriggerTime.OnMechKill, "在此后的第{0}次{1}机甲成功击杀前, "},
                {TriggerTime.OnHeroKill, "在此后的第{0}次{1}英雄成功击杀前, "},
                {TriggerTime.OnSoldierKill, "在此后的第{0}次{1}士兵成功击杀前, "},

                {TriggerTime.OnMechMakeDamage, "在此后的第{0}次{1}机甲造成伤害前, "},
                {TriggerTime.OnHeroMakeDamage, "在此后的第{0}次{1}英雄造成伤害前, "},
                {TriggerTime.OnSoldierMakeDamage, "在此后的第{0}次{1}士兵造成伤害前, "},

                {TriggerTime.OnMechBeHealed, "在此后的第{0}次{1}机甲得到修复前, "},
                {TriggerTime.OnHeroBeHealed, "在此后的第{0}次{1}英雄得到修复前, "},
                {TriggerTime.OnSoldierBeHealed, "在此后的第{0}次{1}士兵得到修复前, "},

                {TriggerTime.OnMechDie, "在此后的第{0}次{1}机甲死亡前, "},
                {TriggerTime.OnHeroDie, "在此后的第{0}次{1}英雄死亡前, "},
                {TriggerTime.OnSoldierDie, "在此后的第{0}次{1}士兵死亡前, "},

                {TriggerTime.OnEquipDie, "在此后的第{0}次{1}装备破坏前, "},
                {TriggerTime.OnEquipEquiped, "在此后的第{0}次{1}穿戴装备前, "},

                {TriggerTime.OnMakeDamage, "在此后的第{0}次{1}造成伤害前, "},
                {TriggerTime.OnMakeSpellDamage, "在此后的第{0}次{1}造成法术伤害前, "},

                {TriggerTime.OnPlayerGetEnergy, "在此后的第{0}次{1}获得能量前, "},
                {TriggerTime.OnPlayerEnergyFull, "在此后的第{0}次{1}能量满槽前, "},
                {TriggerTime.OnPlayerEnergyEmpty, "在此后的第{0}次{1}能量耗尽前, "},
                {TriggerTime.OnPlayerUseEnergy, "在此后的第{0}次{1}消耗能量前, "},
                {TriggerTime.OnPlayerAddLife, "在此后的第{0}次{1}获得生命前, "},
                {TriggerTime.OnPlayerLostLife, "在此后的第{0}次{1}生命减少前, "},
                {TriggerTime.OnEndRound, "在此后的第{0}次{1}回合结束前, "},

                {TriggerTime.OnUseMetal, "在此后的第{0}次{1}消耗金属前, "},
            }
        },
        {
            "en", new SortedDictionary<TriggerTime, string>
            {
                {TriggerTime.None, ""},
                {TriggerTime.OnTrigger, "{0}"},

                {TriggerTime.OnBeginRound, "before {1} next {0}th turn starts, "},
                {TriggerTime.OnDrawCard, "before {1}'s next {0}th draw, "},
                {TriggerTime.OnPlayCard, "before {1}'s next {0}th playing card, "},
                {TriggerTime.OnPlayMechCard, "before {1}'s next {0}th playing Mech card, "},
                {TriggerTime.OnPlayEquipCard, "before {1}'s next {0}th playing Equip card, "},
                {TriggerTime.OnPlaySpellCard, "before {1}'s next {0}th playing Spell card, "},
                {TriggerTime.OnPlayEnergyCard, "before {1}'s next {0}th playing Energy card, "},

                {TriggerTime.OnMechSummon, "before {1} next {0}th mech's summon, "},
                {TriggerTime.OnHeroSummon, "before {1} next {0}th hero-mech's summon, "},
                {TriggerTime.OnSoldierSummon, "before {1} next {0}th soldier-mech's summon, "},

                {TriggerTime.OnMechAttack, "before {1} next {0}th mech's attack, "},
                {TriggerTime.OnHeroAttack, "before {1} next {0}th hero-mech's attack, "},
                {TriggerTime.OnSoldierAttack, "before {1} next {0}th soldier-mech's attack, "},

                {TriggerTime.OnMechInjured, "before {1} next {0}th mech's injury, "},
                {TriggerTime.OnHeroInjured, "before {1} next {0}th hero-mech's injury, "},
                {TriggerTime.OnSoldierInjured, "before {1} next {0}th soldier-mech's injury, "},

                {TriggerTime.OnMechKill, "before {1} next {0}th mech's killing enemy, "},
                {TriggerTime.OnHeroKill, "before {1} next {0}th hero-mech's killing enemy, "},
                {TriggerTime.OnSoldierKill, "before {1} next {0}th soldier-mech's killing enemy, "},

                {TriggerTime.OnMechMakeDamage, "before {1} next {0}th mech's dealing damage, "},
                {TriggerTime.OnHeroMakeDamage, "before {1} next {0}th hero-mech's dealing damage, "},
                {TriggerTime.OnSoldierMakeDamage, "before {1} next {0}th soldier-mech's dealing damage, "},

                {TriggerTime.OnMechBeHealed, "before {1} next {0}th mech's being healed, "},
                {TriggerTime.OnHeroBeHealed, "before {1} next {0}th hero-mech's being healed, "},
                {TriggerTime.OnSoldierBeHealed, "before {1} next {0}th soldier-mech's being healed, "},

                {TriggerTime.OnMechDie, "before {1} next {0}th mech's death, "},
                {TriggerTime.OnHeroDie, "before {1} next {0}th hero-mech's death, "},
                {TriggerTime.OnSoldierDie, "before {1} next {0}th soldier-mech's death, "},

                {TriggerTime.OnEquipDie, "before {1} next {0}th equip's being broken, "},
                {TriggerTime.OnEquipEquiped, "before {1} next {0}th equip's being equipped, "},

                {TriggerTime.OnMakeDamage, "before {1} next {0}th damage dealt, "},
                {TriggerTime.OnMakeSpellDamage, "before {1} next {0}th spell damage dealt, "},

                {TriggerTime.OnPlayerGetEnergy, "before {1} next {0}th player's getting energy, "},
                {TriggerTime.OnPlayerEnergyFull, "before {1} next {0}th player's energy getting full, "},
                {TriggerTime.OnPlayerEnergyEmpty, "before {1} next {0}th player's energy getting empty, "},
                {TriggerTime.OnPlayerUseEnergy, "before {1} next {0}th player's using energy, "},
                {TriggerTime.OnPlayerAddLife, "before {1} next {0}th spaceship's being healed, "},
                {TriggerTime.OnPlayerLostLife, "before {1} next {0}th spaceship's being damaged, "},
                {TriggerTime.OnEndRound, "before {1} next {0}th turn-ends, "},

                {TriggerTime.OnUseMetal, "before {1} next {0}th player's using metal, "},
            }
        }
    };

    /// <summary>
    /// TriggerRange is used together with TriggerTime
    /// If you use a TriggerTime.OnBeginRound and a TriggerRange.EnemyPlayer, then this sideeffect will be triggered in Enemy's Begin Round Phase.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TriggerRange
    {
        None,

        SelfPlayer,
        EnemyPlayer,
        OnePlayer,
        One,
        SelfAnother,
        Another,
        AttachedMech,
        AttachedEquip,
        Self,
    }

    public static SortedDictionary<string, SortedDictionary<TriggerRange, string>> TriggerRangeDesc = new SortedDictionary<string, SortedDictionary<TriggerRange, string>>
    {
        {
            "zh", new SortedDictionary<TriggerRange, string>
            {
                {TriggerRange.None, ""},
                {TriggerRange.SelfPlayer, "我方"},
                {TriggerRange.EnemyPlayer, "敌方"},
                {TriggerRange.OnePlayer, "任一方"},
                {TriggerRange.One, "一个"},
                {TriggerRange.SelfAnother, "我方其他"},
                {TriggerRange.Another, "其他"},
                {TriggerRange.AttachedMech, "属主"},
                {TriggerRange.AttachedEquip, "附属"},
                {TriggerRange.Self, ""},
            }
        },
        {
            "en", new SortedDictionary<TriggerRange, string>
            {
                {TriggerRange.None, ""},
                {TriggerRange.SelfPlayer, "you"},
                {TriggerRange.EnemyPlayer, "enemy"},
                {TriggerRange.OnePlayer, "one player"},
                {TriggerRange.One, "one"},
                {TriggerRange.SelfAnother, "your another"},
                {TriggerRange.Another, "another"},
                {TriggerRange.AttachedMech, "attached"},
                {TriggerRange.AttachedEquip, "attached"},
                {TriggerRange.Self, "this"},
            }
        }
    };

    public static string GetTriggerTimeTriggerRangeDescCombination(TriggerTime tt, TriggerRange tr)
    {
        string str_tt = TriggerTimeDesc[LanguageManager_Common.GetCurrentLanguage()][tt];
        string str_tr = TriggerRangeDesc[LanguageManager_Common.GetCurrentLanguage()][tr];
        return string.Format(str_tt, str_tr);
    }

    public static string GetRemoveTriggerTimeTriggerRangeDescCombination(TriggerTime removeTriggerTime, int removeTriggerTimes, TriggerRange tr)
    {
        string str_tt = TriggerTimeDesc_ForRemoveTriggerTime[LanguageManager_Common.GetCurrentLanguage()][removeTriggerTime];
        string str_tr = TriggerRangeDesc[LanguageManager_Common.GetCurrentLanguage()][tr];
        return Utils.HighlightStringFormat(str_tt, AllColors.ColorDict[AllColors.ColorType.CardHighLightColor], removeTriggerTimes, str_tr);
    }
}