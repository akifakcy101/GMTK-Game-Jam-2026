using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panelleri (Canvas Group Eklenmiş Olmalı)")]
    public CanvasGroup mainMenuPanel;
    public CanvasGroup creditsPanel;
    public CanvasGroup howToPlayPanel1; // YENİ: İlk eğitim paneli
    public CanvasGroup howToPlayPanel2; // YENİ: İkinci eğitim paneli

    [Header("Sahne Geçişi İçin Siyah Ekran")]
    public CanvasGroup transitionFadePanel;

    [Header("Ayarlar")]
    public string gameSceneName = "GameScene";
    public float fadeDuration = 0.5f; // Geçişlerin kaç saniye süreceği

    void Start()
    {
        // Oyun başladığında siyah ekranı hızla şeffaf yapıp kapatıyoruz
        if (transitionFadePanel != null)
        {
            transitionFadePanel.alpha = 1;
            transitionFadePanel.gameObject.SetActive(true);
            StartCoroutine(FadeRoutine(transitionFadePanel, 1, 0, true));
        }

        // Ana menü açık başla
        mainMenuPanel.gameObject.SetActive(true);
        mainMenuPanel.alpha = 1;

        // Diğer tüm panelleri kapalı olarak ayarla
        SetPanelInactive(creditsPanel);
        SetPanelInactive(howToPlayPanel1);
        SetPanelInactive(howToPlayPanel2);
    }

    // Kod tekrarını önlemek için panelleri kapatan yardımcı fonksiyon
    private void SetPanelInactive(CanvasGroup panel)
    {
        if (panel != null)
        {
            panel.gameObject.SetActive(false);
            panel.alpha = 0;
        }
    }

    // 1. Ana menüdeki "Başla" butonuna basıldığında (How To Play 1'i açar)
    public void StartHowToPlaySequence()
    {
        StartCoroutine(SwitchPanelRoutine(mainMenuPanel, howToPlayPanel1));
    }

    // 2. Birinci paneldeki "İleri" butonuna basıldığında (How To Play 2'yi açar)
    public void NextHowToPlayPanel()
    {
        StartCoroutine(SwitchPanelRoutine(howToPlayPanel1, howToPlayPanel2));
    }

    // 3. İkinci paneldeki "Oyuna Başla" butonuna basıldığında (Sahneyi yükler)
    public void StartActualGame()
    {
        StartCoroutine(LoadSceneRoutine());
    }

    private IEnumerator LoadSceneRoutine()
    {
        // Siyah paneli aktif et ve karart
        transitionFadePanel.gameObject.SetActive(true);
        yield return StartCoroutine(FadeRoutine(transitionFadePanel, 0, 1, false));

        // Ekran tamamen siyah olduktan sonra sahneyi yükle
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenCredits()
    {
        StartCoroutine(SwitchPanelRoutine(mainMenuPanel, creditsPanel));
    }

    public void CloseCredits()
    {
        StartCoroutine(SwitchPanelRoutine(creditsPanel, mainMenuPanel));
    }

    // İki panel arasında SİYAH EKRAN kullanarak geçiş yapan fonksiyon
    private IEnumerator SwitchPanelRoutine(CanvasGroup panelToHide, CanvasGroup panelToShow)
    {
        // 1. Önce siyah ekranı aktif et ve ekranı tamamen karart (0'dan 1'e)
        transitionFadePanel.gameObject.SetActive(true);
        yield return StartCoroutine(FadeRoutine(transitionFadePanel, 0, 1, false));

        // 2. Ekran tamamen SİYAH olduğunda, arkada panelleri anında değiştir
        panelToHide.alpha = 0;
        panelToHide.gameObject.SetActive(false);

        panelToShow.alpha = 1;
        panelToShow.gameObject.SetActive(true);

        // 3. Siyah ekranı şeffaflaştırıp yeni paneli ortaya çıkar (1'den 0'a)
        yield return StartCoroutine(FadeRoutine(transitionFadePanel, 1, 0, true));
    }

    // Alpha (opaklık) değerini zamanla değiştiren matematiksel fonksiyon
    private IEnumerator FadeRoutine(CanvasGroup canvasGroup, float startAlpha, float endAlpha, bool disableOnFinish)
    {
        // Geçiş sırasında panele tıklanmasını engelle
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;

        // Geçiş bittiğinde paneli tekrar etkileşime aç veya tamamen kapat
        if (!disableOnFinish)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            // Şeffaf olan panellerin arkadaki butonlara tıklamayı engellememesi için tamamen kapatıyoruz
            canvasGroup.gameObject.SetActive(false);
        }
    }

    public void QuitGame()
    {
        Debug.Log("Oyundan çıkış yapıldı!");
        Application.Quit();
    }
}