using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class FullScreenOverlayToggle : MonoBehaviour
{
    [Header("Görsel / Panel Ayarları")]
    [Tooltip("Ekranı kaplayacak Sprite görseli. (Eğer hazırdaki bir UI Paneli verecekseniz bunu boş bırakabilirsiniz)")]
    public Sprite overlaySprite;

    [Tooltip("Özel olarak hazırlanmış UI Paneli. Boş bırakılırsa overlaySprite'tan otomatik tam ekran Canvas üretilir.")]
    public GameObject overlayPanel;

    [Header("Başlangıç Ayarı")]
    [Tooltip("Oyun başladığında görsel açık mı olsun?")]
    public bool startActive = false;

    private Image generatedImage;
    private GameObject createdCanvasGO;
    private bool isOpen = false;

    private void Awake()
    {
        // Eğer hazır bir UI paneli verilmediyse ve Sprite tanımlıysa otomatik Canvas + UI Image oluştur
        if (overlayPanel == null && overlaySprite != null)
        {
            CreateOverlayCanvas();
        }

        isOpen = startActive;
        UpdateOverlayState();
    }

    private void Update()
    {
        // Q tuşuna basıldığında durumu değiştir
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            ToggleOverlay();
        }
#if !ENABLE_INPUT_SYSTEM_ONLY
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleOverlay();
        }
#endif
    }

    public void ToggleOverlay()
    {
        isOpen = !isOpen;
        UpdateOverlayState();
    }

    private void UpdateOverlayState()
    {
        if (overlayPanel != null)
        {
            overlayPanel.SetActive(isOpen);
        }
        else if (createdCanvasGO != null)
        {
            createdCanvasGO.SetActive(isOpen);
        }
    }

    private void CreateOverlayCanvas()
    {
        // Otomatik Screen Space - Overlay Canvas oluştur
        createdCanvasGO = new GameObject("FullScreen_Q_Overlay_Canvas");
        Canvas canvas = createdCanvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // En üst katman

        CanvasScaler scaler = createdCanvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        createdCanvasGO.AddComponent<GraphicRaycaster>();

        // Tam ekran Image oluştur
        GameObject imageGO = new GameObject("OverlayImage");
        imageGO.transform.SetParent(createdCanvasGO.transform, false);

        generatedImage = imageGO.AddComponent<Image>();
        generatedImage.sprite = overlaySprite;
        generatedImage.preserveAspect = false; // Tam ekran kaplaması için

        // RectTransform'u ekranı %100 kaplayacak şekilde esnet
        RectTransform rect = imageGO.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.one;
    }
}
