using UnityEngine;

public class InfectionManager : MonoBehaviour
{
    public static InfectionManager Instance { get; private set; }

    [Header("感染後の青色ゾンビのPrefab（プレイヤー感染用）")]
    public GameObject blueZombiePrefab;

    [Header("感染後の黄色ゾンビのPrefab（CPU感染用）")]
    public GameObject yellowZombiePrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 人間を感染させゾンビ化させる
    /// </summary>
    /// <param name="human">感染対象の人間オブジェクト</param>
    /// <param name="infectedByCPU">CPUによる感染かどうか</param>
    public void Infect(GameObject human, bool infectedByCPU)
    {
        if (human == null) return;

        Vector3 position = human.transform.position;
        Quaternion rotation = human.transform.rotation;

        GameObject zombiePrefab = infectedByCPU ? yellowZombiePrefab : blueZombiePrefab;

        // 人間オブジェクトを削除
        Destroy(human);

        // 新しいゾンビを生成
        GameObject newZombie = Instantiate(zombiePrefab, position, rotation);

        // タグは"zombie"に統一
        newZombie.tag = "zombie";

        // CPUかプレイヤーかの判定を設定
        MobZombieController mobController = newZombie.GetComponent<MobZombieController>();
        if (mobController != null)
        {
            mobController.isCPU = infectedByCPU;
        }
    }
}
