using UnityEngine;

/// <summary>
/// Один скрипт для учёта монеток и расстояния.
/// Вешай на игрока (Collider с IsTrigger = true + Rigidbody),
/// вызывай StartGame() при старте и StopGame() при поражении.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    [Header("Текущие показатели (видно в инспекторе)")]
    public int CoinCount = 0;
    public float DistanceScore = 0f;

    [Header("Настройки")]
    [Tooltip("Сколько очков за 1 монетку")]
    [SerializeField] private int pointsPerCoin = 1;
    [Tooltip("Коэффициент для перевода времени в дистанцию")]
    [SerializeField] private float distanceMultiplier = 1f;

    public string Coin = "Coin";

    // Флаг, считать ли сейчас
    private bool isGameActive = false;

    private void Update()
    {
        if (!isGameActive) return;

        // Добавляем дистанцию со временем
        DistanceScore += distanceMultiplier * Time.deltaTime;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!isGameActive) return;

        if (other.CompareTag(Coin))
        {
            // Собрали монетку
            CoinCount += pointsPerCoin;
            Destroy(other.gameObject);
        }
        // можно дописать другие теги тут же
    }

    /// <summary>
    /// Запустить сбор очков (сбросит CoinCount и DistanceScore).
    /// </summary>
    public void StartGame()
    {
        CoinCount = 0;
        DistanceScore = 0f;
        isGameActive = true;
    }

    /// <summary>
    /// Остановить сбор очков (например, при смерти игрока).
    /// </summary>
    public void StopGame()
    {
        isGameActive = false;
    }

    private void Start()
    {
        StartGame();
    }
}
