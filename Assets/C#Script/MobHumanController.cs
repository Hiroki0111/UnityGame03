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

    private NavMeshAgent agent;
    private Transform playerZombie;

    private bool isEscaping = false;
    private float escapeTimer = 0f;

    private float wanderTimer = 0f;
    private Vector3 currentWanderTarget;

    void Start()
    {
        GameObject zombieObj = GameObject.FindWithTag("zombie");
        if (zombieObj != null)
            playerZombie = zombieObj.transform;

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
        if (playerZombie == null || escapeTarget == null) return;

        agent.speed = isEscaping ? escapeSpeed : moveSpeed;

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
            if (hit.transform == playerZombie)
            {
                Debug.Log("ゾンビを視界内で発見！");
                return true;
            }
        }

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
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return transform.position;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == escapeTarget.gameObject)
        {
            Debug.Log("[MobHuman] Escape target に到達。削除します。");
            Destroy(gameObject);
        }
    }
}
