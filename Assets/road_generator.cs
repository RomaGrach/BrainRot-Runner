using System.Collections.Generic;
using UnityEngine;

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

    // Internals
    private Queue<GameObject> activeSegments = new Queue<GameObject>();
    private Transform lastEndPoint;  // where next segment will spawn

    void Start()
    {
        if (segmentPrefabs == null || segmentPrefabs.Length == 0)
        {
            Debug.LogError("Road Generator: No segment prefabs assigned!");
            enabled = false;
            return;
        }

        // First spawn uses this object's own position
        lastEndPoint = this.transform;
        for (int i = 0; i < maxActiveSegments; i++)
            SpawnNextSegment();
    }

    void Update()
    {
        if (playerTransform == null) return;

        // If player is getting close enough to the last endpoint, spawn one more
        float dist = lastEndPoint.position.z - playerTransform.position.z;
        if (dist < spawnAheadDistance)
            SpawnNextSegment();
    }

    private void SpawnNextSegment()
    {
        // Pick a random prefab
        GameObject prefab = segmentPrefabs[Random.Range(0, segmentPrefabs.Length)];

        // Instantiate at the last endpoint's world position & rotation (no parent)
        GameObject seg = Instantiate(prefab, lastEndPoint.position, lastEndPoint.rotation);

        // Enqueue for pooling
        activeSegments.Enqueue(seg);

        // Find the child transform tagged as the next point
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

        // Update endpoint for the next spawn
        lastEndPoint = next;

        // If we're over the pool size, destroy the oldest segment
        if (activeSegments.Count > maxActiveSegments)
        {
            GameObject old = activeSegments.Dequeue();
            Destroy(old);
        }
    }
}
