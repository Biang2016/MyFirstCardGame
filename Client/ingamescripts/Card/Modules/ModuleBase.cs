﻿using System.Collections.Generic;
using UnityEngine;

public abstract class ModuleBase : PoolObject, IDragComponent, IMouseHoverComponent
{
    internal int GameObjectID;
    protected GameObjectPool gameObjectPool;
    internal ClientPlayer ClientPlayer;

    public override void PoolRecycle()
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

        if (this is ModulePack)
        {
            ((ModulePack) this).SetNoPreview();
        }

        if (this is ModuleMA)
        {
            ((ModuleMA) this).SetNoPreview();
        }

        gameObject.SetActive(true);
        base.PoolRecycle();
    }

    internal CardInfo_Base CardInfo; //卡牌原始数值信息
    private bool isDead;

    public bool IsDead
    {
        get { return isDead; }
        set { isDead = value; }
    }

    public virtual void Initiate(CardInfo_Base cardInfo, ClientPlayer clientPlayer)
    {
        ClientPlayer = clientPlayer;
        CardInfo = cardInfo;
        MainboardEmissionIntensity = CardInfo.GetCardColorIntensity();
        ChangeColor(ClientUtils.HTMLColorToColor(CardInfo.GetCardColor()));
        Stars = cardInfo.UpgradeInfo.CardLevel;
        BeBrightColor();
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
                    if (Star1) Star1.SetActive(true);
                    if (Star2) Star2.SetActive(true);
                    if (Star3) Star3.SetActive(false);
                    break;
                case 3:
                    if (Star1) Star1.SetActive(true);
                    if (Star2) Star2.SetActive(true);
                    if (Star3) Star3.SetActive(true);
                    break;
                default: break;
            }
        }
    }

    public Renderer MainBoardRenderer;
    protected float MainboardEmissionIntensity = 0f;

    public virtual void ChangeColor(Color color)
    {
        ClientUtils.ChangeColor(MainBoardRenderer, color);
    }

    public void BeDimColor()
    {
        ChangeColor(Color.gray);
    }

    public void BeBrightColor()
    {
        ClientUtils.ChangeColor(MainBoardRenderer, ClientUtils.HTMLColorToColor(CardInfo.GetCardColor()));
    }

    #endregion


    #region 模块交互

    private CardBase detailCard;
    private CardBase detailCard_Weapon;
    private CardBase detailCard_Shield;
    private CardBase detailCard_Pack;
    private CardBase detailCard_MA;

    private void ShowCardDetail() //鼠标悬停放大查看原卡牌信息
    {
        switch (CardInfo.BaseInfo.CardType)
        {
            case CardTypes.Retinue:
                detailCard = (CardRetinue) CardBase.InstantiateCardByCardInfo(CardInfo, GameBoardManager.Instance.CardDetailPreview.transform, RoundManager.Instance.SelfClientPlayer, false);
                detailCard.transform.localScale = Vector3.one * GameManager.Instance.DetailRetinueCardSize;
                detailCard.transform.position = new Vector3(0, 8f, 0);
                detailCard.transform.Translate(Vector3.left * 5f);
                detailCard.GetComponent<BoxCollider>().enabled = false;
                detailCard.GetComponent<DragComponent>().enabled = false;
                detailCard.BeBrightColor();
                detailCard.SetOrderInLayer(200);

                CardRetinue cardRetinue = (CardRetinue) detailCard;
                //cardRetinue.ShowAllSlotHover();

                if (((ModuleRetinue) this).M_Weapon)
                {
                    if (!cardRetinue.Weapon)
                    {
                        cardRetinue.Weapon = GameObjectPoolManager.Instance.Pool_ModuleWeaponDetailPool.AllocateGameObject<ModuleWeapon>(cardRetinue.transform);
                    }

                    CardInfo_Base cw = ((ModuleRetinue) this).M_Weapon.CardInfo;
                    cardRetinue.Weapon.M_ModuleRetinue = (ModuleRetinue) this;
                    cardRetinue.Weapon.Initiate(((ModuleRetinue) this).M_Weapon.GetCurrentCardInfo(), ClientPlayer);
                    cardRetinue.Weapon.GetComponent<DragComponent>().enabled = false;
                    cardRetinue.Weapon.GetComponent<MouseHoverComponent>().enabled = false;
                    cardRetinue.Weapon.SetPreview();

                    detailCard_Weapon = (CardEquip) CardBase.InstantiateCardByCardInfo(cw, GameBoardManager.Instance.CardDetailPreview.transform, RoundManager.Instance.SelfClientPlayer, false);
                    detailCard_Weapon.transform.localScale = Vector3.one * GameManager.Instance.DetailEquipmentCardSize;
                    detailCard_Weapon.transform.position = new Vector3(0, 2f, 0);
                    detailCard_Weapon.transform.Translate(Vector3.right * 0.5f);
                    detailCard_Weapon.transform.Translate(Vector3.back * 3f);
                    detailCard_Weapon.transform.Translate(Vector3.up * 7f);
                    detailCard_Weapon.GetComponent<BoxCollider>().enabled = false;
                    detailCard_Weapon.BeBrightColor();
                    detailCard_Weapon.CardBloom.SetActive(true);
                    detailCard_Weapon.SetOrderInLayer(200);
                }

                if (((ModuleRetinue) this).M_Shield)
                {
                    if (!cardRetinue.Shield)
                    {
                        cardRetinue.Shield = GameObjectPoolManager.Instance.Pool_ModuleShieldDetailPool.AllocateGameObject<ModuleShield>(cardRetinue.transform);
                    }

                    CardInfo_Base cw = ((ModuleRetinue) this).M_Shield.CardInfo;
                    cardRetinue.Shield.M_ModuleRetinue = (ModuleRetinue) this;
                    cardRetinue.Shield.Initiate(((ModuleRetinue) this).M_Shield.GetCurrentCardInfo(), ClientPlayer);
                    cardRetinue.Shield.GetComponent<DragComponent>().enabled = false;
                    cardRetinue.Shield.GetComponent<MouseHoverComponent>().enabled = false;
                    cardRetinue.Shield.SetPreview();

                    detailCard_Shield = (CardEquip) CardBase.InstantiateCardByCardInfo(cw, GameBoardManager.Instance.CardDetailPreview.transform, RoundManager.Instance.SelfClientPlayer, false);
                    detailCard_Shield.transform.localScale = Vector3.one * GameManager.Instance.DetailEquipmentCardSize;
                    detailCard_Shield.transform.position = new Vector3(0, 2f, 0);
                    detailCard_Shield.transform.Translate(Vector3.right * 0.5f);
                    detailCard_Shield.transform.Translate(Vector3.forward * 3f);
                    detailCard_Shield.transform.Translate(Vector3.up * 7f);
                    detailCard_Shield.GetComponent<BoxCollider>().enabled = false;
                    detailCard_Shield.BeBrightColor();
                    detailCard_Shield.CardBloom.SetActive(true);
                    detailCard_Shield.SetOrderInLayer(200);
                }

                if (((ModuleRetinue) this).M_Pack)
                {
                    if (!cardRetinue.Pack)
                    {
                        cardRetinue.Pack = GameObjectPoolManager.Instance.Pool_ModulePackDetailPool.AllocateGameObject<ModulePack>(cardRetinue.transform);
                    }

                    CardInfo_Base cw = ((ModuleRetinue) this).M_Pack.CardInfo;
                    cardRetinue.Pack.M_ModuleRetinue = (ModuleRetinue) this;
                    cardRetinue.Pack.Initiate(((ModuleRetinue) this).M_Pack.GetCurrentCardInfo(), ClientPlayer);
                    cardRetinue.Pack.GetComponent<DragComponent>().enabled = false;
                    cardRetinue.Pack.GetComponent<MouseHoverComponent>().enabled = false;
                    cardRetinue.Pack.SetPreview();

                    detailCard_Pack = (CardEquip) CardBase.InstantiateCardByCardInfo(cw, GameBoardManager.Instance.CardDetailPreview.transform, RoundManager.Instance.SelfClientPlayer, false);
                    detailCard_Pack.transform.localScale = Vector3.one * GameManager.Instance.DetailEquipmentCardSize;
                    detailCard_Pack.transform.position = new Vector3(0, 2f, 0);
                    detailCard_Pack.transform.Translate(Vector3.left * 10.5f);
                    detailCard_Pack.transform.Translate(Vector3.back * 3f);
                    detailCard_Pack.transform.Translate(Vector3.up * 7f);
                    detailCard_Pack.GetComponent<BoxCollider>().enabled = false;
                    detailCard_Pack.BeBrightColor();
                    detailCard_Pack.CardBloom.SetActive(true);
                    detailCard_Pack.SetOrderInLayer(200);
                }

                if (((ModuleRetinue) this).M_MA)
                {
                    if (!cardRetinue.MA)
                    {
                        cardRetinue.MA = GameObjectPoolManager.Instance.Pool_ModuleMADetailPool.AllocateGameObject<ModuleMA>(cardRetinue.transform);
                    }

                    CardInfo_Base cw = ((ModuleRetinue) this).M_MA.CardInfo;
                    cardRetinue.MA.M_ModuleRetinue = (ModuleRetinue) this;
                    cardRetinue.MA.Initiate(((ModuleRetinue) this).M_MA.GetCurrentCardInfo(), ClientPlayer);
                    cardRetinue.MA.GetComponent<DragComponent>().enabled = false;
                    cardRetinue.MA.GetComponent<MouseHoverComponent>().enabled = false;
                    cardRetinue.MA.SetPreview();

                    detailCard_MA = (CardEquip) CardBase.InstantiateCardByCardInfo(cw, GameBoardManager.Instance.CardDetailPreview.transform, RoundManager.Instance.SelfClientPlayer, false);
                    detailCard_MA.transform.localScale = Vector3.one * GameManager.Instance.DetailEquipmentCardSize;
                    detailCard_MA.transform.position = new Vector3(0, 2f, 0);
                    detailCard_MA.transform.Translate(Vector3.left * 10.5f);
                    detailCard_MA.transform.Translate(Vector3.forward * 3f);
                    detailCard_MA.transform.Translate(Vector3.up * 7f);
                    detailCard_MA.GetComponent<BoxCollider>().enabled = false;
                    detailCard_MA.BeBrightColor();
                    detailCard_MA.CardBloom.SetActive(true);
                    detailCard_MA.SetOrderInLayer(200);
                }

                break;
            case CardTypes.Equip:
                detailCard = (CardEquip) CardBase.InstantiateCardByCardInfo(CardInfo, GameBoardManager.Instance.CardDetailPreview.transform, RoundManager.Instance.SelfClientPlayer, false);
                detailCard.transform.localScale = Vector3.one * GameManager.Instance.DetailSingleCardSize;
                detailCard.transform.position = new Vector3(0, 2f, 0);
                detailCard.transform.Translate(Vector3.left * 3.5f);
                detailCard.transform.Translate(Vector3.up * 5f);
                detailCard.GetComponent<BoxCollider>().enabled = false;
                detailCard.BeBrightColor();
                detailCard.SetOrderInLayer(200);
                break;
            default:
                break;
        }

        detailCard.CardBloom.SetActive(true);
        List<CardInfo_Base> cardInfos = new List<CardInfo_Base>();
        if (detailCard != null) cardInfos.Add(detailCard.CardInfo);
        if (detailCard_Weapon != null) cardInfos.Add(detailCard_Weapon.CardInfo);
        if (detailCard_Shield != null) cardInfos.Add(detailCard_Shield.CardInfo);
        if (detailCard_Pack != null) cardInfos.Add(detailCard_Pack.CardInfo);
        if (detailCard_MA != null) cardInfos.Add(detailCard_MA.CardInfo);
        AffixManager.Instance.ShowAffixTips(cardInfos, this is ModuleRetinue ? new List<ModuleRetinue> {(ModuleRetinue) this} : null);
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

        AffixManager.Instance.HideAffixPanel();
    }

    #region SE

    public virtual void OnShowEffects(SideEffectBundle.TriggerTime triggerTime, SideEffectBundle.TriggerRange triggerRange)
    {
    }

    #endregion

    public virtual void DragComponent_OnMouseDown()
    {
    }

    public virtual void DragComponent_OnMousePressed(BoardAreaTypes boardAreaType, List<Slot> slots, ModuleRetinue moduleRetinue, Vector3 dragLastPosition)
    {
    }

    public virtual void DragComponent_OnMouseUp(BoardAreaTypes boardAreaType, List<Slot> slots, ModuleRetinue moduleRetinue, Ship ship, Vector3 dragLastPosition, Vector3 dragBeginPosition, Quaternion dragBeginQuaternion)
    {
    }

    public virtual void DragComponent_SetStates(ref bool canDrag, ref DragPurpose dragPurpose)
    {
        canDrag = ClientPlayer == RoundManager.Instance.SelfClientPlayer;
        dragPurpose = CardInfo.BaseInfo.DragPurpose;
    }

    public virtual float DragComponnet_DragDistance()
    {
        return 0;
    }

    public virtual void MouseHoverComponent_OnMousePressEnterImmediately(Vector3 mousePosition)
    {
    }

    public virtual void MouseHoverComponent_OnHover1Begin(Vector3 mousePosition)
    {
    }

    public virtual void MouseHoverComponent_OnHover1End()
    {
    }

    public virtual void MouseHoverComponent_OnHover2Begin(Vector3 mousePosition)
    {
        if (DragManager.Instance.IsSummonPreview) return;
        ShowCardDetail();
        GameManager.Instance.StartBlurBackGround();
    }

    public virtual void MouseHoverComponent_OnHover2End()
    {
        if (DragManager.Instance.IsSummonPreview) return;
        HideCardDetail();
        if (SelectBuildManager.Instance.M_StateMachine.GetState() == SelectBuildManager.StateMachine.States.Show_ReadOnly) return;
        GameManager.Instance.StopBlurBackGround();
    }

    public virtual void MouseHoverComponent_OnFocusBegin(Vector3 mousePosition)
    {
    }

    public virtual void MouseHoverComponent_OnFocusEnd()
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