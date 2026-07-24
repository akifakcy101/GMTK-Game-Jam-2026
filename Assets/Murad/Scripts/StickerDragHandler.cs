using UnityEngine;
using UnityEngine.UI;

public class StickerDragHandler : MonoBehaviour
{
    [Header("Sınır dışıyken / içindeyken renk feedback (opsiyonel)")]
    public Color validColor = Color.white;
    public Color invalidColor = new Color(1f, 0.4f, 0.4f, 0.85f);

    private RectTransform rectTransform;
    private Image image;
    private Canvas canvas;

    private RectTransform targetPackTableRect;
    private PackingGameManager gameManager;
    private bool isDragging = false;

    // Awake() yerine lazy-load: obje sahne başında inactive ise Awake()
    // hiç çalışmaz ve bu referanslar null kalır. Property her kullanımda
    // component'in atanmış olduğunu garantiler.
    private RectTransform RectTransform
    {
        get
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            return rectTransform;
        }
    }

    private Image Image
    {
        get
        {
            if (image == null) image = GetComponent<Image>();
            return image;
        }
    }

    private Canvas ParentCanvas
    {
        get
        {
            if (canvas == null) canvas = GetComponentInParent<Canvas>(true); // includeInactive: true
            return canvas;
        }
    }

    // GameManager tarafından çağrılır: sticker sprite'ı ata, hedef Pack Table'ı belirle, sürüklemeyi başlat
    public void BeginPlacement(Sprite stickerSprite, RectTransform packTableRect, PackingGameManager manager)
    {
        Image.sprite = stickerSprite;
        targetPackTableRect = packTableRect;
        gameManager = manager;

        gameObject.SetActive(true);
        isDragging = true;
    }

    void Update()
    {
        if (!isDragging) return;

        // Sticker'ı sabit boyutta tutup sadece pozisyonunu mouse'a göre güncelle
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            ParentCanvas.transform as RectTransform,
            Input.mousePosition,
            ParentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : ParentCanvas.worldCamera,
            out localPoint);

        RectTransform.localPosition = localPoint;

        bool isValidPosition = IsFullyInsidePackTable();
        Image.color = isValidPosition ? validColor : invalidColor;

        if (Input.GetMouseButtonDown(0) && isValidPosition)
        {
            PlaceSticker();
        }
    }

    private bool IsFullyInsidePackTable()
    {
        if (targetPackTableRect == null) return false;

        Vector3[] stickerCorners = new Vector3[4];
        RectTransform.GetWorldCorners(stickerCorners);   // sabit boyut, sadece pozisyon değişiyor

        Vector3[] tableCorners = new Vector3[4];
        targetPackTableRect.GetWorldCorners(tableCorners);

        Bounds tableBounds = new Bounds();
        tableBounds.SetMinMax(tableCorners[0], tableCorners[2]);  // bottom-left, top-right

        foreach (var corner in stickerCorners)
        {
            if (!tableBounds.Contains(corner))
                return false;
        }
        return true;
    }

    private void PlaceSticker()
    {
        isDragging = false;
        Image.color = validColor;
        gameManager.OnStickerPlaced();
    }
}