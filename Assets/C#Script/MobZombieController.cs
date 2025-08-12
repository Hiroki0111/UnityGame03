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

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        agent.stoppingDistance = 0.5f;

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
        if (isAttacking) return;

        if (other.CompareTag("Human"))
        {
            isAttacking = true;
            agent.isStopped = true;

            animator.SetTrigger("Attack");
            StartCoroutine(AttackAndInfect(other.gameObject));
        }
    }

    private System.Collections.IEnumerator AttackAndInfect(GameObject human)
    {
        yield return new WaitForSeconds(1.0f); // 攻撃アニメーションの長さに合わせて調整

        if (human != null && human.CompareTag("Human"))
        {
            InfectionManager.Instance?.Infect(human);
        }

        agent.isStopped = false;
        isAttacking = false;
    }

    // 念のため滞在時も感染（アニメーション中）
    private void OnTriggerStay(Collider other)
    {
        if (isAttacking) return;

        if (other.CompareTag("Human"))
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("Attack"))
            {
                InfectionManager.Instance?.Infect(other.gameObject);
            }
        }
    }
}
