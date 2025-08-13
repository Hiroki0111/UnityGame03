using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public Text humanCountText;
    public Text playerZombieCountText;
    public Text cpuZombieCountText;

    private float updateInterval = 0.5f;

    void Start()
    {
        StartCoroutine(UpdateCountsRoutine());
    }

    IEnumerator UpdateCountsRoutine()
    {
        while (true)
        {
            UpdateCounts();
            yield return new WaitForSeconds(updateInterval);
        }
    }

    void UpdateCounts()
    {
        // Human の数を取得
        GameObject[] humans = GameObject.FindGameObjectsWithTag("Human");
        int humanCount = humans.Length;

        // Zombie の数を取得（CPUとプレイヤーで分類）
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("zombie");

        int cpuCount = 0;
        int playerCount = 0;

        foreach (var zombie in zombies)
        {
            MobZombieController mobZombie = zombie.GetComponent<MobZombieController>();
            if (mobZombie != null)
            {
                if (mobZombie.isCPU) cpuCount++;
                else playerCount++;
            }
        }

        // UIに反映
        humanCountText.text = ": " + humanCount;
        playerZombieCountText.text = ": " + playerCount;
        cpuZombieCountText.text = ": " + cpuCount;
    }
}
