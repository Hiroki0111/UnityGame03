using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    public enum CameraMode { Back, Overhead }

    [Header("基本設定")]
    public Transform target;
    public FixedJoystick joystick;

    [Header("バックカメラ設定")]
    public float backDistance = 6f;
    public float backHeight = 3f;
    [Range(0f, 1f)] public float backHeightFollowFactor = 0.3f;
    public float backPositionSpeed = 5f;
    public float backRotationSpeed = 10f;
    public float backTilt = 10f;
    public float swipeSensitivity = 0.2f;

    [Header("上空カメラ設定")]
    public Vector3 overheadOffset = new Vector3(0, 10, -6);
    public float overheadRotationX = 45f;
    public float overheadFollowSpeed = 5f;
    public float overheadRotationLerpSpeed = 5f;
    public float overheadForwardFollowFactor = 0.5f;

    [Header("UI")]
    public Button switchCameraButton;

    [Header("モード")]
    public CameraMode mode = CameraMode.Back;

    private Vector3 currentVelocity;
    private float angleY = 0f;
    private bool isDragging = false;

    void Start()
    {
        if (switchCameraButton != null)
            switchCameraButton.onClick.AddListener(ToggleCameraMode);

        if (target != null)
            ResetCamera(); // 初期角度をキャラ向きに
    }

    void Update()
    {
        HandleInput();
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (mode == CameraMode.Back)
            UpdateBackCamera();
        else
            UpdateOverheadCameraSmooth();
    }

    void UpdateBackCamera()
    {
        Quaternion rotation = Quaternion.Euler(backTilt, angleY, 0f);
        Vector3 offset = rotation * Vector3.back * backDistance;

        float targetHeight = Mathf.Lerp(target.position.y + backHeight, backHeight, 1f - backHeightFollowFactor);
        Vector3 desiredPos = target.position + offset;
        desiredPos.y = targetHeight;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref currentVelocity, 1f / backPositionSpeed);

        Quaternion targetRot = Quaternion.LookRotation(target.position + Vector3.up * backHeight - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, backRotationSpeed * Time.deltaTime);
    }

    void UpdateOverheadCameraSmooth()
    {
        Vector3 forwardOffset = target.forward * overheadOffset.z * overheadForwardFollowFactor;
        Vector3 desiredPos = target.position + overheadOffset + forwardOffset;

        Quaternion desiredRot = Quaternion.Euler(overheadRotationX, angleY, 0f);

        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * overheadFollowSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, Time.deltaTime * overheadRotationLerpSpeed);
    }

    // ===== スワイプ入力処理 =====
    void HandleInput()
    {
        if (!Input.touchSupported) return;

        int joystickFingerId = joystick != null ? joystick.CurrentFingerId : -1;

        foreach (Touch touch in Input.touches)
        {
            if (touch.fingerId == joystickFingerId) continue; // ジョイスティックは無視

            if (touch.phase == TouchPhase.Began)
            {
                isDragging = true;
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                angleY += touch.deltaPosition.x * swipeSensitivity; // ← スワイプで左右回転
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
    }

    // ===== カメラリセット =====
    public void ResetCamera()
    {
        if (target == null) return;

        angleY = target.eulerAngles.y;
        currentVelocity = Vector3.zero;

        if (mode == CameraMode.Back)
            UpdateBackCamera();
        else
            UpdateOverheadCameraSmooth();
    }

    public void SwitchMode(CameraMode newMode)
    {
        mode = newMode;
        ResetCamera();
    }

    public void ToggleCameraMode()
    {
        mode = mode == CameraMode.Back ? CameraMode.Overhead : CameraMode.Back;
        ResetCamera();
    }
}
