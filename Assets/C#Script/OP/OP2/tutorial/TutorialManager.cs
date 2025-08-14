using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("チュートリアルコメント")]
    public GameObject[] tutorialComments;

    [Header("スタートボタン")]
    public Button startButton;

    [Header("暗転用イメージ")]
    public Image fadeImage; // UI Canvas上の黒いImage

    [Header("BGM")]
    public AudioSource bgmAudioSource;

    [Header("暗転時間")]
    public float fadeDuration = 2f;

    [Header("移行するシーン名")]
    public string nextSceneName;

    [Header("人数表示用UI")]
    public Text humanCountText;
    public Text blueZombieCountText;
    public Text yellowZombieCountText;

    private int humanCount = 0;
    private int blueZombieCount = 0;
    private int yellowZombieCount = 0;

    private void Awake()
    {
        Instance = this;

        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonPressed);
            startButton.gameObject.SetActive(false); // ★初期非表示に変更
        }

        if (fadeImage != null)
            fadeImage.color = new Color(0, 0, 0, 0);

        UpdateCountUI();
    }


    /// <summary>
    /// 人間の初期人数を設定
    /// </summary>
    public void SetInitialHumanCount(int count)
    {
        humanCount = count;
        UpdateCountUI();
    }

    /// <summary>
    /// 人間感染時に呼ばれる
    /// </summary>
    public void OnHumanInfected(bool becameBlue)
    {
        // 人間減少
        humanCount = Mathf.Max(humanCount - 1, 0);

        // ゾンビ増加
        if (becameBlue)
            blueZombieCount++;
        else
            yellowZombieCount++;

        UpdateCountUI();

        // コメントを全て非表示
        foreach (var comment in tutorialComments)
            comment.SetActive(false);

        // スタートボタン表示
        if (startButton != null)
            startButton.gameObject.SetActive(true);
    }

    private void UpdateCountUI()
    {
        if (humanCountText != null)
            humanCountText.text = "Human: " + humanCount;
        if (blueZombieCountText != null)
            blueZombieCountText.text = "Blue Zombie: " + blueZombieCount;
        if (yellowZombieCountText != null)
            yellowZombieCountText.text = "Yellow Zombie: " + yellowZombieCount;
    }

    /// <summary>
    /// スタートボタン押下時処理
    /// </summary>
    private void OnStartButtonPressed()
    {
        startButton.gameObject.SetActive(false);
        StartCoroutine(FadeOutAndLoadScene());
    }

    private IEnumerator FadeOutAndLoadScene()
    {
        float timer = 0f;
        float initialVolume = bgmAudioSource != null ? bgmAudioSource.volume : 1f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            // BGMフェードアウト
            if (bgmAudioSource != null)
                bgmAudioSource.volume = Mathf.Lerp(initialVolume, 0f, t);

            // 画面暗転
            if (fadeImage != null)
                fadeImage.color = new Color(0, 0, 0, t);

            yield return null;
        }

        if (fadeImage != null)
            fadeImage.color = new Color(0, 0, 0, 1f);

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }
}
