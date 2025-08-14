using UnityEngine;

public class NpcController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float dashSpeed = 5f;
    public FixedJoystick joystick;
    public Animator animator;
    public Rigidbody rb;

    private bool isDashing = false;
    private float dashDuration = 5f;
    private float dashTimer = 0f;

    void FixedUpdate()
    {
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0; camRight.y = 0;
        camForward.Normalize(); camRight.Normalize();

        Vector3 input = camForward * joystick.Vertical + camRight * joystick.Horizontal;

        if (input.magnitude > 0.1f)
        {
            input.Normalize();
            float speed = isDashing ? dashSpeed : moveSpeed;
            rb.MovePosition(rb.position + input * speed * Time.fixedDeltaTime);
            transform.rotation = Quaternion.LookRotation(input);

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

        if (isDashing)
        {
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f) { isDashing = false; dashTimer = 0f; }
        }
    }

    public void StartDash()
    {
        if (!isDashing)
        {
            isDashing = true;
            dashTimer = dashDuration;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Human")) return;

        // 攻撃アニメーション
        animator.SetTrigger("Attack");

        // プレイヤーは青ゾンビ（infectedByCPU = false）
        bool infectedByCPU = false;

        // InfectionManager で安全に変換
        InfectionManager.Instance?.Infect(other.gameObject, infectedByCPU);

        // チュートリアルUI更新（青ゾンビカウントを増やす）
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnHumanInfected(!infectedByCPU);
        }
    }
}
