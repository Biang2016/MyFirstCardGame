﻿using System.Collections;
using UnityEngine;

public class ModuleMA : ModuleBase
{
    public override void PoolRecycle()
    {
        base.PoolRecycle();
        if (MAEquipAnim) MAEquipAnim.SetTrigger("Hide");
    }

    void Awake()
    {
        gameObjectPool = GameObjectPoolManager.Instance.Pool_ModuleMAPool;
    }

    #region 各模块、自身数值和初始化

    internal ModuleRetinue M_ModuleRetinue;
    [SerializeField] private TextMesh MAName;
    [SerializeField] private TextMesh MAName_en;
    [SerializeField] private Renderer M_Bloom;
    [SerializeField] private Renderer M_BloomSE;
    [SerializeField] private Renderer M_BloomSE_Sub;

    [SerializeField] private Animator MAEquipAnim;

    [SerializeField] private Animator MABloomSEAnim;
    [SerializeField] private Animator MABloomSE_SubAnim;

    public int M_EquipID;

    public override void Initiate(CardInfo_Base cardInfo, ClientPlayer clientPlayer)
    {
        base.Initiate(cardInfo, clientPlayer);
        M_MAName = GameManager.Instance.isEnglish ? cardInfo.BaseInfo.CardName_en : cardInfo.BaseInfo.CardName;
        if (M_Bloom) M_Bloom.gameObject.SetActive(false);
        if (M_BloomSE) M_BloomSE.gameObject.SetActive(false);
    }

    public override void ChangeColor(Color color)
    {
        base.ChangeColor(color);
        ClientUtils.ChangeColor(M_Bloom, color);
        ClientUtils.ChangeColor(M_BloomSE, color);
    }

    public void SetPreview()
    {
        if (M_Bloom) M_Bloom.gameObject.SetActive(true);
    }

    public void SetNoPreview()
    {
    }

    #region 属性

    public CardInfo_Equip GetCurrentCardInfo()
    {
        CardInfo_Equip currentCI = (CardInfo_Equip) CardInfo.Clone();
        return currentCI;
    }

    private string m_MAName;

    public string M_MAName
    {
        get { return m_MAName; }

        set
        {
            m_MAName = value;
            MAName.text = GameManager.Instance.isEnglish ? "" : value;
            MAName_en.text = GameManager.Instance.isEnglish ? value : "";
        }
    }

    #endregion

    #endregion

    #region 模块交互

    #region 交互UX

    public void OnMAEquiped()
    {
        MAEquipAnim.SetTrigger("MAEquiped");
    }


    public override void MouseHoverComponent_OnHover1Begin(Vector3 mousePosition)
    {
        base.MouseHoverComponent_OnHover1Begin(mousePosition);
        if (M_Bloom) M_Bloom.gameObject.SetActive(true);
    }

    public override void MouseHoverComponent_OnHover1End()
    {
        base.MouseHoverComponent_OnHover1End();
        if (M_Bloom) M_Bloom.gameObject.SetActive(false);
    }

    #endregion

    #region SE

    public override void OnShowEffects(SideEffectBundle.TriggerTime triggerTime, SideEffectBundle.TriggerRange triggerRange)
    {
        BattleEffectsManager.Instance.Effect_Main.EffectsShow(Co_ShowSideEffectBloom(ClientUtils.HTMLColorToColor("#FFFFFF"), 0.8f), "ShowSideEffectBloom");
    }

    IEnumerator Co_ShowSideEffectBloom(Color color, float duration)
    {
        M_BloomSE.gameObject.SetActive(true);
        M_BloomSE_Sub.gameObject.SetActive(true);
        MABloomSEAnim.SetTrigger("OnSE");
        MABloomSE_SubAnim.SetTrigger("OnSE");
        ClientUtils.ChangeColor(M_BloomSE, color);
        ClientUtils.ChangeColor(M_BloomSE_Sub, color);
        AudioManager.Instance.SoundPlay("sfx/OnSE");
        yield return new WaitForSeconds(duration);
        MABloomSEAnim.SetTrigger("Reset");
        MABloomSE_SubAnim.SetTrigger("Reset");
        M_BloomSE.gameObject.SetActive(false);
        M_BloomSE_Sub.gameObject.SetActive(false);
        BattleEffectsManager.Instance.Effect_Main.EffectEnd();
    }

    #endregion

    #endregion
}