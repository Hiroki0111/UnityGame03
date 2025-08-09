using UnityEngine;
using Unity.AI.Navigation;

public class NavMeshSurfaceCheck : MonoBehaviour
{
    void Start()
    {
        var surface = GetComponent<NavMeshSurface>();
        if (surface != null)
        {
            Debug.Log("NavMeshSurface is found!");
        }
        else
        {
            Debug.LogError("NavMeshSurface is NOT found!");
        }
    }
}
