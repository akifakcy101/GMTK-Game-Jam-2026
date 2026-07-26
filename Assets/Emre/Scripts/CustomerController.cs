using UnityEngine;

public class CustomerController : MonoBehaviour
{
    public enum CustomerState
    {
        WalkingToPosition,
        WaitingInQueue,
        WalkingToExit
    }

    [Header("Hareket Ayarları")]
    public float moveSpeed = 3f;

    [Header("Durum")]
    public CustomerState currentState = CustomerState.WalkingToPosition;

    [Header("Müşterinin Eşyası")]
    public ItemData assignedItem;
    [Tooltip("Eğer doğarken atanmadıysa kullanılabilecek varsayılan eşya havuzu")]
    public ItemData[] possibleItems;

    [Header("Paketleme İpuçları / İstekleri")]
    [Tooltip("Müşterinin paketleme için istediği kutu/paket türünün indeksi (0: Carton, 1: Container, 2: Foil)")]
    public int requestedPackIndex = 0;
    [Tooltip("Müşterinin paketleme için istediği sticker türünün indeksi (0, 1, 2)")]
    public int requestedStickerIndex = 0;

    [Tooltip("Toplam paket seçeneği sayısı")]
    public int maxPackTypes = 3;
    [Tooltip("Toplam sticker seçeneği sayısı")]
    public int maxStickerTypes = 3;

    [Header("Görsel Gösterim")]
    [Tooltip("Müşterinin üzerinde taşıdığı eşyanın ikonu/görseli (Boş bırakılırsa otomatik oluşturulur)")]
    public SpriteRenderer itemDisplayRenderer;

    [Tooltip("Müşterinin kafasındaki eşya ve baloncukların arkasında duracak tekil arka plan sprite'ı")]
    public Sprite headDisplayBackgroundSprite;

    [Header("Müşteri Görünümü")]
    [Tooltip("Doğarken rastgele seçilecek yandan görünüm sprite'ları (NPC/side sprite'ları)")]
    public Sprite[] possibleBodySprites;

    private Vector3 currentTargetPosition;
    private Vector3 exitPosition;
    private bool isSetupCalled = false;

    private void Start()
    {
        AssignRandomBodySprite();

        if (assignedItem == null)
        {
            TryAssignRandomItem();
        }
        else
        {
            UpdateItemDisplay();
        }

        if (!isSetupCalled)
        {
            requestedPackIndex = Random.Range(0, maxPackTypes);
            requestedStickerIndex = Random.Range(0, maxStickerTypes);
        }
    }

    private void AssignRandomBodySprite()
    {
        if (possibleBodySprites == null || possibleBodySprites.Length == 0) return;

        SpriteRenderer bodyRenderer = GetComponent<SpriteRenderer>();
        if (bodyRenderer == null) return;

        bodyRenderer.sprite = possibleBodySprites[Random.Range(0, possibleBodySprites.Length)];
        bodyRenderer.flipX = true; // Kaynak sprite'lar sağa bakıyor, sola çeviriyoruz
    }

    public void Setup(Vector3 initialTargetPos, Vector3 exitPos, ItemData item = null, int packIndex = -1, int stickerIndex = -1)
    {
        isSetupCalled = true;
        this.currentTargetPosition = initialTargetPos;
        this.exitPosition = exitPos;
        if (item != null)
        {
            this.assignedItem = item;
        }
        else if (this.assignedItem == null)
        {
            TryAssignRandomItem();
        }

        if (packIndex >= 0)
        {
            this.requestedPackIndex = packIndex;
        }
        else
        {
            this.requestedPackIndex = Random.Range(0, maxPackTypes);
        }

        if (stickerIndex >= 0)
        {
            this.requestedStickerIndex = stickerIndex;
        }
        else
        {
            this.requestedStickerIndex = Random.Range(0, maxStickerTypes);
        }

        UpdateItemDisplay();
        currentState = CustomerState.WalkingToPosition;
    }

    private void TryAssignRandomItem()
    {
        if (possibleItems != null && possibleItems.Length > 0)
        {
            assignedItem = possibleItems[Random.Range(0, possibleItems.Length)];
        }
        else
        {
            // Editörde veya Resources'da bulunan ItemData'ları ara
            ItemData[] allItems = Resources.LoadAll<ItemData>("");
            if (allItems != null && allItems.Length > 0)
            {
                assignedItem = allItems[Random.Range(0, allItems.Length)];
            }
        }
    }

    public void UpdateItemDisplay()
    {
        if (assignedItem == null) return;

        if (itemDisplayRenderer == null)
        {
            Transform existingChild = transform.Find("ItemDisplay");
            if (existingChild != null)
            {
                itemDisplayRenderer = existingChild.GetComponent<SpriteRenderer>();
            }
            else
            {
                GameObject child = new GameObject("ItemDisplay");
                child.transform.SetParent(transform);
                // Root 2 kat küçüldüğü için (0.5 scale), mutlak boyutu korumak adına 2 ile çarpılmış değerler
                child.transform.localPosition = new Vector3(0.7f, 0.7f, 0f);
                child.transform.localScale = new Vector3(0.45f, 0.45f, 1f);

                itemDisplayRenderer = child.AddComponent<SpriteRenderer>();
                SpriteRenderer parentSR = GetComponent<SpriteRenderer>();
                if (parentSR != null)
                {
                    itemDisplayRenderer.sortingLayerID = parentSR.sortingLayerID;
                    itemDisplayRenderer.sortingOrder = parentSR.sortingOrder + 1;
                }
            }
        }

        if (itemDisplayRenderer != null)
        {
            Sprite itemSprite = GetItemSprite(assignedItem);
            if (itemSprite != null)
            {
                itemDisplayRenderer.sprite = itemSprite;
                itemDisplayRenderer.enabled = true;
            }
        }
    }

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

    // Sıra ilerlediğinde müşteriye yeni hedefini verir
    public void UpdateTargetPosition(Vector3 newTargetPos)
    {
        this.currentTargetPosition = newTargetPos;
        currentState = CustomerState.WalkingToPosition;
    }

    private void Update()
    {
        UpdateBubbleSprites();
        UpdateBubbleTransforms();

        switch (currentState)
        {
            case CustomerState.WalkingToPosition:
                MoveTowardsPosition(currentTargetPosition);
                if (Vector3.Distance(transform.position, currentTargetPosition) < 0.05f)
                {
                    currentState = CustomerState.WaitingInQueue;
                }
                break;

            case CustomerState.WaitingInQueue:
                // Sıradaki yerinde bekliyor
                break;

            case CustomerState.WalkingToExit:
                MoveTowardsPosition(exitPosition);
                if (Vector3.Distance(transform.position, exitPosition) < 0.05f)
                {
                    Debug.Log("<color=yellow>[Customer]</color> Müşteri çıkışa ulaştı ve yok edildi.");
                    Destroy(gameObject);
                }
                break;
        }
    }

    private void MoveTowardsPosition(Vector3 destination)
    {
        transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
    }

    [Header("Paketleme İstek Görselleri (İkonlar)")]
    [Tooltip("Kutu/paket kapalı veya ikon sprite'ları (0: Carton, 1: Container, 2: Foil)")]
    public Sprite[] packSprites;
    [Tooltip("Sticker sprite'ları (0, 1, 2)")]
    public Sprite[] stickerSprites;
    [Tooltip("İstek balonu arka plan sprite'ı (opsiyonel)")]
    public Sprite bubbleBackgroundSprite;

    [Header("İstek Balonu Konum & Boyut Ayarları")]
    [Tooltip("Balonun müşteriye göre yerel konumu")]
    public Vector3 bubbleOffset = new Vector3(0.5f, 1.6f, 0f);
    [Tooltip("Balonun genel boyutu/ölçeği")]
    public Vector3 bubbleScale = new Vector3(0.8f, 0.8f, 1f);

    [Header("Kutu İkonu Ayarları")]
    [Tooltip("Kutu ikonunun balon içindeki bağıl konumu")]
    public Vector3 packIconOffset = new Vector3(-0.4f, 0f, 0f);
    [Tooltip("Kutu ikonunun boyutu")]
    public Vector3 packIconScale = new Vector3(0.45f, 0.45f, 1f);
    [Tooltip("Kutu arka planının bağıl konumu (Offset)")]
    public Vector3 packBgOffset = Vector3.zero;
    [Tooltip("Kutu arka planının boyutu (Scale)")]
    public Vector3 packBgScale = new Vector3(1.3f, 1.3f, 1f);

    [Header("Sticker İkonu Ayarları")]
    [Tooltip("Sticker ikonunun balon içindeki bağıl konumu")]
    public Vector3 stickerIconOffset = new Vector3(0.4f, 0f, 0f);
    [Tooltip("Sticker ikonunun boyutu")]
    public Vector3 stickerIconScale = new Vector3(0.45f, 0.45f, 1f);
    [Tooltip("Sticker arka planının bağıl konumu (Offset)")]
    public Vector3 stickerBgOffset = Vector3.zero;
    [Tooltip("Sticker arka planının boyutu (Scale)")]
    public Vector3 stickerBgScale = new Vector3(1.3f, 1.3f, 1f);

    private GameObject requestBubbleObject;

    public ItemData GiveItemToPlayer()
    {
        if (assignedItem == null)
        {
            TryAssignRandomItem();
        }

        ItemData itemToGive = assignedItem;

        // Eşya verildikten sonra müşterinin elindeki eşyayı gizle
        if (itemDisplayRenderer != null)
        {
            itemDisplayRenderer.enabled = false;
        }

        // Müşterinin kafasının üstünde istediği kutu ve sticker balonunu göster
        ShowPackingRequestBubble();

        Debug.Log($"<color=cyan>[Customer]</color> Müşteri eşyasını oyuncuya verdi: {(itemToGive != null ? itemToGive.itemName : "Bilinmeyen Eşya")}. İstek -> Pack: {requestedPackIndex}, Sticker: {requestedStickerIndex}");
        return itemToGive;
    }

    public void ShowPackingRequestBubble()
    {
        if (requestBubbleObject == null)
        {
            requestBubbleObject = new GameObject("PackingRequestBubble");
            requestBubbleObject.transform.SetParent(transform);

            SpriteRenderer parentSR = GetComponent<SpriteRenderer>();
            int baseSortingOrder = parentSR != null ? parentSR.sortingOrder + 2 : 10;
            int sortingLayerID = parentSR != null ? parentSR.sortingLayerID : 0;

            // Kutu / Paket ikonu (sol taraf)
            GameObject packObj = new GameObject("PackIcon");
            packObj.transform.SetParent(requestBubbleObject.transform, false);
            SpriteRenderer packSR = packObj.AddComponent<SpriteRenderer>();
            packSR.sortingLayerID = sortingLayerID;
            packSR.sortingOrder = baseSortingOrder + 1;

            GameObject packBgObj = new GameObject("PackBG");
            packBgObj.transform.SetParent(packObj.transform, false);
            packBgObj.transform.localPosition = Vector3.zero;
            packBgObj.transform.localScale = Vector3.one * 1.3f;
            SpriteRenderer packBgSR = packBgObj.AddComponent<SpriteRenderer>();
            packBgSR.sortingLayerID = sortingLayerID;
            packBgSR.sortingOrder = baseSortingOrder;

            // Sticker ikonu (sağ taraf)
            GameObject stickerObj = new GameObject("StickerIcon");
            stickerObj.transform.SetParent(requestBubbleObject.transform, false);
            SpriteRenderer stickerSR = stickerObj.AddComponent<SpriteRenderer>();
            stickerSR.sortingLayerID = sortingLayerID;
            stickerSR.sortingOrder = baseSortingOrder + 1;

            GameObject stickerBgObj = new GameObject("StickerBG");
            stickerBgObj.transform.SetParent(stickerObj.transform, false);
            stickerBgObj.transform.localPosition = Vector3.zero;
            stickerBgObj.transform.localScale = Vector3.one * 1.3f;
            SpriteRenderer stickerBgSR = stickerBgObj.AddComponent<SpriteRenderer>();
            stickerBgSR.sortingLayerID = sortingLayerID;
            stickerBgSR.sortingOrder = baseSortingOrder;
        }

        UpdateBubbleSprites();
        UpdateBubbleTransforms();
        requestBubbleObject.SetActive(true);
    }

    private void UpdateBubbleSprites()
    {
        if (requestBubbleObject == null) return;

        Sprite bgSprite = bubbleBackgroundSprite != null ? bubbleBackgroundSprite : headDisplayBackgroundSprite;

        Transform packObj = requestBubbleObject.transform.Find("PackIcon");
        if (packObj != null)
        {
            SpriteRenderer packSR = packObj.GetComponent<SpriteRenderer>();
            if (packSR != null) packSR.sprite = GetPackSprite(requestedPackIndex);

            Transform packBgObj = packObj.Find("PackBG");
            if (packBgObj != null)
            {
                SpriteRenderer packBgSR = packBgObj.GetComponent<SpriteRenderer>();
                if (packBgSR != null)
                {
                    packBgSR.sprite = bgSprite;
                    packBgSR.enabled = (bgSprite != null);
                }
            }
        }

        Transform stickerObj = requestBubbleObject.transform.Find("StickerIcon");
        if (stickerObj != null)
        {
            SpriteRenderer stickerSR = stickerObj.GetComponent<SpriteRenderer>();
            if (stickerSR != null) stickerSR.sprite = GetStickerSprite(requestedStickerIndex);

            Transform stickerBgObj = stickerObj.Find("StickerBG");
            if (stickerBgObj != null)
            {
                SpriteRenderer stickerBgSR = stickerBgObj.GetComponent<SpriteRenderer>();
                if (stickerBgSR != null)
                {
                    stickerBgSR.sprite = bgSprite;
                    stickerBgSR.enabled = (bgSprite != null);
                }
            }
        }
    }

    private void UpdateBubbleTransforms()
    {
        if (requestBubbleObject == null) return;

        requestBubbleObject.transform.localPosition = bubbleOffset;
        requestBubbleObject.transform.localScale = bubbleScale;

        Transform packTransform = requestBubbleObject.transform.Find("PackIcon");
        if (packTransform != null)
        {
            packTransform.localPosition = packIconOffset;
            packTransform.localScale = packIconScale;

            Transform packBgTransform = packTransform.Find("PackBG");
            if (packBgTransform != null)
            {
                packBgTransform.localPosition = packBgOffset;
                packBgTransform.localScale = packBgScale;
            }
        }

        Transform stickerTransform = requestBubbleObject.transform.Find("StickerIcon");
        if (stickerTransform != null)
        {
            stickerTransform.localPosition = stickerIconOffset;
            stickerTransform.localScale = stickerIconScale;

            Transform stickerBgTransform = stickerTransform.Find("StickerBG");
            if (stickerBgTransform != null)
            {
                stickerBgTransform.localPosition = stickerBgOffset;
                stickerBgTransform.localScale = stickerBgScale;
            }
        }
    }

    private Sprite GetPackSprite(int index)
    {
        if (packSprites != null && index >= 0 && index < packSprites.Length && packSprites[index] != null)
        {
            return packSprites[index];
        }

        // Sahnede PackTableController otomatik arama
        PackTableController packTable = FindObjectOfType<PackTableController>(true);
        if (packTable != null && packTable.packVariants != null && index >= 0 && index < packTable.packVariants.Length)
        {
            var variant = packTable.packVariants[index];
            if (variant.closedSprite != null) return variant.closedSprite;
            if (variant.openSprite != null) return variant.openSprite;
        }

        return null;
    }

    private Sprite GetStickerSprite(int index)
    {
        if (stickerSprites != null && index >= 0 && index < stickerSprites.Length && stickerSprites[index] != null)
        {
            return stickerSprites[index];
        }

        // Sahnede PackingGameManager otomatik arama
        PackingGameManager manager = FindObjectOfType<PackingGameManager>(true);
        if (manager != null && manager.stickerSprites != null && index >= 0 && index < manager.stickerSprites.Length)
        {
            return manager.stickerSprites[index];
        }

        return null;
    }

    // Müşteriyle iş bittiğinde çağrılacak fonksiyon
    public void CompleteAndLeave()
    {
        if (currentState == CustomerState.WalkingToExit) return;
        
        CustomerCountdown countdown = GetComponentInChildren<CustomerCountdown>();
        if (countdown != null)
        {
            countdown.StopCountdown();
        }

        if (requestBubbleObject != null)
        {
            requestBubbleObject.SetActive(false);
        }

        currentState = CustomerState.WalkingToExit;
        Debug.Log("<color=orange>[Customer]</color> Müşteri dükkandan ayrılıyor...");
    }
}
