using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro 用

public class ResultSceneManager : MonoBehaviour
{
    [Header("人数表示用テキスト（TextMeshProUGUI）")]
    public TextMeshProUGUI humanLeftText;
    public TextMeshProUGUI playerConvertedText;
    public TextMeshProUGUI cpuConvertedText;

    [Header("背景画像を表示するUI（Imageコンポーネント）")]
    public Image backgroundImage;

    [Header("勝利時の背景スプライト")]
    public Sprite winSprite;

    [Header("敗北時の背景スプライト")]
    public Sprite loseSprite;

    [Header("タイトルに戻るボタン")]
    public Button backToTitleButton;

    void Start()
    {
        // Null チェック
        if (humanLeftText == null) Debug.LogError("humanLeftText が設定されていません");
        if (playerConvertedText == null) Debug.LogError("playerConvertedText が設定されていません");
        if (cpuConvertedText == null) Debug.LogError("cpuConvertedText が設定されていません");
        if (backgroundImage == null) Debug.LogError("backgroundImage が設定されていません");
        if (backToTitleButton == null) Debug.LogWarning("backToTitleButton が設定されていません");

        // ResultData から値を取得して表示
        if (humanLeftText != null)
            humanLeftText.text = "残り住人: " + ResultData.humanLeft;
        if (playerConvertedText != null)
            playerConvertedText.text = "青ゾンビの数: " + ResultData.playerConverted;
        if (cpuConvertedText != null)
            cpuConvertedText.text = "黄ゾンビの数: " + ResultData.cpuConverted;

        // 勝敗によって背景を差し替え
        if (backgroundImage != null)
            backgroundImage.sprite = ResultData.isWin ? winSprite : loseSprite;

        // ボタンにクリックイベントを追加
        if (backToTitleButton != null)
        {
            backToTitleButton.onClick.AddListener(ReturnToTitle);
        }
    }

    // タイトルシーンに戻る
    void ReturnToTitle()
    {
        SceneManager.LoadScene("TitleScene"); // タイトルシーン名を確認
    }
}
