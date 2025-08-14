using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DifficultySelector : MonoBehaviour
{
    [Header("効果音設定")]
    public AudioSource sfxAudioSource; // 効果音再生用AudioSource
    public AudioClip clickSound;       // ボタン押下時の効果音

    [Header("シーン遷移フェード時間")]
    public float sceneLoadDelay = 0.3f;

    // ----------------- 難易度ボタン -----------------
    public void OnClickEasy()
    {
        GameSettings.cpuZombieCount = 1;
        PlayClickAndLoadScene("OP02");
    }

    public void OnClickNormal()
    {
        GameSettings.cpuZombieCount = 3;
        PlayClickAndLoadScene("OP02");
    }

    public void OnClickHard()
    {
        GameSettings.cpuZombieCount = 5;
        PlayClickAndLoadScene("OP02");
    }

    // ----------------- 共通処理 -----------------
    private void PlayClickAndLoadScene(string sceneName)
    {
        // 効果音を鳴らす
        if (sfxAudioSource != null && clickSound != null)
        {
            sfxAudioSource.PlayOneShot(clickSound);
        }

        // 音が鳴る間に遅延してシーン遷移
        StartCoroutine(LoadSceneWithDelay(sceneName, sceneLoadDelay));
    }

    private IEnumerator LoadSceneWithDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }
}
