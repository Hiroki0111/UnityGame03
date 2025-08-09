using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// モブ人間が自分の視界にゾンビを見つけたら逃げるAI
/// 見つけていない時は徘徊する
/// </summary>
public class MobHumanController : MonoBehaviour
{
    [Header("移動速度")]
    public float moveSpeed = 2f;          // 通常歩く速度
    public float escapeSpeed = 105.5f;     // 逃走時の速度

    [Header("視界検知")]
    public float detectRange = 10f;       // 視認距離
    public float fieldOfViewAngle = 120f; // 視野角（度）

    [Header("逃走時間")]
    public float escapeDuration = 5f;     // 最低逃げる時間

    [Header("逃走地点")]
    public Transform escapeTarget;        // 逃げるポイント

    [Header("アニメーション")]
    public Animator animator;             // Animator参照

    [Header("徘徊設定")]
    public float wanderRadius = 10f;      // 徘徊半径
    public float wanderInterval = 5f;     // 目標変更間隔

    private NavMeshAgent agent;
    private Transform playerZombie;

    private bool isEscaping = false;
    private float escapeTimer = 0f;

    private float wanderTimer = 0f;
    private Vector3 currentWanderTarget;

    void Start()
    {
        // ゾンビをタグで検索
        GameObject zombieObj = GameObject.FindWithTag("zombie");
        if (zombieObj != null)
            playerZombie = zombieObj.transform;

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            Debug.LogError("NavMeshAgentが必要です");

        agent.speed = moveSpeed;
    }

    void Update()
    {
        if (playerZombie == null || escapeTarget == null) return;

        // 視界内にゾンビがいたら逃げる
        if (!isEscaping && IsZombieInView())
        {
            isEscaping = true;
            escapeTimer = escapeDuration;
        }

        if (isEscaping)
        {
            EscapeToTarget();
        }
        else
        {
            NormalBehavior();
        }
    }

    /// <summary>
    /// モブ人間の視界内にゾンビがいるか（距離＋視野角＋遮蔽物なし）
    /// </summary>
    bool IsZombieInView()
    {
        Vector3 toZombie = playerZombie.position - transform.position;
        float distance = toZombie.magnitude;

        if (distance > detectRange) return false;

        float angle = Vector3.Angle(transform.forward, toZombie.normalized);
        if (angle > fieldOfViewAngle * 0.5f) return false;

        Vector3 eyePos = transform.position + Vector3.up * 1.5f;
        Vector3 direction = toZombie.normalized;

        Debug.DrawRay(eyePos, direction * detectRange, Color.green);

        if (Physics.Raycast(eyePos, direction, out RaycastHit hit, detectRange))
        {
            Debug.Log("Raycastヒット：" + hit.transform.name);

            if (hit.transform == playerZombie)
            {
                Debug.Log("ゾンビを視界内で発見！");
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 逃走時の行動：逃走地点に向かって走る
    /// </summary>
    void EscapeToTarget()
    {
        Debug.Log("エスケープは発動してる");
        escapeTimer -= Time.deltaTime;

        agent.speed = escapeSpeed;
        agent.SetDestination(escapeTarget.position);

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", true);
            animator.speed = 1.5f;
            Debug.Log("逃げ出すアニメーションはやってる");
        }

        float distToEscape = Vector3.Distance(transform.position, escapeTarget.position);
        if (distToEscape < 1f)
        {
            Destroy(gameObject);
            return;
        }

        // 逃走時間が切れたら逃走終了（任意で調整可能）
        if (escapeTimer <= 0f)
        {
            isEscaping = false;
            agent.speed = moveSpeed;

            if (animator != null)
            {
                animator.SetBool("isRunning", false);
                animator.SetBool("isWalking", true);
                animator.speed = 1f;
            }
        }
    }

    /// <summary>
    /// 通常時の行動：NavMesh上を徘徊
    /// </summary>
    void NormalBehavior()
    {
        wanderTimer += Time.deltaTime;

        if (wanderTimer >= wanderInterval || agent.remainingDistance < 0.5f)
        {
            currentWanderTarget = GetRandomWanderTarget();
            agent.SetDestination(currentWanderTarget);
            wanderTimer = 0f;
        }

        agent.speed = moveSpeed;

        if (animator != null)
        {
            animator.SetBool("isWalking", true);
            animator.SetBool("isRunning", false);
            animator.speed = 1f;
        }
    }

    /// <summary>
    /// NavMesh内でランダムな徘徊ターゲットを取得
    /// </summary>
    Vector3 GetRandomWanderTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return transform.position;
    }
}
