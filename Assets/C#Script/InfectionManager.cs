using UnityEngine;

public class InfectionManager : MonoBehaviour
{
    public static InfectionManager Instance;

    public GameObject mobZombiePrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Infect(GameObject human)
    {
        Debug.Log($"[Infect] 感染対象: {human.name}");
        if (human == null) return;

        // humanの位置と回転を取得
        Vector3 pos = human.transform.position;
        Quaternion rot = human.transform.rotation;

        // humanオブジェクトを削除
        Destroy(human);

        // ゾンビPrefabを生成
        Instantiate(mobZombiePrefab, pos, rot);

        Debug.Log($"[Infect] {human.name} はゾンビになりました");
    }
}
