using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections.Generic;  // List使うので

[System.Serializable]
public class PrefabSpawnInfo
{
    public GameObject prefab;  // 配置するプレハブ（人間キャラ）
    public int spawnCount = 1; // この種類のプレハブの生成数
}

public class RandomPrefabSpawner : MonoBehaviour
{
    public NavMeshSurface navMeshSurface; // Inspectorから割り当て

    public List<PrefabSpawnInfo> prefabsToSpawn; // プレハブ種類＆生成数リスト

    void Start()
    {
        if (navMeshSurface == null)
        {
            Debug.LogError("NavMeshSurface が割り当てられていません。Inspectorで設定してください。");
            return;
        }

        foreach (var info in prefabsToSpawn)
        {
            if (info.prefab == null) continue;

            for (int i = 0; i < info.spawnCount; i++)
            {
                SpawnPrefab(info.prefab);
            }
        }
    }

    void SpawnPrefab(GameObject prefab)
    {
        Bounds bounds = navMeshSurface.navMeshData.sourceBounds;

        for (int tries = 0; tries < 10; tries++)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.min.y,
                Random.Range(bounds.min.z, bounds.max.z)
            );

            if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                Instantiate(prefab, hit.position, Quaternion.identity);
                return;
            }
        }
        Debug.LogWarning("NavMesh上に適切な位置を見つけられませんでした。");
    }
}
