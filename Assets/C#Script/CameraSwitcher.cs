using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [Tooltip("背後カメラ（スワイプ対応）")]
    public Camera backCamera;

    [Tooltip("上空カメラ（固定俯瞰）")]
    public Camera overheadCamera;

    private Camera activeCamera;

    void Start()
    {
        activeCamera = backCamera;
        backCamera.enabled = true;
        overheadCamera.enabled = false;
    }

    public void SwitchCamera()
    {
        if (activeCamera == backCamera)
        {
            backCamera.enabled = false;
            overheadCamera.enabled = true;
            activeCamera = overheadCamera;
        }
        else
        {
            overheadCamera.enabled = false;
            backCamera.enabled = true;
            activeCamera = backCamera;
        }
    }
}
