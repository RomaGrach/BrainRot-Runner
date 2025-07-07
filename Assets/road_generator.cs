using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Генерирует дорожные сегменты впереди игрока по дизайну "бесконечной дороги".
/// До вызова StartGame() ничего не генерируется.
/// </summary>
public class road_generator : MonoBehaviour
{
    [Header("Prefabs & Pooling")]
    [Tooltip("Array of road segment prefabs. Each prefab must have a child with the 'Next Point' tag.")]
    public GameObject[] segmentPrefabs;
    [Tooltip("How many segments to keep active at once.")]
    public int maxActiveSegments = 7;

    [Header("Spawning")]
    [Tooltip("Player Transform used to decide when to spawn new segments.")]
    public Transform playerTransform;
    [Tooltip("Distance (in Z) ahead of the player at which to spawn the next segment.")]
    public float spawnAheadDistance = 50f;

    [Header("Locator Tag")]
    [Tooltip("Tag of the child object inside each segment that marks the next spawn point.")]
    public string nextPointTag = "Next Point";

    [Header("Start Point")]
    [Tooltip("Начальная точка, от которой будет начинаться генерация сегментов.")]
    public Transform startPoint;

    // Внутренние поля
    private Queue<GameObject> activeSegments = new Queue<GameObject>();
    private Transform lastEndPoint;  // Точка, откуда спавнить следующий сегмент
    private bool isGameStarted = false;

    void Start()
    {
        if (segmentPrefabs == null || segmentPrefabs.Length == 0)
        {
            Debug.LogError("Road Generator: No segment prefabs assigned!");
            enabled = false;
            return;
        }

        activeSegments.Clear();
        isGameStarted = false;
    }

    void Update()
    {
        if (!isGameStarted || playerTransform == null)
            return;

        float dist = lastEndPoint.position.z - playerTransform.position.z;
        if (dist < spawnAheadDistance)
            SpawnNextSegment();
    }

    /// <summary>
    /// Запускает генерацию сегментов: первые maxActiveSegments штук.
    /// Вызывать извне, когда игра действительно стартует.
    /// </summary>
    public void StartGame()
    {
        if (isGameStarted)
            return;

        lastEndPoint = (startPoint != null) ? startPoint : this.transform;
        for (int i = 0; i < maxActiveSegments; i++)
            SpawnNextSegment();

        isGameStarted = true;
    }

    /// <summary>
    /// Останавливает генерацию новых сегментов.
    /// </summary>
    public void StopGame()
    {
        if (!isGameStarted)
            return;

        isGameStarted = false;
    }

    /// <summary>
    /// Очищает все текущие сгенерированные сегменты.
    /// </summary>
    public void ClearGame()
    {
        // Удаляем все активные сегменты
        while (activeSegments.Count > 0)
        {
            GameObject old = activeSegments.Dequeue();
            if (old != null)
                Destroy(old);
        }
    }

    private void SpawnNextSegment()
    {
        GameObject prefab = segmentPrefabs[Random.Range(0, segmentPrefabs.Length)];
        GameObject seg = Instantiate(prefab, lastEndPoint.position, lastEndPoint.rotation);
        activeSegments.Enqueue(seg);

        Transform next = null;
        foreach (Transform t in seg.GetComponentsInChildren<Transform>(true))
        {
            if (t.CompareTag(nextPointTag))
            {
                next = t;
                break;
            }
        }
        if (next == null)
        {
            Debug.LogError($"Road Generator: Prefab '{prefab.name}' needs a child tagged '{nextPointTag}'.");
            return;
        }

        lastEndPoint = next;

        if (activeSegments.Count > maxActiveSegments)
        {
            GameObject old = activeSegments.Dequeue();
            Destroy(old);
        }
    }
}
