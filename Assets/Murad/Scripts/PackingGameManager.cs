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
    private bool isPackTableClosed = false;
    private bool isStickerPlaced = false;

    void Start()
    {
        ResetGame();
    }

    public void OnPutGunPressed()
    {
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
        ResetGame();

        if (parentDesk != null)
        {
            parentDesk.CompleteDeskMinigame();
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
        isPackTableClosed = false;
        isStickerPlaced = false;
    }
}