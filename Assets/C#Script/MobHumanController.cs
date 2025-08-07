using UnityEngine;

public class MobHumanController : MonoBehaviour
{
    [Header("移動速度")]
    public float moveSpeed = 2f;           // 通常歩く速さ
    public float escapeSpeed = 3.5f;       // 逃げる速さ

    [Header("逃げる時間")]
    public float escapeDuration = 5f;      // ゾンビ検知時に逃げる秒数

    [Header("検知範囲")]
    public float detectRange = 5f;         // ゾンビを検知する距離

    [Header("アニメーター")]
    public Animator animator;              // Animatorをセット（Inspectorから）

    private Vector3 targetDir;             // 通常時の移動方向
    private float moveTimer;               // 通常行動の方向変更タイマー

    private bool isEscaping = false;       // 逃げているか
    private float escapeTimer = 0f;         // 逃げる残り時間

    private Transform playerZombie;         // ゾンビのTransform

    void Start()
    {
        // タグ「Player」のゾンビを探す（Startで1回だけ）
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            playerZombie = playerObj.transform;

        // 最初の移動方向を決めておく
        SetRandomDirection();
    }

    void Update()
    {
        if (animator != null)
        {
            Debug.Log($"isWalking: {animator.GetBool("isWalking")}, isRunning: {animator.GetBool("isRunning")}");
        }
        if (playerZombie == null) return;  // ゾンビがいなければ何もしない

        float dist = Vector3.Distance(transform.position, playerZombie.position);

        if (dist < detectRange)
        {
            // ゾンビを検知したら逃げる
            if (!isEscaping)
            {
                isEscaping = true;
                escapeTimer = escapeDuration;

                // 振り向く（ゾンビの逆方向）
                Vector3 dir = (transform.position - playerZombie.position).normalized;
                if (dir != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(dir);
            }
        }

        if (isEscaping)
        {
            EscapeBehavior();
        }
        else
        {
            NormalBehavior();
        }
    }

    void EscapeBehavior()
    {
        escapeTimer -= Time.deltaTime;

        // ゾンビから逃げる方向
        Vector3 escapeDir = (transform.position - playerZombie.position).normalized;
        transform.Translate(escapeDir * escapeSpeed * Time.deltaTime, Space.World);

        // 走るアニメーション（例）
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", true);
            animator.speed = 1.5f;  // アニメーション速度調整可能
        }

        if (escapeTimer <= 0f)
        {
            // 逃げる時間終了
            isEscaping = false;

            // ゾンビがまだいるかチェック（必要なら）
            float dist = Vector3.Distance(transform.position, playerZombie.position);
            if (dist < detectRange)
            {
                // もう一度逃げるか、ここは自由に調整してください
                isEscaping = true;
                escapeTimer = escapeDuration;
            }
            else
            {
                // 通常行動へ戻る
                SetRandomDirection();

                if (animator != null)
                {
                    animator.SetBool("isRunning", false);
                    animator.SetBool("isWalking", true);
                    animator.speed = 1f;
                }
            }
        }
    }

    void NormalBehavior()
    {
        moveTimer -= Time.deltaTime;
        if (moveTimer <= 0f)
        {
            SetRandomDirection();
        }

        if (targetDir != Vector3.zero)
        {
            transform.Translate(targetDir * moveSpeed * Time.deltaTime, Space.World);
            transform.rotation = Quaternion.LookRotation(targetDir);
        }

        if (animator != null)
        {
            animator.SetBool("isWalking", true);
            animator.SetBool("isRunning", false);
            animator.speed = 1f;
        }
    }

    void SetRandomDirection()
    {
        targetDir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
        moveTimer = Random.Range(1f, 3f);
    }
}
