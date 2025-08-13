using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NpcController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float dashSpeed = 5f;
    public FixedJoystick joystick;
    public Animator animator;
    public Button dashButton;
    public Rigidbody rb;
    public SwipeCameraController cameraController;

    [Header("Tutorial UI")]
    public GameObject[] tutorialComments; // チュートリアルのコメントUI（順番に表示）
    public GameObject startButton;        // ゲーム開始ボタン

    private bool isDashing = false;
    private float dashDuration = 5f;
    private float dashTimer = 0f;

    private bool tutorialCompleted = false; // チュートリアル終了フラグ

    void Start()
    {
        if (dashButton != null)
            dashButton.onClick.AddListener(OnDashButtonPressed);

        if (cameraController != null)
            cameraController.ResetCameraBehindTarget();

        // チュートリアルUI初期化
        if (tutorialComments != null)
        {
            foreach (var c in tutorialComments) c.SetActive(true);
        }

        if (startButton != null)
        {
            startButton.SetActive(false);
            startButton.GetComponent<Button>().onClick.AddListener(OnStartButtonPressed);
        }
    }

    void FixedUpdate()
    {
        if (tutorialCompleted) return; // チュートリアル中だけ操作可能にするならここを調整

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
        if (other.CompareTag("Human") && !tutorialCompleted)
        {
            animator.SetTrigger("Attack");
            InfectHuman(other.gameObject);
        }
    }

    private void InfectHuman(GameObject human)
    {
        Debug.Log("InfectHuman called on " + human.name);
        if (InfectionManager.Instance != null)
        {
            InfectionManager.Instance.Infect(human, false);

            // チュートリアルの場合 → コメントを消してStartボタンを出す
            CompleteTutorial();
        }
        else
        {
            Debug.LogWarning("InfectionManager instance is null!");
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Human") && !tutorialCompleted)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("Attack"))
            {
                InfectHuman(other.gameObject);
            }
        }
    }

    private void CompleteTutorial()
    {
        tutorialCompleted = true;

        // コメント全消し
        if (tutorialComments != null)
        {
            foreach (var c in tutorialComments) c.SetActive(false);
        }

        // Startボタン表示
        if (startButton != null)
        {
            startButton.SetActive(true);
        }
    }

    private void OnStartButtonPressed()
    {
        // ゲーム本編シーンへ移動（シーン名は差し替え）
        SceneManager.LoadScene("GamePlayScene");
    }
}
