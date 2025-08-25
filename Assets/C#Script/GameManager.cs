using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private readonly List<GameObject> humans = new List<GameObject>();
    private readonly List<GameObject> blueZombies = new List<GameObject>();
    private readonly List<GameObject> yellowZombies = new List<GameObject>();

    [Header("UI Text")]
    public Text humanCountText;
    public Text blueZombieCountText;
    public Text yellowZombieCountText;
    public Text timerText;

    private float gameTimer = 180f;
    private bool gameEnded = false;
    private bool isGameEnding = false;

    private bool uiDirty = true; // UI更新フラグ

    [Header("演出関連")]
    public AudioSource mainBGM;
    public AudioSource endSFX;
    public AudioSource endBGM;
    public Image fadeImage;
    public float fadeDuration = 2f;

    public GameObject Player { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        InitializeCounts();
        UpdateUI();
    }

    private void Update()
    {
        if (gameEnded) return;

        gameTimer -= Time.deltaTime;
        if (gameTimer < 0f) gameTimer = 0f;

        if (uiDirty) UpdateUI();

        if (gameTimer <= 0 && !isGameEnding)
            StartCoroutine(EndGameSequence("時間切れ！"));
        else if (humans.Count <= 0 && !isGameEnding)
            StartCoroutine(EndGameSequence("住民が全滅！"));
    }

    private void InitializeCounts()
    {
        humans.Clear();
        blueZombies.Clear();
        yellowZombies.Clear();

        foreach (var h in GameObject.FindGameObjectsWithTag("Human")) RegisterHuman(h);
        foreach (var z in GameObject.FindGameObjectsWithTag("BlueZombie")) RegisterBlueZombie(z);
        foreach (var z in GameObject.FindGameObjectsWithTag("YellowZombie")) RegisterYellowZombie(z);

        Player = GameObject.FindGameObjectWithTag("Player");
    }

    private void UpdateUI()
    {
        if (humanCountText != null) humanCountText.text = $"住民: {humans.Count}";
        if (blueZombieCountText != null) blueZombieCountText.text = $"青ゾンビ: {blueZombies.Count + (Player != null ? 1 : 0)}";
        if (yellowZombieCountText != null) yellowZombieCountText.text = $"黄ゾンビ: {yellowZombies.Count}";
        if (timerText != null)
        {
            int m = Mathf.FloorToInt(gameTimer / 60f);
            int s = Mathf.FloorToInt(gameTimer % 60f);
            timerText.text = $"{m:00}:{s:00}";
        }

        uiDirty = false; // 更新完了
    }

    public void RegisterHuman(GameObject human)
    {
        if (human == null) return;
        if (!humans.Contains(human)) { humans.Add(human); uiDirty = true; }
    }
    public void UnregisterHuman(GameObject human)
    {
        if (human == null) return;
        if (humans.Contains(human)) { humans.Remove(human); uiDirty = true; }
    }

    public void RegisterBlueZombie(GameObject zombie)
    {
        if (zombie == null) return;
        if (!blueZombies.Contains(zombie)) { blueZombies.Add(zombie); uiDirty = true; }
    }
    public void UnregisterBlueZombie(GameObject zombie)
    {
        if (zombie == null) return;
        if (blueZombies.Contains(zombie)) { blueZombies.Remove(zombie); uiDirty = true; }
    }

    public void RegisterYellowZombie(GameObject zombie)
    {
        if (zombie == null) return;
        if (!yellowZombies.Contains(zombie)) { yellowZombies.Add(zombie); uiDirty = true; }
    }
    public void UnregisterYellowZombie(GameObject zombie)
    {
        if (zombie == null) return;
        if (yellowZombies.Contains(zombie)) { yellowZombies.Remove(zombie); uiDirty = true; }
    }

    public IReadOnlyList<GameObject> Humans => humans;
    public IReadOnlyList<GameObject> BlueZombies => blueZombies;
    public IReadOnlyList<GameObject> YellowZombies => yellowZombies;

    public IEnumerator EndGameSequence(string reason)
    {
        if (isGameEnding) yield break;
        isGameEnding = true;
        gameEnded = true;

        if (mainBGM != null) mainBGM.Stop();
        if (endSFX != null) { endSFX.Play(); yield return new WaitForSeconds(endSFX.clip.length); }
        if (endBGM != null) { endBGM.Play(); yield return new WaitForSeconds(1f); }

        if (fadeImage != null)
        {
            float t = 0f;
            Color c = fadeImage.color;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(0, 1, t / fadeDuration);
                fadeImage.color = c;
                yield return null;
            }
        }

        int humansLeft = humans.Count;
        int blueCount = blueZombies.Count + (Player != null ? 1 : 0);
        int yellowCount = yellowZombies.Count;

        ResultData.humanLeft = humansLeft;
        ResultData.playerConverted = blueCount;
        ResultData.cpuConverted = yellowCount;
        ResultData.isWin = blueCount > yellowCount;
        ResultData.isDraw = blueCount == yellowCount;

        SceneManager.LoadScene("ResultScene");
    }
}
