using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Serializable classes
[System.Serializable]
public class EnemyWaves 
{
    [Tooltip("time for wave generation from the moment the game started")]
    public float timeToStart;

    [Tooltip("Enemy wave's prefab")]
    public GameObject wave;
}
#endregion

public class LevelController : MonoBehaviour {

    public static LevelController instance; // 싱글톤 인스턴스

    // Serializable classes implements
    public EnemyWaves[] enemyWaves; 

    public GameObject powerUp;
    public float timeForNewPowerup;
    public GameObject[] planets;
    public float timeBetweenPlanets;
    public float planetsSpeed;
    List<GameObject> planetsList = new List<GameObject>();

    Camera mainCamera;   

    [Header("Endless Mode")]
    public bool isEndlessMode = true;      // 무한 모드 여부
    public float spawnInterval = 5f;        // 웨이브 생성 간격

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    // 현재 난이도 배율 계산 (60초마다 0.25씩 증가)
    public float GetDifficultyMultiplier()
    {
        // 기본 1.0 + (흐른 시간 / 60초) * 0.25
        // 예: 1분 경과 시 1.25배, 2분 경과 시 1.5배
        return 1.0f + (Time.timeSinceLevelLoad / 60f) * 0.25f;
    }

    private void Start()
    {
        mainCamera = Camera.main;

        // 정해진 시간에 생성되는 웨이브들
        for (int i = 0; i < enemyWaves.Length; i++) 
        {
            StartCoroutine(CreateEnemyWave(enemyWaves[i].timeToStart, enemyWaves[i].wave));
        }

        // 무한 모드일 경우 무한 생성 코루틴 실행
        if (isEndlessMode)
        {
            StartCoroutine(EndlessEnemyCreation());
        }

        StartCoroutine(PowerupBonusCreation());
        StartCoroutine(PlanetsCreation());
    }

    // 일정 간격으로 랜덤 웨이브를 생성하는 코루틴
    IEnumerator EndlessEnemyCreation()
    {
        yield return new WaitForSeconds(3f); // 초기 대기 시간

        while (isEndlessMode)
        {
            if (enemyWaves != null && enemyWaves.Length > 0 && Player.instance != null)
            {
                int randomIndex = Random.Range(0, enemyWaves.Length);
                GameObject randomWave = enemyWaves[randomIndex].wave;

                if (randomWave != null)
                {
                    Instantiate(randomWave);
                    Debug.Log($"[LevelController] 난이도 {GetDifficultyMultiplier():F2}배 - 새로운 웨이브 생성");
                }
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    
    IEnumerator CreateEnemyWave(float delay, GameObject Wave) 
    {
        if (delay != 0)
            yield return new WaitForSeconds(delay);
        if (Player.instance != null)
            Instantiate(Wave);
    }

    IEnumerator PowerupBonusCreation() 
    {
        while (true) 
        {
            yield return new WaitForSeconds(timeForNewPowerup);
            Instantiate(
                powerUp,
                new Vector2(
                    Random.Range(PlayerMoving.instance.borders.minX, PlayerMoving.instance.borders.maxX), 
                    mainCamera.ViewportToWorldPoint(Vector2.up).y + powerUp.GetComponent<Renderer>().bounds.size.y / 2), 
                Quaternion.identity
                );
        }
    }

    IEnumerator PlanetsCreation()
    {
        for (int i = 0; i < planets.Length; i++)
        {
            planetsList.Add(planets[i]);
        }
        yield return new WaitForSeconds(10);
        while (true)
        {
            int randomIndex = Random.Range(0, planetsList.Count);
            GameObject prefabToSpawn = planetsList[randomIndex];

            if (prefabToSpawn != null)
            {
                GameObject newPlanet = Instantiate(prefabToSpawn);
                newPlanet.GetComponent<DirectMoving>().speed = planetsSpeed;
            }

            planetsList.RemoveAt(randomIndex);

            if (planetsList.Count == 0)
            {
                for (int i = 0; i < planets.Length; i++)
                {
                    planetsList.Add(planets[i]);
                }
            }
            yield return new WaitForSeconds(timeBetweenPlanets);
        }
    }
}
