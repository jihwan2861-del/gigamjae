using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//guns objects in 'Player's' hierarchy
[System.Serializable]
public class Guns
{
    public GameObject rightGun, leftGun, centralGun;
    [HideInInspector] public ParticleSystem leftGunVFX, rightGunVFX, centralGunVFX; 
}

public class PlayerShooting : MonoBehaviour {

    [Tooltip("shooting frequency. the higher the more frequent")]
    public float fireRate;

    [Tooltip("projectile prefab")]
    public GameObject projectileObject;

    //time for a new shot
    [HideInInspector] public float nextFire;


    [Tooltip("current weapon power")]
    [Range(1, 4)]       //change it if you wish
    public int weaponPower = 1; 

    public Guns guns;
    public Sprite[] suitSprites; // 수트 스프라이트 (0:Spade, 1:Heart, 2:Diamond, 3:Club)
    bool shootingIsActive = true; 
    [HideInInspector] public int maxweaponPower = 4; 
    public static PlayerShooting instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
    private void Start()
    {
        //receiving shooting visual effects components
        guns.leftGunVFX = guns.leftGun.GetComponent<ParticleSystem>();
        guns.rightGunVFX = guns.rightGun.GetComponent<ParticleSystem>();
        guns.centralGunVFX = guns.centralGun.GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        if (shootingIsActive)
        {
            if (Time.time > nextFire)
            {
                MakeAShot();                                                         
                nextFire = Time.time + 1 / fireRate;
            }
        }
    }

    //method for a shot
    void MakeAShot() 
    {
        if (CardManager.instance == null) return;

        PokerRank rank = CardManager.instance.GetCurrentHandRank();
        int avgRank = CardManager.instance.hand.Count > 0 ? (int)CardManager.instance.hand.Average(c => c.rank) : 1;
        int baseDamage = avgRank; 

        List<CardData> currentHand = CardManager.instance.hand;
        if (currentHand.Count == 0) return;

        switch (rank)
        {
            case PokerRank.HighCard:
            case PokerRank.None:
                CreateCardProjectile(guns.centralGun.transform.position, Vector3.zero, baseDamage, GetRandomCardFromHand(currentHand));
                guns.centralGunVFX.Play();
                break;

            case PokerRank.OnePair:
            case PokerRank.TwoPair:
                CreateCardProjectile(guns.rightGun.transform.position, Vector3.zero, baseDamage, GetRandomCardFromHand(currentHand));
                guns.rightGunVFX.Play();
                CreateCardProjectile(guns.leftGun.transform.position, Vector3.zero, baseDamage, GetRandomCardFromHand(currentHand));
                guns.leftGunVFX.Play();
                break;

            case PokerRank.Triple:
                CreateCardProjectile(guns.centralGun.transform.position, Vector3.zero, baseDamage, GetRandomCardFromHand(currentHand));
                CreateCardProjectile(guns.rightGun.transform.position, new Vector3(0, 0, -5), baseDamage, GetRandomCardFromHand(currentHand));
                CreateCardProjectile(guns.leftGun.transform.position, new Vector3(0, 0, 5), baseDamage, GetRandomCardFromHand(currentHand));
                guns.centralGunVFX.Play();
                guns.rightGunVFX.Play();
                guns.leftGunVFX.Play();
                break;

            default: // Flush, Straight, FullHouse 등
                CreateCardProjectile(guns.centralGun.transform.position, Vector3.zero, baseDamage * 2, GetRandomCardFromHand(currentHand));
                CreateCardProjectile(guns.rightGun.transform.position, new Vector3(0, 0, -10), baseDamage * 2, GetRandomCardFromHand(currentHand));
                CreateCardProjectile(guns.leftGun.transform.position, new Vector3(0, 0, 10), baseDamage * 2, GetRandomCardFromHand(currentHand));
                CreateCardProjectile(guns.rightGun.transform.position, new Vector3(0, 0, -25), baseDamage * 2, GetRandomCardFromHand(currentHand));
                CreateCardProjectile(guns.leftGun.transform.position, new Vector3(0, 0, 25), baseDamage * 2, GetRandomCardFromHand(currentHand));
                guns.centralGunVFX.Play();
                guns.rightGunVFX.Play();
                guns.leftGunVFX.Play();
                break;
        }
    }

    CardData GetRandomCardFromHand(List<CardData> hand)
    {
        return hand[Random.Range(0, hand.Count)];
    }

    void CreateCardProjectile(Vector3 pos, Vector3 rot, int damage, CardData card)
    {
        GameObject obj = Instantiate(projectileObject, pos, Quaternion.Euler(rot));
        Projectile proj = obj.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.damage = damage;
            proj.cardData = card;

            // 비주얼 업데이트 호출: CardManager에서 전체 카드 이미지 가져오기
            Sprite cardSprite = CardManager.instance.GetCardSprite(card.suit, card.rank);
            if (cardSprite != null)
            {
                proj.UpdateVisuals(cardSprite);
            }
        }
    }

    void CreateLazerShot(GameObject lazer, Vector3 pos, Vector3 rot) //translating 'pooled' lazer shot to the defined position in the defined rotation
    {
        Instantiate(lazer, pos, Quaternion.Euler(rot));
    }
}
