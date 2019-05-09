﻿using NSubstitute;
using UnityEngine;

/// <summary>
/// 鼠标拖拽管理器
/// </summary>
public class DragManager : MonoSingleton<DragManager>
{
    private DragManager()
    {
    }

    void Awake()
    {
    }

    internal Arrow CurrentArrow;

    internal CardRetinue CurrentDrag_CardRetinue;
    internal CardEquip CurrentDrag_CardEquip;
    internal CardSpell CurrentDrag_CardSpell;
    internal ModuleRetinue CurrentDrag_ModuleRetinue;

    private DragComponent currentDrag;

    internal DragComponent CurrentDrag
    {
        get { return currentDrag; }
        set
        {
            currentDrag = value;
            if (currentDrag == null)
            {
                CurrentDrag_CardRetinue = null;
                CurrentDrag_CardEquip = null;
                CurrentDrag_CardSpell = null;
                CurrentDrag_ModuleRetinue = null;
            }
            else
            {
                CurrentDrag_CardRetinue = currentDrag.GetComponent<CardRetinue>();
                CurrentDrag_CardEquip = currentDrag.GetComponent<CardEquip>();
                CurrentDrag_CardSpell = currentDrag.GetComponent<CardSpell>();
                CurrentDrag_ModuleRetinue = currentDrag.GetComponent<ModuleRetinue>();

                if (CurrentDrag_CardEquip && CurrentDrag_CardEquip.Usable && CurrentDrag_CardEquip.ClientPlayer.BattlePlayer.HandManager.CurrentFocusCard == CurrentDrag_CardEquip)
                {
                    MouseHoverManager.Instance.M_StateMachine.SetState(MouseHoverManager.StateMachine.States.DragEquipment);
                }
                else if (CurrentDrag_CardSpell && CurrentDrag_CardSpell.Usable && CurrentDrag_CardSpell.ClientPlayer.BattlePlayer.HandManager.CurrentFocusCard == CurrentDrag_CardSpell)
                {
                    MouseHoverManager.Instance.M_StateMachine.SetState(MouseHoverManager.StateMachine.States.DragSpellTo);
                }
                else if (CurrentDrag_ModuleRetinue)
                {
                    MouseHoverManager.Instance.M_StateMachine.SetState(MouseHoverManager.StateMachine.States.DragRetinueTo);
                }
            }
        }
    }

    public int DragOutDamage = 0; //鼠标拖动时附带的预计伤害

    void Update()
    {
        //if (ConfirmWindowManager.Instance.IsConfirmWindowShow) return;
        //if (ExitMenuPanel.Instance.M_StateMachine.GetState() == ExitMenuPanel.StateMachine.States.Show) ResetCurrentDrag();
        //if (SelectBuildManager.Instance.M_StateMachine.GetState() == SelectBuildManager.StateMachine.States.Show) ResetCurrentDrag();
        if (!Client.Instance.IsPlaying()) ResetCurrentDrag();
        if (UIManager.Instance.GetBaseUIForm<StartMenuPanel>())
        {
            ResetCurrentDrag();
            //TODO
        }

        //if (BattleResultPanel.Instance.IsShow) ResetCurrentDrag();
        if (!IsSummonPreview)
        {
            CommonDrag();
        }
        else
        {
            SummonPreviewDrag();
        }
    }

    private void CommonDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!CurrentDrag)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit raycast;
                Physics.Raycast(ray, out raycast, 10f, GameManager.Instance.Layer_Cards | GameManager.Instance.Layer_Retinues | GameManager.Instance.Layer_Modules);
                if (raycast.collider != null)
                {
                    ColliderReplace colliderReplace = raycast.collider.gameObject.GetComponent<ColliderReplace>();
                    if (colliderReplace)
                    {
                        CurrentDrag = colliderReplace.MyCallerCard.GetComponent<DragComponent>();
                    }
                    else
                    {
                        CurrentDrag = raycast.collider.gameObject.GetComponent<DragComponent>();
                    }

                    CurrentDrag.IsOnDrag = true;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            ResetCurrentDrag();
        }
    }

    private void ResetCurrentDrag()
    {
        if (CurrentDrag)
        {
            CurrentDrag.IsOnDrag = false;
            CurrentDrag = null;
        }
    }

    # region 召唤机甲指定目标预览

    internal bool IsSummonPreview;
    internal bool IsArrowShowBegin;
    internal ModuleRetinue CurrentSummonPreviewRetinue;
    public BattleGroundManager.SummonRetinueTarget SummonRetinueTargetHandler;
    public TargetRange SummonRetinueTargetRange;

    public const int TARGET_SELECT_NONE = -2;

    public void StartArrowAiming(ModuleRetinue retinue, TargetRange targetRange)
    {
        IsSummonPreview = true;
        CurrentSummonPreviewRetinue = retinue;
        SummonRetinueTargetRange = targetRange;
        MouseHoverManager.Instance.M_StateMachine.SetState(MouseHoverManager.StateMachine.States.SummonRetinueTargetOn);
    }

    private void SummonPreviewDrag()
    {
        if (IsArrowShowBegin)
        {
            if (!CurrentArrow || !(CurrentArrow is ArrowAiming)) CurrentArrow = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.ArrowAiming].AllocateGameObject<ArrowAiming>(transform);
            Vector3 cameraPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CurrentArrow.Render(CurrentSummonPreviewRetinue.transform.position, cameraPosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit raycast;
            Physics.Raycast(ray, out raycast, 10f, GameManager.Instance.Layer_Retinues);
            if (raycast.collider == null) //没有选中目标，则撤销
            {
                SummonRetinueTargetHandler(-2);
                if (CurrentArrow) CurrentArrow.PoolRecycle();
                IsSummonPreview = false;
                IsArrowShowBegin = false;
            }
            else
            {
                ModuleRetinue retinue = raycast.collider.GetComponent<ModuleRetinue>();
                if (retinue == null)
                {
                    SummonRetinueTargetHandler(-2);
                    if (CurrentArrow) CurrentArrow.PoolRecycle();
                    IsSummonPreview = false;
                    IsArrowShowBegin = false;
                }
                else
                {
                    if (RoundManager.Instance.SelfClientPlayer.BattlePlayer.BattleGroundManager.CurrentSummonPreviewRetinue == retinue //不可指向自己
                        || RoundManager.Instance.SelfClientPlayer.BattlePlayer.BattleGroundManager.RemoveRetinues.Contains(retinue) //不可是死亡对象
                        || RoundManager.Instance.EnemyClientPlayer.BattlePlayer.BattleGroundManager.RemoveRetinues.Contains(retinue)) //不可是死亡对象
                    {
                        SummonRetinueTargetHandler(-2);
                        if (CurrentArrow) CurrentArrow.PoolRecycle();
                        IsSummonPreview = false;
                        IsArrowShowBegin = false;
                    }
                    else
                    {
                        int targetRetinueID = retinue.M_RetinueID;
                        bool isClientRetinueTempId = false;
                        if (retinue.M_RetinueID == -1) //如果该机甲还未从服务器取得ID，则用tempID
                        {
                            targetRetinueID = retinue.M_ClientTempRetinueID;
                            isClientRetinueTempId = true;
                        }

                        switch (SummonRetinueTargetRange)
                        {
                            case TargetRange.None:
                                SummonRetinueTargetHandler(-2);
                                break;
                            case TargetRange.Mechs:
                                SummonRetinueTargetHandler(targetRetinueID, isClientRetinueTempId);
                                break;
                            case TargetRange.SelfMechs:
                                if (retinue.ClientPlayer == RoundManager.Instance.SelfClientPlayer) SummonRetinueTargetHandler(targetRetinueID, isClientRetinueTempId);
                                else SummonRetinueTargetHandler(-2);
                                break;
                            case TargetRange.EnemyMechs:
                                if (retinue.ClientPlayer == RoundManager.Instance.EnemyClientPlayer) SummonRetinueTargetHandler(targetRetinueID, isClientRetinueTempId);
                                else SummonRetinueTargetHandler(-2);
                                break;
                            case TargetRange.Heroes:
                                if (!retinue.CardInfo.RetinueInfo.IsSoldier) SummonRetinueTargetHandler(targetRetinueID, isClientRetinueTempId);
                                else SummonRetinueTargetHandler(-2);
                                break;
                            case TargetRange.SelfHeroes:
                                if (retinue.ClientPlayer == RoundManager.Instance.SelfClientPlayer && !retinue.CardInfo.RetinueInfo.IsSoldier) SummonRetinueTargetHandler(targetRetinueID, isClientRetinueTempId);
                                else SummonRetinueTargetHandler(-2);
                                break;
                            case TargetRange.EnemyHeroes:
                                if (retinue.ClientPlayer == RoundManager.Instance.EnemyClientPlayer && !retinue.CardInfo.RetinueInfo.IsSoldier) SummonRetinueTargetHandler(targetRetinueID, isClientRetinueTempId);
                                else SummonRetinueTargetHandler(-2);
                                break;
                            case TargetRange.Soldiers:
                                if (retinue.CardInfo.RetinueInfo.IsSoldier) SummonRetinueTargetHandler(targetRetinueID, isClientRetinueTempId);
                                else SummonRetinueTargetHandler(-2);
                                break;
                            case TargetRange.SelfSoldiers:
                                if (retinue.ClientPlayer == RoundManager.Instance.SelfClientPlayer && retinue.CardInfo.RetinueInfo.IsSoldier) SummonRetinueTargetHandler(targetRetinueID, isClientRetinueTempId);
                                else SummonRetinueTargetHandler(-2);
                                break;
                            case TargetRange.EnemySoldiers:
                                if (retinue.ClientPlayer == RoundManager.Instance.EnemyClientPlayer && retinue.CardInfo.RetinueInfo.IsSoldier) SummonRetinueTargetHandler(targetRetinueID, isClientRetinueTempId);
                                else SummonRetinueTargetHandler(-2);
                                break;
                            case TargetRange.SelfShip:
                                SummonRetinueTargetHandler(-2);
                                break;
                            case TargetRange.EnemyShip:
                                SummonRetinueTargetHandler(-2);
                                break;
                            case TargetRange.AllLife:
                                SummonRetinueTargetHandler(targetRetinueID, isClientRetinueTempId);
                                break;
                        }
                    }

                    if (CurrentArrow) CurrentArrow.PoolRecycle();
                    IsSummonPreview = false;
                    IsArrowShowBegin = false;
                }
            }

            MouseHoverManager.Instance.M_StateMachine.SetState(MouseHoverManager.StateMachine.States.BattleNormal);
        }
        else if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonUp(1))
        {
            SummonRetinueTargetHandler(-2);
            if (CurrentArrow) CurrentArrow.PoolRecycle();
            IsSummonPreview = false;
            IsArrowShowBegin = false;
            MouseHoverManager.Instance.M_StateMachine.SetState(MouseHoverManager.StateMachine.States.BattleNormal);
        }
    }

    #endregion
}