using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 다시 추가
using TMPro;
using UnityEngine.SceneManagement; 
using System.Collections;
using System.Collections.Generic;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI instance;

    [Header("Card System UI (TMP)")]
    public TMP_Text handText;
    public TMP_Text storageText;
    public TMP_Text goldText;
    public TMP_Text scoreText; // 점수 표시용 추가
    public TMP_Text rankText;
    public TMP_Text drawCostText;
    public TMP_Text helpText; // "B = 상점" 가이드용 텍스트
    [Header("Game States UI")]
    public GameObject startPanel;      // 시작 화면 패널
    public GameObject gameOverPanel;   // 게임 오버 패널
    public Image fadeImage;            // 화면 암전용 이미지 (Black)

    [Header("Health UI (Pixel)")]
    public Image[] hpHearts;      // 하트 아이콘 5개 배열
    public Sprite fullHeartSprite;  // 가득 찬 하트 이미지
    public Sprite emptyHeartSprite; // 빈 하트 이미지

    [Header("Card Visuals (Sidebar)")]
    public Image[] handImages;     // 핸드 카드 이미지 5개
    public Image[] storageImages;  // 보관함 카드 이미지 5개
    public Sprite[] suitSprites;

    [Header("카드 인터랙션 (B키 모드)")]
    public GameObject actionPanel;       // 선택된 카드 아래 나타나는 버튼 묶음
    public Button drawCardButton;        // 카드 뽑기 버튼
    public Button discardButton;         // 버리기
    public Button storeButton;           // 보관함으로
    public Button handButton;            // 핸드로 (보관함 카드 선택 시)
    public TMP_Text actionCardNameText;  // "♠ A 선택 중"

    [Header("인터랙션 모드 표시")]
    public GameObject shop_BackGround;       // 전체 상점 배경 패널
    public Image      gatcha_card;            // 상단에서 연출이 나오는 카드 이미지
    public GameObject blurPanel;             // 배경 블러 패널
    public GameObject centerHandArea;        // 중앙 카드 부모
    public Image[]    centerHandImages;      // 중앙 카드 이미지들 (5개)
    public GameObject gatchaCardArea;        // 상단 카드 슬롯 영역 (배경)

    [Header("연출 설정 (초)")]
    public float shuffleDuration = 1.0f;     // 셔플되는 시간
    public float resultShowDuration = 2.0f;  // 결과를 보여주는 시간

    [Header("사운드 설정")]
    public AudioSource audioSource;
    public AudioClip shuffleSound;
    public AudioClip revealSound;
    public AudioClip errorSound; // 골드 부족 등
    public AudioClip clickSound; // 버튼 클릭 소리 [추가]

    // ── 상태 ──────────────────────────────────────
    private bool isGameStarted = false;
    private bool isGameOver = false;
    private bool isInteractMode = false;
    private int  selectedHandIndex    = -1;
    private int  selectedStorageIndex = -1;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        // 버튼에 이벤트 연결
        if (drawCardButton != null) drawCardButton.onClick.AddListener(OnDrawCard);
        if (discardButton  != null) discardButton.onClick.AddListener(OnDiscard);
        if (storeButton    != null) storeButton.onClick.AddListener(OnStore);
        if (handButton     != null) handButton.onClick.AddListener(OnMoveToHand);

        // [추가] 초기 패널 설정
        if (startPanel != null) startPanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (fadeImage != null) {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = new Color(0,0,0,0); // 투명하게 시작
        }

        Time.timeScale = 0f; // 시작 전 대기

        // 카드 이미지에 클릭 이벤트 추가
        SetupCardClickEvents();

        HideActionPanel();
    }

    public void StartGame()
    {
        isGameStarted = true;
        Time.timeScale = 1f;
        if (startPanel != null) startPanel.SetActive(false);
        Debug.Log("[PlayerUI] 게임 시작!");
    }

    void Update()
    {
        // 1. 시작 대기 모드
        if (!isGameStarted && !isGameOver)
        {
            if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
            {
                StartGame();
            }
            return;
        }

        if (isGameOver) return;

        // 2. B키 토글
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("[PlayerUI] B키 눌림! 현재 모드: " + (isInteractMode ? "인터랙션" : "게임"));
            if (isInteractMode) ExitInteractMode();
            else                EnterInteractMode();
        }

        // R 키로 재시작
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }

        // 인터랙션 모드가 아닐 때만 UI 자동 갱신
        if (!isInteractMode) RefreshUI();
    }

    // ── 인터랙션 모드 진입/해제 ────────────────────
    void EnterInteractMode()
    {
        isInteractMode = true;
        Time.timeScale = 0f;
        selectedHandIndex = selectedStorageIndex = -1;

        if (shop_BackGround != null) shop_BackGround.SetActive(true);
        if (blurPanel       != null) blurPanel.SetActive(true);
        if (drawCardButton  != null) drawCardButton.gameObject.SetActive(true);
        if (centerHandArea  != null) centerHandArea.SetActive(true);
        if (gatchaCardArea  != null) gatchaCardArea.SetActive(true); 
        
        if (helpText != null) helpText.text = "B = 상점 나가기";

        // [추가] 배경음악 일시정지 (BGM_Manager 태그나 이름을 가진 오브젝트의 AudioSource 정지)
        AudioSource bgm = GameObject.Find("BGM_Manager")?.GetComponent<AudioSource>();
        if (bgm != null) bgm.Pause();

        RefreshUI();
        HideActionPanel();
    }

    void ExitInteractMode()
    {
        isInteractMode = false;
        Time.timeScale = 1f;
        selectedHandIndex = selectedStorageIndex = -1;

        if (shop_BackGround != null) shop_BackGround.SetActive(false);
        if (blurPanel       != null) blurPanel.SetActive(false);
        if (drawCardButton  != null) drawCardButton.gameObject.SetActive(false);
        if (centerHandArea  != null) centerHandArea.SetActive(false);
        if (gatchaCardArea  != null) gatchaCardArea.SetActive(false); 

        if (helpText != null) helpText.text = "B = 상점 열기";
        
        // [추가] 배경음악 재개
        AudioSource bgm = GameObject.Find("BGM_Manager")?.GetComponent<AudioSource>();
        if (bgm != null) bgm.UnPause();

        HideActionPanel();
    }

    // ── UI 새로고침 ────────────────────────────────
    void RefreshUI()
    {
        if (CardManager.instance == null) return;

        // 1. 기본 정보 업데이트
        if (goldText != null) goldText.text = "GOLD " + CardManager.instance.gold;
        if (scoreText != null) scoreText.text = "SCORE " + CardManager.instance.score; // 점수 추가
        if (drawCostText != null) drawCostText.text = "DRAW " + CardManager.instance.drawCost + "G";

        // 1-1. 체력(하트) 업데이트
        if (Player.instance != null && hpHearts != null)
        {
            for (int i = 0; i < hpHearts.Length; i++)
            {
                if (hpHearts[i] == null) continue;
                
                // 현재 체력보다 작으면 하트 표시, 크면 숨김
                if (i < Player.instance.health)
                {
                    hpHearts[i].sprite = fullHeartSprite;
                    hpHearts[i].gameObject.SetActive(true);
                }
                else
                {
                    hpHearts[i].gameObject.SetActive(false); // 하트 숨기기
                }
            }
        }

        if (rankText != null)
        {
            var rank = CardManager.instance.GetCurrentHandRank();
            rankText.text = rank == PokerRank.None ? "" : "RANK " + rank.ToString();
        }

        // 2. 사이드바 핸드
        if (handImages != null)
        {
            for (int i = 0; i < maxHandSlots; i++)
            {
                if (i >= handImages.Length || handImages[i] == null) continue;
                bool hasCard = CardManager.instance.hand != null && i < CardManager.instance.hand.Count;
                handImages[i].gameObject.SetActive(hasCard);
                if (hasCard)
                {
                    var card = CardManager.instance.hand[i];
                    var sprite = CardManager.instance.GetCardSprite(card.suit, card.rank);
                    if (sprite != null) handImages[i].sprite = sprite;
                }
            }
        }
        if (handText != null) handText.text = "HAND";

        // 3. 사이드바 보관함
        if (storageImages != null)
        {
            for (int i = 0; i < maxStorageSlots; i++)
            {
                if (i >= storageImages.Length || storageImages[i] == null) continue;
                bool hasCard = CardManager.instance.storage != null && i < CardManager.instance.storage.Count;
                storageImages[i].gameObject.SetActive(hasCard);
                if (hasCard)
                {
                    var card = CardManager.instance.storage[i];
                    var sprite = CardManager.instance.GetCardSprite(card.suit, card.rank);
                    if (sprite != null) storageImages[i].sprite = sprite;
                }
            }
        }
        if (storageText != null) storageText.text = "STORAGE";

        // 4. 중앙 핸드 슬롯
        if (centerHandImages != null)
        {
            for (int i = 0; i < centerHandImages.Length; i++)
            {
                if (centerHandImages[i] == null) continue;
                bool hasCard = CardManager.instance.hand != null && i < CardManager.instance.hand.Count;
                centerHandImages[i].gameObject.SetActive(hasCard);
                if (hasCard)
                {
                    var card = CardManager.instance.hand[i];
                    var sprite = CardManager.instance.GetCardSprite(card.suit, card.rank);
                    if (sprite != null) centerHandImages[i].sprite = sprite;
                }
            }
        }
    }

    // ── 카드 클릭 이벤트 설정 ──────────────────────
    void SetupCardClickEvents()
    {
        if (handImages != null)
        {
            for (int i = 0; i < handImages.Length; i++)
            {
                if (handImages[i] == null) continue;
                handImages[i].raycastTarget = true; // 클릭 가능하도록 설정
                int idx = i;
                var trigger = handImages[i].gameObject.GetComponent<EventTrigger>()
                           ?? handImages[i].gameObject.AddComponent<EventTrigger>();
                trigger.triggers.Clear(); // 중복 방지
                var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
                entry.callback.AddListener(_ => OnHandCardClicked(idx));
                trigger.triggers.Add(entry);
            }
        }

        if (storageImages != null)
        {
            for (int i = 0; i < storageImages.Length; i++)
            {
                if (storageImages[i] == null) continue;
                storageImages[i].raycastTarget = true; // 클릭 가능하도록 설정
                int idx = i;
                var trigger = storageImages[i].gameObject.GetComponent<EventTrigger>()
                           ?? storageImages[i].gameObject.AddComponent<EventTrigger>();
                trigger.triggers.Clear(); // 중복 방지
                var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
                entry.callback.AddListener(_ => OnStorageCardClicked(idx));
                trigger.triggers.Add(entry);
            }
        }

        // 중앙 카드 클릭 이벤트 추가
        if (centerHandImages != null)
        {
            for (int i = 0; i < centerHandImages.Length; i++)
            {
                if (centerHandImages[i] == null) continue;
                centerHandImages[i].raycastTarget = true;
                int idx = i;
                var trigger = centerHandImages[i].gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>()
                           ?? centerHandImages[i].gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                trigger.triggers.Clear();
                var entry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerClick };
                entry.callback.AddListener(_ => OnHandCardClicked(idx)); // 중앙 카드를 클릭해도 핸드 클릭으로 처리
                trigger.triggers.Add(entry);
            }
        }

        // gatcha_card 클릭 이벤트 추가 (클릭 시 해당 카드를 선택하여 액션바 표시)
        if (gatcha_card != null)
        {
            gatcha_card.raycastTarget = true;
            var trigger = gatcha_card.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>()
                       ?? gatcha_card.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            trigger.triggers.Clear();
            var entry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerClick };
            entry.callback.AddListener(_ => {
                // 방금 뽑힌 카드는 핸드의 마지막 인덱스에 있음
                int lastIdx = CardManager.instance.hand.Count - 1;
                if (lastIdx >= 0) OnHandCardClicked(lastIdx);
            });
            trigger.triggers.Add(entry);
        }
    }

    // ── 카드 클릭 처리 ────────────────────────────
    void OnHandCardClicked(int index)
    {
        Debug.Log($"[PlayerUI] 핸드 카드 클릭 시도: index {index}, 모드 {isInteractMode}");
        if (!isInteractMode) return;
        if (CardManager.instance?.hand == null) return;
        if (index >= CardManager.instance.hand.Count) return;

        selectedHandIndex    = index;
        selectedStorageIndex = -1;

        var card = CardManager.instance.hand[index];
        ShowActionPanel(card, fromStorage: false);
    }

    void OnStorageCardClicked(int index)
    {
        if (!isInteractMode) return;
        if (CardManager.instance?.storage == null) return;
        if (index >= CardManager.instance.storage.Count) return;

        selectedStorageIndex = index;
        selectedHandIndex    = -1;

        var card = CardManager.instance.storage[index];
        ShowActionPanel(card, fromStorage: true);
    }

    // ── 액션 패널 ─────────────────────────────────
    void ShowActionPanel(CardData card, bool fromStorage)
    {
        if (actionPanel != null) 
        {
            actionPanel.SetActive(true);
            actionPanel.transform.SetAsLastSibling();
            
            // 위치와 크기 강제 초기화 제거 (유저 설정 위치 유지)

            // 이미지 컴포넌트 점검 (투명도 등)
            var img = actionPanel.GetComponent<UnityEngine.UI.Image>();
            if (img != null) 
            {
                img.enabled = true;
                img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
            }
        }

        if (actionCardNameText != null)
        {
            string r = card.rank switch { 1=>"A", 11=>"J", 12=>"Q", 13=>"K", _=>card.rank.ToString() };
            string s = card.suit switch {
                CardSuit.Heart   => "♥",
                CardSuit.Club    => "♣",
                CardSuit.Diamond => "♦",
                CardSuit.Spade   => "♠",
                _ => ""
            };
            actionCardNameText.text = $"선택: {s} {r}";
        }

        // 버튼 표시/숨김: 핸드 카드는 보관/버리기, 보관함은 핸드로/버리기
        if (storeButton  != null) storeButton.gameObject.SetActive(!fromStorage);
        if (handButton   != null) handButton.gameObject.SetActive(fromStorage);
        if (discardButton!= null) discardButton.gameObject.SetActive(true);
    }

    void HideActionPanel()
    {
        if (actionPanel != null) actionPanel.SetActive(false);
        selectedHandIndex = selectedStorageIndex = -1;
    }

    // ── 버튼 액션 ─────────────────────────────────
    private Coroutine drawCoroutine;

    public void OnDrawCard()
    {
        PlayClickSound(); // [소리 추가]
        if (CardManager.instance == null) return;

        // [중요] 이미 연출 중이면 무조건 리턴 (두 번 실행 방지)
        if (drawCoroutine != null) return;

        // 골드 부족 체크
        if (CardManager.instance.gold < CardManager.instance.drawCost)
        {
            if (audioSource != null && errorSound != null) audioSource.PlayOneShot(errorSound);
            StartCoroutine(ShowWarningMessage("골드가 부족합니다!"));
            return;
        }

        // 카드 생성
        CardData newCard = CardManager.instance.DrawNewCard();
        if (newCard != null)
        {
            Debug.Log($"<color=orange>[PlayerUI]</color> 드로우 실행 (카드: {newCard.suit} {newCard.rank})");
            drawCoroutine = StartCoroutine(DrawCardAnimation(newCard, CardManager.instance.hand.Count - 1));
        }
    }

    IEnumerator ShowWarningMessage(string msg)
    {
        if (messageText == null) yield break;

        messageText.text = msg;
        messageText.color = Color.red;
        messageText.gameObject.SetActive(true);

        float duration = 1.0f;
        float elapsed = 0f;
        Color startColor = Color.red;
        Color endColor = new Color(1, 0, 0, 0); // 투명한 빨간색

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            messageText.color = Color.Lerp(startColor, endColor, elapsed / duration);
            yield return null;
        }

        messageText.gameObject.SetActive(false);
    }

    public void OnDiscard()
    {
        PlayClickSound(); // [소리 추가]
        if (CardManager.instance == null) return;
        if (selectedHandIndex >= 0 && selectedHandIndex < CardManager.instance.hand.Count)
        {
            CardManager.instance.hand.RemoveAt(selectedHandIndex);
            CardManager.instance.PrintHandStatus("[버린 후]");
            if (gatcha_card != null) gatcha_card.gameObject.SetActive(false);
        }
        else if (selectedStorageIndex >= 0 && selectedStorageIndex < CardManager.instance.storage.Count)
        {
            CardManager.instance.storage.RemoveAt(selectedStorageIndex);
        }
        HideActionPanel();
        RefreshUI();
    }

    public void OnStore()
    {
        PlayClickSound(); // [소리 추가]
        if (CardManager.instance == null) return;
        if (CardManager.instance.storage.Count >= CardManager.instance.maxStorage)
        { Debug.Log("[카드 관리] 보관함 가득!"); return; }
        if (selectedHandIndex < 0 || selectedHandIndex >= CardManager.instance.hand.Count) return;

        var c = CardManager.instance.hand[selectedHandIndex];
        CardManager.instance.storage.Add(c);
        CardManager.instance.hand.RemoveAt(selectedHandIndex);
        CardManager.instance.PrintHandStatus("[보관 후]");

        if (gatcha_card != null) gatcha_card.gameObject.SetActive(false);

        HideActionPanel();
        RefreshUI();
    }

    public void OnMoveToHand()
    {
        PlayClickSound(); // [소리 추가]
        if (CardManager.instance == null) return;
        if (CardManager.instance.hand.Count >= CardManager.instance.maxHand)
        { Debug.Log("[카드 관리] 핸드 가득!"); return; }
        if (selectedStorageIndex < 0 || selectedStorageIndex >= CardManager.instance.storage.Count) return;

        var c = CardManager.instance.storage[selectedStorageIndex];
        CardManager.instance.hand.Add(c);
        CardManager.instance.storage.RemoveAt(selectedStorageIndex);
        CardManager.instance.PrintHandStatus("[핸드로 이동]");

        if (gatcha_card != null) gatcha_card.gameObject.SetActive(false);

        HideActionPanel();
        RefreshUI();
    }

    // ── 연출: 카드 드로우 애니메이션 ────────────────
    System.Collections.IEnumerator DrawCardAnimation(CardData finalCard, int targetIndex)
    {
        Debug.Log($"<color=yellow>[PlayerUI]</color> 애니메이션 시작! 대상 카드: {finalCard.suit} {finalCard.rank}");
        if (gatcha_card == null) 
        { 
            Debug.LogError("[카드 연출] 에러: gatcha_card 이미지가 할당되지 않았습니다!");
            RefreshUI(); 
            yield break; 
        }

        // 1. 초기화
        gatcha_card.gameObject.SetActive(true);
        gatcha_card.transform.SetAsLastSibling();
        
        RectTransform rect = gatcha_card.rectTransform;
        rect.anchoredPosition = new Vector2(-154f, 188f);
        rect.sizeDelta = new Vector2(100f, 150f);
        
        gatcha_card.transform.localScale = Vector3.one;
        gatcha_card.color = Color.white;
        gatcha_card.enabled = true;

        // 2. 셔플 연출
        if (audioSource != null && shuffleSound != null) 
        {
            audioSource.clip = shuffleSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        float startTime = Time.unscaledTime;
        while (Time.unscaledTime - startTime < shuffleDuration)
        {
            int rSuit = Random.Range(0, 4);
            int rRank = Random.Range(1, 14);
            var tempSprite = CardManager.instance.GetCardSprite((CardSuit)rSuit, rRank);
            if (gatcha_card != null) gatcha_card.sprite = tempSprite;
            
            yield return new WaitForSecondsRealtime(0.05f); // 0.05초마다 이미지 교체
        }
        Debug.Log($"[PlayerUI] 셔플 종료 (설정 시간: {shuffleDuration}s)");

        // 3. 최종 결과 노출
        if (audioSource != null) audioSource.Stop(); // 셔플 소리 중지
        if (audioSource != null && revealSound != null) audioSource.PlayOneShot(revealSound);

        var finalSprite = CardManager.instance.GetCardSprite(finalCard.suit, finalCard.rank);
        if (gatcha_card != null) 
        {
            gatcha_card.sprite = finalSprite;
            gatcha_card.transform.localScale = Vector3.one * 1.1f;
        }
        Debug.Log($"<color=green>[PlayerUI]</color> 결과 확정 및 노출 완료: {finalCard.suit} {finalCard.rank}");
        
        RefreshUI();
        Debug.Log("[PlayerUI] 연출 후 RefreshUI 완료");

        // 1.5초 뒤에 자동으로 이미지 꺼짐
        yield return new WaitForSecondsRealtime(1.5f);
        if (gatcha_card != null)
        {
            gatcha_card.gameObject.SetActive(false);
            gatcha_card.enabled = false;
            Debug.Log("[PlayerUI] 가챠 카드 비활성화 완료 (연출 종료)");
        }
        drawCoroutine = null;
    }

    [Header("메시지 설정")]
    public TMP_Text messageText; // TextMeshPro용으로 변경

    private const int maxHandSlots    = 5;
    private const int maxStorageSlots = 5;

    // ── 게임 오버 처리 ──────────────────────────
    public void ShowGameOver()
    {
        Debug.Log("<color=red>[PlayerUI]</color> ShowGameOver 호출됨!");
        if (isGameOver) return;
        isGameOver = true;
        StartCoroutine(GameOverSequence());
    }

    IEnumerator GameOverSequence()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            float elapsed = 0f;
            float duration = 2.0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(0f, 0.8f, elapsed / duration);
                fadeImage.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f; // 게임 정지
        Debug.Log("[PlayerUI] 게임 오버 화면 표시");
    }

    public void ExitGame()
    {
        Debug.Log("[PlayerUI] 게임 종료");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // ── 게임 재시작 ────────────────────────────────
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ── 사운드 헬퍼 ─────────────────────────────
    public void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
