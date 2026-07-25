using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panelleri (Canvas Group Eklenmiş Olmalı)")]
    public CanvasGroup mainMenuPanel;
    public CanvasGroup creditsPanel;

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

        // Ana menü açık, Credits kapalı olarak başla
        mainMenuPanel.gameObject.SetActive(true);
        mainMenuPanel.alpha = 1;

        creditsPanel.gameObject.SetActive(false);
        creditsPanel.alpha = 0;
    }

    public void PlayGame()
    {
        // Oyuna geçerken direkt sahne yüklemek yerine Coroutine başlatıyoruz
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

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            // Mathf.Lerp, iki değer arasında zamana göre pürüzsüz geçiş sağlar
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);

            yield return null; // Bir sonraki frame'e (kareye) kadar bekle
        }

        canvasGroup.alpha = endAlpha;

        // Geçiş bittiğinde paneli tekrar etkileşime aç (eğer kapanmayacaksa)
        if (!disableOnFinish)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    public void QuitGame()
    {
        Debug.Log("Oyundan çıkış yapıldı!");
        Application.Quit();
    }
}