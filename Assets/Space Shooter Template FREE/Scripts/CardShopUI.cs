using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// B키로 게임 일시정지 + 카드 관리 UI 오픈.
/// 직렬화 배열 없이 자식 오브젝트 이름으로 자동 탐색하여 Inspector 에러를 완전 제거.
/// </summary>
public class CardShopUI : MonoBehaviour
{
    public static CardShopUI instance;

    [Header("단순 참조 (Inspector 연결)")]
    public GameObject shopPanel;
    public Button     drawButton;
    public TMP_Text   drawButtonText;
    public GameObject cardDetailPanel;
    public Image      cardDetailImage;
    public TMP_Text   cardDetailText;
    public Button     discardBtn;
    public Button     storeBtn;
    public Button     equipBtn;
    public Button     handBtn;
    public Button     closeButton;
    public TMP_Text   goldDisplayText;

    // 런타임에 이름으로 찾는 슬롯들 (직렬화 X → Inspector 에러 없음)
    private Button[] handSlots      = new Button[5];
    private Image[]  handSlotImages = new Image[5];
    private Button[] storageSlots      = new Button[5];
    private Image[]  storageSlotImages = new Image[5];

    // 상태
    private bool isOpen = false;
    private int  selectedHandIndex    = -1;
    private int  selectedStorageIndex = -1;
    private bool isNewCardMode = false;
    private CardData drawnCard = null;

    void Awake() { if (instance == null) instance = this; }

    void Start()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        if (cardDetailPanel != null) cardDetailPanel.SetActive(false);

        // 자식 오브젝트 이름으로 슬롯 자동 탐색
        FindSlotsByName();

        // 버튼 이벤트
        if (drawButton  != null) drawButton.onClick.AddListener(OnDrawClicked);
        if (closeButton != null) closeButton.onClick.AddListener(CloseShop);
        if (discardBtn  != null) discardBtn.onClick.AddListener(OnDiscard);
        if (storeBtn    != null) storeBtn.onClick.AddListener(OnStore);
        if (equipBtn    != null) equipBtn.onClick.AddListener(OnEquip);
        if (handBtn     != null) handBtn.onClick.AddListener(OnMoveToHand);
    }

    // ─── 자식 이름으로 슬롯 탐색 ────────────────────────────
    void FindSlotsByName()
    {
        for (int i = 0; i < 5; i++)
        {
            Transform handSlot = shopPanel != null
                ? shopPanel.transform.Find($"HandSlot_{i}")
                : transform.Find($"HandSlot_{i}");

            if (handSlot != null)
            {
                handSlots[i] = handSlot.GetComponent<Button>();
                var cardImg = handSlot.Find("CardImage");
                if (cardImg != null) handSlotImages[i] = cardImg.GetComponent<Image>();

                int idx = i;
                if (handSlots[i] != null)
                    handSlots[i].onClick.AddListener(() => OnHandSlotClicked(idx));
            }

            Transform storageSlot = shopPanel != null
                ? shopPanel.transform.Find($"StorageSlot_{i}")
                : transform.Find($"StorageSlot_{i}");

            if (storageSlot != null)
            {
                storageSlots[i] = storageSlot.GetComponent<Button>();
                var cardImg = storageSlot.Find("CardImage");
                if (cardImg != null) storageSlotImages[i] = cardImg.GetComponent<Image>();

                int idx = i;
                if (storageSlots[i] != null)
                    storageSlots[i].onClick.AddListener(() => OnStorageSlotClicked(idx));
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (isOpen) CloseShop();
            else        OpenShop();
        }
    }

    // ─── 열기 / 닫기 ─────────────────────────────────────────
    public void OpenShop()
    {
        isOpen = true;
        Time.timeScale = 0f;
        if (shopPanel != null) shopPanel.SetActive(true);
        if (cardDetailPanel != null) cardDetailPanel.SetActive(false);
        RefreshAllSlots();
    }

    public void CloseShop()
    {
        drawnCard = null;
        isNewCardMode = false;
        isOpen = false;
        Time.timeScale = 1f;
        if (shopPanel != null) shopPanel.SetActive(false);
        if (cardDetailPanel != null) cardDetailPanel.SetActive(false);
    }

    // ─── 슬롯 새로고침 ───────────────────────────────────────
    void RefreshAllSlots()
    {
        if (CardManager.instance == null) return;

        if (goldDisplayText != null)
            goldDisplayText.text = $"💰 GOLD: {CardManager.instance.gold}";
        if (drawButtonText != null)
            drawButtonText.text = $"카드 뽑기 ({CardManager.instance.drawCost}G)";

        for (int i = 0; i < 5; i++)
        {
            bool hasHand = CardManager.instance.hand != null && i < CardManager.instance.hand.Count;
            if (handSlotImages[i] != null)
            {
                handSlotImages[i].color = hasHand ? Color.white : new Color(1,1,1,0.15f);
                if (hasHand)
                {
                    var c = CardManager.instance.hand[i];
                    var sp = CardManager.instance.GetCardSprite(c.suit, c.rank);
                    if (sp != null) handSlotImages[i].sprite = sp;
                }
            }

            bool hasStorage = CardManager.instance.storage != null && i < CardManager.instance.storage.Count;
            if (storageSlotImages[i] != null)
            {
                storageSlotImages[i].color = hasStorage ? Color.white : new Color(1,1,1,0.15f);
                if (hasStorage)
                {
                    var c = CardManager.instance.storage[i];
                    var sp = CardManager.instance.GetCardSprite(c.suit, c.rank);
                    if (sp != null) storageSlotImages[i].sprite = sp;
                }
            }
        }

        if (cardDetailPanel != null) cardDetailPanel.SetActive(false);
    }

    // ─── 드로우 ──────────────────────────────────────────────
    void OnDrawClicked()
    {
        if (CardManager.instance == null) return;
        if (CardManager.instance.gold < CardManager.instance.drawCost)
        {
            Debug.Log("[카드 관리] 골드 부족!");
            return;
        }
        CardManager.instance.gold -= CardManager.instance.drawCost;
        drawnCard = CardManager.instance.GenerateRandomCard(1, 13);
        isNewCardMode = true;
        selectedHandIndex = selectedStorageIndex = -1;
        ShowCardDetail(drawnCard, isNew: true);
        RefreshAllSlots();
    }

    // ─── 슬롯 클릭 ───────────────────────────────────────────
    void OnHandSlotClicked(int index)
    {
        if (CardManager.instance?.hand == null) return;
        if (index >= CardManager.instance.hand.Count) return;
        drawnCard = null; isNewCardMode = false;
        selectedHandIndex = index; selectedStorageIndex = -1;
        ShowCardDetail(CardManager.instance.hand[index], isNew: false, fromStorage: false);
    }

    void OnStorageSlotClicked(int index)
    {
        if (CardManager.instance?.storage == null) return;
        if (index >= CardManager.instance.storage.Count) return;
        drawnCard = null; isNewCardMode = false;
        selectedStorageIndex = index; selectedHandIndex = -1;
        ShowCardDetail(CardManager.instance.storage[index], isNew: false, fromStorage: true);
    }

    // ─── 카드 상세 표시 ──────────────────────────────────────
    void ShowCardDetail(CardData card, bool isNew, bool fromStorage = false)
    {
        if (cardDetailPanel == null || card == null) return;
        cardDetailPanel.SetActive(true);

        if (cardDetailImage != null)
            cardDetailImage.sprite = CardManager.instance?.GetCardSprite(card.suit, card.rank);

        if (cardDetailText != null)
        {
            string r = card.rank switch { 1=>"A", 11=>"J", 12=>"Q", 13=>"K", _ => card.rank.ToString() };
            string s = card.suit switch {
                CardSuit.Heart   => "♥ 하트",
                CardSuit.Club    => "♣ 클로버",
                CardSuit.Diamond => "♦ 다이아",
                CardSuit.Spade   => "♠ 스페이드",
                _ => ""
            };
            cardDetailText.text = $"{s}  {r}";
        }

        if (equipBtn   != null) equipBtn.gameObject.SetActive(isNew);
        if (discardBtn != null) discardBtn.gameObject.SetActive(true);
        if (storeBtn   != null) storeBtn.gameObject.SetActive(!fromStorage);
        if (handBtn    != null) handBtn.gameObject.SetActive(fromStorage);
    }

    // ─── 버튼 액션 ───────────────────────────────────────────
    void OnEquip()
    {
        if (!isNewCardMode || drawnCard == null || CardManager.instance == null) return;
        if (CardManager.instance.hand.Count < CardManager.instance.maxHand)
        {
            CardManager.instance.hand.Add(drawnCard);
            CardManager.instance.PrintHandStatus("[카드 장착]");
        }
        else Debug.Log("[카드 관리] 핸드가 꽉 찼습니다!");
        drawnCard = null; isNewCardMode = false;
        RefreshAllSlots();
    }

    void OnDiscard()
    {
        if (CardManager.instance == null) return;
        if (isNewCardMode && drawnCard != null)
        {
            Debug.Log($"[카드 버림] {drawnCard}");
            drawnCard = null; isNewCardMode = false;
        }
        else if (selectedHandIndex >= 0 && selectedHandIndex < CardManager.instance.hand.Count)
        {
            Debug.Log($"[카드 버림] {CardManager.instance.hand[selectedHandIndex]}");
            CardManager.instance.hand.RemoveAt(selectedHandIndex);
            CardManager.instance.PrintHandStatus("[버린 후 핸드]");
            selectedHandIndex = -1;
        }
        else if (selectedStorageIndex >= 0 && selectedStorageIndex < CardManager.instance.storage.Count)
        {
            Debug.Log($"[보관함 버림] {CardManager.instance.storage[selectedStorageIndex]}");
            CardManager.instance.storage.RemoveAt(selectedStorageIndex);
            selectedStorageIndex = -1;
        }
        RefreshAllSlots();
    }

    void OnStore()
    {
        if (CardManager.instance == null) return;
        if (CardManager.instance.storage.Count >= CardManager.instance.maxStorage)
        { Debug.Log("[카드 관리] 보관함이 가득 찼습니다!"); return; }

        if (isNewCardMode && drawnCard != null)
        {
            CardManager.instance.storage.Add(drawnCard);
            drawnCard = null; isNewCardMode = false;
        }
        else if (selectedHandIndex >= 0 && selectedHandIndex < CardManager.instance.hand.Count)
        {
            var c = CardManager.instance.hand[selectedHandIndex];
            CardManager.instance.storage.Add(c);
            CardManager.instance.hand.RemoveAt(selectedHandIndex);
            CardManager.instance.PrintHandStatus("[보관 후 핸드]");
            selectedHandIndex = -1;
        }
        RefreshAllSlots();
    }

    void OnMoveToHand()
    {
        if (CardManager.instance == null) return;
        if (CardManager.instance.hand.Count >= CardManager.instance.maxHand)
        { Debug.Log("[카드 관리] 핸드가 가득 찼습니다!"); return; }
        if (selectedStorageIndex < 0 || selectedStorageIndex >= CardManager.instance.storage.Count) return;

        var c = CardManager.instance.storage[selectedStorageIndex];
        CardManager.instance.hand.Add(c);
        CardManager.instance.storage.RemoveAt(selectedStorageIndex);
        CardManager.instance.PrintHandStatus("[핸드로 이동]");
        selectedStorageIndex = -1;
        RefreshAllSlots();
    }
}
