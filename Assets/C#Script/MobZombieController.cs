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

    private GameObject targetHuman;
    private NavMeshAgent agent;
    private float searchTimer = 0f;

    private bool isAttacking = false;

    public bool isCPU = false;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        agent.stoppingDistance = 1.0f; // 距離が近すぎるとすり抜ける可能性がある

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isAttacking) return;

        searchTimer += Time.deltaTime;

        if (searchTimer >= searchInterval)
        {
            GameObject[] humans = GameObject.FindGameObjectsWithTag("Human");
            targetHuman = GetClosestHuman(humans);
            searchTimer = 0f;
        }

        if (targetHuman != null)
        {
            agent.SetDestination(targetHuman.transform.position);

            bool isMoving = agent.velocity.magnitude > 0.1f;
            animator.SetBool("isWalking", isMoving);
        }
        else
        {
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
        agent.isStopped = true;

        animator.SetTrigger("Attack");

        // 即感染（演出だけアニメに任せる）
        InfectHuman(other.gameObject);

        // 少し遅れて再び移動を許可（演出のためのクールタイム）
        Invoke(nameof(ResetAttack), 1.0f);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Human"))
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("Attack"))
            {
                InfectHuman(other.gameObject);
            }
        }
    }

    private void InfectHuman(GameObject human)
    {
        if (human != null && human.CompareTag("Human") && InfectionManager.Instance != null)
        {
            InfectionManager.Instance.Infect(human);
        }
    }

    private void ResetAttack()
    {
        isAttacking = false;
        agent.isStopped = false;
    }
}
