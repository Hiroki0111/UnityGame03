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

    [Header("Camera")]
    public SwipeCameraController cameraController; // カメラ制御
    void Start()
    {
        // カメラを初期位置にリセット
        if (cameraController != null)
            cameraController.ResetCameraBehindTarget();
    }
    // カメラリセットボタン押下時
    public void OnCameraResetButtonPressed()
    {
        if (cameraController != null)
            cameraController.ResetCameraBehindTarget();
    }
    void FixedUpdate()
    {
        // 操作可能
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 inputDirection = camForward * joystick.Vertical + camRight * joystick.Horizontal;

        if (inputDirection.magnitude > 0.1f)
        {
            inputDirection.Normalize();
            float speed = isDashing ? dashSpeed : moveSpeed;
            Vector3 move = inputDirection * speed;
            rb.MovePosition(rb.position + move * Time.fixedDeltaTime);
            transform.rotation = Quaternion.LookRotation(inputDirection);

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
            if (dashTimer <= 0f)
            {
                isDashing = false;
                dashTimer = 0f;
            }
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
        if (other.CompareTag("Human"))
        {
            animator.SetTrigger("Attack");

            // 感染処理
            bool becameBlue = Random.value > 0.5f;
            InfectionManager.Instance?.Infect(other.gameObject, becameBlue);

            // チュートリアルManagerに通知
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnHumanInfected(becameBlue);
            }
        }
    }


}
