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

    [Header("勝利時の背景スプライト")]
    public Sprite winSprite;

    [Header("敗北時の背景スプライト")]
    public Sprite loseSprite;

    [Header("タイトルに戻るボタン")]
    public Button backToTitleButton;

    void Start()
    {
        // 各フィールドが Inspector にセットされているかチェック
        if (humanLeftText == null || playerConvertedText == null || cpuConvertedText == null)
        {
            Debug.LogError("ResultSceneManager: テキストUIが設定されていません！");
            return;
        }

        if (backgroundImage == null)
        {
            Debug.LogError("ResultSceneManager: 背景Imageが設定されていません！");
            return;
        }

        if (backToTitleButton == null)
        {
            Debug.LogError("ResultSceneManager: タイトルに戻るボタンが設定されていません！");
            return;
        }

        // ResultData から値を取得して表示
        humanLeftText.text = "残り住人: " + ResultData.humanLeft;
        playerConvertedText.text = "青ゾンビの数: " + ResultData.playerConverted;
        cpuConvertedText.text = "黄ゾンビの数: " + ResultData.cpuConverted;

        // 勝敗によって背景を差し替え
        backgroundImage.sprite = ResultData.isWin ? winSprite : loseSprite;

        // ボタンにクリックイベントを追加
        backToTitleButton.onClick.RemoveAllListeners(); // 念のため既存リスナーを削除
        backToTitleButton.onClick.AddListener(ReturnToTitle);
    }

    // タイトルシーンに戻る
    void ReturnToTitle()
    {
        SceneManager.LoadScene("OP01"); // ここにタイトルシーン名を入力
    }
}
