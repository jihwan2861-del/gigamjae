using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    // 게임 재시작 (Restart 버튼에 연결)
    public void RestartGame()
    {
        Debug.Log("게임 재시작!");
        Time.timeScale = 1f; // 멈췄던 시간을 다시 흐르게 함
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 게임 종료 (Exit 버튼에 연결)
    public void ExitGame()
    {
        Debug.Log("게임 종료!");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // 시작 화면에서 게임 시작 (Start 버튼용)
    public void StartGame()
    {
        Debug.Log("게임 시작!");
        Time.timeScale = 1f;
        gameObject.SetActive(false); // 버튼이 붙은 패널을 끔
    }
}
