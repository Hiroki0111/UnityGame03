using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public Text humanCountText;
    public Text playerZombieCountText;
    public Text cpuZombieCountText;

    public float gameTimeLimit = 180f; // 3分
    private float elapsedTime = 0f;

    private int playerConvertedCount = 0;
    private int cpuConvertedCount = 0;

    private float updateInterval = 0.5f;

    void Start()
    {
        StartCoroutine(UpdateCountsRoutine());
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        // 人間が0になったら or 制限時間終了
        if (GetHumanCount() <= 0 || elapsedTime >= gameTimeLimit)
        {
            EndGame();
        }
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
        int humanCount = GetHumanCount();

        // Zombie の数を取得（CPUとプレイヤーで分類）
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("zombie");
        int cpuCount = 0;
        int playerCount = 1; // プレイヤーは1体固定

        foreach (var zombie in zombies)
        {
            MobZombieController mobZombie = zombie.GetComponent<MobZombieController>();
            if (mobZombie != null)
            {
                if (mobZombie.isCPU) cpuCount++;
                else playerCount++;
            }
        }

        humanCountText.text = ": " + humanCount;
        playerZombieCountText.text = ": " + playerCount;
        cpuZombieCountText.text = ": " + cpuCount;
    }

    int GetHumanCount()
    {
        GameObject[] humans = GameObject.FindGameObjectsWithTag("Human");
        return humans.Length;
    }

    public void AddPlayerConverted()
    {
        playerConvertedCount++;
    }

    public void AddCpuConverted()
    {
        cpuConvertedCount++;
    }

    void EndGame()
    {
        // 値を次のシーンに渡す
        ResultData.humanLeft = GetHumanCount();
        ResultData.playerConverted = playerConvertedCount;
        ResultData.cpuConverted = cpuConvertedCount;

        if (playerConvertedCount > cpuConvertedCount)
            ResultData.isWin = true;
        else
            ResultData.isWin = false;

        SceneManager.LoadScene("ResultScene");
    }
}
