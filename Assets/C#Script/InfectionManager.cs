using UnityEngine;

public class InfectionManager : MonoBehaviour
{
    public static InfectionManager Instance;
    public GameObject mobZombiePrefab; // 感染後に生成するゾンビのPrefab

    void Awake()
    {
        // シングルトンとして、このクラスの唯一のインスタンスを保持
        Instance = this;
    }

    // 人間を感染させてゾンビに置き換える処理
    public void Infect(GameObject human)
    {
        // 人間の現在位置と回転を保存
        Vector3 pos = human.transform.position;
        Quaternion rot = human.transform.rotation;

        // 元の人間オブジェクトを削除
        Destroy(human);

        // 同じ位置・回転でゾンビを生成
        Instantiate(mobZombiePrefab, pos, rot);
    }
}
