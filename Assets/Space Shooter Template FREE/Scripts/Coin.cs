using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Value")]
    public int value = 1;

    [Header("Pop (튀어오르기)")]
    public float popForce = 6f;        // 튀어오르는 초기 속도
    public float popHorizontal = 2f;   // 좌우로 약간 퍼지는 힘 (랜덤)

    [Header("Gravity")]
    public float gravity = 12f;        // 아래로 당기는 중력 크기
    public float groundY = -4.5f;      // 바닥으로 간주할 Y 좌표

    [Header("Magnet")]
    public float magnetRadius = 2.5f;  // 이 거리 이내면 자석 흡수 시작
    public float magnetSpeed = 8f;     // 자석 속도

    // 내부 상태
    private enum CoinState { Pop, Fall, Magnet }
    private CoinState state = CoinState.Pop;

    private Vector2 velocity;
    private Transform playerTransform;

    void Start()
    {
        if (Player.instance != null)
            playerTransform = Player.instance.transform;

        // 생성 직후 위+좌우 랜덤 방향으로 튀어오름
        float randomX = Random.Range(-popHorizontal, popHorizontal);
        velocity = new Vector2(randomX, popForce);
    }

    void Update()
    {
        if (playerTransform != null)
        {
            float dist = Vector2.Distance(transform.position, playerTransform.position);

            // 언제든지 플레이어 근처에 오면 자석 모드로 전환
            if (dist < magnetRadius && state != CoinState.Magnet)
                state = CoinState.Magnet;
        }

        switch (state)
        {
            case CoinState.Pop:
                // 위로 올라가면서 속도가 줄어듦
                velocity.y -= gravity * Time.deltaTime;
                transform.position += (Vector3)(velocity * Time.deltaTime);

                // 최고점 지나서 내려가기 시작하면 Fall 상태로
                if (velocity.y < 0)
                    state = CoinState.Fall;
                break;

            case CoinState.Fall:
                // 중력 적용하여 아래로 낙하
                velocity.y -= gravity * Time.deltaTime;
                transform.position += (Vector3)(velocity * Time.deltaTime);

                // 바닥에 닿으면 멈춤 (약간 통통 튀게 하고 싶으면 여기에 bounce 추가)
                if (transform.position.y <= groundY)
                {
                    Vector3 pos = transform.position;
                    pos.y = groundY;
                    transform.position = pos;
                    velocity = Vector2.zero;
                }
                break;

            case CoinState.Magnet:
                if (playerTransform == null) break;

                // 자석처럼 플레이어를 향해 빠르게 이동
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    playerTransform.position,
                    magnetSpeed * Time.deltaTime
                );

                // 아주 가까워지면 골드 획득 및 파괴
                if (Vector2.Distance(transform.position, playerTransform.position) < 0.4f)
                {
                    if (CardManager.instance != null)
                        CardManager.instance.AddGold(value);

                    Destroy(gameObject);
                }
                break;
        }
    }
}
