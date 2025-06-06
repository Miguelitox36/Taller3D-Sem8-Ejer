using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("UI Elements")]
    public Text levelText;
    public Text waveText;
    public Text enemiesText;
    public Text scoreText;
    public Text instructionsText;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public Text gameOverText;
    public Button restartButton;

    private int currentScore = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {        
        if (instructionsText != null)
        {
            instructionsText.text = "WASD/Arrows: Move | SPACE: Shoot";
        }       
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }       
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (GameManager.instance != null)
        {           
            if (levelText != null)
            {
                levelText.text = "Level: " + GameManager.instance.level;
            }

           
            if (waveText != null)
            {
                int currentWaveDisplay = (GameManager.instance.currentWaveIndex % GameManager.instance.wavesPerLevel) + 1;
                waveText.text = "Wave: " + currentWaveDisplay + "/" + GameManager.instance.wavesPerLevel;
            }
            
            if (scoreText != null)
            {
                scoreText.text = "Score: " + currentScore;
            }
        }
    }

    public void UpdateEnemiesRemaining(int count)
    {
        if (enemiesText != null)
        {
            enemiesText.text = "Enemies: " + count;
        }
    }

    public void AddScore(int points)
    {
        currentScore += points;
        UpdateUI();
    }

    public void ShowGameOver(string reason)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (gameOverText != null)
        {
            gameOverText.text = reason + "\nFinal Score: " + currentScore;
        }        
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {        
        Time.timeScale = 1f;                
        currentScore = 0;
                
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        if (GameManager.instance != null)
        {
            GameManager.instance.RestartGame();
        }

        UpdateUI();
    }
}