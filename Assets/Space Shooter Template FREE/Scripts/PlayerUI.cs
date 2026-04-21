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
    public GameObject interactModeIndicator; // "일시정지 중 - 카드 관리" 표시 오브젝트

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

        if (interactModeIndicator != null) interactModeIndicator.SetActive(true);
        if (drawCardButton        != null) drawCardButton.gameObject.SetActive(true);
        HideActionPanel();
    }

    void ExitInteractMode()
    {
        isInteractMode = false;
        Time.timeScale = 1f;
        selectedHandIndex = selectedStorageIndex = -1;

        if (interactModeIndicator != null) interactModeIndicator.SetActive(false);
        if (drawCardButton        != null) drawCardButton.gameObject.SetActive(false);
        HideActionPanel();
    }

    // ── UI 새로고침 ────────────────────────────────
    void RefreshUI()
    {
        if (CardManager.instance == null) return;

        if (goldText     != null) goldText.text     = "GOLD: " + CardManager.instance.gold;
        if (drawCostText != null) drawCostText.text = "DRAW: "  + CardManager.instance.drawCost + "G";

        if (rankText != null)
        {
            var rank = CardManager.instance.GetCurrentHandRank();
            rankText.text = rank == PokerRank.None ? "" : "RANK: " + rank.ToString();
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
    }

    // ── 카드 클릭 이벤트 설정 ──────────────────────
    void SetupCardClickEvents()
    {
        if (handImages != null)
        {
            for (int i = 0; i < handImages.Length; i++)
            {
                if (handImages[i] == null) continue;
                int idx = i;
                var trigger = handImages[i].gameObject.GetComponent<EventTrigger>()
                           ?? handImages[i].gameObject.AddComponent<EventTrigger>();
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
                int idx = i;
                var trigger = storageImages[i].gameObject.GetComponent<EventTrigger>()
                           ?? storageImages[i].gameObject.AddComponent<EventTrigger>();
                var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
                entry.callback.AddListener(_ => OnStorageCardClicked(idx));
                trigger.triggers.Add(entry);
            }
        }
    }

    // ── 카드 클릭 처리 ────────────────────────────
    void OnHandCardClicked(int index)
    {
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
        if (actionPanel != null) actionPanel.SetActive(true);

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
        if (CardManager.instance == null) return;
        if (CardManager.instance.gold < CardManager.instance.drawCost)
        { Debug.Log("[카드 관리] 골드 부족!"); return; }
        if (CardManager.instance.hand.Count >= CardManager.instance.maxHand)
        { Debug.Log("[카드 관리] 핸드가 가득 찼습니다!"); return; }

        CardManager.instance.DrawNewCard();
        RefreshUI();
    }

    void OnDiscard()
    {
        if (CardManager.instance == null) return;
        if (selectedHandIndex >= 0 && selectedHandIndex < CardManager.instance.hand.Count)
        {
            CardManager.instance.hand.RemoveAt(selectedHandIndex);
            CardManager.instance.PrintHandStatus("[버린 후]");
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
        HideActionPanel();
        RefreshUI();
    }

    private const int maxHandSlots    = 5;
    private const int maxStorageSlots = 5;
}
