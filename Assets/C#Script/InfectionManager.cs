using System.Collections;
using UnityEngine;

public class InfectionManager : MonoBehaviour
{
    // シングルトンとしてアクセス可能
    public static InfectionManager Instance { get; private set; }

    [Header("感染後の青色ゾンビPrefab（プレイヤー感染用）")]
    public GameObject blueZombiePrefab;

    [Header("感染後の黄色ゾンビPrefab（CPU感染用）")]
    public GameObject yellowZombiePrefab;

    [Header("感染時のパーティクルPrefab")]
    public ParticleSystem infectionParticlePrefab;

    [Header("ゾンビ生成までの遅延（秒）")]
    public float zombieSpawnDelay = 0.5f;

    private void Awake()
    {
        // シングルトン初期化
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 人間をゾンビに変換する外部呼び出し用関数
    /// </summary>
    /// <param name="human">感染対象のHumanオブジェクト</param>
    /// <param name="infectedByCPU">CPU感染かプレイヤー感染か</param>
    public void Infect(GameObject human, bool infectedByCPU)
    {
        StartCoroutine(InfectCoroutine(human, infectedByCPU));
    }

    private IEnumerator InfectCoroutine(GameObject human, bool infectedByCPU)
    {
        if (human == null) yield break; // nullチェック

        // human の位置と回転を保持
        Vector3 position = human.transform.position;
        Quaternion rotation = human.transform.rotation;

        // -----------------------------
        // 物理干渉を防ぐ処理
        // -----------------------------
        // Collider無効化で他のオブジェクトに衝突しなくする
        Collider col = human.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Rigidbodyを停止・キネマティック化して吹き飛びを防ぐ
        Rigidbody rb = human.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // 見た目を非表示にして感染演出に集中
        human.SetActive(false);

        // -----------------------------
        // 感染パーティクル再生
        // -----------------------------
        if (infectionParticlePrefab != null)
        {
            ParticleSystem particle = Instantiate(infectionParticlePrefab, position, Quaternion.identity);
            particle.Play();
        }

        // ゾンビ生成までの遅延
        yield return new WaitForSeconds(zombieSpawnDelay);

        // -----------------------------
        // ゾンビ生成処理
        // -----------------------------
        GameObject zombiePrefab = infectedByCPU ? yellowZombiePrefab : blueZombiePrefab;
        GameObject newZombie = Instantiate(zombiePrefab, position, rotation);
        newZombie.tag = "zombie";

        // MobZombieControllerにCPU/プレイヤー情報を渡す
        MobZombieController mobController = newZombie.GetComponent<MobZombieController>();
        if (mobController != null)
            mobController.isCPU = infectedByCPU;

        // -----------------------------
        // GameManagerへの通知
        // -----------------------------
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            if (infectedByCPU)
                gm.AddCpuConverted();
            else
                gm.AddPlayerConverted();
        }

        // 最後にHumanオブジェクトを破棄
        Destroy(human);
    }
}
