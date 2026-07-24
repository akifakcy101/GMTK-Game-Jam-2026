using UnityEngine;
using UnityEngine.UI;

public class PackTableController : MonoBehaviour
{
    [System.Serializable]
    public class PackVariant
    {
        public string packName;   // sadece Inspector'da tanımak için (Carton/Container/Foil)
        public Sprite openSprite;
        public Sprite closedSprite;
    }

    [Header("Sıra Packs butonlarıyla aynı olmalı: 0=Carton, 1=Container, 2=Foil")]
    public PackVariant[] packVariants;

    public PackingGameManager gameManager;

    private Image image;
    private Button button;
    private int currentPackIndex = -1;

    void Awake()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();
    }

    // GameManager, bir Pack seçildiğinde çağırır
    public void SetPack(int packIndex)
    {
        currentPackIndex = packIndex;
        image.sprite = packVariants[packIndex].openSprite;
        button.interactable = true;
    }

    // Bu objenin Button OnClick() listesine bağlanacak
    public void OnTableClicked()
    {
        if (currentPackIndex < 0) return;   // henüz pack seçilmemiş, güvenlik kontrolü

        image.sprite = packVariants[currentPackIndex].closedSprite;
        button.interactable = false;        // tekrar tıklanmasın

        gameManager.OnPackTableClicked();
    }

    // Trash Bin / reset sırasında GameManager tarafından çağrılır
    public void ResetVisual()
    {
        currentPackIndex = -1;
        button.interactable = false;   // pack seçilene kadar tıklanamaz
        // sprite'ı boş bırakabilir ya da varsayılan bir "boş masa" sprite'ı atayabilirsin;
        // şu an hiçbir pack seçili değilken görsel olarak ne göstereceğine karar ver
    }
}