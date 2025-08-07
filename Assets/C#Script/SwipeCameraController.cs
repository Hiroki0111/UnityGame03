using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [Tooltip("追従対象のゾンビ")]
    public Transform target;

    [Tooltip("ゾンビの後ろに置く距離")]
    public float distanceBehind = 6f;

    [Tooltip("カメラの固定高さ")]
    public float fixedHeight = 3f;

    [Tooltip("カメラの追従速度（大きいほど素早く追従）")]
    public float followSpeed = 10f;

    void LateUpdate()
    {
        if (target == null) return;

        // ゾンビの向いている方向（Y軸の回転だけ考慮し、上下は無視）
        Vector3 forward = target.forward;
        forward.y = 0f;
        forward.Normalize();

        // カメラの位置はゾンビの位置のYをfixedHeightに固定して計算
        Vector3 desiredPosition = target.position - forward * distanceBehind;
        desiredPosition.y = fixedHeight;

        // 滑らかに移動
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // カメラはゾンビの頭あたりを注視（Y軸はfixedHeight基準に固定）
        Vector3 lookTarget = target.position;
        lookTarget.y = fixedHeight;

        transform.LookAt(lookTarget);
    }
}
