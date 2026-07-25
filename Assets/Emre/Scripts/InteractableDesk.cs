using UnityEngine;
using UnityEngine.InputSystem;

public class InteractableDesk : MonoBehaviour
{
    [Header("Masa Numarası & Sıra Ayarları")]
    [Tooltip("Bu masa kaçıncı aşama için? (Masa 1 = 0, Masa 2 = 1, Masa 3 = 2)")]
    public int requiredItemStage = 0;

    [Header("Masa & Mekanik Bağlantısı")]
    [Tooltip("Bu masaya geçildiğinde açılacak Mekanik/UI Objesi")]
    public GameObject deskMechanicUI;
    [Tooltip("Alper'in İnceleme Masası scripti (MasaYonetici) eğer sahnede bu masadaysa buraya sürükleyin")]
    public MasaYonetici masaYonetici;

    [Header("Kamera & Görünüm Ayarları")]
    [Tooltip("Masaya geçildiğinde kameranın odaklanacağı konum (Boş bırakılırsa kamera hareket etmez)")]
    public Transform deskCameraPosition;
    [Tooltip("Masaya geçildiğinde oyuncunun görseli gizlensin mi?")]
    public bool hidePlayerWhileInteracting = false;

    [Header("Arayüz İpuçları")]
    [Tooltip("Masaya yaklaşınca çıkan 'E - Masaya Geç' yazısı veya görseli")]
    public GameObject interactPrompt;

    [Header("Aşama İlerleme Ayarı")]
    [Tooltip("Masa kapatıldığında eşya aşaması otomatik ilerlesin mi? (Placeholder / Test için)")]
    public bool autoAdvanceOnClose = true;

    private bool isPlayerInRange = false;
    private bool isInteracting = false;
    private Movement playerMovement;
    private Camera mainCamera;
    private Vector3 originalCameraPosition;

    private void Start()
    {
        mainCamera = Camera.main;

        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        if (deskMechanicUI != null)
            deskMechanicUI.SetActive(false);

        // MasaYonetici bu obje üzerindeyse ve bağlanmadıysa otomatik bul
        if (masaYonetici == null)
            masaYonetici = GetComponent<MasaYonetici>();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // Karakter masanın yanındaysa ve E tuşuna basarsa
        if (isPlayerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!isInteracting)
            {
                TryOpenDesk();
            }
            else
            {
                CloseDesk();
            }
        }
    }

    public void TryOpenDesk()
    {
        PlayerInventory inv = PlayerInventory.Instance;

        if (inv == null || !inv.hasItem)
        {
            Debug.LogWarning("<color=yellow>[InteractableDesk]</color> Elinizde işlenecek bir eşya yok! Önce müşteriden sipariş alın.");
            return;
        }

        if (inv.itemStage != requiredItemStage)
        {
            Debug.LogWarning($"<color=yellow>[InteractableDesk]</color> Bu masa için uygun aşamada değilsiniz! (Gereken Aşama: {requiredItemStage}, Elinizdeki: {inv.itemStage})");
            return;
        }

        OpenDesk();
    }

    private void OpenDesk()
    {
        isInteracting = true;

        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        if (deskMechanicUI != null)
        {
            deskMechanicUI.SetActive(true);
            Debug.Log($"<color=green>[InteractableDesk]</color> Masa {requiredItemStage + 1} açıldı: {deskMechanicUI.name}");
        }

        // Eğer Alper'in MasaYonetici mekaniği bağlıysa onu tetikle ve eşyayı doğur!
        if (masaYonetici != null)
        {
            masaYonetici.MasayiAc();
        }

        // Kamerayı masanın konumuna odakla
        if (mainCamera != null && deskCameraPosition != null)
        {
            originalCameraPosition = mainCamera.transform.position;
            Vector3 targetCamPos = deskCameraPosition.position;
            targetCamPos.z = originalCameraPosition.z; // Z derinliğini bozma
            mainCamera.transform.position = targetCamPos;
        }

        // Karakter görselini gizle
        if (hidePlayerWhileInteracting && playerMovement != null)
        {
            SpriteRenderer sr = playerMovement.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
        }

        if (playerMovement != null)
            playerMovement.enabled = false;
    }

    public void CloseDesk()
    {
        if (!isInteracting) return;

        isInteracting = false;

        if (deskMechanicUI != null)
        {
            deskMechanicUI.SetActive(false);
            Debug.Log($"<color=yellow>[InteractableDesk]</color> Masa {requiredItemStage + 1} kapatıldı.");
        }

        if (masaYonetici != null)
        {
            masaYonetici.MasayiKapat();
        }

        // Kamerayı eski yerine getir
        if (mainCamera != null && deskCameraPosition != null)
        {
            mainCamera.transform.position = originalCameraPosition;
        }

        // Karakter görselini tekrar göster
        if (hidePlayerWhileInteracting && playerMovement != null)
        {
            SpriteRenderer sr = playerMovement.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = true;
        }

        // Placeholder olarak masa kapatıldığında aşamayı otomatik ilerlet (MasaYonetici bağlı değilse)
        if (masaYonetici == null && autoAdvanceOnClose && PlayerInventory.Instance != null && PlayerInventory.Instance.itemStage == requiredItemStage)
        {
            PlayerInventory.Instance.AdvanceItemStage();
        }

        if (isPlayerInRange && interactPrompt != null)
            interactPrompt.SetActive(true);

        if (playerMovement != null)
            playerMovement.enabled = true;
    }

    // Arkadaşlarınızın minigame scriptleri minigame bittiğinde bunu çağırabilir:
    public void CompleteDeskMinigame()
    {
        Debug.Log($"<color=green>[InteractableDesk]</color> Masa {requiredItemStage + 1} Minigame'i başarıyla tamamlandı!");
        if (PlayerInventory.Instance != null && PlayerInventory.Instance.itemStage == requiredItemStage)
        {
            PlayerInventory.Instance.AdvanceItemStage();
        }
        CloseDesk();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<Movement>() != null)
        {
            isPlayerInRange = true;
            playerMovement = other.GetComponent<Movement>();

            if (!isInteracting && interactPrompt != null)
            {
                interactPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<Movement>() != null)
        {
            isPlayerInRange = false;

            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }

            if (isInteracting)
            {
                CloseDesk();
            }

            playerMovement = null;
        }
    }
}
