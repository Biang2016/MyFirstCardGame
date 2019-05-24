﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoginPanel : BaseUIForm
{
    private LoginPanel()
    {
    }

    [SerializeField] private Dropdown ServerDropdown;
    [SerializeField] private InputField UserNameInputField;
    [SerializeField] private InputField PasswordInputField;
    [SerializeField] private Text TipText;

    [SerializeField] private Text ServerText;
    [SerializeField] private Text UserNameText;
    [SerializeField] private Text PasswordText;
    [SerializeField] private Text RegisterText;
    [SerializeField] private Text LoginText;

    [SerializeField] private Button QuitButton;
    [SerializeField] private Button RegisterButton;
    [SerializeField] private Button LoginButton;
    [SerializeField] private Button OfflinePlayButton;

    void Awake()
    {
        UIType.InitUIType(
            isClearStack: false,
            isESCClose: false,
            isClickElsewhereClose: false,
            uiForms_Type: UIFormTypes.Normal,
            uiForms_ShowMode: UIFormShowModes.Normal,
            uiForm_LucencyType: UIFormLucencyTypes.ImPenetrable);

        Proxy.OnClientStateChange += OnClientChangeState;

        RegisterButton.onClick.AddListener(OnRegisterButtonClick);
        LoginButton.onClick.AddListener(OnLoginButtonClick);
        QuitButton.onClick.AddListener(OnQuitButtonClick);
        OfflinePlayButton.onClick.AddListener(OnOfflinePlayButtonClick);

        LanguageManager.Instance.RegisterTextKeys(
            new List<(Text, string)>
            {
                (ServerText, "LoginMenu_ServerText"),
                (UserNameText, "LoginMenu_UserNameText"),
                (PasswordText, "LoginMenu_PasswordText"),
                (RegisterText, "LoginMenu_RegisterText"),
                (LoginText, "LoginMenu_LoginText"),
            });

        foreach (string serverTypeName in Enum.GetNames(typeof(ServerTypes)))
        {
            ServerDropdown.options[(int) Enum.Parse(typeof(ServerTypes), serverTypeName)] = new Dropdown.OptionData(LanguageManager.Instance.GetText("Server_" + serverTypeName));
        }
    }

    public enum ServerTypes
    {
        FormalServer = 0,
        TestServer = 1
    }

    void Start()
    {
        string playerPrefServer = PlayerPrefs.GetString("PreferServer");
        if (string.IsNullOrEmpty(playerPrefServer))
        {
            playerPrefServer = ServerTypes.FormalServer.ToString();
        }

        ServerTypes defaultServerType = (ServerTypes) Enum.Parse(typeof(ServerTypes), playerPrefServer);
        ServerDropdown.value = (int) defaultServerType;
        ServerDropdown.onValueChanged.AddListener(OnChangeServer);
        NetworkManager.Instance.ConnectToServer(defaultServerType);
        UserNameInputField.ActivateInputField();
    }

    void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OnLoginButtonClick();
            }
        }
    }

    public override void Display()
    {
        base.Display();
        MouseHoverManager.Instance.M_StateMachine.SetState(MouseHoverManager.StateMachine.States.None);
        SelectBuildManager.Instance.CurrentGameMode = SelectBuildManager.GameMode.None;
        AudioManager.Instance.BGMLoopInList(new List<string> {"bgm/Login_0", "bgm/Login_1"});
        AudioManager.Instance.SoundPlay("sfx/Thunder1");
    }

    public void OnClientChangeState(ProxyBase.ClientStates clientState)
    {
        if (gameObject.activeInHierarchy)
        {
            switch (clientState)
            {
                case ProxyBase.ClientStates.Offline:
                    ShowTipText(LanguageManager.Instance.GetText("LoginMenu_Disconnected"), 0, float.PositiveInfinity, false);
                    break;
                case ProxyBase.ClientStates.GetId:
                    ShowTipText(LanguageManager.Instance.GetText("LoginMenu_Connected"), 0, float.PositiveInfinity, false);
                    break;
                case ProxyBase.ClientStates.Login:
                    CloseUIForm();
                    break;
            }
        }
    }

    public void OnChangeServer(int value)
    {
        NetworkManager.Instance.ConnectToServer((ServerTypes) value);
        PlayerPrefs.SetString("PreferServer", ((ServerTypes) value).ToString());
    }

    public void OnRegisterButtonClick()
    {
        if (Client.Instance.IsConnect())
        {
            RegisterRequest request = new RegisterRequest(Client.Instance.Proxy.ClientId, UserNameInputField.text, PasswordInputField.text);
            Client.Instance.Proxy.SendMessage(request);
        }
        else
        {
            if (Client.Instance.ClientInvalid)
            {
                ShowUpdateConfirmPanel();
            }
            else
            {
                ShowTipText(LanguageManager.Instance.GetText("LoginMenu_Connecting"), 0, float.PositiveInfinity, true);
                OnChangeServer(ServerDropdown.value);
            }
        }
    }

    public void OnLoginButtonClick()
    {
        if (Client.Instance.IsConnect())
        {
            LoginRequest request = new LoginRequest(Client.Instance.Proxy.ClientId, UserNameInputField.text, PasswordInputField.text);
            Client.Instance.Proxy.SendMessage(request);
        }
        else
        {
            if (Client.Instance.ClientInvalid)
            {
                ShowUpdateConfirmPanel();
            }
            else
            {
                ShowTipText(LanguageManager.Instance.GetText("LoginMenu_Connecting"), 0, float.PositiveInfinity, true);
                OnChangeServer(ServerDropdown.value);
            }
        }
    }

    public void OnOfflinePlayButtonClick()
    {
        NetworkManager.Instance.TerminateConnection();
    }

    public void OnQuitButtonClick()
    {
        NetworkManager.Instance.TerminateConnection();
        Application.Quit();
    }

    public void ShowUpdateConfirmPanel()
    {
        ConfirmPanel cp = UIManager.Instance.ShowUIForms<ConfirmPanel>();
        cp.Initialize(
            LanguageManager.Instance.GetText("LoginMenu_ClientNeedUpdate"),
            LanguageManager.Instance.GetText("Common_Yes"),
            LanguageManager.Instance.GetText("Common_Cancel"),
            (new UnityAction(delegate { Application.OpenURL("www.biangstudio.com/mech-storm"); })) + cp.CloseUIForm,
            cp.CloseUIForm
        );
    }

    IEnumerator ShowTipTextCoroutine;

    public void ShowTipText(string text, float delay, float last, bool showDots)
    {
        if (ShowTipTextCoroutine != null)
        {
            StopCoroutine(ShowTipTextCoroutine);
        }

        ShowTipTextCoroutine = Co_ShowTipText(text, delay, last, showDots);
        StartCoroutine(ShowTipTextCoroutine);
    }

    IEnumerator Co_ShowTipText(string text, float delay, float last, bool showDots)
    {
        yield return new WaitForSeconds(delay);
        TipText.text = text;
        if (!float.IsPositiveInfinity(last))
        {
            yield return new WaitForSeconds(last);
            TipText.text = "";
        }
        else
        {
            if (showDots)
            {
                int dotCount = 0;
                while (true)
                {
                    TipText.text += ".";
                    yield return new WaitForSeconds(0.5f);
                    dotCount++;
                    if (dotCount == 3)
                    {
                        dotCount = 0;
                        TipText.text = text;
                    }
                }
            }
            else
            {
                while (true)
                {
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }
    }
}