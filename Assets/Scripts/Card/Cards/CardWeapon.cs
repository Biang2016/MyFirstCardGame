﻿using System.Collections.Generic;
using UnityEngine;

internal class CardWeapon : CardBase
{
    protected override void Awake()
    {
        base.Awake();
        gameObjectPool = GameObjectPoolManager.GOPM.Pool_WeaponCardPool;
    }

    void Start()
    {
    }

    void Update()
    {
    }

    #region 卡牌上各模块

    public TextMesh WeaponName;

    public TextMesh WeaponDesc;

    private string m_WeaponName;

    public string M_WeaponName
    {
        get { return m_WeaponName; }

        set
        {
            m_WeaponName = value;
            WeaponName.text = M_WeaponName;
        }
    }

    private string m_WeaponDesc;

    public string M_WeaponDesc
    {
        get { return m_WeaponDesc; }

        set
        {
            m_WeaponDesc = value;
            WeaponDesc.text = M_WeaponDesc;
        }
    }

    # endregion

    public override void Initiate(CardInfo_Base cardInfo, Player player)
    {
        base.Initiate(cardInfo, player);
        Player = player;
        CardInfo = cardInfo;
        M_WeaponName = CardInfo.CardName;
        M_WeaponDesc = CardInfo.CardDesc;
    }

    public override void DragComponent_OnMouseUp(BoardAreaTypes boardAreaType, List<SlotAnchor> slotAnchors, ModuleRetinue moduleRetinue, Vector3 dragLastPosition, Vector3 dragBeginPosition, Quaternion dragBeginQuaternion)
    {
        base.DragComponent_OnMouseUp(boardAreaType, slotAnchors, moduleRetinue, dragLastPosition, dragBeginPosition, dragBeginQuaternion);

        if (boardAreaType != Player.MyHandArea) //离开手牌区域
            foreach (var sa in slotAnchors)
                if (sa.M_SlotType == SlotType.Weapon && sa.Player == Player)
                {
                    summonWeapon(sa.M_ModuleRetinue);
                    return;
                }

        transform.SetPositionAndRotation(dragBeginPosition, dragBeginQuaternion); //如果脱手地方还在手中，则收回
        Player.MyHandManager.RefreshCardsPlace();
    }


    public override float DragComponnet_DragDistance()
    {
        return 5f;
    }

    #region 卡牌效果

    //装备武器
    private void summonWeapon(ModuleRetinue moduleRetinue)
    {
        Player.UseCost(M_Cost);
        if (moduleRetinue == null)
        {
            Debug.Log("No retinue on Place BUT SLOT HIT");
            return;
        }

        ModuleWeapon newModueWeapon = GameObjectPoolManager.GOPM.Pool_ModuleWeaponPool.AllocateGameObject(moduleRetinue.transform).GetComponent<ModuleWeapon>();
        newModueWeapon.M_ModuleRetinue = moduleRetinue;
        newModueWeapon.Initiate(CardInfo, Player);
        moduleRetinue.M_Weapon = newModueWeapon;
        PoolRecycle();
        Player.MyHandManager.DropCard(this);
    }

    #endregion
}