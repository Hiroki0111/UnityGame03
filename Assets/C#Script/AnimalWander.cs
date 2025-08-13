using UnityEngine;
using UnityEngine.AI;

public class AnimalWander : MonoBehaviour
{
    [Header("移動パラメータ")]
    public float wanderRadius = 10f; // 移動できる半径（村の広さに応じて調整）
    public float wanderDelay = 3f;   // 次の目的地に向かうまでの待ち時間

    private NavMeshAgent agent;      // 自動経路探索と移動を行うコンポーネント
    private float timer;             // 待ち時間を計測するためのタイマー

    void Start()
    {
        // NavMeshAgentコンポーネントを取得（必須）
        agent = GetComponent<NavMeshAgent>();

        // 最初の移動までの時間を初期化
        timer = wanderDelay;
    }

    void Update()
    {
        // 経過時間をカウント
        timer += Time.deltaTime;

        // 指定時間が経過したら新しい目的地を設定
        if (timer >= wanderDelay)
        {
            // ランダムな位置を取得（NavMesh上にある位置）
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);

            // NavMeshAgentに新しい目的地を設定
            agent.SetDestination(newPos);

            // タイマーをリセット
            timer = 0;
        }
    }

    /// <summary>
    /// 指定範囲内でランダムなNavMesh上の座標を取得する関数
    /// </summary>
    /// <param name="origin">基準位置（現在地）</param>
    /// <param name="dist">半径</param>
    /// <param name="layermask">レイヤーマスク（-1で全て）</param>
    /// <returns>NavMesh上のランダムな位置</returns>
    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        // ランダムな方向ベクトルを作成（球状にランダム）
        Vector3 randomDirection = Random.insideUnitSphere * dist;

        // 現在位置からランダム方向へ移動
        randomDirection += origin;

        NavMeshHit navHit;

        // NavMesh上にサンプリングして一番近い有効な座標を取得
        NavMesh.SamplePosition(randomDirection, out navHit, dist, layermask);

        return navHit.position;
    }
}
