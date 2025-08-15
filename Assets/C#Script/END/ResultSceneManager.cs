using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResultSceneManager : MonoBehaviour
{
    [Header("人数表示用テキスト")]
    public Text humanLeftText;
    public Text playerConvertedText;
    public Text cpuConvertedText;

    [Header("背景画像を表示するUI（Imageコンポーネント）")]
    public Image backgroundImage;

    [Header("勝利/敗北/引き分けの背景スプライト")]
    public Sprite winSprite;
    public Sprite loseSprite;
    public Sprite drawSprite;

    [Header("タイトルに戻るボタン")]
    public Button backToTitleButton;

    [Header("BGMとSE")]
    public AudioSource bgmSource;        // BGM用AudioSource
    public AudioClip bgmClip;           // BGMクリップ
    public AudioSource seSource;        // 効果音用AudioSource
    public AudioClip winClip;
    public AudioClip loseClip;
    public AudioClip drawClip;
    public AudioClip backButtonClip;
    public AudioClip tuika01Clip;

    [Header("フェード用Image（黒）")]
    public Image fadeImage;
    public float fadeDuration = 1f;

    void Start()
    {
        // UI設定確認
        if (humanLeftText == null || playerConvertedText == null || cpuConvertedText == null)
            Debug.LogError("ResultSceneManager: テキストUIが設定されていません！");
        if (backgroundImage == null)
            Debug.LogError("ResultSceneManager: 背景Imageが設定されていません！");
        if (backToTitleButton == null)
            Debug.LogError("ResultSceneManager: タイトルに戻るボタンが設定されていません！");
        if (fadeImage != null)
            fadeImage.gameObject.SetActive(false);

        // BGM再生（ループ）
        if (bgmSource != null && bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        // ボタンは最初非表示
        backToTitleButton.gameObject.SetActive(false);
        backToTitleButton.onClick.RemoveAllListeners();
        backToTitleButton.onClick.AddListener(OnBackButtonPressed);

        // 結果表示開始
        StartCoroutine(ShowResultsCoroutine());
    }

    IEnumerator ShowResultsCoroutine()
    {
        // 文字3回表示（1.5秒間隔でSEを鳴らす）
        playerConvertedText.text = "青ゾンビの数: " + ResultData.playerConverted;
        PlaySFX(tuika01Clip); // ここは任意の文字出現音
        yield return new WaitForSeconds(1.5f);

        cpuConvertedText.text = "黄ゾンビの数: " + ResultData.cpuConverted;
        PlaySFX(tuika01Clip);
        yield return new WaitForSeconds(1.5f);

        humanLeftText.text = "残り住人: " + ResultData.humanLeft;
        PlaySFX(tuika01Clip);
        yield return new WaitForSeconds(1.5f);

        // 文字3回表示後、2秒待って背景画像表示＆SE
        yield return new WaitForSeconds(1f);

        bool isDraw = ResultData.playerConverted == ResultData.cpuConverted;
        if (isDraw)
        {
            backgroundImage.sprite = drawSprite;
            PlaySFX(drawClip);
        }
        else if (ResultData.isWin)
        {
            backgroundImage.sprite = winSprite;
            PlaySFX(winClip);
        }
        else
        {
            backgroundImage.sprite = loseSprite;
            PlaySFX(loseClip);
        }

        // タイトルボタンを表示
        backToTitleButton.gameObject.SetActive(true);
    }


    void OnBackButtonPressed()
    {
        // ボタンSE
        PlaySFX(backButtonClip);

        // フェードアウトしてタイトルに戻る
        if (fadeImage != null)
            StartCoroutine(FadeOutAndLoadTitle());
        else
            SceneManager.LoadScene("Title");
    }

    IEnumerator FadeOutAndLoadTitle()
    {
        fadeImage.gameObject.SetActive(true);
        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Clamp01(timer / fadeDuration);
            fadeImage.color = color;

            // BGMフェードアウト
            if (bgmSource != null)
                bgmSource.volume = 1f - Mathf.Clamp01(timer / fadeDuration);

            yield return null;
        }

        SceneManager.LoadScene("Title");
    }

    void PlaySFX(AudioClip clip)
    {
        if (clip == null || seSource == null) return;
        seSource.PlayOneShot(clip);
    }
}
