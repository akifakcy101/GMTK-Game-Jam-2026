using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class CustomerCountdown : MonoBehaviour
{
    [Header("Arayüz (UI)")]
    public TextMeshProUGUI countdownText;

    [Header("Etkinlikler")]
    [Tooltip("Süre 0'a inince tetiklenir (Fail ekranı vb. buraya bağlanabilir)")]
    public UnityEvent onExpired;

    private float remainingTime;
    private bool isRunning = false;

    // CustomerSpawner, müşteri doğduğunda çağırır
    public void SetDuration(float seconds)
    {
        remainingTime = seconds;
        isRunning = true;
        UpdateCountdownUI();
    }

    public void ReduceTime(float seconds)
    {
        remainingTime = Mathf.Max(0f, remainingTime - seconds);
        UpdateCountdownUI();
        Debug.LogWarning($"<color=orange>[CustomerCountdown]</color> Müşteri süresi {seconds:F1}s kısaltıldı! Kalan: {remainingTime:F1}s");
    }

    private void Update()
    {
        if (!isRunning) return;

        if (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            UpdateCountdownUI();
        }
        else
        {
            remainingTime = 0;
            isRunning = false;
            UpdateCountdownUI();

            Debug.Log("<color=red>[CustomerCountdown]</color> Süre bitti!");
            onExpired?.Invoke();
        }
    }

    private void UpdateCountdownUI()
    {
        if (countdownText == null) return;
        countdownText.text = Mathf.CeilToInt(remainingTime).ToString();
    }
}