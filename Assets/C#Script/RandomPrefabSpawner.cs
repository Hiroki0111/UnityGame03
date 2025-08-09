using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation; // NavMeshSurface に必要

[System.Serializable]
public class PrefabSpawnData
{
    public GameObject prefab;
    public int count;
}

public class RandomPrefabSpawner : MonoBehaviour
{
    [Header("NavMesh情報")]
    public NavMeshSurface navMeshSurface;

    [Header("スポーン設定")]
    public List<PrefabSpawnData> prefabsToSpawn;

    void Start()
    {
        if (navMeshSurface == null)
        {
            Debug.LogError("NavMeshSurfaceが設定されていません。");
            return;
        }

        if (prefabsToSpawn == null || prefabsToSpawn.Count == 0)
        {
            Debug.LogError("プレハブのリストが空です。");
            return;
        }

        foreach (var data in prefabsToSpawn)
        {
            if (data.prefab == null) continue;

            for (int i = 0; i < data.count; i++)
            {
                SpawnPrefab(data.prefab);
            }
        }
    }

    void SpawnPrefab(GameObject prefab)
    {
        Bounds bounds = navMeshSurface.navMeshData.sourceBounds;

        for (int i = 0; i < 10; i++) // 10回まで位置を試す
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.min.y,
                Random.Range(bounds.min.z, bounds.max.z)
            );

            if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                Instantiate(prefab, hit.position, Quaternion.identity);
                return;
            }
        }

        Debug.LogWarning("NavMesh上に有効な位置が見つかりませんでした。");
    }
}
