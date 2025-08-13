using UnityEngine;
using System.Collections;

public class InfectionManager : MonoBehaviour
{
    public static InfectionManager Instance { get; private set; }

    [Header("感染後の青色ゾンビのPrefab（プレイヤー感染用）")]
    public GameObject blueZombiePrefab;

    [Header("感染後の黄色ゾンビのPrefab（CPU感染用）")]
    public GameObject yellowZombiePrefab;

    [Header("感染時パーティクルPrefab")]
    public ParticleSystem infectionParticlePrefab;

    [Header("ゾンビ出現までの遅延")]
    public float zombieSpawnDelay = 0.5f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// 人間を感染させる
    /// </summary>
    public void Infect(GameObject human, bool infectedByCPU)
    {
        if (human == null) return;

        StartCoroutine(InfectCoroutine(human, infectedByCPU));
    }

    private IEnumerator InfectCoroutine(GameObject human, bool infectedByCPU)
    {
        if (human == null) yield break;

        // human の位置と回転を記録
        Vector3 position = human.transform.position;
        Quaternion rotation = human.transform.rotation;

        // 人間を即削除
        Destroy(human);

        // パーティクル生成＆再生
        if (infectionParticlePrefab != null)
        {
            ParticleSystem particle = Instantiate(infectionParticlePrefab, position, Quaternion.identity);
            particle.Play();
        }

        // 指定時間待機
        yield return new WaitForSeconds(zombieSpawnDelay);

        // ゾンビ生成
        GameObject zombiePrefab = infectedByCPU ? yellowZombiePrefab : blueZombiePrefab;
        GameObject newZombie = Instantiate(zombiePrefab, position, rotation);
        newZombie.tag = "zombie";

        // CPUかプレイヤーか判定
        MobZombieController mobController = newZombie.GetComponent<MobZombieController>();
        if (mobController != null)
            mobController.isCPU = infectedByCPU;

        // ゲームマネージャに通知
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            if (infectedByCPU)
                gm.AddCpuConverted();
            else
                gm.AddPlayerConverted();
        }
    }
}
