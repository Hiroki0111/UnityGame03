using UnityEngine;

public class OverheadCameraController : MonoBehaviour
{
    [Header("追従対象のキャラ")]
    public Transform target;

    [Header("カメラ位置設定")]
    public float overheadHeight = 20f;        // 上空からの高さ
    public Vector3 horizontalOffset = Vector3.zero;  // 水平オフセット

    [Header("カメラ追従")]
    public float followSpeed = 5f;

    [Header("カメラ回転（インスペクターで設定可能）")]
    public Vector3 rotationEuler = new Vector3(90f, 0f, 0f);  // 真上ならX=90

    private Vector3 currentVelocity;

    void LateUpdate()
    {
        if (target == null) return;

        // 追従する目標位置
        Vector3 desiredPosition = target.position + horizontalOffset + Vector3.up * overheadHeight;

        // スムーズ追従
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / followSpeed);

        // インスペクターで設定した角度で回転
        transform.rotation = Quaternion.Euler(rotationEuler);
    }
}
