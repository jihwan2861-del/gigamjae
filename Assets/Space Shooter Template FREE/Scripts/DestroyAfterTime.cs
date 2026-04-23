using UnityEngine;

public class DestroyAfterTime : MonoBehaviour {

    public float timeToDestroy = 1.0f; // 삭제될 시간 (초)

    void Start () {
        // 지정된 시간 뒤에 오브젝트 삭제
        Destroy(gameObject, timeToDestroy);
    }
}
