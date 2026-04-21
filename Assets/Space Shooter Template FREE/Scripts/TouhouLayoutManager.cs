using UnityEngine;

[ExecuteAlways]
public class TouhouLayoutManager : MonoBehaviour
{
    public static TouhouLayoutManager instance;

    [Header("Layout Settings")]
    [Range(0.1f, 0.9f)]
    public float gameAreaWidth = 0.7f; // 게임 화면이 차지할 가로 비율 (0.7 = 70%)
    
    private Camera mainCamera;

    private void Awake()
    {
        if (instance == null) instance = this;
        mainCamera = Camera.main;
    }

    private void Start()
    {
        ApplyLayout();
    }

    private void Update()
    {
        // 에디터에서도 실시간으로 확인 가능하도록 함
        if (!Application.isPlaying)
        {
            ApplyLayout();
        }
    }

    public void ApplyLayout()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        // 1. 게임 화면 렌더링 구역을 왼쪽 70%로 고정
        // 이렇게 하면 오른쪽 30% 구역은 카메라가 아예 건드리지 않는 '순수 UI 전용' 공간이 됩니다.
        mainCamera.rect = new Rect(0, 0, gameAreaWidth, 1f);
        
        // 2. 배경색을 검정색으로 지정하여 사이드바 영역과 분리감을 줍니다.
        mainCamera.backgroundColor = Color.black;
        mainCamera.clearFlags = CameraClearFlags.SolidColor;

        // 3. 이동 범위 재계산 트리거
        if (Application.isPlaying && PlayerMoving.instance != null)
        {
            PlayerMoving.instance.ResizeBorders();
        }
    }

    // 사이드바 영역의 시작 X 좌표 (Viewport 0~1 기준)
    public float GetSidebarX()
    {
        return gameAreaWidth;
    }
}
