using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class CounterInteraction : MonoBehaviour
{
    [Header("Bağlantılar")]
    public CustomerSpawner customerSpawner;

    [Header("Arayüz İpuçları")]
    public GameObject interactPrompt;
    public TextMeshProUGUI promptText;

    private bool isPlayerInRange = false;

    private void Start()
    {
        if (customerSpawner == null)
            customerSpawner = FindObjectOfType<CustomerSpawner>();

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    private void Update()
    {
        if (!isPlayerInRange) return;

        UpdatePromptText();

        if (Keyboard.current != null &&
            (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.rKey.wasPressedThisFrame))
        {
            InteractWithCounter();
        }
    }

    private void UpdatePromptText()
    {
        if (promptText == null) return;

        CustomerController frontCustomer = customerSpawner != null ? customerSpawner.GetFrontCustomer() : null;

        if (frontCustomer == null || frontCustomer.currentState != CustomerController.CustomerState.WaitingInQueue)
        {
            promptText.text = "Müşteri Bekleniyor...";
            return;
        }

        PlayerInventory inv = PlayerInventory.Instance;
        if (inv == null) return;

        if (!inv.hasItem)
        {
            string itemStr = frontCustomer.assignedItem != null ? frontCustomer.assignedItem.itemName : "Eşya";
            promptText.text = $"E/R - Siparişi Al ({itemStr})";
        }
        else if (inv.itemStage < 3)
        {
            string itemStr = inv.currentItem != null ? inv.currentItem.itemName : "Eşyayı";
            promptText.text = $"{itemStr} Tamamla! (Aşama: {inv.itemStage}/3)";
        }
        else if (inv.itemStage == 3)
        {
            promptText.text = "E/R - Siparişi Teslim Et";
        }
    }

    private void InteractWithCounter()
    {
        if (customerSpawner == null) return;

        CustomerController frontCustomer = customerSpawner.GetFrontCustomer();

        if (frontCustomer == null || frontCustomer.currentState != CustomerController.CustomerState.WaitingInQueue)
        {
            Debug.LogWarning("<color=yellow>[Counter]</color> Tezgahta bekleyen müşteri yok!");
            return;
        }

        PlayerInventory inv = PlayerInventory.Instance;
        if (inv == null)
        {
            Debug.LogError("<color=red>[Counter]</color> Sahnede PlayerInventory bulunamadı!");
            return;
        }

        // Durum 1: Oyuncunun elinde eşya yoksa -> Müşteriden eşyayı al
        if (!inv.hasItem)
        {
            ItemData customerItem = frontCustomer.GiveItemToPlayer();
            inv.ReceiveItemFromCustomer(customerItem, frontCustomer.requestedPackIndex, frontCustomer.requestedStickerIndex);
            Debug.Log($"<color=green>[Counter]</color> Müşteriden sipariş alındı! Kutu İsteği: {frontCustomer.requestedPackIndex}, Sticker İsteği: {frontCustomer.requestedStickerIndex}");
        }
        // Durum 2: Oyuncu tüm masalardan geçmiş ve eşyayı tamamlamışsa -> Teslim et
        else if (inv.itemStage == 3)
        {
            bool isPackMatch = (inv.appliedPackIndex == frontCustomer.requestedPackIndex);
            bool isStickerMatch = (inv.appliedStickerIndex == frontCustomer.requestedStickerIndex);

            if (isPackMatch && isStickerMatch)
            {
                Debug.Log("<color=green>[Counter]</color> Sipariş doğru paketlendi ve başarıyla teslim edildi! Müşteri ayrılıyor.");
                inv.ClearItem();
                customerSpawner.DismissCurrentCustomer();
            }
            else
            {
                float timePenalty = Random.Range(10f, 20f);
                Debug.LogWarning($"<color=red>[Counter]</color> HATA: Yanlış Paketleme! İstenen (Pack: {frontCustomer.requestedPackIndex}, Sticker: {frontCustomer.requestedStickerIndex}) != Yapılan (Pack: {inv.appliedPackIndex}, Sticker: {inv.appliedStickerIndex}). {timePenalty:F1}s ceza kesildi!");

                CustomerCountdown countdown = frontCustomer.GetComponentInChildren<CustomerCountdown>();
                if (countdown != null)
                {
                    countdown.ReduceTime(timePenalty);
                }

                // Paketlemeyi geçersiz say ve oyuncuyu yeniden paketleme masasına (Aşama 2: Masa 3) yönlendir
                inv.itemStage = 2;
                inv.appliedPackIndex = -1;
                inv.appliedStickerIndex = -1;
                inv.UpdateInventoryUI();

                Debug.Log("<color=yellow>[Counter]</color> Sipariş reddedildi. Tekrar paketlemek için Masa 3'e (Paketleme Masası) dönün.");
            }
        }
        // Durum 3: Eşya henüz tamamlanmamışsa
        else
        {
            Debug.LogWarning($"<color=yellow>[Counter]</color> Eşya henüz tamamlanmadı! Sıradaki Masa: Masa {inv.itemStage + 1}");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<Movement>() != null)
        {
            isPlayerInRange = true;
            if (interactPrompt != null) interactPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<Movement>() != null)
        {
            isPlayerInRange = false;
            if (interactPrompt != null) interactPrompt.SetActive(false);
        }
    }
}
