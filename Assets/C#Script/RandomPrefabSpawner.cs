using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class RandomPrefabSpawner : MonoBehaviour
{
    public GameObject prefabToSpawn; // �z�u����v���n�u
    private NavMeshSurface navMeshSurface; // �����擾

    public int numberOfPrefabsToSpawn = 10;

    void Start()
    {
        // �����ɂ���R���|�[�l���g���擾
        navMeshSurface = GetComponent<NavMeshSurface>();

        if (navMeshSurface == null)
        {
            Debug.LogError("NavMeshSurface �����̃I�u�W�F�N�g�ɑ��݂��܂���B");
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
            Debug.LogWarning("NavMesh��Ɉʒu�𓊉e�ł��܂���ł����B");
        }
    }
}
