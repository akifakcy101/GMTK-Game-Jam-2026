using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }

    [Header("Gün Ayarları (Day 1, Day 2, ...)")]
    public int[] customersPerDay = { 3, 4, 5, 6, 7 };

    [Header("Bağlantılar")]
    public CustomerSpawner customerSpawner;

    [Header("Başlangıç Sayacı (3/2/1/START)")]
    public GameObject startCanvas;
    public TextMeshProUGUI startText;
    public float startStepDuration = 1f;

    [Header("Sonuç Ekranı (Result Menu)")]
    public GameObject resultMenuPanel;
    public TextMeshProUGUI resultTitleText;
    public TextMeshProUGUI resultSubtitleText;
    public Button mainMenuButton;
    public Button continueButton;

    [Header("Sahne Ayarları")]
    public string mainMenuSceneName = "MainMenu";

    private int currentDayIndex = 0; // 0 = Day 1
    private int customersServedThisDay = 0;
    private bool dayEnded = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (resultMenuPanel != null) resultMenuPanel.SetActive(false);
        if (startCanvas != null) startCanvas.SetActive(false);

        if (continueButton != null) continueButton.onClick.AddListener(OnContinuePressed);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuPressed);

        if (customerSpawner != null)
        {
            customerSpawner.OnCustomerDismissed += HandleCustomerDismissed;
            customerSpawner.OnCustomerCountdownExpired += HandleCustomerFailed;
        }
        else
        {
            Debug.LogError("<color=red>[DayManager]</color> customerSpawner atanmamış!");
        }

        StartCoroutine(StartDaySequence());
    }

    private IEnumerator StartDaySequence()
    {
        customersServedThisDay = 0;
        dayEnded = false;

        if (startCanvas != null) startCanvas.SetActive(true);

        if (startText != null)
        {
            startText.text = "3";
            yield return new WaitForSeconds(startStepDuration);
            startText.text = "2";
            yield return new WaitForSeconds(startStepDuration);
            startText.text = "1";
            yield return new WaitForSeconds(startStepDuration);
            startText.text = "START";
            yield return new WaitForSeconds(startStepDuration);
        }

        if (startCanvas != null) startCanvas.SetActive(false);

        BeginDay();
    }

    private void BeginDay()
    {
        int count = customersPerDay[currentDayIndex];
        Debug.Log($"<color=cyan>[DayManager]</color> Day {currentDayIndex + 1} başladı! {count} müşteri geliyor.");
        customerSpawner.SpawnAllForDay(count);
    }

    private void HandleCustomerDismissed()
    {
        if (dayEnded) return;

        customersServedThisDay++;
        int required = customersPerDay[currentDayIndex];

        Debug.Log($"<color=green>[DayManager]</color> Müşteri teslim edildi: {customersServedThisDay}/{required}");

        if (customersServedThisDay >= required)
        {
            DayCompleted();
        }
    }

    private void HandleCustomerFailed()
    {
        if (dayEnded) return;
        GameOver();
    }

    private void DayCompleted()
    {
        dayEnded = true;
        Time.timeScale = 0f;

        ShowResultMenu(
            "You Finished The Day",
            null,
            showMainMenu: false,
            showContinue: true);
    }

    private void GameOver()
    {
        dayEnded = true;
        Time.timeScale = 0f;

        ShowResultMenu(
            "Game Over",
            $"Days You Served: {currentDayIndex + 1}",
            showMainMenu: true,
            showContinue: false);
    }

    private void ShowResultMenu(string title, string subtitle, bool showMainMenu, bool showContinue)
    {
        if (resultMenuPanel != null) resultMenuPanel.SetActive(true);
        if (resultTitleText != null) resultTitleText.text = title;

        if (resultSubtitleText != null)
        {
            bool hasSubtitle = !string.IsNullOrEmpty(subtitle);
            resultSubtitleText.gameObject.SetActive(hasSubtitle);
            resultSubtitleText.text = subtitle;
        }

        if (mainMenuButton != null) mainMenuButton.gameObject.SetActive(showMainMenu);
        if (continueButton != null) continueButton.gameObject.SetActive(showContinue);
    }

    // Result Menu'deki "Continue" butonuna bağlanacak
    public void OnContinuePressed()
    {
        if (resultMenuPanel != null) resultMenuPanel.SetActive(false);
        Time.timeScale = 1f;

        currentDayIndex++;

        if (currentDayIndex >= customersPerDay.Length)
        {
            SceneManager.LoadScene(mainMenuSceneName);
            return;
        }

        StartCoroutine(StartDaySequence());
    }

    // Result Menu'deki "Main Menu" butonuna bağlanacak
    public void OnMainMenuPressed()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
