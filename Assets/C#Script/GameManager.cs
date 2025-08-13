using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("UI参照")]
    public Text humanCountText;
    public Text playerZombieCountText;
    public Text cpuZombieCountText;
    public Text timerText; // 残り時間表示用

    [Header("ゲーム時間設定")]
    public float gameTime = 180f; // 3分 = 180秒

    private float remainingTime;

    private int playerConverted = 0;
    private int cpuConverted = 0;

    private float updateInterval = 0.5f;

    void Start()
    {
        remainingTime = gameTime;
        StartCoroutine(UpdateCountsRoutine());
    }

    void Update()
    {
        UpdateTimer();
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
        int playerCount = 1; // プレイヤー本体1匹

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

        // 開始から5秒経過してから終了判定
        if (Time.timeSinceLevelLoad > 5f && humanCount <= 0)
        {
            EndGame();
        }
    }


    void UpdateTimer()
    {
        remainingTime -= Time.deltaTime;
        if (remainingTime < 0)
        {
            remainingTime = 0;
            EndGame();
        }

        // 分:秒 形式に変換
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void AddPlayerConverted()
    {
        playerConverted++;
    }

    public void AddCpuConverted()
    {
        cpuConverted++;
    }

    void EndGame()
    {
        // 結果を保持して次のシーンへ
        ResultData.humanLeft = GameObject.FindGameObjectsWithTag("Human").Length;
        ResultData.playerConverted = playerConverted;
        ResultData.cpuConverted = cpuConverted;

        // 勝敗判定（例：プレイヤーがCPUより多く感染させたら勝ち）
        ResultData.isWin = playerConverted > cpuConverted;

        SceneManager.LoadScene("ResultScene"); // リザルトシーンに遷移
    }

}
