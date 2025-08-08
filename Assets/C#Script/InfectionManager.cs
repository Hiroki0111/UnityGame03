using UnityEngine;
public class InfectionManager : MonoBehaviour
{
    public static InfectionManager Instance;
    public GameObject mobZombiePrefab;

    void Awake()
    {
        Instance = this; // ƒVƒ“ƒOƒ‹ƒgƒ“
    }

    public void Infect(GameObject human)
    {
        Vector3 pos = human.transform.position;
        Quaternion rot = human.transform.rotation;
        Destroy(human);
        Instantiate(mobZombiePrefab, pos, rot);
    }
}