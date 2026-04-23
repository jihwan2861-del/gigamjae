using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This script defines which sprite the 'Player" uses and its health.
/// </summary>

public class Player : MonoBehaviour
{
    public GameObject destructionFX;
    public static Player instance; 

    [Header("Health Settings")]
    public int health = 6;
    public int maxHealth = 6;
    private SpriteRenderer spriteRenderer;

    [Header("Regeneration Settings")]
    public float regenDelay = 15f;    // 회복 시작 대기 시간
    public float regenInterval = 1f; // 회복 간격 (1초당 1씩)
    private float lastDamageTime;
    private bool isRegenerating = false;

    private void Update()
    {
        // 15초 동안 피해를 입지 않았고, 현재 체력이 최대가 아니며, 아직 회복 중이 아닐 때
        if (Time.time - lastDamageTime >= regenDelay && health < maxHealth && !isRegenerating)
        {
            StartCoroutine(RegenRoutine());
        }
    }

    IEnumerator RegenRoutine()
    {
        isRegenerating = true;
        while (health < maxHealth && Time.time - lastDamageTime >= regenDelay)
        {
            Heal(1);
            yield return new WaitForSeconds(regenInterval);
        }
        isRegenerating = false;
    }

    [Header("Sound Settings")]
    public AudioSource audioSource;
    public AudioClip hitSound;
    public AudioClip healSound;

    [Header("Invincibility Settings")]
    public float invincibilityDuration = 1.5f; // 무적 지속 시간
    private bool isInvincible = false;

    private void Awake()
    {
        if (instance == null) 
            instance = this;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // AudioSource가 없으면 자동으로 추가하거나 가져옴
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    // 데미지 처리
    public void GetDamage(int damage)   
    {
        // [추가] 무적 상태라면 데미지를 무시함
        if (isInvincible) return;

        health -= damage;
        lastDamageTime = Time.time; // [추가] 마지막 피격 시간 갱신
        Debug.Log($"[Player] 피격! 남은 HP: {health}");

        // [추가] 카메라 쉐이크 호출 (지속시간 0.2초, 강도 0.3)
        if (CameraShake.instance != null) CameraShake.instance.Shake(0.2f, 0.3f);

        if (audioSource != null && hitSound != null) audioSource.PlayOneShot(hitSound);

        if (health <= 0)
        {
            Destruction();
        }
        else
        {
            // 무적 루틴 시작
            StartCoroutine(InvincibilityRoutine());
        }
    }

    // 무적 상태 처리 코루틴
    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        
        float elapsed = 0f;
        while (elapsed < invincibilityDuration)
        {
            // 스프라이트 깜빡임 연출
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        spriteRenderer.enabled = true; // 항상 마지막엔 켜줌
        isInvincible = false;
    }

    // 체력 회복 및 초록색 연출
    public void Heal(int amount)
    {
        health = Mathf.Min(health + amount, maxHealth);

        if (audioSource != null && healSound != null) audioSource.PlayOneShot(healSound);

        StartCoroutine(GreenFlash());
        Debug.Log($"[Player] 체력 회복 (+{amount})! 현재 HP: {health}");
    }

    IEnumerator GreenFlash()
    {
        if (spriteRenderer == null) yield break;
        
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = new Color(0.2f, 1f, 0.2f, 1f); // 초록색 빛
        
        yield return new WaitForSeconds(1.0f);
        
        spriteRenderer.color = originalColor;
    }

    IEnumerator DamageFlash()
    {
        if (spriteRenderer == null) yield break;
        spriteRenderer.color = Color.black; // 검은색으로 변경
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }

    void Destruction()
    {
        Instantiate(destructionFX, transform.position, Quaternion.identity);
        
        // 게임 오버 화면 호출
        if (PlayerUI.instance != null)
        {
            PlayerUI.instance.ShowGameOver();
        }

        // 플레이어 오브젝트는 바로 파괴하지 않고 비활성화하거나, 
        // 혹은 파괴하더라도 UI에서 상태를 잡아야 합니다.
        // 여기서는 일단 비활성화 후 파괴로 처리하겠습니다.
        gameObject.SetActive(false);
        Destroy(gameObject, 0.1f);
    }
}
