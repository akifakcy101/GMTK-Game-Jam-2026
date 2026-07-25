using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Eşya Bilgileri")]
    public string itemID;
    public string itemName;
    
    [Header("Görseller")]
    [Tooltip("UI ve Envanterde görünecek ikon")]
    public Sprite itemIcon;
    
    [Tooltip("Masada doğurulacak/işlenecek Prefab (İsteğe bağlı)")]
    public GameObject itemPrefab;
}
