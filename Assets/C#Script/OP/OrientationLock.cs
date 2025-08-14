using UnityEngine;

public class OrientationLock : MonoBehaviour
{
    void Awake()
    {
        // 画面向きを横に固定
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        // 自動回転を無効化（安全のため）
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
    }
}
