using UnityEngine;
using UnityEngine.SceneManagement;

public class DifficultySelector : MonoBehaviour
{
    public void OnClickEasy()
    {
        GameSettings.cpuZombieCount = 1;
        SceneManager.LoadScene("OP02"); // ゲームプレイシーン名に置き換えてください
    }

    public void OnClickNormal()
    {
        GameSettings.cpuZombieCount = 3;
        SceneManager.LoadScene("OP02");
    }

    public void OnClickHard()
    {
        GameSettings.cpuZombieCount = 5;
        SceneManager.LoadScene("OP02");
    }
}
