using System;

[Serializable]
public enum CardSuit
{
    Spade,
    Heart,
    Diamond,
    Club,
    Joker,
    ColorJoker
}

[Serializable]
public class CardData
{
    public CardSuit suit;
    public int rank; // 1(A) to 13(K)

    public CardData(CardSuit suit, int rank)
    {
        this.suit = suit;
        this.rank = rank;
    }

    public override string ToString()
    {
        if (suit == CardSuit.Joker) return "Black Joker";
        if (suit == CardSuit.ColorJoker) return "Color Joker";
        
        string suitName = suit.ToString();
        string rankName = rank.ToString();
        if (rank == 1) rankName = "A";
        else if (rank == 11) rankName = "J";
        else if (rank == 12) rankName = "Q";
        else if (rank == 13) rankName = "K";

        return $"{suitName} {rankName}";
    }
}
