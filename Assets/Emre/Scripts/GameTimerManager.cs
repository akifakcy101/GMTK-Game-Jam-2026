using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class GameTimerManager : MonoBehaviour
{
    [Header("Zaman Ayarları")]
    [Tooltip("Saniye cinsinden tur/oyun süresi")]
    public float totalTimeInSeconds = 60f;

    [Header("Arayüz (UI)")]
    public TextMeshProUGUI timerText;

    [Header("Etkinlikler")]
    public UnityEvent onTimeFinished;

    private float currentTime;
    private bool isTimerRunning = false;

    private void Start()
    {
        currentTime = totalTimeInSeconds;
        isTimerRunning = true;
        UpdateTimerUI();
    }

    private void Update()
    {
        if (!isTimerRunning) return;

        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerUI();
        }
        else
        {
            currentTime = 0;
            isTimerRunning = false;
            UpdateTimerUI();
            
            Debug.Log("SÜRE BİTTİ!");
            onTimeFinished?.Invoke();
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void PauseTimer() => isTimerRunning = false;
    public void ResumeTimer() => isTimerRunning = true;
    public void AddTime(float seconds) => currentTime += seconds;
}
