using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CardManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("CardManager");
                    _instance = go.AddComponent<CardManager>();
                    // DontDestroyOnLoad(go); // 필요한 경우 주석 해제
                }
            }
            return _instance;
        }
    }
    private static CardManager _instance;

    public List<CardData> hand = new List<CardData>();
    public List<CardData> storage = new List<CardData>();
    
    public int gold = 0;
    public int drawCost = 10;
    
    [Header("Visual Resources")]
    public Sprite[] cardSprites; // 16열 4행으로 자른 스프라이트 배열
    
    [Header("Settings")]
    public int maxHand = 5;
    public int maxStorage = 5;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            // DontDestroyOnLoad(gameObject); // 필요한 경우 주석 해제
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public Sprite GetCardSprite(CardSuit suit, int rank)
    {
        if (cardSprites == null || cardSprites.Length == 0) return null;

        // 수트별 행(Row) 결정: 하트=0, 클로버=1, 다이아=2, 스페이드=3
        int row = 0;
        switch (suit)
        {
            case CardSuit.Heart:   row = 0; break;
            case CardSuit.Club:    row = 1; break;
            case CardSuit.Diamond: row = 2; break;
            case CardSuit.Spade:   row = 3; break;
        }

        // 인덱스 계산: 수트마다 13장 (rank 1=0번, rank 13=12번)
        // 예: 하트1 → 0, 하트13 → 12, 클로버1 → 13, 스페이드13 → 51
        int index = (row * 13) + (rank - 1);

        if (index >= 0 && index < cardSprites.Length)
            return cardSprites[index];

        Debug.LogWarning($"카드 스프라이트 인덱스 범위 초과: suit={suit}, rank={rank}, index={index}");
        return null;
    }

    private void Start()
    {
        // 5장으로 시작
        for (int i = 0; i < maxHand; i++)
        {
            DrawInitialCard();
        }
        PrintHandStatus("[게임 시작] 초기 핸드");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) DrawNewCard();
        if (Input.GetKeyDown(KeyCode.T)) DiscardLastCard();
        if (Input.GetKeyDown(KeyCode.F)) StoreLastCard();
    }

    void DrawInitialCard()
    {
        hand.Add(GenerateRandomCard(1, 5)); // 초반용 1-5 랭크
    }

    // ─── 디버그: 현재 핸드 상태 출력 ───
    public void PrintHandStatus(string context = "")
    {
        string header = string.IsNullOrEmpty(context) ? "[핸드 상태]" : context;

        // 카드 목록
        string cards = "";
        foreach (var c in hand)
        {
            string rankStr = c.rank.ToString();
            if (c.rank == 1)  rankStr = "A";
            if (c.rank == 11) rankStr = "J";
            if (c.rank == 12) rankStr = "Q";
            if (c.rank == 13) rankStr = "K";

            string suitStr = "";
            switch (c.suit)
            {
                case CardSuit.Heart:   suitStr = "♥"; break;
                case CardSuit.Club:    suitStr = "♣"; break;
                case CardSuit.Diamond: suitStr = "♦"; break;
                case CardSuit.Spade:   suitStr = "♠"; break;
            }
            cards += $"[{rankStr}{suitStr}] ";
        }

        PokerRank rank = GetCurrentHandRank();
        Debug.Log($"{header}\n  카드: {cards}\n  족보: {rank}");
    }

    public void DrawNewCard()
    {
        if (gold >= drawCost)
        {
            if (hand.Count < maxHand)
            {
                gold -= drawCost;
                hand.Add(GenerateRandomCard(1, 13));
                PrintHandStatus("[카드 드로우]");
            }
            else
            {
                Debug.Log("[카드 드로우 실패] 핸드가 가득 찼습니다!");
            }
        }
        else
        {
            Debug.Log($"[카드 드로우 실패] 골드 부족 (현재: {gold} / 필요: {drawCost})");
        }
    }

    public void DiscardLastCard()
    {
        if (hand.Count > 0)
        {
            var discarded = hand.Last();
            hand.RemoveAt(hand.Count - 1);
            Debug.Log($"[카드 버림] {discarded}");
            PrintHandStatus("[버린 후 핸드]");
        }
    }

    public void StoreLastCard()
    {
        if (hand.Count > 0 && storage.Count < maxStorage)
        {
            CardData card = hand.Last();
            hand.RemoveAt(hand.Count - 1);
            storage.Add(card);
            Debug.Log($"[카드 보관] {card} → 보관함으로 이동");
            PrintHandStatus("[보관 후 핸드]");
        }
    }

    public CardData GenerateRandomCard(int minRank, int maxRank)
    {
        CardSuit suit = (CardSuit)Random.Range(0, 4); // Spade, Heart, Diamond, Club
        int rank = Random.Range(minRank, maxRank + 1);
        return new CardData(suit, rank);
    }

    public void AddGold(int amount)
    {
        gold += amount;
    }

    public void StealCard(CardSuit suitToSteal)
    {
        var card = storage.FirstOrDefault(c => c.suit == suitToSteal);
        if (card != null) storage.Remove(card);
    }

    public PokerRank GetCurrentHandRank()
    {
        if (hand.Count == 0) return PokerRank.None;

        var groups = hand.GroupBy(c => c.rank).OrderByDescending(g => g.Count()).ToList();
        bool isFlush = hand.Count == 5 && hand.All(c => c.suit == hand[0].suit);
        
        var sortedRanks = hand.Select(c => c.rank).OrderBy(r => r).Distinct().ToList();
        bool isStraight = sortedRanks.Count == 5 && (sortedRanks.Last() - sortedRanks.First() == 4);

        if (isFlush && isStraight) return PokerRank.StraightFlush;
        if (groups[0].Count() == 4) return PokerRank.FourOfAKind;
        if (groups[0].Count() == 3 && groups.Count > 1 && groups[1].Count() == 2) return PokerRank.FullHouse;
        if (isFlush) return PokerRank.Flush;
        if (isStraight) return PokerRank.Straight;
        if (groups[0].Count() == 3) return PokerRank.Triple;
        if (groups[0].Count() == 2 && groups.Count > 1 && groups[1].Count() == 2) return PokerRank.TwoPair;
        if (groups[0].Count() == 2) return PokerRank.OnePair;

        return PokerRank.HighCard;
    }
}

public enum PokerRank
{
    None,
    HighCard,
    OnePair,
    TwoPair,
    Triple,
    Straight,
    Flush,
    FullHouse,
    FourOfAKind,
    StraightFlush
}
