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
    public Image fadeImage;

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
    private int blueZombieCount = 1;  // プレイヤー1人
    private int yellowZombieCount = 0;

    private void Awake()
    {
        Instance = this;

        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonPressed);
            startButton.gameObject.SetActive(false);
        }

        if (fadeImage != null)
            fadeImage.color = new Color(0, 0, 0, 0);
    }

    private void Start()
    {
        InitializeCounts();
    }

    /// <summary>
    /// 初期人数カウント（青/黄ゾンビ、Human）を正確に設定
    /// </summary>
    private void InitializeCounts()
    {
        // 人間の数
        humanCount = GameObject.FindGameObjectsWithTag("Human").Length;

        // 黄色ゾンビをシーン内でカウント
        yellowZombieCount = 0;
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("zombie");
        foreach (var z in zombies)
        {
            MobZombieController mob = z.GetComponent<MobZombieController>();
            if (mob != null && mob.isCPU)
                yellowZombieCount++;
        }

        UpdateCountUI();
    }

    /// <summary>
    /// 人間感染時に呼ばれる
    /// </summary>
    public void OnHumanInfected(bool becameBlue)
    {
        humanCount = Mathf.Max(humanCount - 1, 0);

        if (becameBlue)
            blueZombieCount++;
        else
            yellowZombieCount++;

        UpdateCountUI();

        // チュートリアルコメント非表示
        foreach (var comment in tutorialComments)
            comment.SetActive(false);

        // スタートボタン表示
        if (startButton != null)
            startButton.gameObject.SetActive(true);
    }

    private void UpdateCountUI()
    {
        if (humanCountText != null)
            humanCountText.text = ": " + humanCount;
        if (blueZombieCountText != null)
            blueZombieCountText.text = ": " + blueZombieCount;
        if (yellowZombieCountText != null)
            yellowZombieCountText.text = ": " + yellowZombieCount;
    }

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

            if (bgmAudioSource != null)
                bgmAudioSource.volume = Mathf.Lerp(initialVolume, 0f, t);

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
