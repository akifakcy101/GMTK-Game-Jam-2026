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

    private int selectedPackIndex = -1; 
    private bool isPackTableClosed = false;
    private bool isStickerPlaced = false;

    void Start()
    {
        ResetGame();
    }

    public void OnPutGunPressed()
    {
        gunObject.SetActive(true);
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
        SceneManager.LoadScene(nextSceneName);
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