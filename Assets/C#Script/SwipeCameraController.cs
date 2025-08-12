using UnityEngine;

public class SwipeCameraController : MonoBehaviour
{
    [Tooltip("追従対象のキャラ")]
    public Transform target;

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

        // rotationを基に後ろ向きのベクトル(Vector3.back)を回転させることで、
        // キャラの背面方向のオフセット位置を計算
        Vector3 offset = rotation * Vector3.back * distanceBehind;

        // キャラの高さとカメラの高さ追従度合いを計算
        float targetHeight = Mathf.Lerp(
            target.position.y + heightOffset, // キャラの高さ + オフセット
            heightOffset,                     // 基準高さ（シーンの地面など）
            1f - heightFollowFactor           // 追従度合いの逆数
        );

        // 目標カメラ位置はキャラの位置 + 後ろオフセット（x,z）＋高さ(y)
        Vector3 desiredPosition = target.position + offset;
        desiredPosition.y = targetHeight;

        // カメラの位置を滑らかに追従させる
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / positionFollowSpeed);

        // カメラはキャラの正面（キャラの位置＋高さ）を見るように回転を滑らかに追従
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
            // エディタやPC向けマウス操作
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

    // 外部から呼んで、カメラをキャラ背面に自動リセットする関数
    public void ResetCameraBehindTarget()
    {
        if (target == null) return;

        // キャラのY軸回転を取得（0～360度）
        float targetYAngle = target.eulerAngles.y;

        // キャラの向いているY軸の角度をそのまま使用
        // （Vector3.back に回転をかける仕様なので 180度は不要）
        angleY = target.eulerAngles.y;

        // ここからカメラの位置と回転を即座にリセットする処理を追加

        // 回転Quaternionを計算（angleYのみ）
        Quaternion rotation = Quaternion.Euler(0f, angleY, 0f);

        // カメラ位置のオフセットを計算
        Vector3 offset = rotation * Vector3.back * distanceBehind;

        // カメラの高さを計算（targetHeight）
        float targetHeight = Mathf.Lerp(
            target.position.y + heightOffset,
            heightOffset,
            1f - heightFollowFactor
        );

        // 目標カメラ位置
        Vector3 desiredPosition = target.position + offset;
        desiredPosition.y = targetHeight;

        // 位置を即座に変更
        transform.position = desiredPosition;

        // 回転を即座にキャラ方向に向ける
        Quaternion targetRotation = Quaternion.LookRotation(target.position + Vector3.up * heightOffset - transform.position);
        transform.rotation = targetRotation;

        // 速度ベクトルはリセットしておく（SmoothDamp用）
        currentVelocity = Vector3.zero;
    }

}
