using TMPro;
using UnityEngine;
using YG;

/// <summary>
/// Один скрипт для учёта монеток и расстояния.
/// Вешать на игрока (Collider с IsTrigger = true + Rigidbody).
/// Вызывать StartGame() при старте и StopGame() при поражении.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    [Header("Current Metrics (Visible in Inspector)")]
    public int CoinCount = 0;
    public float DistanceScore = 0f;

    [Header("Settings")]
    [Tooltip("Сколько очков за 1 монетку")]
    [SerializeField]
    private int pointsPerCoin = 1;
    [Tooltip("Коэффициент для перевода времени в дистанцию")]
    [SerializeField]
    private float distanceMultiplier = 1f;

    [Header("UI References")]
    public TextMeshProUGUI scoreMenuRecord;
    public TextMeshProUGUI scoreGame;
    public TextMeshProUGUI scoreAfterGame;

    public TextMeshProUGUI moneyMenu;
    public TextMeshProUGUI moneyGame;
    public TextMeshProUGUI moneyAfterGame;

    // Flag to control scoring
    private bool isGameActive = false;

    private void Start()
    {
        // Загружаем сохранённые данные
        // YG2.saves уже инициализировано SDK

        // Обновляем UI в меню сразу при старте
        UpdateMenuUI();
    }

    private void Update()
    {
        if (!isGameActive)
            return;

        // Добавляем дистанцию со временем
        DistanceScore += distanceMultiplier * Time.deltaTime;
        UpdateGameUI();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!isGameActive)
            return;

        if (other.CompareTag("coin"))
        {
            // Собрали монетку
            CoinCount += pointsPerCoin;
            Destroy(other.gameObject);
            UpdateGameUI();
        }
    }

    /// <summary>
    /// Запустить сбор очков: сбросить CoinCount и DistanceScore.
    /// </summary>
    public void StartGame()
    {
        // Сбрасываем текущие значения run
        CoinCount = 0;
        DistanceScore = 0f;
        isGameActive = true;

        // Во время игры отображаем только значения run
        UpdateGameUI();
    }

    /// <summary>
    /// Остановить сбор очков (например, при смерти игрока).
    /// </summary>
    public void StopGame()
    {
        isGameActive = false;

        // Сохраняем run-величины в YG2
        YG2.saves.coins += CoinCount;
        if ((int)DistanceScore > YG2.saves.MaxScore)
        {
            YG2.saves.MaxScore = (int)DistanceScore;
        }
        // Принудительно сохраняем прогресс
        YG2.SaveProgress();

        // Обновим UI после игры
        UpdateAfterGameUI();

        // Обновим UI меню, чтобы показать новые сохранённые значения
        UpdateMenuUI();
    }

    /// <summary>
    /// Обновление UI в меню (вызывается при старте сцены или после StopGame()).
    /// </summary>
    private void UpdateMenuUI()
    {
        if (scoreMenuRecord != null)
            scoreMenuRecord.text = YG2.saves.MaxScore.ToString();
        if (moneyMenu != null)
            moneyMenu.text = YG2.saves.coins.ToString();
    }

    /// <summary>
    /// Обновление UI во время игры (каждый кадр и при сборе монетки).
    /// </summary>
    private void UpdateGameUI()
    {
        if (scoreGame != null)
            scoreGame.text = ((int)DistanceScore).ToString();
        if (moneyGame != null)
            moneyGame.text = CoinCount.ToString();
    }

    /// <summary>
    /// Обновление UI после игры (однократно при StopGame()).
    /// </summary>
    private void UpdateAfterGameUI()
    {
        if (scoreAfterGame != null)
            scoreAfterGame.text = ((int)DistanceScore).ToString();
        if (moneyAfterGame != null)
            moneyAfterGame.text = CoinCount.ToString();
    }
}
