using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class AspectRatioFixer
{
    static AspectRatioFixer()
    {
        // 에디터가 로드될 때 자동으로 실행되어 빌드 설정을 16:10으로 맞춥니다.
        Set16TenAspectRatio();
    }

    [MenuItem("Tools/Set Aspect Ratio to 16:10")]
    public static void Set16TenAspectRatio()
    {
        // Standalone (Windows/Mac/Linux) 빌드 설정
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 800;
        PlayerSettings.defaultIsFullScreen = false; // 창 모드 권장
        PlayerSettings.resizableWindow = true;

        Debug.Log("<color=cyan>[AspectRatioFixer]</color> 빌드 해상도가 1280x800 (16:10)으로 설정되었습니다!");
    }
}
