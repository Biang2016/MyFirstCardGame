﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class BattleGroundManager : MonoBehaviour
{
    internal bool BattleGroundIsFull;
    private Vector3 _defaultRetinuePosition = Vector3.zero;

    internal ClientPlayer ClientPlayer;
    private List<ModuleRetinue> Retinues = new List<ModuleRetinue>();

    public void Reset()
    {
        foreach (ModuleRetinue moduleRetinue in Retinues)
        {
            moduleRetinue.PoolRecycle();
        }

        ClientPlayer = null;
        previewRetinuePlace = -1;
        Retinues.Clear();
        RemoveRetinues.Clear();
    }

    internal int ComputePosition(Vector3 dragLastPosition)
    {
        int index = Mathf.RoundToInt(Mathf.Floor(dragLastPosition.x / GameManager.GM.RetinueInterval - (Retinues.Count + 1) % 2 * 0.5f) + (Retinues.Count / 2 + 1));
        if (index < 0) index = 0;
        if (index >= Retinues.Count) index = Retinues.Count;
        return index;
    }

    internal ModuleRetinue CheckRetinueOnPosition(Vector3 dragLastPosition)
    {
        var index = Mathf.RoundToInt(Mathf.Floor(dragLastPosition.x / GameManager.GM.RetinueInterval - (Retinues.Count + 1) % 2 * 0.5f) + (Retinues.Count / 2 + 1));
        if (index < 0 || index >= Retinues.Count)
            return null;
        return Retinues[index];
    }

    public void AddRetinue_PrePass(CardInfo_Retinue retinueCardInfo, int retinuePlaceIndex, int retinueId)
    {
        if (ClientPlayer == null) return;
        if (previewRetinuePlace != -1)
        {
            previewRetinuePlace = -1;
        }

        ModuleRetinue retinue = GameObjectPoolManager.GOPM.Pool_ModuleRetinuePool.AllocateGameObject(transform).GetComponent<ModuleRetinue>();
        retinue.Initiate(retinueCardInfo, ClientPlayer);
        retinue.transform.Rotate(Vector3.up, 180);
        Retinues.Insert(retinuePlaceIndex, retinue);
        retinue.M_RetinueID = retinueId;

        BattleGroundIsFull = Retinues.Count == GamePlaySettings.MaxRetinueNumber;
    }

    public void AddRetinue(CardInfo_Retinue retinueCardInfo, int retinuePlaceIndex, int retinueId)
    {
        ModuleRetinue retinue = GetRetinue(retinueId);
        SetNewRetinuePlace(retinue);
        BattleEffectsManager.BEM.Effect_RefreshBattleGroundOnAddRetinue.EffectsShow(Co_RefreshBattleGroundAnim(BattleEffectsManager.BEM.Effect_RefreshBattleGroundOnAddRetinue), "Co_RefreshBattleGroundAnim");
        retinue.OnSummoned();
    }

    private int previewRetinuePlace;

    public void AddRetinuePreview(int placeIndex)
    {
        if (Retinues.Count == 0) return;
        if (previewRetinuePlace == -1 || previewRetinuePlace != placeIndex)
        {
            previewRetinuePlace = placeIndex;
            BattleEffectsManager.BEM.Effect_RefreshBattleGroundOnAddRetinue.EffectsShow(Co_RefreshBattleGroundAnim(BattleEffectsManager.BEM.Effect_RefreshBattleGroundOnAddRetinue), "Co_RefreshBattleGroundAnim");
        }
    }

    public void RemoveRetinuePreview()
    {
        if (previewRetinuePlace != -1)
        {
            previewRetinuePlace = -1;
            BattleEffectsManager.BEM.Effect_Main.EffectsShow(Co_RefreshBattleGroundAnim(BattleEffectsManager.BEM.Effect_Main), "Co_RefreshBattleGroundAnim");
        }
    }

    public void RemoveRetinue(int retinueId)
    {
        BattleEffectsManager.BEM.Effect_Main.EffectsShow(Co_RemoveRetinue(GetRetinue(retinueId)), "Co_RemoveRetinue");
        BattleEffectsManager.BEM.Effect_Main.EffectsShow(Co_RefreshBattleGroundAnim(BattleEffectsManager.BEM.Effect_Main), "Co_RefreshBattleGroundAnim");
    }

    List<ModuleRetinue> RemoveRetinues = new List<ModuleRetinue>(); //即将要被移除的随从名单

    public void RemoveRetinueTogatherAdd(int retinueId)
    {
        ModuleRetinue retinue = GetRetinue(retinueId);
        retinue.PoolRecycle();
        RemoveRetinues.Add(retinue);
    }

    public void RemoveRetinueTogather()
    {
        foreach (ModuleRetinue removeRetinue in RemoveRetinues)
        {
            Retinues.Remove(removeRetinue);
            ClientLog.CL.Print("remove:" + removeRetinue.M_RetinueID);
        }

        RemoveRetinues.Clear();

        BattleGroundIsFull = Retinues.Count == GamePlaySettings.MaxRetinueNumber;
    }

    public void RemoveRetinueTogatherEnd()
    {
        BattleEffectsManager.BEM.Effect_Main.EffectsShow(Co_RefreshBattleGroundAnim(BattleEffectsManager.BEM.Effect_Main), "Co_RefreshBattleGroundAnim");
    }

    IEnumerator Co_RemoveRetinue(ModuleRetinue retinue)
    {
        retinue.PoolRecycle();
        Retinues.Remove(retinue);

        BattleGroundIsFull = Retinues.Count == GamePlaySettings.MaxRetinueNumber;
        yield return null;
        BattleEffectsManager.BEM.Effect_Main.EffectEnd();
    }


    internal void SetNewRetinuePlace(ModuleRetinue retinue)
    {
        retinue.transform.localPosition = _defaultRetinuePosition;
        retinue.transform.transform.Translate(Vector3.left * (Retinues.IndexOf(retinue) - Retinues.Count / 2.0f + 0.5f) * GameManager.GM.RetinueInterval, Space.Self);
    }

    IEnumerator Co_RefreshBattleGroundAnim(BattleEffectsManager.Effects myParentEffects)
    {
        float duration = 0.05f;
        float tick = 0;

        Vector3[] translations = new Vector3[Retinues.Count];

        int actualPlaceCount = previewRetinuePlace == -1 ? Retinues.Count : Retinues.Count + 1;

        List<ModuleRetinue> movingRetinues = new List<ModuleRetinue>();

        for (int i = 0; i < Retinues.Count; i++)
        {
            movingRetinues.Add(Retinues[i]);

            int actualPlace = i;
            if (previewRetinuePlace != -1 && i >= previewRetinuePlace)
            {
                actualPlace += 1;
            }

            Vector3 ori = Retinues[i].transform.localPosition;
            Vector3 offset = Vector3.left * (actualPlace - actualPlaceCount / 2.0f + 0.5f) * GameManager.GM.RetinueInterval;

            Retinues[i].transform.localPosition = _defaultRetinuePosition;
            Retinues[i].transform.Translate(offset, Space.Self);

            translations[i] = Retinues[i].transform.localPosition - ori;
            Retinues[i].transform.localPosition = ori;
        }


        while (tick <= duration)
        {
            float timeDelta = Mathf.Min(duration - tick, Time.deltaTime);

            for (int i = 0; i < movingRetinues.Count; i++)
            {
                movingRetinues[i].transform.localPosition += translations[i] * timeDelta / duration;
            }

            tick += Time.deltaTime;

            yield return null;
        }

        yield return null;

        myParentEffects.EffectEnd();
    }

    #region 装备牌相关操作会显示提示性Slot边框

    List<SlotAnchor> relatedSlotAnchors = new List<SlotAnchor>();

    private IEnumerator currentShowSlotBloom;

    int tmp;

    public void ShowTipSlotBlooms(SlotTypes slotType)
    {
        StopShowSlotBloom();
        foreach (ModuleRetinue retinue in Retinues)
        {
            if (retinue.SlotAnchor1.M_Slot.MSlotTypes == slotType)
            {
                relatedSlotAnchors.Add(retinue.SlotAnchor1);
            }

            if (retinue.SlotAnchor2.M_Slot.MSlotTypes == slotType)
            {
                relatedSlotAnchors.Add(retinue.SlotAnchor2);
            }

            if (retinue.SlotAnchor3.M_Slot.MSlotTypes == slotType)
            {
                relatedSlotAnchors.Add(retinue.SlotAnchor3);
            }

            if (retinue.SlotAnchor4.M_Slot.MSlotTypes == slotType)
            {
                relatedSlotAnchors.Add(retinue.SlotAnchor4);
            }
        }

        currentShowSlotBloom = Co_ShowSlotBloom();
        BattleEffectsManager.BEM.Effect_TipSlotBloom.EffectsShow(currentShowSlotBloom, "Co_ShowSlotBloom");
    }

    IEnumerator Co_ShowSlotBloom()
    {
        while (true)
        {
            foreach (SlotAnchor sa in relatedSlotAnchors)
            {
                sa.ShowHoverGO();
            }

            yield return new WaitForSeconds(0.4f);
            foreach (SlotAnchor sa in relatedSlotAnchors)
            {
                sa.HideHoverShowGO();
            }

            yield return new WaitForSeconds(0.4f);
        }
    }

    public void StopShowSlotBloom()
    {
        if (currentShowSlotBloom != null)
        {
            BattleEffectsManager.SideEffect cur_Effect = BattleEffectsManager.BEM.Effect_TipSlotBloom.GetCurrentSideEffect();
            if (cur_Effect != null && cur_Effect.Enumerator == currentShowSlotBloom)
            {
                BattleEffectsManager.BEM.Effect_TipSlotBloom.EffectEnd();
                ClientLog.CL.PrintWarning("Stop Effect_TipSlotBloom");
                currentShowSlotBloom = null;
            }
        }

        foreach (SlotAnchor sa in relatedSlotAnchors)
        {
            sa.HideHoverShowGO();
        }

        relatedSlotAnchors.Clear();
    }

    #endregion

    public void EquipWeapon(CardInfo_Weapon cardInfo, int retinueId)
    {
        ModuleRetinue retinue = GetRetinue(retinueId);
        ModuleWeapon newModueWeapon = GameObjectPoolManager.GOPM.Pool_ModuleWeaponPool.AllocateGameObject(retinue.transform).GetComponent<ModuleWeapon>();
        newModueWeapon.M_ModuleRetinue = retinue;
        newModueWeapon.Initiate(cardInfo, ClientPlayer);
        retinue.M_Weapon = newModueWeapon;
    }

    public void EquipShield(CardInfo_Shield cardInfo, int retinueId)
    {
        ModuleRetinue retinue = GetRetinue(retinueId);
        ModuleShield newModuleShield = GameObjectPoolManager.GOPM.Pool_ModuleShieldPool.AllocateGameObject(retinue.transform).GetComponent<ModuleShield>();
        newModuleShield.M_ModuleRetinue = retinue;
        newModuleShield.Initiate(cardInfo, ClientPlayer);
        retinue.M_Shield = newModuleShield;
    }


    #region Utils

    public ModuleRetinue GetRetinue(int retinueId)
    {
        foreach (ModuleRetinue moduleRetinue in Retinues)
        {
            if (moduleRetinue.M_RetinueID == retinueId)
            {
                return moduleRetinue;
            }
        }

        return null;
    }

    public int GetRetinuePlaceIndex(ModuleRetinue moduleRetinue)
    {
        return Retinues.IndexOf(moduleRetinue);
    }

    #endregion

    #region GameProcess

    internal void BeginRound()
    {
        foreach (var mr in Retinues) mr.OnBeginRound();
    }

    internal void EndRound()
    {
        foreach (var mr in Retinues) mr.OnEndRound();
    }

    #endregion
}