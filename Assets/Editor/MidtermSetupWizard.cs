using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MidtermSetupWizard : EditorWindow
{
    [MenuItem("Tools/Midterm Project/Setup Everything")]
    public static void SetupEverything()
    {
        SetupLayoutManager();
        SetupCardManager();
        SetupUISystem();
        CreateCardShopUI();
        Debug.Log("모든 UI와 매니저 설정이 완료되었습니다!");
    }

    [MenuItem("Tools/Midterm Project/Create Card Shop UI")]
    public static void CreateCardShopUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject cGo = new GameObject("Canvas");
            canvas = cGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cGo.AddComponent<CanvasScaler>();
            cGo.AddComponent<GraphicRaycaster>();
        }

        var old = GameObject.Find("CardShopPanel");
        if (old != null) DestroyImmediate(old);

        // ─── 루트 패널 ───────────────────────────
        GameObject panel = MakePanel(canvas.transform, "CardShopPanel",
            Vector2.zero, Vector2.one, new Color(0, 0, 0, 0.88f));
        panel.SetActive(false);

        CardShopUI shopUI = panel.AddComponent<CardShopUI>();
        shopUI.shopPanel = panel;

        // 제목
        MakeTMP(panel.transform, "TitleText", "🃏  카드 관리  (B키: 닫기)",
            new Vector2(0.5f, 0.93f), new Vector2(600, 50), 32);

        // 골드 표시
        shopUI.goldDisplayText = MakeTMP(panel.transform, "GoldText", "💰 GOLD: 0",
            new Vector2(0.5f, 0.86f), new Vector2(300, 40), 26);

        // 카드 뽑기 버튼
        GameObject drawBtnGo = MakeButton(panel.transform, "DrawCardButton",
            new Vector2(0.5f, 0.79f), new Vector2(220, 50), "카드 뽑기  (10G)",
            new Color(0.2f, 0.6f, 1f));
        shopUI.drawButton = drawBtnGo.GetComponent<Button>();
        shopUI.drawButtonText = drawBtnGo.GetComponentInChildren<TMP_Text>();

        // HAND 라벨
        MakeTMP(panel.transform, "HandLabel", "── HAND ──",
            new Vector2(0.5f, 0.70f), new Vector2(400, 35), 22);

        // 핸드 슬롯 5개 (이름으로 생성 → CardShopUI가 Start()에서 자동 탐색)
        for (int i = 0; i < 5; i++)
        {
            float xPos = 0.2f + i * 0.15f;
            MakeCardSlot(panel.transform, $"HandSlot_{i}", new Vector2(xPos, 0.60f));
        }

        // STORAGE 라벨
        MakeTMP(panel.transform, "StorageLabel", "── STORAGE ──",
            new Vector2(0.5f, 0.50f), new Vector2(400, 35), 22);

        // 보관함 슬롯 5개 (이름으로 생성 → CardShopUI가 Start()에서 자동 탐색)
        for (int i = 0; i < 5; i++)
        {
            float xPos = 0.2f + i * 0.15f;
            MakeCardSlot(panel.transform, $"StorageSlot_{i}", new Vector2(xPos, 0.40f));
        }

        // ─── 카드 상세 패널 ──────────────────────
        GameObject detailPanel = MakePanel(panel.transform, "CardDetailPanel",
            new Vector2(0.25f, 0.10f), new Vector2(0.75f, 0.35f),
            new Color(0.08f, 0.08f, 0.18f, 0.97f));
        detailPanel.SetActive(false);
        shopUI.cardDetailPanel = detailPanel;

        // 확대 카드 이미지
        GameObject detailImgGo = new GameObject("CardDetailImage");
        detailImgGo.transform.SetParent(detailPanel.transform, false);
        var di = detailImgGo.AddComponent<RectTransform>();
        di.anchorMin = new Vector2(0.03f, 0.15f);
        di.anchorMax = new Vector2(0.32f, 0.92f);
        di.offsetMin = di.offsetMax = Vector2.zero;
        shopUI.cardDetailImage = detailImgGo.AddComponent<Image>();
        shopUI.cardDetailImage.preserveAspect = true;

        // 카드 이름 텍스트
        shopUI.cardDetailText = MakeTMP(detailPanel.transform, "CardDetailText", "카드 이름",
            new Vector2(0.65f, 0.75f), new Vector2(250, 45), 24);

        // 액션 버튼들
        shopUI.equipBtn   = MakeButton(detailPanel.transform, "EquipBtn",
            new Vector2(0.45f, 0.35f), new Vector2(120, 40), "장착",
            new Color(0.2f, 0.75f, 0.3f)).GetComponent<Button>();

        shopUI.storeBtn   = MakeButton(detailPanel.transform, "StoreBtn",
            new Vector2(0.62f, 0.35f), new Vector2(120, 40), "보관함으로",
            new Color(0.8f, 0.6f, 0.1f)).GetComponent<Button>();

        shopUI.handBtn    = MakeButton(detailPanel.transform, "HandBtn",
            new Vector2(0.45f, 0.35f), new Vector2(120, 40), "핸드로",
            new Color(0.2f, 0.5f, 0.9f)).GetComponent<Button>();

        shopUI.discardBtn = MakeButton(detailPanel.transform, "DiscardBtn",
            new Vector2(0.80f, 0.35f), new Vector2(120, 40), "버리기",
            new Color(0.85f, 0.2f, 0.2f)).GetComponent<Button>();

        // 닫기 버튼
        GameObject closeBtnGo = MakeButton(panel.transform, "CloseButton",
            new Vector2(0.5f, 0.07f), new Vector2(180, 48), "닫기  (B)",
            new Color(0.45f, 0.45f, 0.45f));
        shopUI.closeButton = closeBtnGo.GetComponent<Button>();

        // 씬 변경사항 저장 마킹 (이게 없으면 씬 저장 시 레퍼런스가 날아갈 수 있음)
        EditorUtility.SetDirty(panel);
        EditorUtility.SetDirty(shopUI);
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[CardShopUI] 자동 생성 완료! Ctrl+S로 씬 저장 후 B키를 눌러 카드 관리 화면을 열어보세요.");
    }

    // ──────────────────────────────────────────────
    //  기존 SetupEverything 헬퍼들
    // ──────────────────────────────────────────────

    private static void SetupLayoutManager()
    {
        if (FindObjectOfType<TouhouLayoutManager>() == null)
        {
            new GameObject("LayoutManager").AddComponent<TouhouLayoutManager>();
        }
    }

    private static void SetupCardManager()
    {
        if (FindObjectOfType<CardManager>() == null)
        {
            new GameObject("CardManager").AddComponent<CardManager>();
        }
    }

    private static void SetupUISystem()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGo = new GameObject("Canvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
        }

        GameObject sidebar = GameObject.Find("SidebarPanel");
        if (sidebar == null)
        {
            sidebar = new GameObject("SidebarPanel");
            sidebar.transform.SetParent(canvas.transform, false);
            var rect = sidebar.AddComponent<RectTransform>();
            sidebar.AddComponent<Image>().color = new Color(0, 0, 0, 0.7f);
            rect.anchorMin = new Vector2(0.7f, 0);
            rect.anchorMax = new Vector2(1.0f, 1);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        var playerUI = FindObjectOfType<PlayerUI>();
        if (playerUI == null)
        {
            GameObject uiGo = new GameObject("PlayerUI");
            uiGo.transform.SetParent(sidebar.transform, false);
            playerUI = uiGo.AddComponent<PlayerUI>();
            var rect = uiGo.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        SetupUITexts(playerUI, sidebar.transform);
        SetupUIImageSlots(playerUI, sidebar.transform);
    }

    private static void SetupUITexts(PlayerUI ui, Transform parent)
    {
        ui.goldText     = CreateText(parent, "GoldText",     new Vector2(0.5f, 0.9f),  "GOLD: 0");
        ui.rankText     = CreateText(parent, "RankText",     new Vector2(0.5f, 0.8f),  "RANK: NONE");
        ui.handText     = CreateText(parent, "HandText",     new Vector2(0.5f, 0.65f), "HAND");
        ui.storageText  = CreateText(parent, "StorageText",  new Vector2(0.5f, 0.35f), "STORAGE");
        ui.drawCostText = CreateText(parent, "DrawCostText", new Vector2(0.5f, 0.1f),  "COST: 10");
    }

    private static void SetupUIImageSlots(PlayerUI ui, Transform parent)
    {
        ui.handImages = new Image[5];
        for (int i = 0; i < 5; i++)
            ui.handImages[i] = CreateImage(parent, $"HandImg_{i}", new Vector2(0.2f + i * 0.15f, 0.55f));

        ui.storageImages = new Image[5];
        for (int i = 0; i < 5; i++)
            ui.storageImages[i] = CreateImage(parent, $"StorageImg_{i}", new Vector2(0.2f + i * 0.15f, 0.25f));
    }

    // ──────────────────────────────────────────────
    //  공용 헬퍼
    // ──────────────────────────────────────────────

    private static GameObject MakePanel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = anchorMin; r.anchorMax = anchorMax;
        r.offsetMin = r.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = color;
        return go;
    }

    private static TMP_Text MakeTMP(Transform parent, string name, string text,
        Vector2 anchor, Vector2 size, int fs)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fs;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = anchor;
        r.sizeDelta = size; r.anchoredPosition = Vector2.zero;
        return tmp;
    }

    private static GameObject MakeButton(Transform parent, string name,
        Vector2 anchor, Vector2 size, string label, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = r.anchorMax = anchor;
        r.sizeDelta = size; r.anchoredPosition = Vector2.zero;
        go.AddComponent<Image>().color = color;
        go.AddComponent<Button>();

        GameObject lGo = new GameObject("Label");
        lGo.transform.SetParent(go.transform, false);
        var tmp = lGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 18;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        var lr = lGo.GetComponent<RectTransform>();
        lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
        lr.offsetMin = lr.offsetMax = Vector2.zero;
        return go;
    }

    private static GameObject MakeCardSlot(Transform parent, string name, Vector2 anchor)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = r.anchorMax = anchor;
        r.sizeDelta = new Vector2(60, 88);
        r.anchoredPosition = Vector2.zero;
        go.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.35f);
        go.AddComponent<Button>();

        GameObject imgGo = new GameObject("CardImage");
        imgGo.transform.SetParent(go.transform, false);
        var ir = imgGo.AddComponent<RectTransform>();
        ir.anchorMin = Vector2.zero; ir.anchorMax = Vector2.one;
        ir.offsetMin = new Vector2(3, 3); ir.offsetMax = new Vector2(-3, -3);
        var img = imgGo.AddComponent<Image>();
        img.preserveAspect = true;
        img.color = new Color(1, 1, 1, 0.2f);
        return go;
    }

    private static TMP_Text CreateText(Transform parent, string name, Vector2 pos, string content)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<TextMeshProUGUI>();
        text.text = content; text.fontSize = 24;
        text.alignment = TextAlignmentOptions.Center;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = pos;
        rect.sizeDelta = new Vector2(250, 50);
        return text;
    }

    private static Image CreateImage(Transform parent, string name, Vector2 pos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.preserveAspect = true;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = pos;
        rect.sizeDelta = new Vector2(40, 60);
        return img;
    }
}
