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
    public AudioSource bgmSource;
    public AudioClip bgmClip;
    public AudioSource seSource;
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
        if (fadeImage != null)
            fadeImage.gameObject.SetActive(false);

        if (bgmSource != null && bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.volume = 1f;
            bgmSource.Play();
        }

        backToTitleButton.gameObject.SetActive(false);
        backToTitleButton.onClick.RemoveAllListeners();
        backToTitleButton.onClick.AddListener(OnBackButtonPressed);

        StartCoroutine(ShowResultsCoroutine());
    }

    IEnumerator ShowResultsCoroutine()
    {
        // 青ゾンビ
        playerConvertedText.text = "青ゾンビの数: " + ResultData.playerConverted;
        PlaySFX(tuika01Clip);
        yield return new WaitForSeconds(1.5f);

        // 黄ゾンビ
        cpuConvertedText.text = "黄ゾンビの数: " + ResultData.cpuConverted;
        PlaySFX(tuika01Clip);
        yield return new WaitForSeconds(1.5f);

        // 残り住人
        humanLeftText.text = "残り住人: " + ResultData.humanLeft;
        PlaySFX(tuika01Clip);
        yield return new WaitForSeconds(2f);

        // 勝敗表示
        if (ResultData.isDraw)
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

        backToTitleButton.gameObject.SetActive(true);
    }

    void OnBackButtonPressed()
    {
        PlaySFX(backButtonClip);

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

            if (bgmSource != null)
                bgmSource.volume = 1f - Mathf.Clamp01(timer / fadeDuration);

            yield return null;
        }

        if (bgmSource != null)
        {
            bgmSource.Stop();
            bgmSource.volume = 1f;
        }

        SceneManager.LoadScene("Title");
    }

    void PlaySFX(AudioClip clip)
    {
        if (clip == null || seSource == null) return;
        seSource.PlayOneShot(clip);
    }
}
