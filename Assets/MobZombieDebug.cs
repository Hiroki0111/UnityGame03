using UnityEngine;

public class MobZombieDebug : MonoBehaviour
{
    private MobZombieController mobZombieController;

    void Start()
    {
        mobZombieController = GetComponent<MobZombieController>();
        if (mobZombieController == null)
        {
            Debug.LogError("MobZombieController コンポーネントがありません！");
        }
    }

    void Update()
    {
        if (mobZombieController == null) return;

        // 現在のターゲット
        var target = GetCurrentTarget();
        if (target != null)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);
            Debug.Log($"[MobZombieDebug] ターゲット: {target.name}, 距離: {distance:F2}");
        }
        else
        {
            Debug.Log("[MobZombieDebug] ターゲットなし");
        }
    }

    GameObject GetCurrentTarget()
    {
        // MobZombieController の private変数targetHumanには直接アクセスできないので
        // Reflectionで取る方法もあるが、ここでは代替案として

        // シンプルに近い人間を自分で探す（debug目的なのでOK）
        GameObject[] humans = GameObject.FindGameObjectsWithTag("Human");
        if (humans.Length == 0) return null;

        GameObject closest = null;
        float minDist = Mathf.Infinity;
        Vector3 pos = transform.position;

        foreach (var human in humans)
        {
            float dist = Vector3.Distance(pos, human.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = human;
            }
        }
        return closest;
    }
}
