using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimerManager : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;

    [SerializeField] private float startTime = 180f;

    private float currentTime;
    private bool timerRunning = true;

    private void Start()
    {
        currentTime = startTime;
        UpdateTimerUI();
    }

    private void Update()
    {
        if (!timerRunning)
            return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            timerRunning = false;

            TimeEnded();
        }

        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);

        timerText.text =
            string.Format("{0:00}:{1:00}",
            minutes,
            seconds);

            if (currentTime <= 30)
{
    timerText.color = Color.red;
}
    }

    private void TimeEnded()
    {
        SceneManager.LoadScene("Level1");
    }
}