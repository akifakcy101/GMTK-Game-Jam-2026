using UnityEngine;
using UnityEngine.UI;

public class PackingGameManager : MonoBehaviour
{
    [Header("Gun")]
    public GameObject gunObject;

    [Header("Pack Table (tek obje, kendi içinde 3 pack türünü yönetiyor)")]
    public PackTableController packTable;

    [Header("Sticker Butonları (Stickers altındaki 3 buton)")]
    public Button[] stickerButtons;

    [Header("Sticker Sprite'ları (stickerButtons ile aynı sırada)")]
    public Sprite[] stickerSprites;

    [Header("Sürüklenen Sticker Preview Objesi")]
    public GameObject draggedStickerPreview;

    [Header("Alt Butonlar")]
    public Button getPackedGunButton;
    public Button trashBinButton;

    [Header("Masa Bağlantısı")]
    [Tooltip("Bu paketleme masasının bağlı olduğu InteractableDesk (Get Packed Gun'da minigame'i tamamlatmak için)")]
    public InteractableDesk parentDesk;

    private int selectedPackIndex = -1;
    private int selectedStickerIndex = -1;
    private bool isPackTableClosed = false;
    private bool isStickerPlaced = false;

    void Start()
    {
        if (parentDesk == null)
        {
            parentDesk = GetComponentInParent<InteractableDesk>();
        }

        SetupStickerButtonVisuals();
        ResetGame();
    }

    private void OnEnable()
    {
        if (parentDesk == null)
        {
            parentDesk = GetComponentInParent<InteractableDesk>();
        }

        SetupStickerButtonVisuals();
        ResetGame();
        OnPutGunPressed();
    }

    private void SetupStickerButtonVisuals()
    {
        if (stickerButtons == null || stickerSprites == null) return;

        for (int i = 0; i < stickerButtons.Length; i++)
        {
            if (stickerButtons[i] == null) continue;

            // Eğer ilgili indekste sprite varsa butonun Image component'ine ata
            if (i < stickerSprites.Length && stickerSprites[i] != null)
            {
                Image btnImg = stickerButtons[i].GetComponent<Image>();
                if (btnImg != null)
                {
                    btnImg.sprite = stickerSprites[i];
                    btnImg.color = Color.white;
                    btnImg.preserveAspect = true; // Görselin oranını koru (basık/yassı görünmesini engeller)
                }
            }

            // Buton üzerindeki "Sticker", "Sticker 1" gibi yazıları/yazı objelerini gizle
            var tmpro = stickerButtons[i].GetComponentInChildren<TMPro.TMP_Text>(true);
            if (tmpro != null)
            {
                tmpro.gameObject.SetActive(false);
            }

            var textLegacy = stickerButtons[i].GetComponentInChildren<Text>(true);
            if (textLegacy != null)
            {
                textLegacy.gameObject.SetActive(false);
            }
        }
    }

    public void OnPutGunPressed()
    {
        // Eğer paket zaten seçildiyse veya kapatıldıysa tekrar silah koyulamaz
        if (isPackTableClosed || selectedPackIndex >= 0)
        {
            Debug.LogWarning("<color=yellow>[PackingGameManager]</color> Kutu seçildiği veya kapatıldığı için tekrar silah koyulamaz.");
            return;
        }

        UpdateGunVisual();
        gunObject.SetActive(true);
    }

    // Müşterinin gerçekte sipariş ettiği eşyayı (PlayerInventory.currentItem) gösterir
    private void UpdateGunVisual()
    {
        Image gunImage = gunObject.GetComponent<Image>();
        if (gunImage == null) return;

        ItemData item = PlayerInventory.Instance != null ? PlayerInventory.Instance.currentItem : null;
        Sprite itemSprite = GetItemSprite(item);
        if (itemSprite != null)
        {
            gunImage.sprite = itemSprite;
        }
    }

    // itemIcon boşsa itemPrefab'ın kendi SpriteRenderer'ındaki görsele düşer
    private Sprite GetItemSprite(ItemData item)
    {
        if (item == null) return null;
        if (item.itemIcon != null) return item.itemIcon;
        if (item.itemPrefab != null)
        {
            SpriteRenderer sr = item.itemPrefab.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) return sr.sprite;
        }
        return null;
    }

    public void OnPackSelected(int packIndex)
    {
        selectedPackIndex = packIndex;

        packTable.gameObject.SetActive(true);
        packTable.SetPack(packIndex);

        isPackTableClosed = false;
    }

    public void OnPackTableClicked()
    {
        isPackTableClosed = true;
        gunObject.SetActive(false);

        foreach (var stickerBtn in stickerButtons)
            stickerBtn.interactable = true;

        CheckReadyState();
    }

    public void OnStickerButtonPressed(int stickerIndex)
    {
        if (isStickerPlaced) return;          // sadece 1 sticker koyulabilir
        if (selectedPackIndex < 0) return;    // kontrol

        selectedStickerIndex = stickerIndex;

        RectTransform targetTableRect = packTable.GetComponent<RectTransform>();

        StickerDragHandler dragHandler =
            draggedStickerPreview.GetComponent<StickerDragHandler>();

        dragHandler.BeginPlacement(stickerSprites[stickerIndex], targetTableRect, this);
    }

    public void OnStickerPlaced()
    {
        isStickerPlaced = true;

        foreach (var stickerBtn in stickerButtons) stickerBtn.interactable = false;

        CheckReadyState();
    }

    private void CheckReadyState()
    {
        getPackedGunButton.interactable = isPackTableClosed && isStickerPlaced;
    }

    public void OnGetPackedGunPressed()
    {
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.appliedPackIndex = selectedPackIndex;
            PlayerInventory.Instance.appliedStickerIndex = selectedStickerIndex;

            if (PlayerInventory.Instance.itemStage == 2)
            {
                PlayerInventory.Instance.AdvanceItemStage();
            }

            Debug.Log($"<color=cyan>[PackingGameManager]</color> Paketleme tamamlandı! Aşama 3 yapıldı. Kutu: {selectedPackIndex}, Sticker: {selectedStickerIndex}");
        }

        InteractableDesk targetDesk = parentDesk;
        if (targetDesk == null) targetDesk = GetComponentInParent<InteractableDesk>();
        if (targetDesk == null) targetDesk = FindObjectOfType<InteractableDesk>();

        ResetGame();

        // "E" tuşuna basılmış gibi masadan doğrudan çık ve oyuncuyu/kamerayı eski haline getir
        if (targetDesk != null)
        {
            targetDesk.CloseDesk();
        }
    }

    public void OnTrashBinPressed()
    {
        ResetGame();
    }

    private void ResetGame()
    {
        gunObject.SetActive(false);
        
        packTable.ResetVisual();
        packTable.gameObject.SetActive(false);

        foreach (var stickerBtn in stickerButtons)
            stickerBtn.interactable = false;

        draggedStickerPreview.SetActive(false);

        getPackedGunButton.interactable = false;

        selectedPackIndex = -1;
        selectedStickerIndex = -1;
        isPackTableClosed = false;
        isStickerPlaced = false;
    }
}