using UnityEngine;
using UnityEngine.UI;

public class PlayerZombieController : MonoBehaviour
{
    // 基本移動速度（超スロー）
    public float moveSpeed = 0.00002f;

    // ダッシュ移動速度（遅め）
    public float dashSpeed = 0.1f;

    // ジョイスティックUIコンポーネント（操作入力用）
    public FixedJoystick joystick;

    // プレイヤーのアニメーター（歩行・攻撃など制御）
    public Animator animator;

    // ダッシュボタンUI
    public Button dashButton;

    // 物理演算用Rigidbodyコンポーネント
    public Rigidbody rb;

    [Header("攻撃設定")]
    [Range(0.1f, 10f)]
    public float attackRange = 1.0f;

    // 攻撃対象のレイヤーマスク（人間レイヤー）
    public LayerMask humanLayer;

    // 攻撃間隔（秒）
    public float attackInterval = 2.0f;

    // ダッシュ中かどうか
    private bool isDashing = false;

    private float dashDuration = 5f;
    private float dashTimer = 0f;

    // 攻撃のクールタイマー
    private float attackTimer = 0f;

    void Start()
    {
        if (dashButton != null)
            dashButton.onClick.AddListener(OnDashButtonPressed);
    }

    void FixedUpdate()
    {
        // 移動処理（ジョイスティック操作）
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
            animator.speed = isDashing ? 2.0f : 1.2f;
        }
        else
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isDashing", false);
            animator.speed = 0.2f;
        }

        // 攻撃処理（自動）
        attackTimer -= Time.fixedDeltaTime;
        if (attackTimer <= 0f)
        {
            TryAttack();
            attackTimer = attackInterval;
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

    // 自動攻撃判定＆実行
    void TryAttack()
    {
        // 半径attackRange内の人間をレイヤーマスクで取得
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, humanLayer);

        if (hits.Length > 0)
        {
            // 一番近い人間を攻撃
            Collider closestHuman = null;
            float minDist = Mathf.Infinity;

            foreach (Collider hit in hits)
            {
                if (hit.gameObject == this.gameObject) continue; // 自分は除外

                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestHuman = hit;
                }
            }

            if (closestHuman != null)
            {
                Attack(closestHuman.gameObject);
            }
        }
    }

    void Attack(GameObject human)
    {
        animator.speed = 10.0f;
        animator.SetTrigger("Attack");

        // 感染処理は攻撃アニメーション中に呼ぶのが良いのでInvokeやCoroutineで遅延可能
        Invoke(nameof(ResetAnimatorSpeed), 1.5f);

        // 感染処理呼び出し（シングルトンのInfectionManagerを想定）
        InfectionManager.Instance.Infect(human);
    }

    void ResetAnimatorSpeed()
    {
        animator.speed = 1.0f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
