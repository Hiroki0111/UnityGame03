using UnityEngine;
using UnityEngine.UI;

public class PlayerZombieController : MonoBehaviour
{
    public float moveSpeed = 0.00002f;    // 超スロー歩行速度（1秒で2cm進むイメージ）
    public float dashSpeed = 0.1f;     // 遅めのダッシュ速度（1秒で10cm進むイメージ）
    public FixedJoystick joystick;     // ジョイスティックUI
    public Animator animator;          // アニメーター
    public Button dashButton;          // ダッシュボタン
    public Rigidbody rb;               // Rigidbody（物理演算用）

    private bool isDashing = false;
    private float dashDuration = 5f;
    private float dashTimer = 0f;

    void Start()
    {
        if (dashButton != null)
            dashButton.onClick.AddListener(OnDashButtonPressed);
    }

    void FixedUpdate()
    {
        Vector3 direction = new Vector3(joystick.Horizontal, 0, joystick.Vertical);

        if (isDashing)
        {
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
                dashTimer = 0f;
            }
        }

        if (direction.magnitude > 0.1f)
        {
            float speed = isDashing ? dashSpeed : moveSpeed;
            Vector3 move = direction.normalized * speed;
            rb.MovePosition(rb.position + move * Time.fixedDeltaTime);
            transform.rotation = Quaternion.LookRotation(direction);

            animator.SetBool("isWalking", true);
            animator.SetBool("isDashing", isDashing);
            animator.speed = isDashing ? 2.0f : 1.2f;  // アニメーションも遅く設定
        }
        else
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isDashing", false);
            animator.speed = 0.2f;
        }
    }

    public void OnDashButtonPressed()
    {
        if (!isDashing)
        {
            isDashing = true;
            dashTimer = dashDuration;
        }
    }


    // UIボタンから呼び出される「噛みつき」処理
    // public void TryBite()
    // {
    //     // プレイヤーの周囲biteRange内にある「人間」オブジェクトを取得
    //     Collider[] hits = Physics.OverlapSphere(transform.position, biteRange, humanLayer);

    //     // 範囲内のすべての人間に対して感染処理を実行
    //     foreach (Collider hit in hits)
    //     {
    //         // 感染マネージャーを通じて対象を感染させる（例：人間をゾンビ化）
    //         InfectionManager.Instance.Infect(hit.gameObject);
    //     }

    //     // 噛みつきアニメーションを再生
    //     animator.SetTrigger("bite");
    // }
}
