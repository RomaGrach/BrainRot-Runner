using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MagnetController : MonoBehaviour
{
    [Header("Pickup Settings")]
    [Tooltip("Small trigger collider to pick up the magnet by the player")]
    public Collider pickupCollider;

    [Header("Attraction Settings")]
    [Tooltip("Sphere trigger collider for attracting coins")]
    public Collider attractionCollider;
    [Tooltip("Initial speed of coins when they start moving towards the player")]
    public float initialAttractSpeed = 2f;
    [Tooltip("Acceleration applied to coins each second")]
    public float attractAcceleration = 1f;
    [Tooltip("Offset from player's position where coins should be attracted to")]
    public Vector3 coinAttractOffset = Vector3.zero;

    [Header("Movement to Player")]
    [Tooltip("Height above player where the magnet moves to")]
    public float heightAbovePlayer = 2f;
    [Tooltip("Duration in seconds for the magnet to move above the player position")]
    public float moveDuration = 0.5f;

    [Header("Lifetime")]
    [Tooltip("Total duration in seconds for which the magnet remains active after pickup")]
    public float magnetDuration = 5f;

    private Rigidbody _rb;
    private bool _isPicked = false;
    private List<CoinData> _coins = new List<CoinData>();
    private Coroutine _lifetimeCoroutine;
    private Transform _playerTransform;

    private struct CoinData { public Transform transform; public float speed; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;

        if (pickupCollider != null) pickupCollider.isTrigger = true;
        if (attractionCollider != null)
        {
            attractionCollider.isTrigger = true;
            attractionCollider.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isPicked && other.CompareTag("Player"))
        {
            _isPicked = true;
            if (pickupCollider != null) pickupCollider.enabled = false;
            if (attractionCollider != null) attractionCollider.enabled = true;

            _playerTransform = other.transform;
            transform.SetParent(_playerTransform, false);

            StartCoroutine(MoveAbovePlayer());
            _lifetimeCoroutine = StartCoroutine(MagnetLifetime());
        }
        else if (_isPicked && other.CompareTag("coin"))
        {
            Transform coinTransform = other.transform;
            if (coinTransform != null && !_coins.Exists(c => c.transform == coinTransform))
            {
                _coins.Add(new CoinData { transform = coinTransform, speed = initialAttractSpeed });
            }
        }
    }

    private IEnumerator MoveAbovePlayer()
    {
        Vector3 startLocal = transform.localPosition;
        Vector3 endLocal = new Vector3(0f, heightAbovePlayer, 0f);
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            transform.localPosition = Vector3.Lerp(startLocal, endLocal, t);
            yield return null;
        }

        transform.localPosition = endLocal;
        StartCoroutine(AttractCoins());
    }

    private IEnumerator AttractCoins()
    {
        while (true)
        {
            for (int i = _coins.Count - 1; i >= 0; i--)
            {
                var data = _coins[i];
                if (data.transform == null)
                {
                    _coins.RemoveAt(i);
                    continue;
                }

                data.speed += attractAcceleration * Time.deltaTime;
                Vector3 targetPos = _playerTransform.position + coinAttractOffset;
                Vector3 dir = (targetPos - data.transform.position).normalized;
                data.transform.position += dir * data.speed * Time.deltaTime;
                _coins[i] = data;
            }

            yield return null;
        }
    }

    private IEnumerator MagnetLifetime()
    {
        yield return new WaitForSeconds(magnetDuration);
        Destroy(gameObject);
    }
}
