using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MobZombieController : MonoBehaviour
{
    [Header("移動設定")]
    public float speed = 1.2f;

    [Header("探索設定")]
    public float searchInterval = 1.0f;

    [Header("アニメーター")]
    public Animator animator;

    [Header("CPU制御")]
    public bool isCPU = false;

    public enum TeamType { Blue, Yellow }
    public TeamType team;

    private GameObject targetHuman;
    private NavMeshAgent agent;
    private float searchTimer = 0f;
    private bool isAttacking = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        agent.stoppingDistance = 1.0f;

        // NavMesh上に配置
        if (!agent.isOnNavMesh)
        {
            agent.enabled = false;
            agent.enabled = true;
            agent.Warp(transform.position);
        }

        if (animator == null) animator = GetComponent<Animator>();

        var gm = GameManager.Instance;
        if (gm != null)
        {
            if (team == TeamType.Blue) gm.RegisterBlueZombie(gameObject);
            else gm.RegisterYellowZombie(gameObject);
        }
    }

    void Update()
    {
        if (isAttacking) return;

        // ターゲット更新
        searchTimer += Time.deltaTime;
        if (searchTimer >= searchInterval)
        {
            GameObject[] humans = GameObject.FindGameObjectsWithTag("Human");
            targetHuman = GetClosestHuman(humans);
            searchTimer = 0f;
        }

        // 移動
        if (targetHuman != null && agent.isOnNavMesh && agent.enabled)
        {
            agent.SetDestination(targetHuman.transform.position);

            if (animator != null)
                animator.SetBool("isWalking", agent.velocity.magnitude > 0.1f);
        }
        else
        {
            if (animator != null)
                animator.SetBool("isWalking", false);
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

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Human") || isAttacking) return;

        isAttacking = true;
        if (agent != null && agent.isOnNavMesh && agent.enabled)
            agent.isStopped = true;

        if (animator != null) animator.SetTrigger("Attack");

        InfectHuman(other.gameObject);
        Invoke(nameof(ResetAttack), 1.0f);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Human") && animator != null)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("Attack"))
                InfectHuman(other.gameObject);
        }
    }

    private void InfectHuman(GameObject human)
    {
        if (human != null && human.CompareTag("Human") && InfectionManager.Instance != null)
        {
            if (CompareTag("Player") || team == TeamType.Blue)
                InfectionManager.Instance.Infect(human, false);
            else
                InfectionManager.Instance.Infect(human, true);
        }
    }

    private void ResetAttack()
    {
        isAttacking = false;
        if (agent != null && agent.isOnNavMesh && agent.enabled)
            agent.isStopped = false;
    }
}
