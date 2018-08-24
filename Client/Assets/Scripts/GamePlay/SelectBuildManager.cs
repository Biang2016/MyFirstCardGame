﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 选牌窗口
/// </summary>
public partial class SelectBuildManager : MonoSingletion<SelectBuildManager>
{
    private SelectBuildManager()
    {
    }

    private int cardSelectLayer;

    void Awake()
    {
        Canvas.gameObject.SetActive(true);
        cardSelectLayer = 1 << LayerMask.NameToLayer("CardSelect");
        SelectCardCount = 0;
        HeroCardCount = 0;
        M_StateMachine = new StateMachine();
        Proxy.OnClientStateChange += NetworkStateChange;
        Proxy.OnClientStateChange += NetworkStateChange_Build;

        RetinueContent.transform.position = new Vector3(0, RetinueContent.transform.position.y, RetinueContent.transform.position.z);
        SelectionContent.transform.position = new Vector3(0, SelectionContent.transform.position.y, SelectionContent.transform.position.z);
    }

    void Start()
    {
        AddAllCards();
        M_StateMachine.SetState(StateMachine.States.Hide);
        ConfirmButton.gameObject.SetActive(false);
        CloseButton.gameObject.SetActive(true);
        RetinueCountMaxNumberText.text = GamePlaySettings.MaxHeroNumber.ToString();
    }

    [SerializeField] private Transform AllCardsContent;

    [SerializeField] private Transform RetinueContent;

    [SerializeField] private Transform SelectionContent;

    [SerializeField] private Canvas Canvas;

    [SerializeField] private Canvas Canvas_BG;

    [SerializeField] private Transform PreviewContent;

    [SerializeField] private Button ConfirmButton;

    [SerializeField] private Button CloseButton;

    [SerializeField] private Button SelectAllButton;

    [SerializeField] private Button UnSelectAllButton;

    [SerializeField] private Text CountNumberText;

    [SerializeField] private Text RetinueCountNumberText;

    [SerializeField] private Text RetinueCountMaxNumberText;

    [SerializeField] private Camera Camera;

    [SerializeField] private Transform SelectCardPrefab;


    void Update()
    {
        M_StateMachine.Update();
    }

    public StateMachine M_StateMachine;

    public class StateMachine
    {
        public StateMachine()
        {
            state = States.Default;
            previousState = States.Default;
        }

        public enum States
        {
            Default,
            Hide,
            Show,
        }

        private States state;
        private States previousState;

        public void SetState(States newState)
        {
            if (state != newState)
            {
                switch (newState)
                {
                    case States.Hide:
                        HideWindow();
                        break;

                    case States.Show:
                        ShowWindow();
                        break;
                }

                previousState = state;
                state = newState;
            }
        }

        public void ReturnToPreviousState()
        {
            SetState(previousState);
        }

        public States GetState()
        {
            return state;
        }

        public void Update()
        {
            if (ExitMenuManager.Instance.M_StateMachine.GetState() == ExitMenuManager.StateMachine.States.Show) return;
            if (state == States.Hide)
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    SetState(States.Show);
                }
            }
            else
            {
                bool isMouseDown = ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && EventSystem.current.IsPointerOverGameObject());
                bool isMouseUp = ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) && EventSystem.current.IsPointerOverGameObject());
                if (Instance.CurrentPreviewCard)
                {
                    if (Input.GetKeyDown(KeyCode.Escape) || isMouseDown) Instance.HidePreviewCard();
                    else if (Input.GetKeyDown(KeyCode.Tab))
                    {
                        Instance.HidePreviewCard();
                        SetState(States.Hide);
                    }
                }
                else
                {
                    if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab))
                    {
                        SetState(States.Hide);
                    }
                    else if (isMouseDown)
                    {
                        Instance.OnMouseDown();
                    }
                    else if (isMouseUp)
                    {
                        Instance.OnMouseUp();
                    }
                }
            }
        }

        private void ShowWindow()
        {
            GameManager.Instance.StartBlurBackGround();
            Instance.Canvas.enabled = true;
            Instance.Canvas_BG.enabled = true;
            MouseHoverManager.Instance.M_StateMachine.SetState(MouseHoverManager.StateMachine.States.SelectCardWindow);
            if (Client.Instance.IsLogin()) StartMenuManager.Instance.M_StateMachine.SetState(StartMenuManager.StateMachine.States.Hide);
        }

        private void HideWindow()
        {
            Instance.Canvas.enabled = false;
            Instance.Canvas_BG.enabled = false;
            GameManager.Instance.StopBlurBackGround();
            MouseHoverManager.Instance.M_StateMachine.ReturnToPreviousState();
            if (Client.Instance.IsLogin()) StartMenuManager.Instance.M_StateMachine.SetState(StartMenuManager.StateMachine.States.Show);
        }
    }

    private CardBase mouseLeftDownCard;
    private CardBase mouseRightDownCard;
    private Vector3 mouseDownPosition;

    private CardBase CurrentPreviewCard;
    private CardBase PreviewCard;

    private void OnMouseDown()
    {
        if (BuildRenamePanel.gameObject.activeSelf) return;
        mouseDownPosition = Input.mousePosition;
        Ray ray = Camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit raycast;
        Physics.Raycast(ray, out raycast, 10f, cardSelectLayer);
        if (raycast.collider != null)
        {
            CardBase card = raycast.collider.gameObject.GetComponent<CardBase>();
            if (Input.GetMouseButtonDown(1))
            {
                if (card)
                {
                    mouseRightDownCard = card;
                }
                else
                {
                    mouseRightDownCard = null;
                }
            }
            else if (Input.GetMouseButtonDown(0) && card)
            {
                mouseLeftDownCard = card;
            }
        }
        else
        {
            mouseRightDownCard = null;
            mouseLeftDownCard = null;
            HidePreviewCard();
        }
    }

    private void OnMouseUp()
    {
        if (BuildRenamePanel.gameObject.activeSelf) return;
        Ray ray = Camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit raycast;
        Physics.Raycast(ray, out raycast, 10f, cardSelectLayer);
        if (raycast.collider != null)
        {
            CardBase card = raycast.collider.gameObject.GetComponent<CardBase>();
            if (Input.GetMouseButtonUp(1))
            {
                if (card && mouseRightDownCard == card)
                {
                    ShowPreviewCard(card);
                }
                else
                {
                    HidePreviewCard();
                }
            }
            else if (Input.GetMouseButtonUp(0) && card)
            {
                if ((Input.mousePosition - mouseDownPosition).magnitude < 50)
                {
                    if (mouseLeftDownCard == card)
                    {
                        SelectCard(card);
                    }
                }
            }
        }
        else
        {
            HidePreviewCard();
        }

        mouseLeftDownCard = null;
        mouseRightDownCard = null;
    }

    public void NetworkStateChange(ProxyBase.ClientStates clientState)
    {
        bool isConnected = clientState == ProxyBase.ClientStates.Login || clientState == ProxyBase.ClientStates.Login;
        ConfirmButton.gameObject.SetActive(isConnected);
        CloseButton.gameObject.SetActive(!isConnected);
    }

    #region 卡片初始化

    public void AddAllCards()
    {
        foreach (CardInfo_Base cardInfo in AllCards.CardDict.Values)
        {
            if (cardInfo.CardID == 999 || cardInfo.CardID == 99) continue;
            AddCardIntoCardSelectWindow(cardInfo);
        }
    }

    public void AddCardIntoCardSelectWindow(CardInfo_Base cardInfo)
    {
        CardBase newCard = CardBase.InstantiateCardByCardInfo(cardInfo, AllCardsContent, null, true);
        newCard.transform.localScale = Vector3.one * 120;
        newCard.transform.rotation = Quaternion.Euler(90, 180, 0);
        newCard.BeDimColor();
        allCards.Add(newCard.CardInfo.CardID, newCard);
    }

    #endregion

    #region 选择卡片

    private Dictionary<int, CardBase> allCards = new Dictionary<int, CardBase>();
    private Dictionary<int, SelectCard> SelectedCards = new Dictionary<int, SelectCard>();
    private Dictionary<int, SelectCard> SelectedHeros = new Dictionary<int, SelectCard>();

    private int selectCardCount;

    public int SelectCardCount
    {
        get { return selectCardCount; }
        set
        {
            selectCardCount = value;
            CountNumberText.text = selectCardCount.ToString();
        }
    }

    private bool isSelectedHeroFull = false;
    private bool isSelectedHeroEmpty = true;
    private int _heroCardCount;

    public int HeroCardCount
    {
        get { return _heroCardCount; }
        set
        {
            _heroCardCount = value;
            RetinueCountNumberText.text = _heroCardCount.ToString();
            isSelectedHeroFull = _heroCardCount >= GamePlaySettings.MaxHeroNumber;
            isSelectedHeroEmpty = _heroCardCount == 0;
        }
    }

    private void SelectCard(CardBase card)
    {
        if (CurrentEditBuildButton == null)
        {
            NoticeManager.Instance.ShowInfoPanelCenter("请创建卡组!", 0f, 1f);
            return;
        }

        bool isHero = card.CardInfo.BaseInfo.CardType == CardTypes.Retinue && !card.CardInfo.BattleInfo.IsSodier;
        if (isHero)
        {
            if (isSelectedHeroFull)
            {
                NoticeManager.Instance.ShowInfoPanelCenter("可携带英雄卡牌数量已达上限", 0, 1f);
                return;
            }

            if (SelectedHeros.ContainsKey(card.CardInfo.CardID))
            {
                int count = ++SelectedHeros[card.CardInfo.CardID].Count;
                card.SetBlockCountValue(count);
            }
            else
            {
                SelectCard retinueSelect = GenerateNewSelectCard(card, RetinueContent);
                SelectedHeros.Add(card.CardInfo.CardID, retinueSelect);
                List<SelectCard> SCs = RetinueContent.GetComponentsInChildren<SelectCard>(true).ToList();
                SCs.Sort((a, b) => a.Cost.CompareTo(b.Cost));
                RetinueContent.DetachChildren();
                foreach (SelectCard selectCard in SCs)
                {
                    selectCard.transform.SetParent(RetinueContent);
                }

                card.SetBlockCountValue(1);
                card.BeBrightColor();
                card.CardBloom.SetActive(true);
            }

            if (!isSwitchingBuildInfo) CurrentEditBuildButton.AddHeroCard(card.CardInfo.CardID);

            HeroCardCount++;
        }
        else
        {
            if (SelectedCards.ContainsKey(card.CardInfo.CardID))
            {
                int count = ++SelectedCards[card.CardInfo.CardID].Count;
                card.SetBlockCountValue(count);
            }
            else
            {
                SelectCard newSC = GenerateNewSelectCard(card, SelectionContent);
                SelectedCards.Add(card.CardInfo.CardID, newSC);
                List<SelectCard> SCs = SelectionContent.GetComponentsInChildren<SelectCard>(true).ToList();
                SCs.Sort((a, b) => a.Cost.CompareTo(b.Cost));
                SelectionContent.DetachChildren();
                foreach (SelectCard selectCard in SCs)
                {
                    selectCard.transform.SetParent(SelectionContent);
                }

                card.SetBlockCountValue(1);
                card.BeBrightColor();
                card.CardBloom.SetActive(true);
            }

            if (!isSwitchingBuildInfo) CurrentEditBuildButton.AddCard(card.CardInfo.CardID);

            SelectCardCount++;
        }
    }

    private SelectCard GenerateNewSelectCard(CardBase card, Transform parenTransform)
    {
        SelectCard newSC = GameObjectPoolManager.Instance.Pool_SelectCardPool.AllocateGameObject(parenTransform).GetComponent<SelectCard>();

        newSC.Count = 1;
        newSC.Cost = card.CardInfo.BaseInfo.Cost;
        newSC.Text_CardName.text = card.CardInfo.BaseInfo.CardName;
        newSC.CardButton.onClick.RemoveAllListeners();
        newSC.CardButton.onClick.AddListener(delegate { UnSelectCard(card); });
        Color cardColor = ClientUtils.HTMLColorToColor(card.CardInfo.BaseInfo.CardColor);
        newSC.CardButton.image.color = new Color(cardColor.r, cardColor.g, cardColor.b, 1f);
        return newSC;
    }

    private void UnSelectCard(CardBase card)
    {
        if (CurrentEditBuildButton == null)
        {
            NoticeManager.Instance.ShowInfoPanelCenter("请创建卡组!", 0f, 1f);
            return;
        }

        bool isRetinue = card.CardInfo.BaseInfo.CardType == CardTypes.Retinue && !card.CardInfo.BattleInfo.IsSodier;

        if (isRetinue)
        {
            int count = --SelectedHeros[card.CardInfo.CardID].Count;
            card.SetBlockCountValue(count);
            if (SelectedHeros[card.CardInfo.CardID].Count == 0)
            {
                SelectedHeros[card.CardInfo.CardID].PoolRecycle();
                SelectedHeros.Remove(card.CardInfo.CardID);
                card.BeDimColor();
                card.CardBloom.SetActive(false);
            }

            if (!isSwitchingBuildInfo) CurrentEditBuildButton.RemoveHeroCard(card.CardInfo.CardID);

            HeroCardCount--;
        }
        else
        {
            int count = --SelectedCards[card.CardInfo.CardID].Count;
            card.SetBlockCountValue(count);
            if (SelectedCards[card.CardInfo.CardID].Count == 0)
            {
                SelectedCards[card.CardInfo.CardID].PoolRecycle();
                SelectedCards.Remove(card.CardInfo.CardID);
                card.BeDimColor();
                card.CardBloom.SetActive(false);
            }

            if (!isSwitchingBuildInfo) CurrentEditBuildButton.RemoveCard(card.CardInfo.CardID);

            SelectCardCount--;
        }
    }

    public void SelectAllCard()
    {
        if (CurrentEditBuildButton == null)
        {
            NoticeManager.Instance.ShowInfoPanelCenter("请创建卡组!", 0f, 1f);
        }
        else
        {
            foreach (CardBase cardBase in allCards.Values)
            {
                if (SelectedCards.ContainsKey(cardBase.CardInfo.CardID)) continue;
                if (SelectedHeros.ContainsKey(cardBase.CardInfo.CardID)) continue;
                SelectCard(cardBase);
            }
        }
    }

    private bool isSwitchingBuildInfo;

    private void SelectCardsByBuildInfo(BuildInfo buildInfo)
    {
        isSwitchingBuildInfo = true;
        UnSelectAllCard();
        foreach (int buildInfoBeginRetinueID in buildInfo.BeginRetinueIDs)
        {
            SelectCard(allCards[buildInfoBeginRetinueID]);
        }

        foreach (int cardID in buildInfo.CardIDs)
        {
            SelectCard(allCards[cardID]);
        }

        isSwitchingBuildInfo = false;
    }

    public void UnSelectAllCard()
    {
        foreach (KeyValuePair<int, CardBase> kv in allCards)
        {
            kv.Value.BeDimColor();
            kv.Value.CardBloom.SetActive(false);
            kv.Value.SetBlockCountValue(0);
            if (SelectedCards.ContainsKey(kv.Key))
            {
                SelectedCards[kv.Key].PoolRecycle();
                SelectedCards.Remove(kv.Key);
            }

            if (SelectedHeros.ContainsKey(kv.Key))
            {
                SelectedHeros[kv.Key].PoolRecycle();
                SelectedHeros.Remove(kv.Key);
            }
        }

        if (!isSwitchingBuildInfo)
        {
            if (CurrentEditBuildButton != null)
            {
                CurrentEditBuildButton.BuildInfo.CardIDs.Clear();
                CurrentEditBuildButton.BuildInfo.BeginRetinueIDs.Clear();
            }
        }

        SelectCardCount = 0;
        HeroCardCount = 0;
    }

    public void OnConfirmSubmitCardDeckButtonClick()
    {
        if (CurrentEditBuildButton == null)
        {
            NoticeManager.Instance.ShowInfoPanelCenter("请创建卡组!", 0f, 1f);
        }
        else
        {
            Client.Instance.Proxy.OnSendBuildInfo(CurrentEditBuildButton.BuildInfo);
            NoticeManager.Instance.ShowInfoPanelCenter("更新卡组成功", 0, 1f);
        }
    }

    public void OnCloseButtonClick()
    {
        M_StateMachine.SetState(StateMachine.States.Hide);
        if (Client.Instance.IsLogin())
        {
            StartMenuManager.Instance.M_StateMachine.SetState(StartMenuManager.StateMachine.States.Show);
        }
    }

    #endregion

    #region 预览卡片、升级卡片

    private void ShowPreviewCard(CardBase card)
    {
        HidePreviewCard();
        CurrentPreviewCard = card;
        PreviewCard = CardBase.InstantiateCardByCardInfo(card.CardInfo, PreviewContent, null, true);
        PreviewCard.transform.localScale = Vector3.one * 300;
        PreviewCard.transform.rotation = Quaternion.Euler(90, 180, 0);
        PreviewCard.transform.localPosition = new Vector3(0, 0, -300);
        PreviewCard.CardBloom.SetActive(true);
        PreviewCard.ChangeCardBloomColor(ClientUtils.HTMLColorToColor("#FFDD8C"));
    }

    private void HidePreviewCard()
    {
        if (PreviewCard)
        {
            PreviewCard.CardBloom.SetActive(true);
            PreviewCard.PoolRecycle();
            PreviewCard = null;
            CurrentPreviewCard = null;
        }
    }

    #endregion
}