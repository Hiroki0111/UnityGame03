using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class DOTweenIntroController : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform imageA;
    public RectTransform imageB;
    public RectTransform imageC;
    public RectTransform finalLogo;
    public Image blackPanel;

    [Header("Particles")]
    public ParticleSystem combineParticles;

    [Header("Audio")]
    public AudioSource audioImageFly;    // 画像が飛ぶ音
    public AudioSource audioParticle;    // パーティクル音
    public AudioSource audioLogo;        // ロゴ表示音

    [Header("Animation Settings")]
    public Vector3 targetPosA;
    public Vector3 targetPosB;
    public Vector3 targetPosC;
    public Vector3 finalLogoEndPos;

    [Header("Individual Start Offsets (カメラ後ろから飛んでくる位置)")]
    public Vector3 startOffsetA = new Vector3(0, 0, -800f);
    public Vector3 startOffsetB = new Vector3(0, 0, -800f);
    public Vector3 startOffsetC = new Vector3(0, 0, -800f);
    public Vector3 logoStartOffset = new Vector3(0, 0, -500f);

    [Header("Durations")]
    public float moveDuration = 0.8f;        // カメラ後ろからUIまで
    public float stayDuration = 0.5f;        // UI前での滞在
    public float delayBetweenImages = 0.5f;  // パーティクル後の次画像まで
    public float logoMoveDuration = 2.0f;
    public float fadeDuration = 1.0f;
    public string nextSceneName;

    private CanvasGroup logoCg;

    private void Start()
    {
        if (blackPanel != null)
            blackPanel.color = Color.black;

        // ロゴ初期設定
        if (finalLogo != null)
        {
            logoCg = finalLogo.GetComponent<CanvasGroup>();
            if (logoCg == null)
                logoCg = finalLogo.gameObject.AddComponent<CanvasGroup>();
            logoCg.alpha = 1f;
            finalLogo.gameObject.SetActive(false);
        }

        PlayIntro();
    }

    private void PlayIntro()
    {
        Sequence seq = DOTween.Sequence();

        // --- 画像A ---
        imageA.position += startOffsetA;
        seq.AppendCallback(() =>
        {
            imageA.gameObject.SetActive(true);
            if (audioImageFly != null) audioImageFly.Play();
        });
        seq.Append(imageA.DOMove(targetPosA, moveDuration).SetEase(Ease.OutQuad));
        seq.AppendInterval(stayDuration);
        seq.AppendCallback(() => imageA.gameObject.SetActive(false));
        seq.AppendCallback(() =>
        {
            if (combineParticles != null) combineParticles.Play();
            if (audioParticle != null) audioParticle.Play();
        });
        seq.AppendInterval(delayBetweenImages);

        // --- 画像B ---
        imageB.position += startOffsetB;
        seq.AppendCallback(() =>
        {
            imageB.gameObject.SetActive(true);
            if (audioImageFly != null) audioImageFly.Play();
        });
        seq.Append(imageB.DOMove(targetPosB, moveDuration).SetEase(Ease.OutQuad));
        seq.AppendInterval(stayDuration);
        seq.AppendCallback(() => imageB.gameObject.SetActive(false));
        seq.AppendCallback(() =>
        {
            if (combineParticles != null) combineParticles.Play();
            if (audioParticle != null) audioParticle.Play();
        });
        seq.AppendInterval(delayBetweenImages);

        // --- 画像C ---
        imageC.position += startOffsetC;
        seq.AppendCallback(() =>
        {
            imageC.gameObject.SetActive(true);
            if (audioImageFly != null) audioImageFly.Play();
        });
        seq.Append(imageC.DOMove(targetPosC, moveDuration).SetEase(Ease.OutQuad));
        seq.AppendInterval(stayDuration);
        seq.AppendCallback(() => imageC.gameObject.SetActive(false));
        seq.AppendCallback(() =>
        {
            if (combineParticles != null) combineParticles.Play();
            if (audioParticle != null) audioParticle.Play();
        });
        seq.AppendInterval(delayBetweenImages);

        // --- ロゴ ---
        seq.AppendCallback(() =>
        {
            finalLogo.gameObject.SetActive(true);
            finalLogo.position += logoStartOffset;
            if (audioLogo != null) audioLogo.Play();
        });

        // カメラに向かってロゴ移動＆フェードアウト
        Vector3 cameraTargetPos = Camera.main.transform.position + Camera.main.transform.forward * 0.5f;
        seq.Append(finalLogo.DOMove(cameraTargetPos, logoMoveDuration).SetEase(Ease.InOutQuad));

        // ロゴの透明度フェードと音フェードを同時に実行
        seq.Join(logoCg.DOFade(0f, logoMoveDuration));
        if (audioLogo != null)
            seq.Join(audioLogo.DOFade(0f, logoMoveDuration));

        // --- ブラックアウト ---
        if (blackPanel != null)
            seq.Append(blackPanel.DOFade(1f, fadeDuration));

        // --- シーン切替 ---
        seq.AppendCallback(() => SceneManager.LoadScene(nextSceneName));
    }
    public void GoToNextScene()
    { if (!string.IsNullOrEmpty(nextSceneName)) SceneManager.LoadScene("OP01"); }
}
