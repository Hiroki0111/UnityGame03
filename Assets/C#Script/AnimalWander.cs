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

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // NavMeshAgent の移動挙動を強化
        agent.stoppingDistance = 1.0f; // 壁ギリギリまで行かない
        agent.radius = 0.8f; // 馬の体格に合わせて調整
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        timer = wanderDelay;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= wanderDelay)
        {
            Vector3 newPos = GetValidRandomPosition();
            agent.SetDestination(newPos);

            timer = 0;
        }
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
            // 進行方向に壁があるかをレイキャストでチェック
        } while (IsWallInPath(newPos) && safetyCounter < 10);

        return newPos;
    }

    /// <summary>
    /// 目的地までの直線に壁があるか判定
    /// </summary>
    private bool IsWallInPath(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;

        // 高さを少し上げてレイキャスト（地面に当たらないように）
        if (Physics.Raycast(transform.position + Vector3.up, direction, out RaycastHit hit, wallCheckDistance))
        {
            if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Obstacle"))
            {
                return true; // 壁や障害物あり
            }
        }
        return false;
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
