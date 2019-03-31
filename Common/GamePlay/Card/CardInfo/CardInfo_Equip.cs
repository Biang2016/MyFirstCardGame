﻿public class CardInfo_Equip : CardInfo_Base
{
    public CardInfo_Equip()
    {
    }

    public CardInfo_Equip(int cardID, BaseInfo baseInfo, UpgradeInfo upgradeInfo, EquipInfo equipInfo, WeaponInfo weaponInfo, ShieldInfo shieldInfo, PackInfo packInfo, MAInfo maInfo, SideEffectBundle sideEffectBundle, SideEffectBundle sideEffectBundle_OnBattleGround)
        : base(cardID: cardID,
            baseInfo: baseInfo,
            sideEffectBundle: sideEffectBundle,
            sideEffectBundle_OnBattleGround: sideEffectBundle_OnBattleGround)
    {
        switch (equipInfo.SlotType)
        {
            case SlotTypes.Weapon:
            {
                WeaponInfo = weaponInfo;
                break;
            }
            case SlotTypes.Shield:
            {
                ShieldInfo = shieldInfo;
                break;
            }
            case SlotTypes.Pack:
            {
                PackInfo = packInfo;
                break;
            }
            case SlotTypes.MA:
            {
                MAInfo = maInfo;
                break;
            }
        }

        EquipInfo = equipInfo;
        UpgradeInfo = upgradeInfo;
        Pro_Initialize();
    }

    public override string GetCardDescShow()
    {
        string CardDescShow = "";

        switch (EquipInfo.SlotType)
        {
            case SlotTypes.Weapon:
            {
                if (WeaponInfo.IsFrenzy) CardDescShow += LanguageManager_Common.GetText("KeyWords_Frenzy") + ". ";
                if (WeaponInfo.IsSentry) CardDescShow += LanguageManager_Common.GetText("KeyWords_Sentry") + ". ";
                if (WeaponInfo.WeaponType == WeaponTypes.Sword)
                {
                    CardDescShow += LanguageManager_Common.GetText("KeyWords_Sword") + ". ";
                    CardDescShow += string.Format(LanguageManager_Common.GetText("KeyWords_AttackPlus"), BaseInfo.AddHightLightColorToText(WeaponInfo.Attack.ToString()));
                    CardDescShow += string.Format(LanguageManager_Common.GetText("KeyWords_ChargePlus"), BaseInfo.AddHightLightColorToText(WeaponInfo.Energy + "/" + WeaponInfo.EnergyMax));
                }
                else if (WeaponInfo.WeaponType == WeaponTypes.Gun)
                {
                    CardDescShow += LanguageManager_Common.GetText("KeyWords_Gun") + ". ";
                    CardDescShow += string.Format(LanguageManager_Common.GetText("KeyWords_ShootAttackPlus"), BaseInfo.AddHightLightColorToText(WeaponInfo.Attack.ToString()));
                    CardDescShow += string.Format(LanguageManager_Common.GetText("KeyWords_Bullets"), BaseInfo.AddHightLightColorToText(WeaponInfo.Energy + "/" + WeaponInfo.EnergyMax));
                }
                else if (WeaponInfo.WeaponType == WeaponTypes.SniperGun)
                {
                    CardDescShow += LanguageManager_Common.GetText("KeyWords_SniperGun") + ". ";
                    CardDescShow += string.Format(LanguageManager_Common.GetText("KeyWords_SniperBulletAttackPlus"), BaseInfo.AddHightLightColorToText(WeaponInfo.Attack.ToString()));
                    CardDescShow += string.Format(LanguageManager_Common.GetText("KeyWords_SniperBulletPlus"), BaseInfo.AddHightLightColorToText(WeaponInfo.Energy + "/" + WeaponInfo.EnergyMax));
                }

                break;
            }
            case SlotTypes.Shield:
            {
                if (ShieldInfo.IsDefence) CardDescShow += LanguageManager_Common.GetText("KeyWords_Defense");
                if (ShieldInfo.ShieldType == ShieldTypes.Armor)
                {
                    CardDescShow += LanguageManager_Common.GetText("KeyWords_Armor") + ". ";
                    CardDescShow += string.Format(LanguageManager_Common.GetText("KeyWords_DefenseDamage"), BaseInfo.AddHightLightColorToText(ShieldInfo.Armor.ToString()));
                }
                else if (ShieldInfo.ShieldType == ShieldTypes.Shield)
                {
                    CardDescShow += LanguageManager_Common.GetText("KeyWords_Shield") + ". ";
                    CardDescShow += string.Format(LanguageManager_Common.GetText("KeyWords_ReduceDamage"), BaseInfo.AddHightLightColorToText(ShieldInfo.Shield.ToString()));
                }

                break;
            }
            case SlotTypes.Pack:
            {
                if (PackInfo.IsFrenzy) CardDescShow += LanguageManager_Common.GetText("KeyWords_Frenzy") + ". ";
                if (PackInfo.IsSniper) CardDescShow += LanguageManager_Common.GetText("KeyWords_Sniper") + ". ";
                if (PackInfo.IsDefence) CardDescShow += LanguageManager_Common.GetText("KeyWords_Defense") + ". ";
                if (PackInfo.DodgeProp != 0)
                {
                    CardDescShow += LanguageManager_Common.GetText("KeyWords_Dodge") + ". ";
                    CardDescShow += string.Format(LanguageManager_Common.GetText("KeyWords_PRValue"), BaseInfo.AddHightLightColorToText(PackInfo.DodgeProp + "%"));
                }

                break;
            }
            case SlotTypes.MA:
            {
                if (PackInfo.IsFrenzy) CardDescShow += LanguageManager_Common.GetText("KeyWords_Frenzy") + ". ";
                if (PackInfo.IsSniper) CardDescShow += LanguageManager_Common.GetText("KeyWords_Sniper") + ". ";
                if (PackInfo.IsDefence) CardDescShow += LanguageManager_Common.GetText("KeyWords_Defense") + ". ";
                break;
            }
        }

        CardDescShow += base.GetCardDescShow();

        CardDescShow = CardDescShow.TrimEnd(",.; \n".ToCharArray());

        return CardDescShow;
    }

    public override string GetCardColor()
    {
        switch (EquipInfo.SlotType)
        {
            case SlotTypes.Weapon:
                return AllColors.ColorDict[AllColors.ColorType.WeaponCardColor];
            case SlotTypes.Shield:
                return AllColors.ColorDict[AllColors.ColorType.ShieldCardColor];
            case SlotTypes.Pack:
                return AllColors.ColorDict[AllColors.ColorType.PackCardColor];
            case SlotTypes.MA:
                return AllColors.ColorDict[AllColors.ColorType.MACardColor];
        }

        return null;
    }

    public override float GetCardColorIntensity()
    {
        switch (EquipInfo.SlotType)
        {
            case SlotTypes.Weapon:
                return AllColors.IntensityDict[AllColors.ColorType.WeaponCardColor];
            case SlotTypes.Shield:
                return AllColors.IntensityDict[AllColors.ColorType.ShieldCardColor];
            case SlotTypes.Pack:
                return AllColors.IntensityDict[AllColors.ColorType.PackCardColor];
            case SlotTypes.MA:
                return AllColors.IntensityDict[AllColors.ColorType.MACardColor];
        }

        return 0f;
    }

    public override CardInfo_Base Clone()
    {
        CardInfo_Equip cb = new CardInfo_Equip(
            cardID: CardID,
            baseInfo: BaseInfo,
            upgradeInfo: UpgradeInfo,
            equipInfo: EquipInfo,
            weaponInfo: WeaponInfo,
            shieldInfo: ShieldInfo,
            packInfo: PackInfo,
            maInfo: MAInfo,
            sideEffectBundle: SideEffectBundle.Clone(),
            sideEffectBundle_OnBattleGround: SideEffectBundle_OnBattleGround.Clone());
        return cb;
    }

    public override string GetCardTypeDesc()
    {
        switch (EquipInfo.SlotType)
        {
            case SlotTypes.Weapon:
            {
                switch (WeaponInfo.WeaponType)
                {
                    case WeaponTypes.Sword:
                    {
                        return LanguageManager_Common.GetText("KeyWords_CardEquip_Sword");
                    }
                    case WeaponTypes.Gun:
                    {
                        return LanguageManager_Common.GetText("KeyWords_CardEquip_Gun");
                    }
                    case WeaponTypes.SniperGun:
                    {
                        return LanguageManager_Common.GetText("KeyWords_CardEquip_SniperGun");
                    }
                }

                return "";
            }
            case SlotTypes.Shield:
            {
                return LanguageManager_Common.GetText("KeyWords_CardEquip_Shield");
            }
            case SlotTypes.Pack:
            {
                return LanguageManager_Common.GetText("KeyWords_CardEquip_Pack");
            }
            case SlotTypes.MA:
            {
                return LanguageManager_Common.GetText("KeyWords_CardEquip_MA");
            }
        }

        return "";
    }
}