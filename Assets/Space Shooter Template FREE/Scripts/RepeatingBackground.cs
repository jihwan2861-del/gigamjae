using UnityEngine;

/// <summary>
/// 배경 오브젝트의 위치를 자동으로 교정하고 무한 루프를 구현하는 스크립트입니다.
/// LateUpdate를 사용하여 프레임 간 이동 오차를 방지합니다.
/// </summary>
public class RepeatingBackground : MonoBehaviour
{
    private float spriteHeight;
    private Camera mainCam;
    private Transform otherBackground;

    private void Start()
    {
        mainCam = Camera.main;
        
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            spriteHeight = sr.bounds.size.y;
        }

        // 같은 부모 아래에 있는 다른 배경 오브젝트를 찾습니다.
        if (transform.parent != null)
        {
            foreach (Transform child in transform.parent)
            {
                if (child != transform && child.GetComponent<RepeatingBackground>())
                {
                    otherBackground = child;
                    break;
                }
            }
        }
    }

    // 모든 이동 연산이 끝난 후 실행하여 틈 발생을 방지합니다.
    private void LateUpdate()
    {
        if (mainCam == null || otherBackground == null) return;

        // 카메라의 하단 세계 좌표 계산
        float camBottom = mainCam.transform.position.y - mainCam.orthographicSize;

        // 배경의 위쪽 끝이 카메라 아래쪽 경계선보다 낮아지면 재배치
        if (transform.position.y + spriteHeight / 2f < camBottom)
        {
            // 다른 배경 타일의 바로 위쪽으로 정확히 스냅
            // 미세한 틈을 막기 위해 0.02f 정도 겹치게 배치합니다.
            Vector3 targetPos = otherBackground.position;
            targetPos.y += spriteHeight - 0.02f;
            transform.position = targetPos;
        }
    }
}
