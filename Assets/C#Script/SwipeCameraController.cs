using UnityEngine;

public class SwipeCameraController : MonoBehaviour
{
    [Tooltip("追従対象のキャラ")]
    public Transform target;

    [Tooltip("ジョイスティック（FixedJoystick）")]
    public FixedJoystick joystick; // ← インスペクターでアサイン

    [Tooltip("キャラの背中側からどれだけ離れるかの距離")]
    public float distanceBehind = 6f;

    [Tooltip("キャラの上方向の高さオフセット")]
    public float heightOffset = 3f;

    [Tooltip("上下の動きをどれだけ追従するか（0=追従なし, 1=完全追従）")]
    [Range(0f, 1f)] public float heightFollowFactor = 0.3f;

    [Tooltip("カメラの位置追従速度")]
    public float positionFollowSpeed = 5f;

    [Tooltip("カメラの回転追従速度")]
    public float rotationFollowSpeed = 10f;

    [Tooltip("スワイプの感度")]
    public float swipeSensitivity = 0.2f;

    private Vector3 currentVelocity;   // SmoothDamp用の速度参照
    private float angleY = 0f;         // 水平方向の回転角度（度数）
    private Vector2 lastTouchPosition; // 最後にタッチした位置
    private bool isDragging = false;   // スワイプ中かどうか

    void Update()
    {
        HandleSwipeInput();
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 水平方向の回転をQuaternionに変換（y軸回転のみ）
        Quaternion rotation = Quaternion.Euler(0f, angleY, 0f);

        // キャラ背後のオフセット計算
        Vector3 offset = rotation * Vector3.back * distanceBehind;

        // 高さ追従計算
        float targetHeight = Mathf.Lerp(
            target.position.y + heightOffset,
            heightOffset,
            1f - heightFollowFactor
        );

        // 目標カメラ位置
        Vector3 desiredPosition = target.position + offset;
        desiredPosition.y = targetHeight;

        // 位置追従
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref currentVelocity,
            1f / positionFollowSpeed
        );

        // 回転追従
        Quaternion targetRotation = Quaternion.LookRotation(
            target.position + Vector3.up * heightOffset - transform.position
        );
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationFollowSpeed * Time.deltaTime
        );
    }

    void HandleSwipeInput()
    {
        if (!Input.touchSupported) return; // スマホ専用

        int joystickFingerId = -1;
        if (joystick != null)
            joystickFingerId = joystick.CurrentFingerId; // ← FixedJoystickに追加したプロパティ

        foreach (Touch touch in Input.touches)
        {
            // ジョイスティック用の指はスワイプに使わない
            if (touch.fingerId == joystickFingerId)
                continue;

            if (touch.phase == TouchPhase.Began)
            {
                lastTouchPosition = touch.position;
                isDragging = true;
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                Vector2 delta = touch.deltaPosition;
                angleY += delta.x * swipeSensitivity;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
    }

    public void ResetCameraBehindTarget()
    {
        if (target == null) return;

        // キャラの角度を反映
        angleY = target.eulerAngles.y;

        // 回転Quaternion
        Quaternion rotation = Quaternion.Euler(0f, angleY, 0f);

        // カメラ位置オフセット
        Vector3 offset = rotation * Vector3.back * distanceBehind;

        // 高さ
        float targetHeight = Mathf.Lerp(
            target.position.y + heightOffset,
            heightOffset,
            1f - heightFollowFactor
        );

        // 即座に位置変更
        Vector3 desiredPosition = target.position + offset;
        desiredPosition.y = targetHeight;
        transform.position = desiredPosition;

        // 即座に回転変更
        Quaternion targetRotation = Quaternion.LookRotation(
            target.position + Vector3.up * heightOffset - transform.position
        );
        transform.rotation = targetRotation;

        currentVelocity = Vector3.zero;
    }
}
