using UnityEngine;
using UnityEngine.AI;

public class MobHumanController : MonoBehaviour
{
    [Header("移動速度")]
    public float moveSpeed = 2f;
    public float escapeSpeed = 15.5f;

    [Header("視界検知")]
    public float detectRange = 10f;
    public float fieldOfViewAngle = 120f;

    [Header("逃走時間")]
    public float escapeDuration = 5f;

    [Header("逃走地点")]
    public Transform escapeTarget;

    [Header("アニメーション")]
    public Animator animator;

    [Header("徘徊設定")]
    public float wanderRadius = 10f;
    public float wanderInterval = 5f;

    [Header("衝突判定用")]
    public LayerMask obstacleLayerMask; // 壁や縦もののLayerを設定
    public float collisionCheckDistance = 0.5f;

    private NavMeshAgent agent;
    private bool isEscaping = false;
    private float escapeTimer = 0f;
    private float wanderTimer = 0f;
    private Vector3 currentWanderTarget;

    private GameManager gm;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent がアタッチされていません！");
            enabled = false;
            return;
        }

        agent.acceleration = 50f;
        agent.angularSpeed = 120f;
        agent.speed = moveSpeed;
        agent.ResetPath();

        if (animator != null)
            animator.applyRootMotion = false;

        gm = GameManager.Instance;
        gm?.RegisterHuman(gameObject);
    }

    void Update()
    {
        if (gm == null || escapeTarget == null) return;

        bool zombieInView = false;

        // プレイヤーゾンビ
        if (gm.Player != null && IsZombieInView(gm.Player.transform))
            zombieInView = true;

        // Blue Zombies
        if (!zombieInView)
        {
            foreach (var zombie in gm.BlueZombies)
            {
                if (IsZombieInView(zombie.transform))
                {
                    zombieInView = true;
                    break;
                }
            }
        }

        // Yellow Zombies
        if (!zombieInView)
        {
            foreach (var zombie in gm.YellowZombies)
            {
                if (IsZombieInView(zombie.transform))
                {
                    zombieInView = true;
                    break;
                }
            }
        }

        agent.speed = isEscaping ? escapeSpeed : moveSpeed;

        if (!isEscaping && zombieInView)
        {
            isEscaping = true;
            escapeTimer = escapeDuration;
        }

        if (isEscaping) EscapeToTarget();
        else NormalBehavior();

        // 壁や縦ものとの衝突チェック
        CheckCollisionAndTurn();
    }

    bool IsZombieInView(Transform zombieTransform)
    {
        Vector3 toZombie = zombieTransform.position - transform.position;
        float distance = toZombie.magnitude;
        if (distance > detectRange) return false;

        float angle = Vector3.Angle(transform.forward, toZombie.normalized);
        if (angle > fieldOfViewAngle * 0.5f) return false;

        Vector3 eyePos = transform.position + Vector3.up * 1.5f;
        Vector3 direction = toZombie.normalized;

        if (Physics.Raycast(eyePos, direction, out RaycastHit hit, detectRange))
            return hit.transform == zombieTransform;

        return false;
    }

    void EscapeToTarget()
    {
        escapeTimer -= Time.deltaTime;
        agent.SetDestination(escapeTarget.position);

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", true);
            animator.speed = 1.5f;
        }

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

    Vector3 GetRandomWanderTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius + transform.position;
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return transform.position;
    }

    // 壁に衝突したら反転（タグWall）
    void CheckCollisionAndTurn()
    {
        Vector3 forward = transform.forward;
        float checkDistance = 0.5f;

        // 前方にRayを飛ばしてWallタグを判定
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, forward, out RaycastHit hit, checkDistance))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                // 進行方向を反転してUターン
                Vector3 newDirection = Quaternion.Euler(0f, 180f, 0f) * forward;
                currentWanderTarget = transform.position + newDirection * wanderRadius;
                agent.SetDestination(currentWanderTarget);
            }
        }
    }


    void OnDestroy()
    {
        gm?.UnregisterHuman(gameObject);
    }
}
