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
        StartCoroutine(InfectCoroutine(human, infectedByCPU));
    }

    private IEnumerator InfectCoroutine(GameObject human, bool infectedByCPU)
    {
        if (human == null) yield break;

        Vector3 position = human.transform.position;
        Quaternion rotation = human.transform.rotation;

        // 衝突停止・吹っ飛び防止
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

        if (infectionParticlePrefab != null)
        {
            ParticleSystem particle = Instantiate(infectionParticlePrefab, position, Quaternion.identity);
            particle.Play();
        }

        yield return new WaitForSeconds(zombieSpawnDelay);

        GameObject zombiePrefab = infectedByCPU ? yellowZombiePrefab : blueZombiePrefab;
        GameObject newZombie = Instantiate(zombiePrefab, position, rotation);
        newZombie.tag = "zombie";

        MobZombieController mobController = newZombie.GetComponent<MobZombieController>();
        if (mobController != null)
            mobController.isCPU = infectedByCPU;

        // GameManagerに通知
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            if (infectedByCPU) gm.AddCpuConverted();
            else gm.AddPlayerConverted();
        }

        Destroy(human);
    }
}
