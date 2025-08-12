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
        // Human�̐��𐔂���
        GameObject[] humans = GameObject.FindGameObjectsWithTag("Human");
        int humanCount = humans.Length;

        // Zombie�̐��𐔂���iCPU�ƃv���C���[�ŕ�����j
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("Zombie");

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

        // UI�ɔ��f
        humanCountText.text = "Humans: " + humanCount;
        playerZombieCountText.text = "Player Zombies: " + playerCount;
        cpuZombieCountText.text = "CPU Zombies: " + cpuCount;
    }
}
