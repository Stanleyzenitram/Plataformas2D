using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string firstLevelScene = "Nivel1";
    [SerializeField] private float gameOverDelay = 3f;

    private bool isPaused = false;
    private bool isGameOver = false;
    private bool isVictory = false;

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f; // por si venías de una pausa o de morir
    }

    private void Update()
    {
        if (isGameOver || isVictory) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        if (isPaused || isGameOver || isVictory) return;

        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ShowGameOver()
    {
        if (isGameOver || isVictory) return;

        isGameOver = true;
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;

        StartCoroutine(ReturnToFirstLevelRoutine());
    }

    public void ShowVictory()
    {
        if (isVictory || isGameOver) return;

        isVictory = true;

        if (pausePanel != null) pausePanel.SetActive(false);
        victoryPanel.SetActive(true);
        Time.timeScale = 0f;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySound(AudioManager.Instance.levelCompleteClip);
    }

    // timeScale es 0, por eso se usa tiempo real
    private IEnumerator ReturnToFirstLevelRoutine()
    {
        yield return new WaitForSecondsRealtime(gameOverDelay);
        GoToFirstLevel();
    }

    // Botón "Reintentar": vuelve siempre al Nivel 1
    public void Retry()
    {
        GoToFirstLevel();
    }

    public void GoToFirstLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(firstLevelScene);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }
}