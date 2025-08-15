using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class ExplanationUIManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource bgmAudio;      // ループ再生するBGM
    public AudioSource buttonSEAudio; // ボタン押下時の効果音

    [Header("UI")]
    public Image blackPanel;          // ブラックアウト用パネル
    public float fadeDuration = 1f;   // フェード時間

    [Header("Scene")]
    public string nextSceneName = "GamePlayScene";

    private void Start()
    {
        if (bgmAudio != null)
        {
            bgmAudio.loop = true;
            bgmAudio.Play();
        }

        if (blackPanel != null)
        {
            blackPanel.color = new Color(0, 0, 0, 0); // 透明で初期化
        }
    }

    // ゲーム開始ボタン
    public void OnClickStartGame()
    {
        PlayButtonTransition();
    }

    private void PlayButtonTransition()
    {
        // SE 再生
        if (buttonSEAudio != null)
            buttonSEAudio.Play();

        Sequence seq = DOTween.Sequence();

        // BGM フェードアウト
        if (bgmAudio != null)
        {
            seq.Join(bgmAudio.DOFade(0f, fadeDuration));
        }

        // ブラックアウト
        if (blackPanel != null)
        {
            seq.Join(blackPanel.DOFade(1f, fadeDuration));
        }

        // フェード完了後にシーン切替
        seq.AppendCallback(() =>
        {
            SceneManager.LoadScene(nextSceneName);
        });
    }

    // 難易度選択に戻るボタン
    public void OnClickBackToDifficultySelect()
    {
        SceneManager.LoadScene("OP01");
    }

    // チュートリアルシーンに移動
    public void OnClickGoToTutorial()
    {
        SceneManager.LoadScene("tutorial");
    }
}
