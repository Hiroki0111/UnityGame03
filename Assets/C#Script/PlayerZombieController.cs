using UnityEngine;
using UnityEngine.UI;

public class PlayerZombieController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;         // 通常移動速度
    public float dashSpeed = 5f;         // ダッシュ中の移動速度
    public FixedJoystick joystick;       // ジョイスティック入力
    public Rigidbody rb;                 // プレイヤーのRigidbody

    [Header("Animation")]
    public Animator animator;            // アニメーター

    [Header("Dash")]
    public Button dashButton;            // ダッシュボタン
    public CanvasGroup dashButtonCanvasGroup; // ダッシュボタンのフェード用
    public float dashDuration = 5f;      // ダッシュ持続時間
    public float fadeDuration = 3f;      // ダッシュボタンフェードイン時間

    [Header("Camera")]
    public SwipeCameraController cameraController; // カメラ制御

    [Header("Attack")]
    public float attackCooldown = 1f;   // 攻撃間隔（秒）
    private float attackTimer = 0f;     // 攻撃クールタイマー

    private bool isDashing = false;     // ダッシュ中フラグ
    private float dashTimer = 0f;       // ダッシュ残り時間

    // ダッシュ状態管理
    private enum DashState { Ready, Dashing, FadingIn }
    private DashState dashState = DashState.Ready;
    private float fadeTimer = 0f;       // フェード進行時間

    void Start()
    {
        // ボタンにリスナー追加
        if (dashButton != null)
            dashButton.onClick.AddListener(OnDashButtonPressed);

        // カメラを初期位置にリセット
        if (cameraController != null)
            cameraController.ResetCameraBehindTarget();
    }

    void Update()
    {
        // 攻撃タイマーを減算（0以下にはならない）
        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        HandleMovement();     // 移動処理
        HandleDashState();    // ダッシュ状態の更新
    }

    // プレイヤー移動処理
    void HandleMovement()
    {
        // カメラ基準の前後左右ベクトルを作成
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // ジョイスティック入力から移動方向計算
        Vector3 inputDirection = camForward * joystick.Vertical + camRight * joystick.Horizontal;

        if (inputDirection.magnitude > 0.1f) // ある程度の入力がある場合
        {
            inputDirection.Normalize();
            float speed = isDashing ? dashSpeed : moveSpeed; // ダッシュ中は速く
            Vector3 move = inputDirection * speed;

            // Rigidbodyで移動
            rb.MovePosition(rb.position + move * Time.fixedDeltaTime);
            // キャラクター回転
            transform.rotation = Quaternion.LookRotation(inputDirection);

            // アニメーション切り替え
            animator.SetBool("isWalking", true);
            animator.SetBool("isDashing", isDashing);
            animator.speed = isDashing ? 2.0f : 1.2f;
        }
        else
        {
            // 移動なし
            animator.SetBool("isWalking", false);
            animator.SetBool("isDashing", false);
            animator.speed = 0.2f;
        }
    }

    // ダッシュ状態管理
    void HandleDashState()
    {
        switch (dashState)
        {
            case DashState.Dashing:
                // ダッシュ時間を減らす
                dashTimer -= Time.fixedDeltaTime;
                if (dashTimer <= 0f)
                {
                    isDashing = false;
                    dashState = DashState.FadingIn;
                    fadeTimer = 0f; // フェードイン開始
                }
                break;

            case DashState.FadingIn:
                // フェードイン処理
                fadeTimer += Time.fixedDeltaTime;
                if (dashButtonCanvasGroup != null)
                    dashButtonCanvasGroup.alpha = Mathf.Clamp01(fadeTimer / fadeDuration);

                if (fadeTimer >= fadeDuration)
                    dashState = DashState.Ready; // ダッシュ再使用可能
                break;

            case DashState.Ready:
                // 特に処理なし
                break;
        }
    }

    // ダッシュボタン押下時
    public void OnDashButtonPressed()
    {
        if (dashState == DashState.Ready)
        {
            if (dashButtonCanvasGroup != null)
                dashButtonCanvasGroup.alpha = 0f; // 使用中は非表示

            isDashing = true;
            dashTimer = dashDuration;
            dashState = DashState.Dashing;
        }
    }

    // カメラリセットボタン押下時
    public void OnCameraResetButtonPressed()
    {
        if (cameraController != null)
            cameraController.ResetCameraBehindTarget();
    }

    // ヒューマンに接触している間に呼ばれる
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Human") && attackTimer <= 0f)
        {
            animator.SetTrigger("Attack");   // 攻撃アニメーション
            InfectHuman(other.gameObject);   // 感染処理
            attackTimer = attackCooldown;    // 攻撃クールリセット
        }
    }

    // ヒューマンを感染させる処理
    private void InfectHuman(GameObject human)
    {
        if (human == null) return;

        Debug.Log("InfectHuman called on " + human.name);

        if (InfectionManager.Instance != null)
            InfectionManager.Instance.Infect(human, false); // 実際の感染処理
        else
            Debug.LogWarning("InfectionManager instance is null!");
    }
}
