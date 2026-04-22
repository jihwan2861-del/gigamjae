using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script defines 'Enemy's' health and behavior. 
/// </summary>
public class Enemy : MonoBehaviour {

    #region FIELDS
    [Tooltip("Health points in integer")]
    public int health;

    [Tooltip("Enemy's projectile prefab")]
    public GameObject Projectile;

    [Tooltip("VFX prefab generating after destruction")]
    public GameObject destructionVFX;
    public GameObject hitEffect;
    
    [HideInInspector] public int shotChance; //probability of 'Enemy's' shooting during tha path
    [HideInInspector] public float shotTimeMin, shotTimeMax; //max and min time for shooting from the beginning of the path
    
    [Header("Card System")]
    public CardData cardData; // 적의 카드 속성
    public GameObject coinPrefab; // 드랍할 코인 프리팹
    #endregion

    protected virtual void Start()
    {
        // 적 생성 시 랜덤 카드 부여
        if (CardManager.instance != null)
        {
            cardData = CardManager.instance.GenerateRandomCard(1, 5);
        }

        Invoke("ActivateShooting", Random.Range(shotTimeMin, shotTimeMax));
    }

    //coroutine making a shot
    void ActivateShooting() 
    {
        if (Random.value < (float)shotChance / 100)                             //if random value less than shot probability, making a shot
        {                         
            Instantiate(Projectile,  gameObject.transform.position, Quaternion.identity);             
        }
    }

    //method of getting damage for the 'Enemy'
    public virtual void GetDamage(int damage, CardData bulletCard = null) 
    {
        int finalDamage = damage;

        if (bulletCard != null && cardData != null)
        {
            // 1. 같은 카드(수트+숫자)일 경우 즉사
            if (bulletCard.suit == cardData.suit && bulletCard.rank == cardData.rank)
            {
                Debug.Log("JACKPOT! One-shot Kill!");
                finalDamage = health + 100;
            }
            // 2. 같은 숫자일 경우 데미지 2배
            else if (bulletCard.rank == cardData.rank)
            {
                Debug.Log("Double Damage! Ranked Match.");
                finalDamage *= 2;
            }
        }

        health -= finalDamage;           //reducing health for damage value, if health is less than 0, starting destruction procedure
        if (health <= 0)
            Destruction();
        else
            Instantiate(hitEffect,transform.position,Quaternion.identity,transform);
    }    

    //if 'Enemy' collides 'Player', 'Player' gets the damage equal to projectile's damage value
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (Projectile.GetComponent<Projectile>() != null)
                Player.instance.GetDamage(Projectile.GetComponent<Projectile>().damage);
            else
                Player.instance.GetDamage(1);
        }
    }

    //method of destroying the 'Enemy'
    public virtual void Destruction()                           
    {        
        Instantiate(destructionVFX, transform.position, Quaternion.identity); 

        if (coinPrefab != null)
        {
            Vector3 coinPos = new Vector3(transform.position.x, transform.position.y, 0f);
            Instantiate(coinPrefab, coinPos, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
