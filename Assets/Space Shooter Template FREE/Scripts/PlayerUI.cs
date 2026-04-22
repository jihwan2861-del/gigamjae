using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI instance;

    [Header("Card System UI (TMP)")]
    public TMP_Text handText;
    public TMP_Text storageText;
    public TMP_Text goldText;
    public TMP_Text rankText;
    public TMP_Text drawCostText;

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

    // ── 상태 ──────────────────────────────────────
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

        // 카드 이미지에 클릭 이벤트 추가
        SetupCardClickEvents();

        HideActionPanel();
    }

    void Update()
    {
        // B키 토글
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("[PlayerUI] B키 눌림! 현재 모드: " + (isInteractMode ? "인터랙션" : "게임"));
            if (isInteractMode) ExitInteractMode();
            else                EnterInteractMode();
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
        
        HideActionPanel();
    }

    // ── UI 새로고침 ────────────────────────────────
    void RefreshUI()
    {
        if (CardManager.instance == null) return;

        if (goldText     != null) goldText.text     = "GOLD " + CardManager.instance.gold;
        if (drawCostText != null) drawCostText.text = "DRAW "  + CardManager.instance.drawCost + "G";

        if (rankText != null)
        {
            var rank = CardManager.instance.GetCurrentHandRank();
            rankText.text = rank == PokerRank.None ? "" : "RANK " + rank.ToString();
        }

        // 핸드 카드 이미지
        for (int i = 0; i < maxHandSlots; i++)
        {
            if (handImages == null || i >= handImages.Length || handImages[i] == null) continue;
            bool has = CardManager.instance.hand != null && i < CardManager.instance.hand.Count;
            handImages[i].gameObject.SetActive(has);
            if (has)
            {
                var c = CardManager.instance.hand[i];
                var sp = CardManager.instance.GetCardSprite(c.suit, c.rank);
                if (sp != null) handImages[i].sprite = sp;
            }
        }
        if (handText != null) handText.text = "HAND";

        // 보관함 카드 이미지
        for (int i = 0; i < maxStorageSlots; i++)
        {
            if (storageImages == null || i >= storageImages.Length || storageImages[i] == null) continue;
            bool has = CardManager.instance.storage != null && i < CardManager.instance.storage.Count;
            storageImages[i].gameObject.SetActive(has);
            if (has)
            {
                var c = CardManager.instance.storage[i];
                var sp = CardManager.instance.GetCardSprite(c.suit, c.rank);
                if (sp != null) storageImages[i].sprite = sp;
            }
        }
        if (storageText != null) storageText.text = "STORAGE";

        // 중앙 카드 이미지 (추가됨!)
        if (centerHandArea != null && centerHandArea.activeSelf && centerHandImages != null)
        {
            for (int i = 0; i < centerHandImages.Length; i++)
            {
                if (centerHandImages[i] == null) continue;
                bool has = CardManager.instance.hand != null && i < CardManager.instance.hand.Count;
                centerHandImages[i].gameObject.SetActive(has);
                if (has)
                {
                    var c = CardManager.instance.hand[i];
                    var sp = CardManager.instance.GetCardSprite(c.suit, c.rank);
                    if (sp != null) centerHandImages[i].sprite = sp;
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
            
            // 위치와 크기 강제 초기화 (화면 중앙)
            var rect = actionPanel.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = Vector2.zero;
                rect.localScale = Vector3.one;
            }

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
    void OnDrawCard()
    {
        Debug.Log("[PlayerUI] 카드 뽑기 버튼 클릭!");
        if (CardManager.instance == null) return;
        if (CardManager.instance.gold < CardManager.instance.drawCost)
        { Debug.Log("[카드 관리] 골드 부족!"); return; }

        CardData newCard = CardManager.instance.DrawNewCard();
        if (newCard != null)
        {
            Debug.Log($"[PlayerUI] 애니메이션 시작: {newCard.suit} {newCard.rank}");
            StartCoroutine(DrawCardAnimation(newCard, CardManager.instance.hand.Count - 1));
        }
    }

    void OnDiscard()
    {
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

    void OnStore()
    {
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

    void OnMoveToHand()
    {
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
        Debug.Log($"[카드 연출] 시작! gatcha_card에서 연출 진행");
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

        // 2. 셔플 연출 (설정된 shuffleDuration 동안 진행)
        float elapsed = 0f;
        while (elapsed < shuffleDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            int rSuit = Random.Range(0, 4);
            int rRank = Random.Range(1, 14);
            gatcha_card.sprite = CardManager.instance.GetCardSprite((CardSuit)rSuit, rRank);
            yield return new WaitForSecondsRealtime(0.05f);
        }

        // 3. 최종 결과 노출 및 대기 (유저의 조작이 있을 때까지 활성화 유지)
        gatcha_card.sprite = CardManager.instance.GetCardSprite(finalCard.suit, finalCard.rank);
        gatcha_card.transform.localScale = Vector3.one * 1.1f;
        Debug.Log($"[카드 연출] 결과 확정: {finalCard.suit} {finalCard.rank}. 유저 액션 대기 중...");
        
        // RefreshUI를 호출하여 아래쪽 슬롯들에도 카드가 나타나게 함
        RefreshUI();
    }

    private const int maxHandSlots    = 5;
    private const int maxStorageSlots = 5;
}
