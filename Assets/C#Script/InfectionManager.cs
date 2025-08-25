using System.Collections;
using UnityEngine;

public class InfectionManager : MonoBehaviour
{
    public static InfectionManager Instance { get; private set; }

    public GameObject blueZombiePrefab;
    public GameObject yellowZombiePrefab;
    public ParticleSystem infectionParticlePrefab;
    public float zombieSpawnDelay = 0.5f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Infect(GameObject human, bool infectedByCPU)
    {
        if (human == null) return;

        HumanStatus status = human.GetComponent<HumanStatus>();
        if (status != null && status.isBeingInfected) return; // すでに感染中なら無視

        if (status == null) human.AddComponent<HumanStatus>();

        status = human.GetComponent<HumanStatus>();
        status.isBeingInfected = true;

        StartCoroutine(InfectCoroutine(human, infectedByCPU));
    }

    private IEnumerator InfectCoroutine(GameObject human, bool infectedByCPU)
    {
        if (human == null) yield break;

        GameManager.Instance.UnregisterHuman(human);

        Vector3 position = human.transform.position;
        Quaternion rotation = human.transform.rotation;

        Collider col = human.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Rigidbody rb = human.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        human.SetActive(false);

        // パーティクル生成（破棄は5秒後）
        if (infectionParticlePrefab != null)
        {
            ParticleSystem particle = Instantiate(infectionParticlePrefab, position, Quaternion.identity);
            particle.Play();
            Destroy(particle.gameObject, 5f);
        }

        yield return new WaitForSeconds(zombieSpawnDelay);

        GameObject zombiePrefab = infectedByCPU ? yellowZombiePrefab : blueZombiePrefab;
        GameObject newZombie = Instantiate(zombiePrefab, position, rotation);
        newZombie.tag = infectedByCPU ? "YellowZombie" : "BlueZombie";

        if (infectedByCPU) GameManager.Instance.RegisterYellowZombie(newZombie);
        else GameManager.Instance.RegisterBlueZombie(newZombie);

        MobZombieController mzc = newZombie.GetComponent<MobZombieController>();
        if (mzc != null)
            mzc.team = infectedByCPU ? MobZombieController.TeamType.Yellow : MobZombieController.TeamType.Blue;
    }
}

// 人間に付けるフラグ用
public class HumanStatus : MonoBehaviour
{
    public bool isBeingInfected = false;
}
