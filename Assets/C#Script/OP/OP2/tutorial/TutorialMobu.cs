using UnityEngine;
using UnityEngine.AI;

public class TutorialMobu : MonoBehaviour
{
    [Header("移動速度")]
    public float moveSpeed = 2f;
    public float escapeSpeed = 15.5f;

    [Header("視界検知")]
    public float detectRange = 10f;
    public float fieldOfViewAngle = 120f;

    [Header("逃走時間")]
    public float escapeDuration = 5f;

    [Header("アニメーション")]
    public Animator animator;

    [Header("徘徊設定")]
    public float wanderRadius = 10f;
    public float wanderInterval = 5f;

    private NavMeshAgent agent;
    private bool isEscaping = false;
    private float escapeTimer = 0f;
    private float wanderTimer = 0f;
    private Vector3 currentWanderTarget;

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
    }

    void Update()
    {
        // 「zombie」タグのオブジェクトすべてを検知
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("zombie");
        if (zombies.Length == 0) return;

        bool zombieInView = false;
        Transform nearestZombie = null;
        float nearestDist = Mathf.Infinity;

        foreach (var zombieObj in zombies)
        {
            float dist = Vector3.Distance(transform.position, zombieObj.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearestZombie = zombieObj.transform;
            }

            if (IsZombieInView(zombieObj.transform))
            {
                zombieInView = true;
            }
        }

        agent.speed = isEscaping ? escapeSpeed : moveSpeed;

        if (!isEscaping && zombieInView)
        {
            isEscaping = true;
            escapeTimer = escapeDuration;
            // 逃げ始めるときに即座に方向を決める
            if (nearestZombie != null)
                RunAwayFrom(nearestZombie.position);
        }

        if (isEscaping)
        {
            EscapeToTarget(nearestZombie);
        }
        else
        {
            NormalBehavior();
        }
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
        {
            if (hit.transform == zombieTransform)
            {
                return true;
            }
        }
        return false;
    }

    void EscapeToTarget(Transform zombie)
    {
        escapeTimer -= Time.deltaTime;

        if (zombie != null && agent.remainingDistance < 0.5f)
        {
            // ゾンビの反対方向へ新しい逃走先を決める
            RunAwayFrom(zombie.position);
        }

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

    void RunAwayFrom(Vector3 threatPosition)
    {
        Vector3 dirToThreat = (transform.position - threatPosition).normalized;
        Vector3 escapeTarget = transform.position + dirToThreat * wanderRadius;

        // NavMesh 上の有効な位置を探す
        if (NavMesh.SamplePosition(escapeTarget, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
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
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return transform.position;
    }
}
