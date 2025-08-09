using UnityEngine;

public class FollowCamera : MonoBehaviour
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

    private Vector3 currentVelocity;

    void LateUpdate()
    {
        if (target == null) return;

        // Y軸だけを使った方向ベクトル
        Vector3 forward = target.forward;
        forward.y = 0f;
        forward.Normalize();

        // 上下を少し追従する高さ計算
        float targetHeight = Mathf.Lerp(
            target.position.y + heightOffset,
            heightOffset,
            1f - heightFollowFactor
        );

        // 理想位置
        Vector3 desiredPosition = target.position - forward * distanceBehind;
        desiredPosition.y = targetHeight;

        // スムーズに位置を追従
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / positionFollowSpeed);

        // カメラの回転も滑らかに追従
        Quaternion targetRotation = Quaternion.LookRotation(target.position + Vector3.up * heightOffset - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationFollowSpeed * Time.deltaTime);
    }
}
