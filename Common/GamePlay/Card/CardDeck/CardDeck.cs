﻿using System;
using System.Collections.Generic;

public class CardDeck
{
    /// <summary>
    /// 本类中封装卡组操作的基本功能
    /// </summary>
    private BuildInfo M_BuildInfo;

    private List<CardInfo_Base> Cards = new List<CardInfo_Base>();
    public List<CardInfo_Base> BeginRetinueCards = new List<CardInfo_Base>();
    private List<CardInfo_Base> AbandonCards = new List<CardInfo_Base>();

    public bool IsEmpty = false;
    public bool IsAbandonCardsEmpty = false;

    public delegate void OnCardDeckCountChange(int count);

    public OnCardDeckCountChange CardDeckCountChangeHandler;

    private void checkEmpty()
    {
        IsEmpty = Cards.Count == 0;
        IsAbandonCardsEmpty = AbandonCards.Count == 0;
    }

    public CardDeck(BuildInfo cdi, OnCardDeckCountChange handler)
    {
        M_BuildInfo = cdi;
        CardDeckCountChangeHandler = handler;
        AppendCards(AllCards.GetCards(M_BuildInfo.CardIDs.ToArray()));
        AppendRetinueCards(AllCards.GetCards(M_BuildInfo.BeginRetinueIDs.ToArray()));
        checkEmpty();
        if (GamePlaySettings.SuffleBuild) SuffleSelf();
    }

    private void AddCard(CardInfo_Base cardInfo, int index)
    {
        Cards.Insert(index, cardInfo);
        checkEmpty();
        CardDeckCountChangeHandler(Cards.Count);
    }

    private void AppendCard(CardInfo_Base cardInfo)
    {
        Cards.Add(cardInfo);
        checkEmpty();
        CardDeckCountChangeHandler(Cards.Count);
    }

    private void AppendCards(List<CardInfo_Base> cardInfos)
    {
        Cards.AddRange(cardInfos);
        checkEmpty();
        CardDeckCountChangeHandler(Cards.Count);
    }

    private void AppendRetinueCards(List<CardInfo_Base> retinueCardInfos)
    {
        BeginRetinueCards.AddRange(retinueCardInfos);
    }

    private void RemoveCard(CardInfo_Base cardInfo)
    {
        Cards.Remove(cardInfo);
        checkEmpty();
        CardDeckCountChangeHandler(Cards.Count);
    }

    private void RemoveCards(List<CardInfo_Base> cardInfos)
    {
        foreach (CardInfo_Base cardInfoBase in cardInfos)
        {
            Cards.Remove(cardInfoBase);
        }

        checkEmpty();
        CardDeckCountChangeHandler(Cards.Count);
    }

    public CardInfo_Type FindATypeOfCard<CardInfo_Type>() where CardInfo_Type : CardInfo_Base
    {
        foreach (CardInfo_Base cb in Cards)
        {
            if (cb is CardInfo_Type)
            {
                return (CardInfo_Type) cb;
            }
        }

        return null;
    }

    public List<CardInfo_Type> FindATypeOfCards<CardInfo_Type>(int cardNumber) where CardInfo_Type : CardInfo_Base
    {
        List<CardInfo_Type> resList = new List<CardInfo_Type>();
        int count = 0;
        foreach (CardInfo_Base cb in Cards)
        {
            if (cb is CardInfo_Type)
            {
                count++;
                resList.Add((CardInfo_Type) cb);
                if (count >= cardNumber)
                {
                    break;
                }
            }
        }

        return resList;
    }

    public CardInfo_Base DrawCardOnTop()
    {
        if (Cards.Count > 0)
        {
            CardInfo_Base res = Cards[0];
            RemoveCard(res);
            AbandonCards.Add(res);
            checkEmpty();
            return res;
        }
        else
        {
            return null;
        }
    }

    public List<CardInfo_Base> DrawCardsOnTop(int cardNumber)
    {
        List<CardInfo_Base> resList = new List<CardInfo_Base>();
        for (int i = 0; i < Math.Min(Cards.Count, cardNumber); i++)
        {
            resList.Add(Cards[i]);
        }

        foreach (CardInfo_Base cb in resList)
        {
            RemoveCard(cb);
            AbandonCards.Add(cb);
            checkEmpty();
        }

        return resList;
    }


    public CardInfo_Base GetFirstCardInfo()
    {
        if (Cards.Count > 0)
        {
            return Cards[0];
        }
        else
        {
            return null;
        }
    }

    public List<CardInfo_Base> GetTopCardsInfo(int cardNumber)
    {
        List<CardInfo_Base> resList = new List<CardInfo_Base>();
        for (int i = 0; i < Math.Min(Cards.Count, cardNumber); i++)
        {
            resList.Add(Cards[i]);
        }

        return resList;
    }

    public void SuffleSelf()
    {
        Suffle(Cards);
    }

    public static void Suffle(List<CardInfo_Base> targetCardList)
    {
        for (int i = 0; i < targetCardList.Count * 1; i++)
        {
            int cardNum1 = new Random().Next(0, targetCardList.Count);
            int cardNum2 = new Random().Next(0, targetCardList.Count);
            if (cardNum1 != cardNum2)
            {
                CardInfo_Base tmp = targetCardList[cardNum1];
                targetCardList[cardNum1] = targetCardList[cardNum2];
                targetCardList[cardNum2] = tmp;
            }
            else
            {
                i--;
            }
        }
    }

    public bool GetASodiersCardToTheTop()
    {
        CardInfo_Base target_cb = null;
        foreach (CardInfo_Base cb in Cards)
        {
            if (cb.BaseInfo.CardType == CardTypes.Retinue && !cb.BattleInfo.IsSodier)
            {
                target_cb = cb;
                break;
            }
        }

        if (target_cb != null)
        {
            RemoveCard(target_cb);
            AddCard(target_cb, 0);
            return true;
        }

        return false;
    }

    public void AbandonCardRecycle()
    {
        foreach (CardInfo_Base ac in AbandonCards)
        {
            AppendCard(ac);
        }

        checkEmpty();
        AbandonCards.Clear();
        Suffle(Cards);
    }
}
