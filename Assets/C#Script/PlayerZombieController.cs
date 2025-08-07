using UnityEngine;
using UnityEngine.UI; // ボタン用

public class PlayerZombieController : MonoBehaviour
{
    public float moveSpeed = 3f;       // 通常移動速度
    public float dashSpeed = 6f;       // ダッシュ時の速度（通常の2倍とか）
    public FixedJoystick joystick;     // ジョイスティックUI
    public float biteRange = 1.5f;     // 噛みつき有効距離
    public LayerMask humanLayer;       // 噛みつく対象のレイヤー
    public Animator animator;          // アニメーション制御

    public Button dashButton;          // UIのダッシュボタン

    private bool isDashing = false;    // 現在ダッシュ中かどうか
    private float dashDuration = 5f;   // ダッシュ持続時間（秒）
    private float dashTimer = 0f;      // ダッシュ経過時間

    void Start()
    {
        if (dashButton != null)
        {
            dashButton.onClick.AddListener(OnDashButtonPressed);
        }
    }

    void Update()
    {
        Vector3 direction = new Vector3(joystick.Horizontal, 0, joystick.Vertical);

        // ダッシュ時間のカウントダウン
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                // ダッシュ終了
                isDashing = false;
                dashTimer = 0f;
            }
        }

        if (direction.magnitude > 0.1f)
        {
            float speed = isDashing ? dashSpeed : moveSpeed;

            transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);
            transform.rotation = Quaternion.LookRotation(direction);

            animator.SetBool("isWalking", true);
            animator.SetBool("isDashing", isDashing);
        }
        else
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isDashing", false);
        }
    }

    // ダッシュボタンが押されたときに呼ばれる
    public void OnDashButtonPressed()
    {
        // ダッシュ中じゃなければ開始
        if (!isDashing)
        {
            isDashing = true;
            dashTimer = dashDuration;
        }
    }

    // スタミナなどの制御で外部から呼び出せる場合（必要に応じて）
    public void SetCanDash(bool value)
    {
        if (!value)
        {
            // 強制的にダッシュ終了
            isDashing = false;
            dashTimer = 0f;
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
