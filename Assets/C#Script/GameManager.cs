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
    public float gameTime = 180f; // 3分

    [Header("BGM / SE設定")]
    public AudioSource mainBGM;   // ゲーム中ループBGM
    public AudioSource endSFX;    // 勝敗ジングル効果音
    public AudioSource endBGM;    // エンディングBGM

    [Header("フェード設定")]
    public Image fadeImage;       // UI Imageを黒にして画面フェード用
    public float fadeDuration = 2f;

    private float remainingTime;
    private int playerConverted = 0;
    private int cpuConverted = 0;
    private float updateInterval = 0.5f;
    private bool isGameEnding = false;

    void Start()
    {
        remainingTime = gameTime;
        StartCoroutine(UpdateCountsRoutine());

        // ゲーム開始時BGM再生
        if (mainBGM != null)
        {
            mainBGM.loop = true;
            mainBGM.Play();
        }

        // フェードを透明から開始
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
        }
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
        if (isGameEnding) return; // 終了処理中なら更新しない

        GameObject[] humans = GameObject.FindGameObjectsWithTag("Human");
        int humanCount = humans.Length;

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

        humanCountText.text = ": " + humanCount;
        playerZombieCountText.text = ": " + playerCount;
        cpuZombieCountText.text = ": " + cpuCount;

        // 開始から5秒経過後に全滅判定
        if (Time.timeSinceLevelLoad > 5f && humanCount <= 0)
        {
            StartCoroutine(EndGameSequence());
        }
    }

    void UpdateTimer()
    {
        if (isGameEnding) return; // 終了処理中ならカウントしない

        remainingTime -= Time.deltaTime;
        if (remainingTime < 0)
        {
            remainingTime = 0;
            StartCoroutine(EndGameSequence());
        }

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

    IEnumerator EndGameSequence()
    {
        if (isGameEnding) yield break; // 二重実行防止
        isGameEnding = true;

        // 結果保存
        ResultData.humanLeft = GameObject.FindGameObjectsWithTag("Human").Length;
        ResultData.playerConverted = playerConverted;
        ResultData.cpuConverted = cpuConverted;
        ResultData.isWin = playerConverted > cpuConverted;

        // メインBGM停止
        if (mainBGM != null) mainBGM.Stop();

        // ① 効果音再生（勝ち・負けで切り替え）
        if (endSFX != null)
        {
            endSFX.Play();
            yield return new WaitForSeconds(endSFX.clip.length); // 再生完了まで待つ
        }

        // ② エンディングBGM再生
        if (endBGM != null)
        {
            endBGM.Play();
            yield return new WaitForSeconds(1f); // 1秒ぐらい流してからフェード開始（好みで調整）
        }

        // ③ フェードアウト
        if (fadeImage != null)
        {
            float t = 0f;
            Color c = fadeImage.color;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(0, 1, t / fadeDuration);
                fadeImage.color = c;
                yield return null;
            }
        }

        // ④ シーン遷移
        SceneManager.LoadScene("ResultScene");
    }
}
