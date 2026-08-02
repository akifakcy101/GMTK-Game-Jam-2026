using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class FingerprintData
{
    public Vector3 relativePosition;
    public float zRotation;

    public FingerprintData(Vector3 relPos, float rot)
    {
        relativePosition = relPos;
        zRotation = rot;
    }
}

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [Header("Envanter Durumu")]
    public bool hasItem = false;
    [Tooltip("-1: Eşya Yok, 0: Müşteriden Alındı (Ham), 1: Masa 1 Bitti, 2: Masa 2 Bitti, 3: Masa 3 Bitti (Teslime Hazır)")]
    public int itemStage = -1;
    
    [Header("Mevcut Eşya Verisi")]
    public ItemData currentItem;

    [Header("Masa 1 Verileri (Parmak İzleri)")]
    public bool isFingerprintsGenerated = false;
    public List<FingerprintData> currentFingerprints = new List<FingerprintData>();

    [Header("Masa 2 Verileri (Barkod)")]
    public bool isBarcodeErased = false;
    public bool isNewBarcodeAttached = false;

    [Header("Masa 3 Verileri (Paketleme Sipariş İsteği)")]
    public int requestedPackIndex = -1;
    public int requestedStickerIndex = -1;
    public int appliedPackIndex = -1;
    public int appliedStickerIndex = -1;

    [Header("Arayüz Gösterimi (İsteğe Bağlı)")]
    public Image inventoryIconDisplay;
    public TextMeshProUGUI inventoryNameDisplay;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateInventoryUI();
    }

    public void ReceiveItemFromCustomer(ItemData item, int packIndex = 0, int stickerIndex = 0)
    {
        hasItem = true;
        itemStage = 0; // Ham aşama
        currentItem = item;
        requestedPackIndex = packIndex;
        requestedStickerIndex = stickerIndex;
        appliedPackIndex = -1;
        appliedStickerIndex = -1;

        // Masa 1 ve Masa 2 kalıcı verilerini sıfırla
        isFingerprintsGenerated = false;
        currentFingerprints.Clear();
        isBarcodeErased = false;
        isNewBarcodeAttached = false;

        string nameStr = currentItem != null ? currentItem.itemName : "Bilinmeyen Eşya";
        Debug.Log($"<color=cyan>[Inventory]</color> Müşteriden eşya alındı: {nameStr} (Aşama 0), İstek -> Paket: {requestedPackIndex}, Sticker: {requestedStickerIndex}");

        UpdateInventoryUI();
    }

    public void AdvanceItemStage()
    {
        if (!hasItem) return;

        itemStage++;
        Debug.Log($"<color=green>[Inventory]</color> Eşya aşaması ilerletildi! Yeni Aşama: {itemStage}");
        UpdateInventoryUI();
    }

    public void ClearItem()
    {
        hasItem = false;
        itemStage = -1;
        currentItem = null;
        requestedPackIndex = -1;
        requestedStickerIndex = -1;
        appliedPackIndex = -1;
        appliedStickerIndex = -1;
        isFingerprintsGenerated = false;
        currentFingerprints.Clear();
        isBarcodeErased = false;
        isNewBarcodeAttached = false;
        Debug.Log("<color=yellow>[Inventory]</color> Envanter temizlendi.");
        UpdateInventoryUI();
    }

    public void UpdateInventoryUI()
    {
        if (inventoryIconDisplay != null)
        {
            if (hasItem && currentItem != null && currentItem.itemIcon != null)
            {
                inventoryIconDisplay.sprite = currentItem.itemIcon;
                inventoryIconDisplay.enabled = true;
            }
            else
            {
                inventoryIconDisplay.sprite = null;
                inventoryIconDisplay.enabled = false;
            }
        }

        if (inventoryNameDisplay != null)
        {
            if (hasItem && currentItem != null)
            {
                inventoryNameDisplay.text = $"{currentItem.itemName} (Aşama {itemStage})";
            }
            else
            {
                inventoryNameDisplay.text = "Boş";
            }
        }
    }
}
