using UnityEngine;
using UnityEngine.AI;

public class AnimalWander : MonoBehaviour
{
    [Header("移動パラメータ")]
    public float wanderRadius = 10f; // 移動できる半径
    public float wanderDelay = 3f;   // 次の目的地までの待ち時間
    public float wallCheckDistance = 2f; // 壁を事前検知する距離

    private NavMeshAgent agent;
    private float timer;
    private Vector3 currentWanderTarget;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // NavMeshAgent の移動挙動を強化
        agent.stoppingDistance = 1.0f;
        agent.radius = 0.8f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        timer = wanderDelay;
        SetNewWanderTarget();
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 前方に壁や障害物があればUターン
        CheckWallAhead();

        // タイマーで次の目的地を設定
        if (timer >= wanderDelay || agent.remainingDistance < 0.5f)
        {
            SetNewWanderTarget();
            timer = 0;
        }
    }

    private void SetNewWanderTarget()
    {
        currentWanderTarget = GetValidRandomPosition();
        agent.SetDestination(currentWanderTarget);
    }

    /// <summary>
    /// NavMesh 上のランダム地点を取得し、壁があれば再取得
    /// </summary>
    private Vector3 GetValidRandomPosition()
    {
        int safetyCounter = 0;
        Vector3 newPos;

        do
        {
            newPos = RandomNavSphere(transform.position, wanderRadius, -1);
            safetyCounter++;
        } while (IsWallInPath(newPos) && safetyCounter < 10);

        return newPos;
    }

    /// <summary>
    /// 目的地までの直線に壁があるか判定
    /// </summary>
    private bool IsWallInPath(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;

        if (Physics.Raycast(transform.position + Vector3.up, direction, out RaycastHit hit, wallCheckDistance))
        {
            if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Obstacle"))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 前方にWallやObstacleがあったらUターン
    /// </summary>
    private void CheckWallAhead()
    {
        Vector3 forward = transform.forward;

        if (Physics.Raycast(transform.position + Vector3.up, forward, out RaycastHit hit, wallCheckDistance))
        {
            if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Obstacle"))
            {
                // Uターン方向を決める
                Vector3 newDirection = Quaternion.Euler(0f, 180f, 0f) * forward;
                currentWanderTarget = transform.position + newDirection * wanderRadius;
                agent.SetDestination(currentWanderTarget);
            }
        }
    }

    /// <summary>
    /// NavMesh 上のランダム座標を取得
    /// </summary>
    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randomDirection = Random.insideUnitSphere * dist;
        randomDirection += origin;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, dist, layermask);

        return navHit.position;
    }
}
