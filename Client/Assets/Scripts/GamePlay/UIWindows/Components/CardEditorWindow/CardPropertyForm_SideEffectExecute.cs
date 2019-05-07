﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CardPropertyForm_SideEffectExecute : PoolObject
{
    [SerializeField] private Transform SideEffectRowContainer;
    [SerializeField] private Button AddSideEffectButton;
    [SerializeField] private Button DeleteButton;
    [SerializeField] private Dropdown ExecuteSettingTypeDropdown;

    public override void PoolRecycle()
    {
        base.PoolRecycle();
        CardPropertyForm_ExecuteSettingRow?.PoolRecycle();
        foreach (CardPropertyForm_SideEffect cpfse in CardPropertyFormSideEffects)
        {
            cpfse.PoolRecycle();
        }

        CardPropertyFormSideEffects.Clear();
    }

    private CardPropertyForm_ExecuteSetting CardPropertyForm_ExecuteSettingRow;
    private List<CardPropertyForm_SideEffect> CardPropertyFormSideEffects = new List<CardPropertyForm_SideEffect>();

    public void Initialize(SideEffectExecute.SideEffectFrom sideEffectFrom, SideEffectExecute see, UnityAction onRefreshText, UnityAction onDeleteButtonClick)
    {
        ExecuteSettingTypeDropdown.onValueChanged.RemoveAllListeners();
        ExecuteSettingTypeDropdown.options.Clear();

        foreach (SideEffectExecute.ExecuteSettingTypes est in SideEffectExecute.ValidExecuteSettingTypesForSideEffectFrom[sideEffectFrom])
        {
            ExecuteSettingTypeDropdown.options.Add(new Dropdown.OptionData(est.ToString()));
        }

        SetValue(see.ExecuteSettingType.ToString());
        ExecuteSettingTypeDropdown.onValueChanged.AddListener(
            delegate(int value)
            {
                string optionStr = ExecuteSettingTypeDropdown.options[value].text;
                SideEffectExecute.ExecuteSettingTypes est = (SideEffectExecute.ExecuteSettingTypes) Enum.Parse(typeof(SideEffectExecute.ExecuteSettingTypes), optionStr);
                see.M_ExecuteSetting = SideEffectExecute.ExecuteSetting_Presets[est];
                Initialize(sideEffectFrom, see, onRefreshText, onDeleteButtonClick);
                StartCoroutine(ClientUtils.UpdateLayout((RectTransform) UIManager.Instance.GetBaseUIForm<CardEditorPanel>().CardPropertiesContainer));
            });

        DeleteButton.onClick.RemoveAllListeners();
        DeleteButton.onClick.AddListener(
            delegate
            {
                onDeleteButtonClick();
                StartCoroutine(ClientUtils.UpdateLayout((RectTransform) UIManager.Instance.GetBaseUIForm<CardEditorPanel>().CardPropertiesContainer));
            }
        );

        CardPropertyForm_ExecuteSettingRow?.PoolRecycle();
        foreach (CardPropertyForm_SideEffect cpfse in CardPropertyFormSideEffects)
        {
            cpfse.PoolRecycle();
        }

        CardPropertyFormSideEffects.Clear();

        AddSideEffectButton.onClick.RemoveAllListeners();
        AddSideEffectButton.onClick.AddListener(delegate
        {
            see.SideEffectBases.Add(AllSideEffects.GetSideEffect("Damage").Clone());
            Initialize(sideEffectFrom, see, onRefreshText, onDeleteButtonClick);
            onRefreshText();
            StartCoroutine(ClientUtils.UpdateLayout((RectTransform) UIManager.Instance.GetBaseUIForm<CardEditorPanel>().CardPropertiesContainer));
        });

        bool isReadOnly = see.ExecuteSettingType != SideEffectExecute.ExecuteSettingTypes.Others;
        CardPropertyForm_ExecuteSettingRow = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.CardPropertyForm_ExecuteSetting].AllocateGameObject<CardPropertyForm_ExecuteSetting>(SideEffectRowContainer);
        CardPropertyForm_ExecuteSettingRow.Initialize(see, onRefreshText, isReadOnly);

        ExecuteSettingTypeDropdown.interactable = sideEffectFrom != SideEffectExecute.SideEffectFrom.Buff;
        AddSideEffectButton.gameObject.SetActive(sideEffectFrom != SideEffectExecute.SideEffectFrom.Buff);
        DeleteButton.gameObject.SetActive(sideEffectFrom != SideEffectExecute.SideEffectFrom.Buff);

        foreach (SideEffectBase se in see.SideEffectBases)
        {
            CardPropertyForm_SideEffect cpfse = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.CardPropertyForm_SideEffect].AllocateGameObject<CardPropertyForm_SideEffect>(SideEffectRowContainer);
            cpfse.Initialize(see, null, se, onRefreshText,
                delegate
                {
                    see.SideEffectBases.Remove(se);
                    Initialize(sideEffectFrom, see, onRefreshText, onDeleteButtonClick);
                    onRefreshText();
                    StartCoroutine(ClientUtils.UpdateLayout((RectTransform) UIManager.Instance.GetBaseUIForm<CardEditorPanel>().CardPropertiesContainer));
                });
            CardPropertyFormSideEffects.Add(cpfse);
        }
    }

    private void SetValue(string value_str)
    {
        int setValue = -1;
        for (int i = 0; i < ExecuteSettingTypeDropdown.options.Count; i++)
        {
            if (value_str.Equals(ExecuteSettingTypeDropdown.options[i].text))
            {
                setValue = i;
                break;
            }
        }

        if (setValue != -1)
        {
            ExecuteSettingTypeDropdown.value = 0;
            ExecuteSettingTypeDropdown.value = setValue;
        }
    }
}