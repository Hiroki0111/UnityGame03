using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections.Generic;

[System.Serializable]
public class PrefabSpawnInfo
{
    public GameObject prefab;
    public int spawnCount = 1; // CPUゾンビ以外の通常スポーン数
    public bool isCPUZombie = false;
}

public class RandomPrefabSpawner : MonoBehaviour
{
    public NavMeshSurface navMeshSurface;
    public List<PrefabSpawnInfo> prefabsToSpawn;

    [Header("生成判定")]
    public LayerMask obstacleLayerMask; // 家や柵のLayerを設定
    public float minDistanceBetweenSpawns = 2f;

    private List<GameObject> spawnedObjects = new List<GameObject>();

    void Start()
    {
        if (navMeshSurface == null)
        {
            Debug.LogError("NavMeshSurface 未設定");
            return;
        }

        foreach (var info in prefabsToSpawn)
        {
            if (info.prefab == null) continue;

            int spawnCount = info.isCPUZombie ? GameSettings.cpuZombieCount : info.spawnCount;

            for (int i = 0; i < spawnCount; i++)
            {
                SpawnPrefab(info.prefab, info.isCPUZombie);
            }
        }
    }

    void SpawnPrefab(GameObject prefab, bool isCPUZombie)
    {
        Bounds bounds = navMeshSurface.navMeshData.sourceBounds;

        for (int tries = 0; tries < 30; tries++) // 試行回数制限
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.min.y + 1f,
                Random.Range(bounds.min.z, bounds.max.z)
            );

            // NavMesh上の有効位置をサンプル
            if (!NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                continue;

            // 既存オブジェクトとの距離チェック
            bool tooClose = false;
            foreach (var obj in spawnedObjects)
            {
                if (Vector3.Distance(hit.position, obj.transform.position) < minDistanceBetweenSpawns)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose) continue;

            // 障害物との重なりチェック
            if (Physics.CheckSphere(hit.position, 0.5f, obstacleLayerMask))
                continue;

            // オブジェクト生成
            GameObject newObj = Instantiate(prefab, hit.position, Quaternion.identity);

            if (isCPUZombie)
            {
                MobZombieController mob = newObj.GetComponent<MobZombieController>();
                if (mob != null) mob.isCPU = true;
            }

            spawnedObjects.Add(newObj);
            return;
        }

        Debug.LogWarning("適切なNavMesh位置を見つけられませんでした: " + prefab.name);
    }
}