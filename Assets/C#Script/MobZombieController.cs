using UnityEngine;
using UnityEngine.AI;

public class MobZombieController : MonoBehaviour
{
    [Header("移動設定")]
    public float speed = 1.2f;
    public float infectionDistance = 1.0f;

    [Header("探索設定")]
    public float searchInterval = 1.0f;

    [Header("アニメーター")]
    public Animator animator;

    private GameObject targetHuman;
    private NavMeshAgent agent;
    private float searchTimer = 0f;
    private bool isAttacking = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent が必要です！");
            return;
        }

        agent.speed = speed;
        agent.stoppingDistance = infectionDistance;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        if (agent == null || isAttacking) return;

        searchTimer += Time.deltaTime;

        if (searchTimer >= searchInterval)
        {
            GameObject[] humans = GameObject.FindGameObjectsWithTag("Human");
            if (humans.Length > 0)
            {
                targetHuman = GetClosestHuman(humans);
                Debug.Log($"[MobZombie] 新ターゲット: {targetHuman.name}");
            }
            else
            {
                targetHuman = null;
                Debug.Log("[MobZombie] ターゲットなし");
            }
            searchTimer = 0f;
        }

        if (targetHuman != null)
        {
            agent.SetDestination(targetHuman.transform.position);

            float dist = Vector3.Distance(transform.position, targetHuman.transform.position);
            Debug.Log($"[MobZombie] ターゲットとの距離: {dist:F2}");

            if (dist <= infectionDistance && !isAttacking)
            {
                Debug.Log("[MobZombie] 攻撃開始");
                StartCoroutine(AttackRoutine());
            }
        }

        if (animator != null)
        {
            bool isMoving = agent.velocity.magnitude > 0.1f;
            animator.SetBool("isWalking", isMoving);
        }
    }

    GameObject GetClosestHuman(GameObject[] humans)
    {
        GameObject closest = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (GameObject human in humans)
        {
            float dist = Vector3.Distance(currentPos, human.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = human;
            }
        }
        return closest;
    }

    System.Collections.IEnumerator AttackRoutine()
    {
        isAttacking = true;
        agent.isStopped = true;

        animator.SetTrigger("Attack");
        Debug.Log("[MobZombie] Attack トリガー発動");

        // 攻撃アニメーションが終わるまで待機（例：1秒）
        yield return new WaitForSeconds(1.0f);

        // 攻撃範囲内の最も近い人間を探す
        GameObject attackTarget = GetClosestHumanInAttackRange();

        if (attackTarget != null)
        {
            Debug.Log("[MobZombie] 感染処理対象: " + attackTarget.name);
            InfectionManager.Instance.Infect(attackTarget);
        }
        else
        {
            Debug.LogWarning("[MobZombie] 攻撃範囲内にターゲットなし");
        }

        agent.isStopped = false;
        isAttacking = false;
    }

    GameObject GetClosestHumanInAttackRange()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, infectionDistance);
        GameObject closest = null;
        float minDist = Mathf.Infinity;

        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("Human"))
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = hit.gameObject;
                }
            }
        }
        return closest;
    }
}
