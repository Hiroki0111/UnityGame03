using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExplanationUIManager : MonoBehaviour
{


    // ゲーム開始ボタン用
    public void OnClickStartGame()
    {
        SceneManager.LoadScene("GamePlayScene"); // ゲームプレイシーン名に置き換え
    }

    // 難易度選択に戻るボタン用
    public void OnClickBackToDifficultySelect()
    {
        SceneManager.LoadScene("OP01"); // 難易度選択シーン名に置き換え
    }

    // チュートリアルシーンに移動するボタン用
    public void OnClickGoToTutorial()
    {
        SceneManager.LoadScene("tutorial"); // チュートリアルシーン名に置き換え
    }
}
