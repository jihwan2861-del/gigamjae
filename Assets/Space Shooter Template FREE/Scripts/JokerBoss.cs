using UnityEngine;

public class JokerBoss : Enemy
{
    public bool isColorJoker = false;
    public float stealInterval = 5f;
    private float nextStealTime;

    protected override void Start()
    {
        base.Start();
        health = 50; // 보스 체력 설정
        nextStealTime = Time.time + stealInterval;
    }

    private void Update()
    {
        if (Time.time > nextStealTime)
        {
            StealPattern();
            nextStealTime = Time.time + stealInterval;
        }
    }

    void StealPattern()
    {
        if (CardManager.instance == null) return;

        if (isColorJoker)
        {
            CardManager.instance.StealCard(CardSuit.Heart);
            CardManager.instance.StealCard(CardSuit.Diamond);
        }
        else
        {
            CardManager.instance.StealCard(CardSuit.Spade);
            CardManager.instance.StealCard(CardSuit.Club);
        }
    }

    public override void GetDamage(int damage, CardData bulletCard = null)
    {
        int finalDamage = damage;

        if (bulletCard != null)
        {
            if (isColorJoker)
            {
                if (bulletCard.suit == CardSuit.Heart || bulletCard.suit == CardSuit.Diamond)
                {
                    finalDamage = (int)(finalDamage * 1.5f);
                }
            }
            else
            {
                if (bulletCard.suit == CardSuit.Spade || bulletCard.suit == CardSuit.Club)
                {
                    finalDamage = (int)(finalDamage * 1.5f);
                }
            }
        }

        health -= finalDamage;
        if (health <= 0)
            Destruction();
        else
            Instantiate(hitEffect, transform.position, Quaternion.identity, transform);
    }
}
