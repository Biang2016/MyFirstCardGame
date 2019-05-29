﻿using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HeroCardPicIcon : PoolObject
{
    private HeroCardPicIcon()
    {
    }

    public override void PoolRecycle()
    {
        base.PoolRecycle();
    }

    [SerializeField] private Image PicImage;
    [SerializeField] private Text CardIDText;

    public void Initialize(int pictureID,int cardID)
    {
        ClientUtils.ChangeCardPicture(PicImage, pictureID);
        CardIDText.text = cardID.ToString();
    }
}