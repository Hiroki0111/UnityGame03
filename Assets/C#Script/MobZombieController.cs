using UnityEngine;
using UnityEngine.AI;

public class MobZombieController : MonoBehaviour
{
    [Header("追跡設定")]
    public float speed = 1.2f;
    public float infectionDistance = 1.0f;

    [Header("捜索設定")]
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
        if (animator != null)
        {
            Debug.Log("Attackトリガー発動");
            animator.SetTrigger("Attack");
        }
        else
        {
            Debug.LogError("Animatorがnull");
        }
    }

    void Update()
    {
        if (animator == null)
            Debug.LogError("Animator が取得できていません！");

        if (agent == null || isAttacking) return;

        searchTimer += Time.deltaTime;

        // 人間を探す
        if (targetHuman == null && searchTimer >= searchInterval)
        {
            GameObject[] humans = GameObject.FindGameObjectsWithTag("Human");
            if (humans.Length > 0)
            {
                targetHuman = GetClosestHuman(humans);
            }
            searchTimer = 0f;
        }

        // 追跡と攻撃判定
        if (targetHuman != null)
        {
            agent.SetDestination(targetHuman.transform.position);

            float dist = Vector3.Distance(transform.position, targetHuman.transform.position);
            if (dist <= infectionDistance)
            {
                StartCoroutine(AttackAndInfect());
            }
        }

        // アニメーション：歩くか止まる
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

    System.Collections.IEnumerator AttackAndInfect()
    {
        isAttacking = true;
        agent.isStopped = true;
        Debug.LogError("attackメソッドは行われてる");
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // 感染まで待機（攻撃モーションに合わせて調整）
        yield return new WaitForSeconds(0.7f); // アニメーションに合わせて調整

        if (targetHuman != null)
        {
            InfectionManager.Instance.Infect(targetHuman);
        }

        targetHuman = null;
        agent.isStopped = false;
        isAttacking = false;
    }
}
