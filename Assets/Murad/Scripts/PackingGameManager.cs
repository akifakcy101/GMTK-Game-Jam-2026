using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    [Header("Sahne Ayarları")]
    public string nextSceneName;

    private int selectedPackIndex = -1;   // hangi pack türü seçildi (0/1/2)
    private bool isPackTableClosed = false;
    private bool isStickerPlaced = false;

    void Start()
    {
        ResetGame();
    }

    // ---------- PUT GUN ----------
    // "Put Gun" butonuna bağlanacak
    public void OnPutGunPressed()
    {
        gunObject.SetActive(true);
    }

    // ---------- PACK SEÇİMİ ----------
    // "Packs" altındaki her butona bağlanacak (Carton=0, Container=1, Foil=2)
    public void OnPackSelected(int packIndex)
    {
        selectedPackIndex = packIndex;

        packTable.gameObject.SetActive(true);
        packTable.SetPack(packIndex);   // PackTable kendi sprite'ını ve interactable durumunu ayarlıyor

        isPackTableClosed = false;
    }

    // ---------- PACK TABLE TIKLAMA ----------
    // PackTableController tarafından çağrılır (kendi OnTableClicked'ından sonra)
    public void OnPackTableClicked()
    {
        isPackTableClosed = true;
        gunObject.SetActive(false);   // Pack Table kapandı, Gun artık görünmesin

        foreach (var stickerBtn in stickerButtons)
            stickerBtn.interactable = true;

        CheckReadyState();
    }

    // ---------- STICKER SEÇİMİ ----------
    // "Stickers" altındaki her butona bağlanacak (0/1/2)
    public void OnStickerButtonPressed(int stickerIndex)
    {
        if (isStickerPlaced) return;          // sadece 1 sticker koyulabilir
        if (selectedPackIndex < 0) return;    // henüz pack seçilmemiş, güvenlik kontrolü

        RectTransform targetTableRect = packTable.GetComponent<RectTransform>();

        StickerDragHandler dragHandler =
            draggedStickerPreview.GetComponent<StickerDragHandler>();

        dragHandler.BeginPlacement(stickerSprites[stickerIndex], targetTableRect, this);
    }

    // StickerDragHandler, sticker yerleştirildiğinde çağırır
    public void OnStickerPlaced()
    {
        isStickerPlaced = true;

        foreach (var stickerBtn in stickerButtons)
            stickerBtn.interactable = false;

        CheckReadyState();
    }

    private void CheckReadyState()
    {
        getPackedGunButton.interactable = isPackTableClosed && isStickerPlaced;
    }

    // ---------- GET PACKED GUN ----------
    public void OnGetPackedGunPressed()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    // ---------- TRASH BIN (RESET) ----------
    // "Trash Bin" butonuna bağlanacak
    public void OnTrashBinPressed()
    {
        ResetGame();
    }

    private void ResetGame()
    {
        gunObject.SetActive(true);   // silah baştan görünür

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