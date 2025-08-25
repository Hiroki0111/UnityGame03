using UnityEngine;

public class SwipeCameraController : MonoBehaviour
{
    public enum CameraMode
    {
        SwipeFollow,
        Overhead
    }

    [Header("基本設定")]
    public Transform target;          // 追従対象プレイヤー
    public FixedJoystick joystick;    // プレイヤー操作用ジョイスティック

    [Header("スワイプ追従カメラ設定")]
    public float distanceBehind = 6f;
    public float heightOffset = 3f;
    [Range(0f, 1f)] public float heightFollowFactor = 0.3f;
    public float positionFollowSpeed = 5f;
    public float rotationFollowSpeed = 10f;
    public float swipeSensitivity = 0.2f;

    [Header("上空カメラ設定")]
    public Vector3 overheadOffset = new Vector3(0, 15, 0);
    public Vector3 overheadRotationEuler = new Vector3(90, 0, 0);

    [Header("カメラ切替")]
    public CameraMode mode = CameraMode.SwipeFollow;

    private Vector3 currentVelocity;
    private float angleY = 0f;
    private bool isDragging = false;

    void Update()
    {
        if (mode == CameraMode.SwipeFollow)
            HandleSwipeInput();
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (mode == CameraMode.SwipeFollow)
            UpdateSwipeCamera();
        else if (mode == CameraMode.Overhead)
            UpdateOverheadCamera();
    }

    void UpdateSwipeCamera()
    {
        Quaternion rotation = Quaternion.Euler(0f, angleY, 0f);
        Vector3 offset = rotation * Vector3.back * distanceBehind;

        float targetHeight = Mathf.Lerp(target.position.y + heightOffset, heightOffset, 1f - heightFollowFactor);

        Vector3 desiredPosition = target.position + offset;
        desiredPosition.y = targetHeight;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / positionFollowSpeed);

        Quaternion targetRotation = Quaternion.LookRotation(target.position + Vector3.up * heightOffset - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationFollowSpeed * Time.deltaTime);
    }

    void UpdateOverheadCamera()
    {
        transform.position = target.position + overheadOffset;
        transform.rotation = Quaternion.Euler(overheadRotationEuler);
    }

    void HandleSwipeInput()
    {
        if (!Input.touchSupported) return;

        int joystickFingerId = joystick != null ? joystick.CurrentFingerId : -1;

        foreach (Touch touch in Input.touches)
        {
            if (touch.fingerId == joystickFingerId) continue;

            if (touch.phase == TouchPhase.Began) isDragging = true;
            else if (touch.phase == TouchPhase.Moved && isDragging) angleY += touch.deltaPosition.x * swipeSensitivity;
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) isDragging = false;
        }
    }

    public void ResetCamera()
    {
        if (target == null) return;

        if (mode == CameraMode.SwipeFollow)
        {
            angleY = target.eulerAngles.y;
            currentVelocity = Vector3.zero;
            UpdateSwipeCamera();
        }
        else if (mode == CameraMode.Overhead)
        {
            UpdateOverheadCamera();
        }
    }

    // カメラ切替ボタンなどで呼ぶ
    public void SwitchMode(CameraMode newMode)
    {
        mode = newMode;
        ResetCamera();
    }
}
