using UnityEngine;
using UnityEngine.SceneManagement;

public class DifficultySelector : MonoBehaviour
{
    public void OnClickEasy()
    {
        GameSettings.cpuZombieCount = 1;
        SceneManager.LoadScene("GamePlayScene"); // ゲームプレイシーン名に置き換えてください
    }

    public void OnClickNormal()
    {
        GameSettings.cpuZombieCount = 3;
        SceneManager.LoadScene("GamePlayScene");
    }

    public void OnClickHard()
    {
        GameSettings.cpuZombieCount = 5;
        SceneManager.LoadScene("GamePlayScene");
    }
}
