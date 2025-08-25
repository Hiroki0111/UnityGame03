using UnityEngine;
using UnityEngine.AI;

public class TutorialMobu : MonoBehaviour
{
    [Header("移動")]
    public float moveSpeed = 3f;
    public float escapeSpeed = 6f;

    [Header("検知")]
    public float detectRange = 10f;
    public float fieldOfView = 120f;

    [Header("逃走時間")]
    public float escapeDuration = 5f;

    private NavMeshAgent agent;
    private Animator animator;
    private bool isEscaping = false;
    private float escapeTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.speed = moveSpeed;

        animator = GetComponent<Animator>();
        if (animator != null) animator.applyRootMotion = false;
    }

    void Update()
    {
        GameObject nearestZombie = FindNearestZombie(out float distance);

        if (nearestZombie != null && distance <= detectRange)
        {
            Vector3 dirToZombie = nearestZombie.transform.position - transform.position;
            if (Vector3.Angle(transform.forward, dirToZombie) <= fieldOfView * 0.5f)
            {
                // ゾンビが視界内 → 逃げる
                isEscaping = true;
                escapeTimer = escapeDuration;
                RunAwayFrom(nearestZombie.transform.position);
            }
        }

        if (isEscaping)
        {
            escapeTimer -= Time.deltaTime;
            if (escapeTimer <= 0f)
            {
                isEscaping = false;
                if (agent != null) agent.speed = moveSpeed;
            }
        }
    }

    GameObject FindNearestZombie(out float nearestDistance)
    {
        nearestDistance = Mathf.Infinity;
        GameObject nearest = null;
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("BlueZombie");
        zombies = CombineArrays(zombies, GameObject.FindGameObjectsWithTag("YellowZombie"));

        foreach (var z in zombies)
        {
            float dist = Vector3.Distance(transform.position, z.transform.position);
            if (dist < nearestDistance)
            {
                nearestDistance = dist;
                nearest = z;
            }
        }
        return nearest;
    }

    GameObject[] CombineArrays(GameObject[] a, GameObject[] b)
    {
        GameObject[] result = new GameObject[a.Length + b.Length];
        a.CopyTo(result, 0);
        b.CopyTo(result, a.Length);
        return result;
    }

    void RunAwayFrom(Vector3 threatPos)
    {
        if (agent == null) return;

        Vector3 dir = (transform.position - threatPos).normalized;
        Vector3 target = transform.position + dir * 5f;

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.speed = escapeSpeed;
            agent.SetDestination(hit.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        // InfectionManager02 が存在しない場合は処理しない
        if (InfectionManager02.Instance == null)
        {
            Debug.LogWarning("InfectionManager02 がシーンに存在しません！");
            return;
        }

        // BlueZombie に触れた場合
        if (other.gameObject.CompareTag("BlueZombie"))
        {
            InfectionManager02.Instance.Infect(this.gameObject, false);
        }
        // YellowZombie に触れた場合
        else if (other.gameObject.CompareTag("YellowZombie"))
        {
            InfectionManager02.Instance.Infect(this.gameObject, true);
        }
    }

}
