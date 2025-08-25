using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MiniMapController : MonoBehaviour
{
    [Header("MiniMap設定")]
    public GameObject miniMapRoot;
    public GameObject humanIconPrefab;
    public GameObject blueZombieIconPrefab;
    public GameObject yellowZombieIconPrefab;
    public GameObject playerIconPrefab;

    public Vector2 worldMin = new Vector2(-50f, -50f);
    public Vector2 worldMax = new Vector2(50f, 50f);

    private Dictionary<GameObject, GameObject> iconMap = new Dictionary<GameObject, GameObject>();
    private bool mapActive = false;
    private RectTransform miniMapRect;

    private void Start()
    {
        if (miniMapRoot != null)
        {
            miniMapRoot.SetActive(false);
            miniMapRect = miniMapRoot.GetComponent<RectTransform>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            ToggleMap();

        if (!mapActive) return;

        RefreshIcons();
    }

    public void ToggleMap()
    {
        mapActive = !mapActive;
        if (miniMapRoot != null) miniMapRoot.SetActive(mapActive);
        Time.timeScale = mapActive ? 0f : 1f;
    }

    private void RefreshIcons()
    {
        var humans = GameManager.Instance?.Humans;
        var blueZombies = GameManager.Instance?.BlueZombies;
        var yellowZombies = GameManager.Instance?.YellowZombies;

        UpdateIconsSafe(humans, humanIconPrefab);
        UpdateIconsSafe(blueZombies, blueZombieIconPrefab);
        UpdateIconsSafe(yellowZombies, yellowZombieIconPrefab);
        UpdatePlayerIcon();

        HandleTransformingHumans(humans, blueZombies, yellowZombies);
        RemoveDestroyedIcons();
    }

    private void UpdateIconsSafe(IReadOnlyList<GameObject> list, GameObject prefab)
    {
        if (list == null || prefab == null || miniMapRect == null) return;

        foreach (var obj in list)
        {
            if (obj == null) continue;

            if (!iconMap.ContainsKey(obj))
            {
                GameObject icon = Instantiate(prefab, miniMapRoot.transform);
                iconMap[obj] = icon;
            }

            UpdateIconPosition(obj, iconMap[obj]);
        }
    }

    private void HandleTransformingHumans(IReadOnlyList<GameObject> humans, IReadOnlyList<GameObject> blueZombies, IReadOnlyList<GameObject> yellowZombies)
    {
        if (humans == null) humans = new List<GameObject>();

        var toRemove = new List<GameObject>();
        foreach (var kvp in iconMap)
        {
            if (kvp.Value == null) continue;

            // 人間アイコンだけ対象
            if (kvp.Value.CompareTag("HumanIcon") && !humans.Contains(kvp.Key))
            {
                toRemove.Add(kvp.Key);
                Destroy(kvp.Value);

                // 青ゾンビなら青アイコン生成
                if (blueZombies != null && blueZombies.Contains(kvp.Key))
                {
                    GameObject icon = Instantiate(blueZombieIconPrefab, miniMapRoot.transform);
                    iconMap[kvp.Key] = icon;
                }
                // 黄ゾンビなら黄アイコン生成
                else if (yellowZombies != null && yellowZombies.Contains(kvp.Key))
                {
                    GameObject icon = Instantiate(yellowZombieIconPrefab, miniMapRoot.transform);
                    iconMap[kvp.Key] = icon;
                }
            }
        }

        foreach (var key in toRemove)
            iconMap.Remove(key);
    }

    private void RemoveDestroyedIcons()
    {
        var toRemove = new List<GameObject>();
        foreach (var kvp in iconMap)
        {
            if (kvp.Key == null || kvp.Value == null)
            {
                if (kvp.Value != null) Destroy(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var key in toRemove)
            iconMap.Remove(key);
    }

    private void UpdatePlayerIcon()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || playerIconPrefab == null || miniMapRect == null) return;

        if (!iconMap.ContainsKey(player))
        {
            GameObject icon = Instantiate(playerIconPrefab, miniMapRoot.transform);
            iconMap[player] = icon;
        }

        UpdateIconPosition(player, iconMap[player]);
    }

    private void UpdateIconPosition(GameObject obj, GameObject icon)
    {
        if (obj == null || icon == null || miniMapRect == null) return;

        Vector3 worldPos = obj.transform.position;
        float mapWidth = miniMapRect.rect.width;
        float mapHeight = miniMapRect.rect.height;

        float normalizedX = (worldPos.x - worldMin.x) / (worldMax.x - worldMin.x);
        float normalizedY = (worldPos.z - worldMin.y) / (worldMax.y - worldMin.y);

        float iconX = normalizedX * mapWidth - mapWidth / 2f;
        float iconY = normalizedY * mapHeight - mapHeight / 2f;

        icon.GetComponent<RectTransform>().localPosition = new Vector3(iconX, iconY, 0f);
    }
}
