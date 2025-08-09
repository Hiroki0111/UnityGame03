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

    // 攻撃間隔（秒）
    public float attackInterval = 2.0f;

    // ダッシュ中かどうか
    private bool isDashing = false;

    private float dashDuration = 5f;
    private float dashTimer = 0f;


    private bool isAttacking = false;
    private GameObject targetHuman;

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

    }

    public void OnDashButtonPressed()
    {
        if (!isDashing)
        {
            isDashing = true;
            dashTimer = dashDuration;
        }
    }

    System.Collections.IEnumerator AttackAndInfect()
    {
        isAttacking = true;

        Debug.Log("Attackトリガー発動");
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // 攻撃アニメーションの途中で感染
        yield return new WaitForSeconds(0.7f);

        if (targetHuman != null)
        {
            InfectionManager.Instance.Infect(targetHuman);
            targetHuman = null;
        }

        isAttacking = false;
    }

    // コライダー接触時（予備）
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Human"))
        {
            Debug.Log("人間に接触 → 攻撃開始");
            StartCoroutine(AttackAndInfect());
        }
    }
}
