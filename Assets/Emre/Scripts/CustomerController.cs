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

    public ItemData GiveItemToPlayer()
    {
        if (assignedItem == null)
        {
            TryAssignRandomItem();
        }

        ItemData itemToGive = assignedItem;

        // Eşya verildikten sonra müşterinin elindeki eşya görselini gizle
        if (itemDisplayRenderer != null)
        {
            itemDisplayRenderer.enabled = false;
        }

        Debug.Log($"<color=cyan>[Customer]</color> Müşteri eşyasını oyuncuya verdi: {(itemToGive != null ? itemToGive.itemName : "Bilinmeyen Eşya")}");
        return itemToGive;
    }

    // Müşteriyle iş bittiğinde çağrılacak fonksiyon
    public void CompleteAndLeave()
    {
        if (currentState == CustomerState.WalkingToExit) return;
        
        currentState = CustomerState.WalkingToExit;
        Debug.Log("<color=orange>[Customer]</color> Müşteri dükkandan ayrılıyor...");
    }
}
