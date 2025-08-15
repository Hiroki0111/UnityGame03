using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections.Generic;

[System.Serializable]
public class PrefabSpawnInfo
{
    // スポーンするPrefab
    public GameObject prefab;

    // 何体スポーンさせるか
    public int spawnCount = 1;

    // CPU制御のゾンビかどうかを判別するフラグ
    public bool isCPUZombie = false;
}

public class RandomPrefabSpawner : MonoBehaviour
{
    // NavMeshSurfaceコンポーネント（NavMeshの情報を持つ）
    public NavMeshSurface navMeshSurface;

    // スポーン対象のPrefab情報リスト
    public List<PrefabSpawnInfo> prefabsToSpawn;

    // CPUゾンビのスポーン数（外部からセットする。例：難易度によって1,3,5など）
    public int cpuZombieCount = 1;

    // すでにスポーン済みのオブジェクトを保持し、距離チェックに使用
    private List<GameObject> spawnedObjects = new List<GameObject>();

    void Start()
    {
        if (navMeshSurface == null)
        {
            Debug.LogError("NavMeshSurface が割り当てられていません。Inspectorで設定してください。");
            return;
        }

        // CPUゾンビのspawnCountを難易度設定に合わせて上書き
        foreach (var info in prefabsToSpawn)
        {
            if (info.isCPUZombie)
            {
                info.spawnCount = GameSettings.cpuZombieCount;
            }
        }

        // 全てのPrefabをスポーン
        foreach (var info in prefabsToSpawn)
        {
            if (info.prefab == null) continue;

            for (int i = 0; i < info.spawnCount; i++)
            {
                SpawnPrefab(info.prefab, info.isCPUZombie);
            }
        }
    }


    /// <summary>
    /// NavMesh上のランダムな位置にPrefabをスポーンする。
    /// 既存のスポーン済みオブジェクトとの距離が近すぎないように配置を調整。
    /// </summary>
    /// <param name="prefab">スポーンするPrefab</param>
    /// <param name="isCPUZombie">CPU制御フラグ</param>
    void SpawnPrefab(GameObject prefab, bool isCPUZombie)
    {
        // NavMeshの有効範囲のBoundsを取得
        Bounds bounds = navMeshSurface.navMeshData.sourceBounds;

        float minDistance = 2.0f;  // キャラ同士の最小距離（この距離より近い位置にはスポーンしない）

        // スポーン位置探索は最大20回トライ
        for (int tries = 0; tries < 20; tries++)
        {
            // NavMeshの範囲内からランダムにx,z座標を決定
            Vector3 randomPosition = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.min.y, // y座標はNavMeshの最低値（地面の高さ）を使用
                Random.Range(bounds.min.z, bounds.max.z)
            );

            // NavMesh上の有効な位置かどうかチェック。半径2m以内で近いポイントを探す
            if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                // 既にスポーン済みのオブジェクトと距離が近すぎるか判定
                bool tooClose = false;
                foreach (var existingObj in spawnedObjects)
                {
                    // hit.position と既存オブジェクトの距離がminDistance未満なら近すぎると判定
                    if (Vector3.Distance(hit.position, existingObj.transform.position) < minDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }

                // 近すぎる場合は位置を再抽選
                if (tooClose) continue;

                // 適切な位置が見つかったのでPrefabを生成
                GameObject obj = Instantiate(prefab, hit.position, Quaternion.identity);

                // CPUゾンビならisCPUフラグをセット
                if (isCPUZombie)
                {
                    MobZombieController mobController = obj.GetComponent<MobZombieController>();
                    if (mobController != null)
                    {
                        mobController.isCPU = true;
                    }
                }

                // スポーン済みリストに追加して位置チェック対象に含める
                spawnedObjects.Add(obj);

                // スポーン成功したので処理終了
                return;
            }
        }

        // 20回試しても適切な位置が見つからなかった場合の警告ログ
        Debug.LogWarning("NavMesh上に適切な位置を見つけられませんでした。");
    }
    public void SpawnAll()
    {
        // spawnedObjectsリスト初期化
        spawnedObjects.Clear();

        // NavMeshSurfaceチェック
        if (navMeshSurface == null)
        {
            Debug.LogError("NavMeshSurface が割り当てられていません。Inspectorで設定してください。");
            return;
        }

        foreach (var info in prefabsToSpawn)
        {
            if (info.prefab == null) continue;

            int countToSpawn = info.isCPUZombie ? cpuZombieCount : info.spawnCount;

            for (int i = 0; i < countToSpawn; i++)
            {
                SpawnPrefab(info.prefab, info.isCPUZombie);
            }
        }
    }

}