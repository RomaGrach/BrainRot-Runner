using UnityEngine;

public class GameManager : MonoBehaviour
{

    public road_generator road_generator;
    public ScoreManager ScoreManager;
    public PlayerController PlayerController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void StartGame()
    {
        road_generator.ClearGame();
        PlayerController.StartGame();
        road_generator.StartGame();
        ScoreManager.StartGame();
    }

    public void lousGame()
    {
        road_generator.StopGame();
        ScoreManager.StopGame();
    }

    public void Gohome()
    {
        road_generator.ClearGame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
