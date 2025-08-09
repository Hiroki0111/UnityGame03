using UnityEngine;

public class SwipeCameraController : MonoBehaviour
{
    [Tooltip("追従対象のゾンビ")]
    public Transform target;

    [Tooltip("ゾンビの後ろに置く距離")]
    public float distanceBehind = 6f;

    [Tooltip("ゾンビの上から見る高さ（基準）")]
    public float heightOffset = 3f;

    [Tooltip("上下の動きをどれだけ追従するか（0=追従なし, 1=完全追従）")]
    [Range(0f, 1f)] public float heightFollowFactor = 0.3f;

    [Tooltip("カメラの追従速度（位置）")]
    public float positionFollowSpeed = 5f;

    [Tooltip("カメラの回転追従速度")]
    public float rotationFollowSpeed = 10f;

    [Tooltip("スワイプの感度")]
    public float swipeSensitivity = 0.2f;

    private Vector3 currentVelocity;
    private float angleY = 0f;
    private Vector2 lastTouchPosition;
    private bool isDragging = false;

    void Update()
    {
        HandleSwipeInput();
    }

    void LateUpdate()
    {
        if (target == null) return;

        // スワイプで回転させた方向をもとにカメラ位置を決定
        Quaternion rotation = Quaternion.Euler(0f, angleY, 0f);
        Vector3 offset = rotation * Vector3.back * distanceBehind;

        // 高さを追従して調整
        float targetHeight = Mathf.Lerp(
            target.position.y + heightOffset,
            heightOffset,
            1f - heightFollowFactor
        );

        Vector3 desiredPosition = target.position + offset;
        desiredPosition.y = targetHeight;

        // カメラの位置をスムーズに追従
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / positionFollowSpeed);

        // カメラの回転をターゲット方向に滑らかに追従
        Quaternion targetRotation = Quaternion.LookRotation(target.position + Vector3.up * heightOffset - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationFollowSpeed * Time.deltaTime);
    }

    void HandleSwipeInput()
    {
        if (Input.touchSupported)
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

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
        else
        {
            // マウス操作（エディタやPC向け）
            if (Input.GetMouseButtonDown(0))
            {
                lastTouchPosition = Input.mousePosition;
                isDragging = true;
            }
            else if (Input.GetMouseButton(0) && isDragging)
            {
                Vector2 delta = (Vector2)Input.mousePosition - lastTouchPosition;
                lastTouchPosition = Input.mousePosition;
                angleY += delta.x * swipeSensitivity;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }
        }
    }
}
