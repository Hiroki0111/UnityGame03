using UnityEngine;
using UnityEngine.UI;

public class PlayerZombieController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float dashSpeed = 5f;
    public FixedJoystick joystick;
    public Animator animator;
    public Button dashButton;
    public Rigidbody rb;
    public SwipeCameraController cameraController;

    private bool isDashing = false;
    private float dashDuration = 5f;
    private float dashTimer = 0f;

    // 攻撃対象を記録してアニメ後に感染
    private GameObject currentTarget;

    void Start()
    {
        if (dashButton != null)
            dashButton.onClick.AddListener(OnDashButtonPressed);

        if (cameraController != null)
            cameraController.ResetCameraBehindTarget();
    }

    void FixedUpdate()
    {
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

    public void OnDashButtonPressed()
    {
        if (!isDashing)
        {
            isDashing = true;
            dashTimer = dashDuration;
        }
    }

    public void OnCameraResetButtonPressed()
    {
        if (cameraController != null)
            cameraController.ResetCameraBehindTarget();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Human"))
        {
            // 攻撃トリガーセット
            animator.SetTrigger("Attack");

            // 攻撃モーション中に感染させるため、攻撃アニメーション終わりかOnTriggerStayで呼ぶ
            InfectHuman(other.gameObject);
        }
    }

    private void InfectHuman(GameObject human)
    {
        Debug.Log("InfectHuman called on " + human.name);
        if (InfectionManager.Instance != null)
        {
            // プレイヤー感染と仮定してfalseを渡す
            InfectionManager.Instance.Infect(human, false);
        }
        else
        {
            Debug.LogWarning("InfectionManager instance is null!");
        }
    }



    // 攻撃アニメーションの終わり付近で自動実行（モーション中も可）
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Human"))
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("Attack"))
            {
                // 攻撃モーション中なら即感染
                InfectHuman(other.gameObject);
            }
        }
    }

}