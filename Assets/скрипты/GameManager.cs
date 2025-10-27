using TMPro;
using UnityEngine;
using YG;  // PluginYG2 namespace

namespace YG
{
    public partial class SavesYG
    {
        // Ваши данные для сохранения
        public int coins = 0; // Пример
        public int MaxScore = 0;
        public bool[] skins = new bool[] { true, false, false, false };
        public int NowSkin = 0;
        public float[] itemsDur = new float[] { 10f };
        public int[] itemsDurPrise = new int[] { 400 };
    }
}

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public road_generator road_generator;
    public ScoreManager ScoreManager;
    public PlayerController PlayerController;
    public Camera_Controller Camera_Controller;

    [Header("Player")]
    public GameObject player;

    public GameObject canvasAfterGame;
    public GameObject canvasGame;

    // --- Добавлено для отображения сохранённых параметров в Инспекторе ---
    [Header("Save Data Visualization")]
    [Tooltip("Текущее количество монет из saves.coins")]
    public int savedCoins;
    [Tooltip("Текущий MaxScore из saves.MaxScore")]
    public int savedMaxScore;
    public bool[] skins;
    [Tooltip("Текущий индекс скина из saves.NowSkin")]
    public int savedNowSkin;
    [Tooltip("Текущие длительности из saves.itemsDur")]
    public float[] savedItemsDur;
    [Tooltip("Текущие стоимости из saves.itemsDurPrise")]
    public int[] savedItemsDurPrise;
    // --------------------------------------------------------------------

    // Временной масштаб перед паузой
    private float previousTimeScale = 1f;
    private bool isPaused = false;

    private SavesYG saves => YG2.saves;


    // <summary>
    /// Сброс всех сохранённых данных в значения по умолчанию.
    /// Вызывается из контекстного меню в инспекторе (Reset Progress).
    /// </summary>
    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        // Устанавливаем значения по умолчанию
        saves.coins = 0;
        saves.MaxScore = 0;
        saves.skins = new bool[] { true, false, false, false };
        saves.NowSkin = 0;
        saves.itemsDur = new float[] { 10f };
        saves.itemsDurPrise = new int[] { 400 };

        // Сохраняем изменения
        YG2.SaveProgress();

        Debug.Log("Progress has been reset to default values.");
    }

    /// <summary>
    /// Добавляет 100 монет к текущему балансу.
    /// Вызывается из контекстного меню в инспекторе (Add 100 Coins).
    /// </summary>
    [ContextMenu("Add 100 Coins")]
    public void Add100Coins()
    {
        saves.coins += 100;
        YG2.SaveProgress();
        Debug.Log("Added 100 coins.");
    }



    void Start()
    {
        // Здесь осталось всё без изменений
        AudioManager.Instance.PlayMenuMusic();
    }

    /// <summary>
    /// Запускает игру: очищает дорожные сегменты, стартует игрок, дорогу и счёт.
    /// </summary>
    public void StartGame()
    {
        Gohome();
        AudioManager.Instance.PlayLevelMusic();
        AudioManager.Instance.PlayLevelStart();
        road_generator.ClearGame();
        PlayerController.StartGame();
        road_generator.StartGame();
        ScoreManager.StartGame();
        Camera_Controller.StartGameCamera();
    }

    /// <summary>
    /// Останавливает игру (игрок погиб): останавливает генерацию и счёт.
    /// </summary>
    public void lousGame()
    {
        AudioManager.Instance.PlayLevelEnd();
        AudioManager.Instance.PlayMenuMusic();
        road_generator.StopGame();
        ScoreManager.StopGame();
        canvasAfterGame.SetActive(true);
        canvasGame.SetActive(false);
        YG2.SaveProgress();
    }

    /// <summary>
    /// Возвращает игрока домой: очищает дорогу и сбрасывает состояние игрока.
    /// </summary>
    public void Gohome()
    {
        YG2.SaveProgress();
        road_generator.ClearGame();
        PlayerController.RestartGame();
        Camera_Controller.ReturnCamera();
    }

    /// <summary>
    /// Ставит игру на паузу: остановка времени.
    /// </summary>
    public void PauseGame()
    {
        if (isPaused) return;
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        isPaused = true;
    }

    /// <summary>
    /// Снимает паузу: восстанавливает предыдущий Time.timeScale.
    /// </summary>
    public void ResumeGame()
    {
        if (!isPaused) return;
        Time.timeScale = previousTimeScale;
        isPaused = false;
    }

    public void ShowAd()
    {
        YG2.InterstitialAdvShow();
    }

    void Update()
    {
        // Обновляем поля для Инспектора каждый кадр
        savedCoins = saves.coins;
        savedMaxScore = saves.MaxScore;
        savedNowSkin = saves.NowSkin;
        savedItemsDur = saves.itemsDur;
        savedItemsDurPrise = saves.itemsDurPrise;
        skins = saves.skins;

        // При необходимости можно слушать ввод для паузы/снятия паузы:
        // if (Input.GetKeyDown(KeyCode.Escape))
        // {
        //     if (isPaused) ResumeGame(); else PauseGame();
        // }
    }
}
