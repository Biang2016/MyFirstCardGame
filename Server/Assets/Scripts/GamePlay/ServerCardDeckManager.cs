﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

/// <summary>
/// 本类中封装卡组操作的游戏逻辑高级功能
/// </summary>
internal class ServerCardDeckManager
{
    public ServerPlayer ServerPlayer;

    public CardDeckInfo M_UnlockCards;
    public CardDeckInfo M_LockCards;
    public List<CardDeck> M_CardDecks;

    public ServerCardDeckManager(ServerPlayer serverPlayer)
    {
        ServerPlayer = serverPlayer;
    }

    private CardDeck m_CurrentCardDeck;

    public CardDeck M_CurrentCardDeck
    {
        get { return m_CurrentCardDeck; }
        set { m_CurrentCardDeck = value; }
    }

    public CardInfo_Base DrawRetinueCard()
    {
        if (M_CurrentCardDeck.IsEmpty)
        {
            M_CurrentCardDeck.AbandonCardRecycle();
        }

        M_CurrentCardDeck.GetARetinueCardToTheTop();
        CardInfo_Base newCardInfoBase = DrawCardOnTop();
        return newCardInfoBase;
    }

    public CardInfo_Base DrawCardOnTop()
    {
        if (M_CurrentCardDeck.IsEmpty)
        {
            M_CurrentCardDeck.AbandonCardRecycle();
        }

        CardInfo_Base newCardInfoBase = M_CurrentCardDeck.DrawCardOnTop();
        return newCardInfoBase;
    }

    public List<CardInfo_Base> DrawCardsOnTop(int cardNumber)
    {
        if (M_CurrentCardDeck.IsEmpty)
        {
            M_CurrentCardDeck.AbandonCardRecycle();
        }

        List<CardInfo_Base> newCardInfoBases = M_CurrentCardDeck.DrawCardsOnTop(cardNumber);
        List<int> cardIds = new List<int>();
        foreach (CardInfo_Base newCardInfoBase in newCardInfoBases)
        {
            cardIds.Add(newCardInfoBase.CardID);
        }
        return newCardInfoBases;
    }
}