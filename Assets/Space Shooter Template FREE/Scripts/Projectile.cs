using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // 숫자 표시를 위해 추가

public class Projectile : MonoBehaviour {

    [Tooltip("Damage which a projectile deals to another object. Integer")]
    public int damage;

    [Tooltip("Whether the projectile belongs to the ‘Enemy’ or to the ‘Player’")]
    public bool enemyBullet;

    [Tooltip("Whether the projectile is destroyed in the collision, or not")]
    public bool destroyedByCollision;

    [Header("Card Visuals")]
    public SpriteRenderer suitRenderer; // 수트 아이콘용Renderer
    public TMP_Text rankText;           // 숫자 표시용 텍스트

    [Header("Card Data")]
    public CardData cardData; 

    public void UpdateVisuals(Sprite cardSprite)
    {
        if (cardData == null) return;

        // 1. 전체 카드 이미지 설정
        if (suitRenderer != null && cardSprite != null)
        {
            suitRenderer.sprite = cardSprite;
            // 카드 이미지가 잘 보이도록 스케일 조정이 필요할 수 있습니다.
        }

        // 2. 숫자는 이미지에 포함되어 있으므로 텍스트는 비활성화
        if (rankText != null)
        {
            rankText.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if (enemyBullet && collision.tag == "Player") 
        {
            Player.instance.GetDamage(damage); 
            if (destroyedByCollision)
                Destruction();
        }
        else if (!enemyBullet && collision.tag == "Enemy")
        {
            collision.GetComponent<Enemy>().GetDamage(damage, cardData);
            if (destroyedByCollision)
                Destruction();
        }
    }

    void Destruction() 
    {
        Destroy(gameObject);
    }
}


