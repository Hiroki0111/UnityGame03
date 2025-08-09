using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class RandomPrefabSpawner : MonoBehaviour
{
    public GameObject prefabToSpawn; // 配置するプレハブ
    private NavMeshSurface navMeshSurface; // 自動取得

    public int numberOfPrefabsToSpawn = 10;

    void Start()
    {
        // 自分にあるコンポーネントを取得
        navMeshSurface = GetComponent<NavMeshSurface>();

        if (navMeshSurface == null)
        {
            Debug.LogError("NavMeshSurface がこのオブジェクトに存在しません。");
            return;
        }

        for (int i = 0; i < numberOfPrefabsToSpawn; i++)
        {
            SpawnPrefab();
        }
    }

    void SpawnPrefab()
    {
        Bounds bounds = navMeshSurface.navMeshData.sourceBounds;

        Vector3 randomPosition = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            bounds.min.y,
            Random.Range(bounds.min.z, bounds.max.z)
        );

        if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            randomPosition = hit.position;
            Instantiate(prefabToSpawn, randomPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("NavMesh上に位置を投影できませんでした。");
        }
    }
}
