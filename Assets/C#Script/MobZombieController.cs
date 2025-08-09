using UnityEngine;
using UnityEngine.AI;

public class MobZombieController : MonoBehaviour
{
    [Header("移動設定")]
    public float speed = 1.2f;                 // ゾンビの移動速度
    public float infectionDistance = 1.0f;     // 感染（攻撃）距離

    [Header("探索設定")]
    public float searchInterval = 1.0f;        // 人間探索の間隔（秒）

    [Header("アニメーター")]
    public Animator animator;

    private GameObject targetHuman;            // 追いかける人間
    private NavMeshAgent agent;                // 経路探索エージェント
    private float searchTimer = 0f;            // 探索タイマー
    private bool isAttacking = false;          // 攻撃中フラグ

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
        if (animator == null)
            Debug.LogError("Animator が取得できません！");

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

        // 人間を追いかける
        if (targetHuman != null)
        {
            agent.SetDestination(targetHuman.transform.position);

            float dist = Vector3.Distance(transform.position, targetHuman.transform.position);
            if (dist <= infectionDistance)
            {
                StartCoroutine(AttackAndInfect());
            }
        }

        // アニメーション設定
        if (animator != null)
        {
            bool isMoving = agent.velocity.magnitude > 0.1f;
            animator.SetBool("isWalking", isMoving);
        }
    }

    // 一番近い人間を取得
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

    // 攻撃して感染させる
    System.Collections.IEnumerator AttackAndInfect()
    {
        isAttacking = true;
        agent.isStopped = true;

        Debug.Log("Attackトリガー発動");
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // 攻撃アニメーションの途中で感染
        yield return new WaitForSeconds(0.7f);

        if (targetHuman != null)
        {
            InfectionManager.Instance.Infect(targetHuman);
            targetHuman = null;
        }

        agent.isStopped = false;
        isAttacking = false;
    }

    // コライダー接触時（予備）
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Human"))
        {
            Debug.Log("人間に接触 → 攻撃開始");
            StartCoroutine(AttackAndInfect());
        }
    }
}
