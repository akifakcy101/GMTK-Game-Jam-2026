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

    public void ReceiveItemFromCustomer(ItemData item)
    {
        hasItem = true;
        itemStage = 0; // Ham aşama
        currentItem = item;

        // Masa 1 kalıcı verilerini sıfırla
        isFingerprintsGenerated = false;
        currentFingerprints.Clear();

        string nameStr = currentItem != null ? currentItem.itemName : "Bilinmeyen Eşya";
        Debug.Log($"<color=cyan>[Inventory]</color> Müşteriden eşya alındı: {nameStr} (Aşama 0)");

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
        isFingerprintsGenerated = false;
        currentFingerprints.Clear();
        Debug.Log("<color=yellow>[Inventory]</color> Envanter temizlendi.");
        UpdateInventoryUI();
    }

    private void UpdateInventoryUI()
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
