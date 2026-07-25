using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [Header("Envanter Durumu")]
    public bool hasItem = false;
    [Tooltip("-1: Eşya Yok, 0: Müşteriden Alındı (Ham), 1: Masa 1 Bitti, 2: Masa 2 Bitti, 3: Masa 3 Bitti (Teslime Hazır)")]
    public int itemStage = -1;
    public string currentItemName = "";

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

    public void ReceiveItemFromCustomer(string itemName)
    {
        hasItem = true;
        itemStage = 0; // Ham aşama
        currentItemName = itemName;
        Debug.Log($"<color=cyan>[Inventory]</color> Müşteriden eşya alındı: {itemName} (Aşama 0)");
    }

    public void AdvanceItemStage()
    {
        if (!hasItem) return;

        itemStage++;
        Debug.Log($"<color=green>[Inventory]</color> Eşya aşaması ilerletildi! Yeni Aşama: {itemStage}");
    }

    public void ClearItem()
    {
        hasItem = false;
        itemStage = -1;
        currentItemName = "";
        Debug.Log("<color=yellow>[Inventory]</color> Envanter temizlendi.");
    }
}
