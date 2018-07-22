﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

internal abstract class ModuleBase : MonoBehaviour, IGameObjectPool, IDragComponent, IMouseHoverComponent
{
    internal int GameObjectID;
    protected GameObjectPool gameObjectPool;
    internal ClientPlayer ClientPlayer;

    public virtual void PoolRecycle()
    {
        GameObjectID = -1;
        if (GetComponent<DragComponent>())
        {
            GetComponent<DragComponent>().enabled = true;
        }

        if (GetComponent<MouseHoverComponent>())
        {
            GetComponent<MouseHoverComponent>().enabled = true;
        }

        if (GetComponent<BoxCollider>())
        {
            GetComponent<BoxCollider>().enabled = true;
        }

        if (this is ModuleWeapon)
        {
            ((ModuleWeapon) this).SetNoPreview();
        }

        if (this is ModuleShield)
        {
            ((ModuleShield) this).SetNoPreview();
        }

        gameObjectPool.RecycleGameObject(gameObject);
    }

    internal CardInfo_Base CardInfo; //卡牌原始数值信息

    void Start()
    {
    }

    void Update()
    {
    }

    //工具，初始化数字块
    protected void initiateNumbers(ref GameObject Number, ref CardNumberSet cardNumberSet, NumberSize numberType, CardNumberSet.TextAlign textAlign, GameObject block)
    {
        if (!Number)
        {
            Number = GameObjectPoolManager.GOPM.Pool_CardNumberSetPool.AllocateGameObject(block.transform);
            cardNumberSet = Number.GetComponent<CardNumberSet>();
            cardNumberSet.initiate(0, numberType, textAlign);
        }
        else
        {
            cardNumberSet = Number.GetComponent<CardNumberSet>();
            cardNumberSet.initiate(0, numberType, textAlign);
        }
    }

    protected void initiateNumbers(ref GameObject Number, ref CardNumberSet cardNumberSet, NumberSize numberType, CardNumberSet.TextAlign textAlign, GameObject block, char firstSign)
    {
        if (!Number)
        {
            Number = GameObjectPoolManager.GOPM.Pool_CardNumberSetPool.AllocateGameObject(block.transform);
            cardNumberSet = Number.GetComponent<CardNumberSet>();
            cardNumberSet.initiate(firstSign, 0, numberType, textAlign);
        }
        else
        {
            cardNumberSet = Number.GetComponent<CardNumberSet>();
            cardNumberSet.initiate(firstSign, 0, numberType, textAlign);
        }
    }

    public virtual void Initiate(CardInfo_Base cardInfo, ClientPlayer clientPlayer)
    {
        ClientPlayer = clientPlayer;
        CardInfo = cardInfo;
        ChangeColor(CardBase.HTMLColorToColor(cardInfo.BaseInfo.CardColor));
        Stars = cardInfo.UpgradeInfo.CardLevel;
    }

    #region 各模块

    public GameObject Star1;
    public GameObject Star2;
    public GameObject Star3;
    [SerializeField] protected int stars;

    public virtual int Stars
    {
        get { return stars; }

        set
        {
            stars = value;
            switch (value)
            {
                case 0:
                    if (Star1) Star1.SetActive(false);
                    if (Star2) Star2.SetActive(false);
                    if (Star3) Star3.SetActive(false);
                    break;
                case 1:
                    if (Star1) Star1.SetActive(true);
                    if (Star2) Star2.SetActive(false);
                    if (Star3) Star3.SetActive(false);
                    break;
                case 2:
                    if (Star1) Star1.SetActive(false);
                    if (Star2) Star2.SetActive(true);
                    if (Star3) Star3.SetActive(false);
                    break;
                case 3:
                    if (Star1) Star1.SetActive(false);
                    if (Star2) Star2.SetActive(false);
                    if (Star3) Star3.SetActive(true);
                    break;
                default: break;
            }
        }
    }

    public Renderer MainBoardRenderer;

    public void ChangeColor(Color newColor)
    {
        if (MainBoardRenderer)
        {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            MainBoardRenderer.GetPropertyBlock(mpb);
            mpb.SetColor("_Color", newColor);
            mpb.SetColor("_EmissionColor", newColor);
            MainBoardRenderer.SetPropertyBlock(mpb);
        }
        else
        {
            ClientLog.CL.Print("No MainBoardRenderer");
        }
    }

    #endregion


    #region 模块交互

    private CardBase detailCard;
    private CardBase detailCard_Weapon;
    private CardBase detailCard_Shield;
    private CardBase detailCard_Pack;
    private CardBase detailCard_MA;


    private void ShowCardDetail(Vector3 mousePosition) //鼠标悬停放大查看原卡牌信息
    {
        switch (CardInfo.BaseInfo.CardType)
        {
            case CardTypes.Retinue:
                detailCard = (CardRetinue) CardBase.InstantiateCardByCardInfo(CardInfo, GameBoardManager.GBM.CardDetailPreview.transform, ClientPlayer);
                detailCard.transform.localScale = Vector3.one * GameManager.GM.DetailCardSizeRetinue;
                detailCard.transform.position = new Vector3(mousePosition.x, 8f, mousePosition.z);
                detailCard.transform.Translate(Vector3.left * 5f);
                detailCard.GetComponent<BoxCollider>().enabled = false;
                detailCard.GetComponent<DragComponent>().enabled = false;

                if (((ModuleRetinue) this).M_Weapon)
                {
                    if (!((CardRetinue) detailCard).Weapon)
                    {
                        ((CardRetinue) detailCard).Weapon = GameObjectPoolManager.GOPM.Pool_ModuleWeaponDetailPool.AllocateGameObject(detailCard.transform).GetComponent<ModuleWeapon>();
                    }

                    CardInfo_Base cw = ((ModuleRetinue) this).M_Weapon.CardInfo;
                    ((CardRetinue) detailCard).Weapon.M_ModuleRetinue = (ModuleRetinue) this;
                    ((CardRetinue) detailCard).Weapon.Initiate(((ModuleRetinue) this).M_Weapon.CardInfo, ClientPlayer);
                    ((CardRetinue) detailCard).Weapon.GetComponent<DragComponent>().enabled = false;
                    ((CardRetinue) detailCard).Weapon.GetComponent<MouseHoverComponent>().enabled = false;
                    ((CardRetinue) detailCard).Weapon.SetPreview();

                    detailCard_Weapon = (CardWeapon) CardBase.InstantiateCardByCardInfo(cw, GameBoardManager.GBM.CardDetailPreview.transform, ClientPlayer);
                    detailCard_Weapon.transform.localScale = Vector3.one * GameManager.GM.DetailCardModuleSize;
                    detailCard_Weapon.transform.position = new Vector3(mousePosition.x, 2f, mousePosition.z);
                    detailCard_Weapon.transform.Translate(Vector3.right * 0.5f);
                    detailCard_Weapon.transform.Translate(Vector3.back * 3f);
                    detailCard_Weapon.transform.Translate(Vector3.up * 5f);
                    detailCard_Weapon.GetComponent<BoxCollider>().enabled = false;
                }

                if (((ModuleRetinue) this).M_Shield)
                {
                    if (!((CardRetinue) detailCard).Shield)
                    {
                        ((CardRetinue) detailCard).Shield = GameObjectPoolManager.GOPM.Pool_ModuleShieldDetailPool.AllocateGameObject(detailCard.transform).GetComponent<ModuleShield>();
                    }

                    CardInfo_Base cw = ((ModuleRetinue) this).M_Shield.CardInfo;
                    ((CardRetinue) detailCard).Shield.M_ModuleRetinue = (ModuleRetinue) this;
                    ((CardRetinue) detailCard).Shield.Initiate(((ModuleRetinue) this).M_Shield.CardInfo, ClientPlayer);
                    ((CardRetinue) detailCard).Shield.GetComponent<DragComponent>().enabled = false;
                    ((CardRetinue) detailCard).Shield.GetComponent<MouseHoverComponent>().enabled = false;

                    detailCard_Shield = (CardShield) CardBase.InstantiateCardByCardInfo(cw, GameBoardManager.GBM.CardDetailPreview.transform, ClientPlayer);
                    detailCard_Shield.transform.localScale = Vector3.one * GameManager.GM.DetailCardModuleSize;
                    detailCard_Shield.transform.position = new Vector3(mousePosition.x, 2f, mousePosition.z);
                    detailCard_Shield.transform.Translate(Vector3.right * 0.5f);
                    detailCard_Shield.transform.Translate(Vector3.forward * 3f);
                    detailCard_Shield.transform.Translate(Vector3.up * 5f);
                    detailCard_Shield.GetComponent<BoxCollider>().enabled = false;
                    ((CardRetinue) detailCard).Shield.SetPreview();
                }

                break;
            case CardTypes.Weapon:
                detailCard = (CardWeapon) CardBase.InstantiateCardByCardInfo(CardInfo, GameBoardManager.GBM.CardDetailPreview.transform, ClientPlayer);
                detailCard.transform.localScale = Vector3.one * GameManager.GM.DetailCardSize;
                detailCard.transform.position = new Vector3(mousePosition.x, 2f, mousePosition.z);
                detailCard.transform.Translate(Vector3.left * 3.5f);
                detailCard.transform.Translate(Vector3.up * 5f);
                detailCard.GetComponent<BoxCollider>().enabled = false;
                break;
            case CardTypes.Shield:
                detailCard = (CardShield) CardBase.InstantiateCardByCardInfo(CardInfo, GameBoardManager.GBM.CardDetailPreview.transform, ClientPlayer);
                detailCard.transform.localScale = Vector3.one * GameManager.GM.DetailCardSize;
                detailCard.transform.position = new Vector3(mousePosition.x, 2f, mousePosition.z);
                detailCard.transform.Translate(Vector3.left * 3.5f);
                detailCard.transform.Translate(Vector3.up * 5f);
                detailCard.GetComponent<BoxCollider>().enabled = false;
                break;
            default:
                break;
        }
    }

    private void HideCardDetail()
    {
        if (detailCard)
        {
            detailCard.PoolRecycle();
            detailCard = null;
        }

        if (detailCard_Weapon)
        {
            detailCard_Weapon.PoolRecycle();
            detailCard_Weapon = null;
        }

        if (detailCard_Shield)
        {
            detailCard_Shield.PoolRecycle();
            detailCard_Shield = null;
        }

        if (detailCard_Pack)
        {
            detailCard_Pack.PoolRecycle();
            detailCard_Pack = null;
        }

        if (detailCard_MA)
        {
            detailCard_MA.PoolRecycle();
            detailCard_MA = null;
        }
    }

    public virtual void DragComponent_OnMouseDown()
    {
    }

    public virtual void DragComponent_OnMousePressed(BoardAreaTypes boardAreaType, List<SlotAnchor> slotAnchors, ModuleRetinue moduleRetinue)
    {
    }

    public virtual void DragComponent_OnMouseUp(BoardAreaTypes boardAreaType, List<SlotAnchor> slotAnchors, ModuleRetinue moduleRetinue, Vector3 dragLastPosition, Vector3 dragBeginPosition, Quaternion dragBeginQuaternion)
    {
    }

    public virtual void DragComponent_SetStates(ref bool canDrag, ref DragPurpose dragPurpose)
    {
        canDrag = true;
        dragPurpose = CardInfo.BaseInfo.DragPurpose;
    }

    public virtual float DragComponnet_DragDistance()
    {
        return 0;
    }

    public virtual void MouseHoverComponent_OnMousePressEnterImmediately(Vector3 mousePosition)
    {
    }

    public virtual void MouseHoverComponent_OnMouseEnterImmediately(Vector3 mousePosition)
    {
    }

    public virtual void MouseHoverComponent_OnMouseEnter(Vector3 mousePosition)
    {
        ShowCardDetail(mousePosition);
        GameManager.GM.StartBlurBackGround();
    }

    public virtual void MouseHoverComponent_OnMouseOver()
    {
    }

    public virtual void MouseHoverComponent_OnMouseLeave()
    {
        HideCardDetail();
        GameManager.GM.StopBlurBackGround();
    }

    public virtual void MouseHoverComponent_OnMouseLeaveImmediately()
    {
    }

    public virtual void MouseHoverComponent_OnMousePressLeaveImmediately()
    {
    }

    public virtual void DragComponent_SetHasTarget(ref bool hasTarget)
    {
        hasTarget = true;
    }

    public virtual void DragComponnet_DragOutEffects()
    {
    }

    #endregion
}