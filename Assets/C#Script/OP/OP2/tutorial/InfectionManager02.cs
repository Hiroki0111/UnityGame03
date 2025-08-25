using System.Collections;
using UnityEngine;

public class InfectionManager02 : MonoBehaviour
{
    public static InfectionManager02 Instance { get; private set; }

    [Header("ゾンビのPrefab")]
    public GameObject blueZombiePrefab;
    public GameObject yellowZombiePrefab;

    [Header("感染エフェクト")]
    public ParticleSystem infectionParticlePrefab;

    [Header("ゾンビ生成までの遅延(秒)")]
    public float zombieSpawnDelay = 0.3f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// infectedByCPU = true → 黄色ゾンビ化, false → 青ゾンビ化
    /// </summary>
    public void Infect(GameObject human, bool infectedByCPU)
    {
        if (human == null) return;
        StartCoroutine(InfectCoroutine(human, infectedByCPU));
    }

    private IEnumerator InfectCoroutine(GameObject human, bool infectedByCPU)
    {
        if (human == null) yield break;

        // 位置退避
        Vector3 position = human.transform.position;
        Quaternion rotation = human.transform.rotation;

        // Human を一旦無効化（FindGameObjectsWithTag は非アクティブを数えない）
        var col = human.GetComponent<Collider>();
        if (col) col.enabled = false;
        var rb = human.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        human.SetActive(false);

        // エフェクト
        if (infectionParticlePrefab != null)
        {
            var fx = Instantiate(infectionParticlePrefab, position, Quaternion.identity);
            fx.Play();
        }

        // 少し待ってから置換
        yield return new WaitForSeconds(zombieSpawnDelay);

        // Human を完全に削除
        Destroy(human);

        // ゾンビ生成 & タグ設定（安全のため毎回明示）
        GameObject prefab = infectedByCPU ? yellowZombiePrefab : blueZombiePrefab;
        if (prefab != null)
        {
            GameObject z = Instantiate(prefab, position, rotation);
            z.tag = infectedByCPU ? "YellowZombie" : "BlueZombie";
        }
        else
        {
            Debug.LogWarning("InfectionManager02: ゾンビPrefabが未設定です。Inspectorで設定してください。");
        }

        // 1フレーム待ってから再集計（Instantiate/Destroyの反映を確実にする）
        yield return null;

        // 人数更新（タグで正確に再集計してUIに反映）
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.RecountAndUpdateUI();
    }
}
