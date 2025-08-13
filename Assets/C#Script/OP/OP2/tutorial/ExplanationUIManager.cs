using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExplanationUIManager : MonoBehaviour
{
    public Text cpuCountText;  // CPUゾンビ数表示用テキスト

    void Start()
    {
        cpuCountText.text = $"CPUゾンビ数: {GameSettings.cpuZombieCount}体";
    }

    // ゲーム開始ボタン用
    public void OnClickStartGame()
    {
        SceneManager.LoadScene("GamePlayScene"); // ゲームプレイシーン名に置き換え
    }

    // 難易度選択に戻るボタン用
    public void OnClickBackToDifficultySelect()
    {
        SceneManager.LoadScene("DifficultySelectScene"); // 難易度選択シーン名に置き換え
    }

    // チュートリアルシーンに移動するボタン用
    public void OnClickGoToTutorial()
    {
        SceneManager.LoadScene("TutorialScene"); // チュートリアルシーン名に置き換え
    }
}
