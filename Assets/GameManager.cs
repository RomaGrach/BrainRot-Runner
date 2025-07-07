using TMPro;
using UnityEngine;


namespace YG
{
    public partial class SavesYG
    {
        // Ваши данные для сохранения
        public int coins = 5; // Пример
        public int MaxScore = 0;
    }
}


public class GameManager : MonoBehaviour
{
    [Header("References")]
    public road_generator road_generator;
    public ScoreManager ScoreManager;
    public PlayerController PlayerController;

    [Header("Player")]
    public GameObject player;

    public GameObject canvasAfterGame;
    public GameObject canvasGame;

    



    // Временной масштаб перед паузой
    private float previousTimeScale = 1f;
    private bool isPaused = false;

    void Start()
    {
        
    }

    /// <summary>
    /// Запускает игру: очищает дорожные сегменты, стартует игрок, дорогу и счёт.
    /// </summary>
    public void StartGame()
    {
        Gohome();
        road_generator.ClearGame();
        PlayerController.StartGame();
        road_generator.StartGame();
        ScoreManager.StartGame();
    }

    /// <summary>
    /// Останавливает игру (игрок погиб): останавливает генерацию и счёт.
    /// </summary>
    public void lousGame()
    {
        road_generator.StopGame();
        ScoreManager.StopGame();
        canvasAfterGame.SetActive(true);
        canvasGame.SetActive(false);
    }

    /// <summary>
    /// Возвращает игрока домой: очищает дорогу и сбрасывает состояние игрока.
    /// </summary>
    public void Gohome()
    {
        road_generator.ClearGame();
        PlayerController.RestartGame();

        
    }

    /// <summary>
    /// Ставит игру на паузу: остановка времени.
    /// </summary>
    public void PauseGame()
    {
        if (isPaused) return;
        // Сохраняем текущий Time.timeScale
        previousTimeScale = Time.timeScale;
        // Останавливаем время
        Time.timeScale = 0f;
        isPaused = true;
    }

    /// <summary>
    /// Снимает паузу: восстанавливает предыдущий Time.timeScale.
    /// </summary>
    public void ResumeGame()
    {
        if (!isPaused) return;
        // Восстанавливаем предыдущий Time.timeScale
        Time.timeScale = previousTimeScale;
        isPaused = false;
    }

    void Update()
    {
        // При необходимости можно слушать ввод для паузы/снятия паузы:
        // if (Input.GetKeyDown(KeyCode.Escape))
        // {
        //     if (isPaused) ResumeGame(); else PauseGame();
        // }
    }
}
