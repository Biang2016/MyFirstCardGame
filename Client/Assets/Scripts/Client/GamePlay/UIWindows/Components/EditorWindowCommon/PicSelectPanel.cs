﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;
using UnityEngine.UI;

public class PicSelectPanel : MonoBehaviour
{
    [SerializeField] private GameObject PicSelectGridPanel;
    [SerializeField] private GridLayoutGroup PicSelectGridContainer;
    [SerializeField] private Button PicSelectGridOpenButton;
    [SerializeField] private Button PicSelectGridCloseButton;
    public Text Label;
    private List<PicPreviewButton> PicPreviewButtons = new List<PicPreviewButton>();

    internal bool IsOpen = false;

    internal void InitializePicSelectGrid(string labelStrKey)
    {
        LanguageManager.Instance.RegisterTextKey(Label, labelStrKey);

        PicSelectGridPanel.SetActive(false);
        PicSelectGridCloseButton.gameObject.SetActive(false);
        foreach (PicPreviewButton ppb in PicPreviewButtons)
        {
            ppb.PoolRecycle();
        }

        PicPreviewButtons.Clear();

        SortedDictionary<int, Sprite> SpriteDict = new SortedDictionary<int, Sprite>();
        for (int i = 0; i <= 10; i++)
        {
            SpriteAtlas sa = AtlasManager.LoadAtlas("CardPics_" + i);
            Sprite[] Sprites = new Sprite[sa.spriteCount];
            sa.GetSprites(Sprites);
            foreach (Sprite sprite in Sprites)
            {
                SpriteDict.Add(int.Parse(sprite.name.Replace("(Clone)", "")), sprite);
            }
        }

        foreach (KeyValuePair<int, Sprite> kv in SpriteDict)
        {
            PicPreviewButton ppb = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.PicPreviewButton].AllocateGameObject<PicPreviewButton>(PicSelectGridContainer.transform);
            ppb.Initialize(kv.Value, delegate
            {
                OnClickPicAction?.Invoke(kv.Key.ToString(), false);
                OnPicSelectGridCloseButtonClick();
            });
            PicPreviewButtons.Add(ppb);
        }
    }

    public UnityAction<string, bool> OnClickPicAction;
    public UnityAction OnOpenPanelAction;
    public UnityAction OnClosePanelAction;

    public void OnPicSelectGridOpenButtonClick()
    {
        PicSelectGridPanel.SetActive(true);
        PicSelectGridCloseButton.gameObject.SetActive(true);
        PicSelectGridOpenButton.gameObject.SetActive(false);
        OnOpenPanelAction?.Invoke();
        IsOpen = true;
    }

    public void OnPicSelectGridCloseButtonClick()
    {
        PicSelectGridPanel.SetActive(false);
        PicSelectGridCloseButton.gameObject.SetActive(false);
        PicSelectGridOpenButton.gameObject.SetActive(true);
        OnClosePanelAction?.Invoke();
        IsOpen = false;
    }
}