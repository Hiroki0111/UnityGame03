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


    private float remainingTime;
    private int playerConverted = 0;
    private int cpuConverted = 0;
    private float updateInterval = 0.5f;
    private bool isGameEnding = false;

    public static bool gameEnded = false; // 感染停止フラグ

    [SerializeField] private AudioSource mainBGM;
    [SerializeField] private AudioSource endSFX;
    [SerializeField] private AudioSource endBGM;
    [SerializeField] private UnityEngine.UI.Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;
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

    public IEnumerator EndGameSequence()
    {
        // すでに終了処理が始まっていたら何もしない
        if (isGameEnding) yield break;
        isGameEnding = true;

        gameEnded = true; // 感染停止フラグ

        // --- 音・演出 ---
        if (mainBGM != null) mainBGM.Stop();

        if (endSFX != null)
        {
            endSFX.Play();
            yield return new WaitForSeconds(endSFX.clip.length);
        }

        if (endBGM != null)
        {
            endBGM.Play();
            yield return new WaitForSeconds(1f);
        }

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

        // ====== 集計・保存 ======
        int humansLeft = GameObject.FindGameObjectsWithTag("Human").Length;
        var allZombies = GameObject.FindGameObjectsWithTag("zombie");
        int blueCount = 0;
        int yellowCount = 0;

        foreach (var z in allZombies)
        {
            var pzc = z.GetComponent<PlayerZombieController>();
            if (pzc != null)
            {
                if (pzc.team == PlayerZombieController.TeamType.Blue) blueCount++;
                else yellowCount++;
                continue;
            }

            var mzc = z.GetComponent<MobZombieController>();
            if (mzc != null)
            {
                if (mzc.team == MobZombieController.TeamType.Blue) blueCount++;
                else yellowCount++;
            }
        }

        ResultData.humanLeft = humansLeft;
        ResultData.playerConverted = blueCount;
        ResultData.cpuConverted = yellowCount;
        ResultData.isWin = blueCount > yellowCount;
        // ==========================

        SceneManager.LoadScene("ResultScene");
    }



}