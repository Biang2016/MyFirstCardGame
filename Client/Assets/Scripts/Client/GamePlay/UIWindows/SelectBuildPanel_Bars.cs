﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class SelectBuildPanel
{
    [SerializeField] private GameObject CoinBarContainer;
    [SerializeField] private Slider CoinSlider;
    [SerializeField] private GameObject BudgetIcon;
    [SerializeField] private Text BudgetText;
    [SerializeField] private Text LeftCoinNumberText;
    [SerializeField] private Text TotalCoinNumberText;
    [SerializeField] private Transform MyCoinTextMinPosPivot;
    [SerializeField] private Transform MyCoinTextMaxPosPivot;

    [SerializeField] private GameObject LifeBarContainer;
    [SerializeField] private Slider LifeSlider;
    [SerializeField] private Text LifeText;
    [SerializeField] private Text MyLifeText;
    [SerializeField] private Text MaxLifeText;
    [SerializeField] private Transform MyLifeTextMinPosPivot;
    [SerializeField] private Transform MyLifeTextMaxPosPivot;

    [SerializeField] private GameObject EnergyBarContainer;
    [SerializeField] private Slider EnergySlider;
    [SerializeField] private Text EnergyText;
    [SerializeField] private Text MyEnergyText;
    [SerializeField] private Text MaxEnergyText;
    [SerializeField] private Transform MyEnergyTextMinPosPivot;
    [SerializeField] private Transform MyEnergyTextMaxPosPivot;

    [SerializeField] private GameObject DrawCardNumberBarContainer;
    [SerializeField] private Slider DrawCardNumberSlider;
    [SerializeField] private Text DrawCardNumText;
    [SerializeField] private Text MyDrawCardNumberText;
    [SerializeField] private Text MaxDrawCardNumberText;
    [SerializeField] private Transform MyCardNumberTextMinPosPivot;
    [SerializeField] private Transform MyCardNumberTextMaxPosPivot;
    [SerializeField] private Text AddCardNumberCoinText;
    [SerializeField] private Text DecCardNumberCoinText;
    [SerializeField] private Image AddCardNumberCoinImage;
    [SerializeField] private Image DecCardNumberCoinImage;
    [SerializeField] private Animator AddCardNumberCoinAnim;
    [SerializeField] private Animator DecCardNumberCoinAnim;

    void Awake_Bars()
    {
        LanguageManager.Instance.RegisterTextKeys(new List<(Text, string)>
        {
            (LifeText, "SelectBuildManagerSelect_LifeText"),
            (EnergyText, "SelectBuildManagerSelect_EnergyText"),
            (BudgetText, "SelectBuildManagerSelect_BudgetText"),
            (DrawCardNumText, "SelectBuildManagerSelect_DrawCardNumText"),
        });
        LanguageManager.Instance.RegisterTextFontBinding(new List<Text>
        {
            TotalCoinNumberText,
            MaxLifeText,
            MaxEnergyText,
            MaxDrawCardNumberText
        });
        LifeSlider.interactable = true;
        EnergySlider.interactable = true;
        CoinSlider.interactable = false;
        DrawCardNumberSlider.interactable = true;
    }

    void Start_Bars()
    {
        BudgetIcon.SetActive(!LanguageManager.Instance.IsEnglish);
    }

    void Init_Bars()
    {
        BudgetIcon.SetActive(!LanguageManager.Instance.IsEnglish);
        InitializeSliders();
    }

    void SetReadOnly_Bars(bool isReadOnly)
    {
        LifeSlider.interactable = !isReadOnly;
        EnergySlider.interactable = !isReadOnly;
        DrawCardNumberSlider.interactable = !isReadOnly;
    }

    private void ShowSliders(bool isShow)
    {
        CoinBarContainer.SetActive(isShow);
        LifeBarContainer.SetActive(isShow);
        EnergyBarContainer.SetActive(isShow);
        DrawCardNumberBarContainer.SetActive(isShow);
    }

    public void InitializeSliders()
    {
        CoinSlider.onValueChanged.RemoveAllListeners();
        LifeSlider.onValueChanged.RemoveAllListeners();
        EnergySlider.onValueChanged.RemoveAllListeners();
        DrawCardNumberSlider.onValueChanged.RemoveAllListeners();

        CoinSlider.value = (float) CurrentGamePlaySettings.DefaultCoin / CurrentGamePlaySettings.DefaultMaxCoin;
        CoinSlider.minValue = 0;
        CoinSlider.maxValue = CurrentGamePlaySettings.DefaultMaxCoin;
        LifeSlider.value = (float) CurrentGamePlaySettings.DefaultLife / CurrentGamePlaySettings.DefaultLifeMax;
        LifeSlider.minValue = 0;
        LifeSlider.maxValue = CurrentGamePlaySettings.DefaultLifeMax;
        EnergySlider.value = (float) CurrentGamePlaySettings.DefaultEnergy / CurrentGamePlaySettings.DefaultEnergyMax;
        EnergySlider.maxValue = CurrentGamePlaySettings.DefaultEnergyMax;
        EnergySlider.minValue = 0;
        DrawCardNumberSlider.value = CurrentGamePlaySettings.DefaultDrawCardNum;

        CoinSlider.onValueChanged.AddListener(OnCoinSliderValueChange);
        LifeSlider.onValueChanged.AddListener(OnLifeSliderValueChange);
        EnergySlider.onValueChanged.AddListener(OnEnergySliderValueChange);
        DrawCardNumberSlider.onValueChanged.AddListener(OnDrawCardNumberSliderValueChange);

        TotalCoinNumberText.text = CurrentGamePlaySettings.DefaultMaxCoin.ToString();
        MaxLifeText.text = CurrentGamePlaySettings.DefaultLifeMax.ToString();
        MaxEnergyText.text = CurrentGamePlaySettings.DefaultEnergyMax.ToString();
        MaxDrawCardNumberText.text = CurrentGamePlaySettings.MaxDrawCardNum.ToString();

        RefreshDrawCardCoinText(CurrentGamePlaySettings.DefaultDrawCardNum);
    }

    private void RefreshDrawCardCoinText(int drawCardNum)
    {
        if (drawCardNum == CurrentGamePlaySettings.MaxDrawCardNum)
        {
            AddCardNumberCoinImage.enabled = false;
            AddCardNumberCoinText.text = "";
            DecCardNumberCoinImage.enabled = true;
            DecCardNumberCoinText.text = "+" + (GamePlaySettings.DrawCardNumToCoin[drawCardNum] - GamePlaySettings.DrawCardNumToCoin[drawCardNum - 1]).ToString();
        }
        else if (drawCardNum < CurrentGamePlaySettings.MaxDrawCardNum && drawCardNum > CurrentGamePlaySettings.MinDrawCardNum)
        {
            AddCardNumberCoinImage.enabled = true;
            AddCardNumberCoinText.text = (GamePlaySettings.DrawCardNumToCoin[drawCardNum] - GamePlaySettings.DrawCardNumToCoin[drawCardNum + 1]).ToString();
            DecCardNumberCoinImage.enabled = true;
            DecCardNumberCoinText.text = "+" + (GamePlaySettings.DrawCardNumToCoin[drawCardNum] - GamePlaySettings.DrawCardNumToCoin[drawCardNum - 1]).ToString();
        }
        else if (drawCardNum == CurrentGamePlaySettings.MinDrawCardNum)
        {
            AddCardNumberCoinImage.enabled = true;
            AddCardNumberCoinText.text = (GamePlaySettings.DrawCardNumToCoin[drawCardNum] - GamePlaySettings.DrawCardNumToCoin[drawCardNum + 1]).ToString();
            DecCardNumberCoinImage.enabled = false;
            DecCardNumberCoinText.text = "";
        }
    }

    private bool ShowMessages = false;

    private void RefreshCoinLifeEnergy(bool showMessages = true)
    {
        ShowMessages = showMessages;
        CurrentEditBuildButton.BuildInfo.GamePlaySettings = CurrentGamePlaySettings;
        CoinSlider.value = CurrentGamePlaySettings.DefaultMaxCoin - CurrentEditBuildButton.BuildInfo.BuildConsumeCoin;
        LifeSlider.value = CurrentEditBuildButton.BuildInfo.Life;
        EnergySlider.value = CurrentEditBuildButton.BuildInfo.Energy;
        ShowMessages = true;
    }

    private void RefreshDrawCardNum()
    {
        DrawCardNumberSlider.value = CurrentEditBuildButton.BuildInfo.DrawCardNum;
        OnDrawCardNumberSliderValueChange(DrawCardNumberSlider.value);
    }

    private void OnCoinSliderValueChange(float value)
    {
        LeftCoinNumberText.text = (CurrentGamePlaySettings.DefaultMaxCoin - CurrentEditBuildButton.BuildInfo.BuildConsumeCoin).ToString();
        LeftCoinNumberText.rectTransform.localPosition = Vector3.Lerp(MyCoinTextMinPosPivot.localPosition, MyCoinTextMaxPosPivot.localPosition, value / CurrentGamePlaySettings.DefaultMaxCoin);
    }

    private float noBudgetNoticeInterval = 3f;
    private float lastNoBudgetNoticeTime;

    private void OnLifeSliderValueChange(float value)
    {
        float maxValueNow = ((CurrentGamePlaySettings.DefaultMaxCoin - CurrentEditBuildButton.BuildInfo.CardConsumeCoin - CurrentEditBuildButton.BuildInfo.EnergyConsumeCoin - CurrentEditBuildButton.BuildInfo.DrawCardNumConsumeCoin +
                              CurrentGamePlaySettings.DefaultLifeMin * GamePlaySettings.LifeToCoin) / GamePlaySettings.LifeToCoin);
        float minValue = CurrentGamePlaySettings.DefaultLifeMin;
        if (value > maxValueNow)
        {
            LifeSlider.value = maxValueNow;
            if (ShowMessages)
            {
                if (Time.time - lastNoBudgetNoticeTime > noBudgetNoticeInterval)
                {
                    lastNoBudgetNoticeTime = Time.time;
                    NoticeManager.Instance.ShowInfoPanelCenter(LanguageManager.Instance.GetText("SelectBuildManagerBuild_BudgetLimitedChooseFewerCards"), 0f, 1f);
                }
            }

            return;
        }
        else if (value < minValue)
        {
            LifeSlider.value = minValue;
            if (ShowMessages)
            {
                if (Time.time - lastNoBudgetNoticeTime > noBudgetNoticeInterval)
                {
                    lastNoBudgetNoticeTime = Time.time;
                    NoticeManager.Instance.ShowInfoPanelCenter(LanguageManager.Instance.GetText("SelectBuildManagerLifeMagicCoinCardnum_UnsafeIfLifeLow"), 0f, 1f);
                }
            }

            return;
        }

        CurrentEditBuildButton.BuildInfo.Life = Mathf.RoundToInt(value);
        MyLifeText.text = CurrentEditBuildButton.BuildInfo.Life.ToString();
        MyLifeText.rectTransform.localPosition = Vector3.Lerp(MyLifeTextMinPosPivot.localPosition, MyLifeTextMaxPosPivot.localPosition, value / CurrentGamePlaySettings.DefaultLifeMax);

        CoinSlider.value = CurrentGamePlaySettings.DefaultMaxCoin - CurrentEditBuildButton.BuildInfo.BuildConsumeCoin;
    }

    private void OnEnergySliderValueChange(float value)
    {
        float maxValueNow = (CurrentGamePlaySettings.DefaultMaxCoin - CurrentEditBuildButton.BuildInfo.CardConsumeCoin - CurrentEditBuildButton.BuildInfo.LifeConsumeCoin - CurrentEditBuildButton.BuildInfo.DrawCardNumConsumeCoin) / GamePlaySettings.EnergyToCoin;
        float minValue = 0;
        if (value > maxValueNow)
        {
            EnergySlider.value = maxValueNow;
            if (ShowMessages)
            {
                if (Time.time - lastNoBudgetNoticeTime > noBudgetNoticeInterval)
                {
                    lastNoBudgetNoticeTime = Time.time;
                    NoticeManager.Instance.ShowInfoPanelCenter(LanguageManager.Instance.GetText("SelectBuildManagerBuild_BudgetLimitedChooseFewerCards"), 0f, 1f);
                }
            }

            return;
        }
        else if (value < minValue)
        {
            EnergySlider.value = minValue;
            return;
        }

        CurrentEditBuildButton.BuildInfo.Energy = Mathf.RoundToInt(value);
        MyEnergyText.text = CurrentEditBuildButton.BuildInfo.Energy.ToString();
        MyEnergyText.rectTransform.localPosition = Vector3.Lerp(MyEnergyTextMinPosPivot.localPosition, MyEnergyTextMaxPosPivot.localPosition, value / CurrentGamePlaySettings.DefaultEnergyMax);

        CoinSlider.value = CurrentGamePlaySettings.DefaultMaxCoin - CurrentEditBuildButton.BuildInfo.BuildConsumeCoin;
    }

    private void OnDrawCardNumberSliderValueChange(float value)
    {
        int drawCardNum = Mathf.RoundToInt(value);
        if (drawCardNum == CurrentEditBuildButton.BuildInfo.DrawCardNum)
        {
            MyDrawCardNumberText.text = drawCardNum.ToString();
            MyDrawCardNumberText.rectTransform.localPosition = Vector3.Lerp(MyCardNumberTextMinPosPivot.localPosition, MyCardNumberTextMaxPosPivot.localPosition, value / CurrentGamePlaySettings.MaxDrawCardNum);
            CoinSlider.value = CurrentGamePlaySettings.DefaultMaxCoin - CurrentEditBuildButton.BuildInfo.BuildConsumeCoin;
            return;
        }

        int leftCoin = CurrentGamePlaySettings.DefaultMaxCoin - CurrentEditBuildButton.BuildInfo.CardConsumeCoin - CurrentEditBuildButton.BuildInfo.LifeConsumeCoin - CurrentEditBuildButton.BuildInfo.EnergyConsumeCoin;
        int minDrawCardNum = CurrentGamePlaySettings.MinDrawCardNum;
        int maxDrawCardNum = 0;
        for (int i = CurrentGamePlaySettings.MinDrawCardNum; i <= CurrentGamePlaySettings.MaxDrawCardNum; i++)
        {
            if (GamePlaySettings.DrawCardNumToCoin[i] - GamePlaySettings.DrawCardNumToCoin[CurrentGamePlaySettings.MinDrawCardNum] <= leftCoin)
            {
                maxDrawCardNum = i;
            }
        }

        if (drawCardNum > maxDrawCardNum)
        {
            DrawCardNumberSlider.value = maxDrawCardNum;
            if (ShowMessages)
            {
                if (Time.time - lastNoBudgetNoticeTime > noBudgetNoticeInterval)
                {
                    lastNoBudgetNoticeTime = Time.time;
                    NoticeManager.Instance.ShowInfoPanelCenter(LanguageManager.Instance.GetText("SelectBuildManagerBuild_BudgetLimitedChooseFewerCards"), 0f, 1f);
                }
            }

            return;
        }
        else if (drawCardNum < minDrawCardNum)
        {
            DrawCardNumberSlider.value = minDrawCardNum;
            return;
        }

        CurrentEditBuildButton.BuildInfo.DrawCardNum = drawCardNum;
        MyDrawCardNumberText.text = drawCardNum.ToString();
        MyDrawCardNumberText.rectTransform.localPosition = Vector3.Lerp(MyCardNumberTextMinPosPivot.localPosition, MyCardNumberTextMaxPosPivot.localPosition, value / CurrentGamePlaySettings.MaxDrawCardNum);

        CoinSlider.value = CurrentGamePlaySettings.DefaultMaxCoin - CurrentEditBuildButton.BuildInfo.BuildConsumeCoin;

        AddCardNumberCoinAnim.SetTrigger("Jump");
        DecCardNumberCoinAnim.SetTrigger("Jump");
        AudioManager.Instance.SoundPlay("sfx/OnMoneyChange");
        RefreshDrawCardCoinText(drawCardNum);
    }
}