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

    // 内部保持（必要なら参照用）
    private int humanCount = 0;
    private int blueZombieCount = 0;
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
        RecountAndUpdateUI();
    }

    /// <summary>
    /// タグで人数を再集計してUIへ反映（常に正確）
    /// </summary>
    public void RecountAndUpdateUI()
    {
        humanCount = SafeCountByTag("Human");
        blueZombieCount = SafeCountByTag("BlueZombie");
        yellowZombieCount = SafeCountByTag("YellowZombie");

        UpdateCountUI();
    }

    /// <summary>
    /// 既存呼び出し互換用：感染後に呼ぶと再集計する
    /// </summary>
    public void OnHumanInfected(bool becameBlue)
    {
        RecountAndUpdateUI();

        // チュートリアルコメント非表示
        if (tutorialComments != null)
        {
            foreach (var comment in tutorialComments)
                if (comment) comment.SetActive(false);
        }

        // スタートボタン表示
        if (startButton != null)
            startButton.gameObject.SetActive(true);
    }

    private int SafeCountByTag(string tag)
    {
        try { return GameObject.FindGameObjectsWithTag(tag).Length; }
        catch { return 0; } // タグ未登録時の保護
    }

    private void UpdateCountUI()
    {
        if (humanCountText != null) humanCountText.text = ": " + humanCount;
        if (blueZombieCountText != null) blueZombieCountText.text = ": " + blueZombieCount;
        if (yellowZombieCountText != null) yellowZombieCountText.text = ": " + yellowZombieCount;
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