using UnityEngine;
using UnityEngine.UI;

public class PackTableController : MonoBehaviour
{
    [System.Serializable]
    public class PackVariant
    {
        public string packName;
        public GameObject packObject;   // Pack Carton / Pack Container / Pack Foil objesinin kendisi
        public Sprite openSprite;
        public Sprite closedSprite;
    }

    [Header("Sıra Packs butonlarıyla aynı olmalı: 0=Carton, 1=Container, 2=Foil")]
    public PackVariant[] packVariants;

    public PackingGameManager gameManager;

    private Image image;
    private Button button;
    private int currentPackIndex = -1;

    // Awake() yerine lazy-load kullanıyoruz: obje sahne başında inactive ise
    // Awake() hiç çalışmaz ve image/button null kalır. Bu property her
    // kullanımda component'in atanmış olduğunu garantiler.
    private Image Image
    {
        get
        {
            if (image == null) image = GetComponent<Image>();
            return image;
        }
    }

    private Button Btn
    {
        get
        {
            if (button == null) button = GetComponent<Button>();
            return button;
        }
    }

    // PackTableController.cs — artık kendi Image/Button'ı yok, her metod seçili child'a erişiyor

    public void SetPack(int packIndex)
    {
        currentPackIndex = packIndex;

        for (int i = 0; i < packVariants.Length; i++)
            packVariants[i].packObject.SetActive(i == packIndex);   // sadece seçilen child aktif

        var img = packVariants[packIndex].packObject.GetComponent<Image>();
        var btn = packVariants[packIndex].packObject.GetComponent<Button>();
        img.sprite = packVariants[packIndex].openSprite;
        btn.interactable = true;
    }

    public void OnTableClicked(int packIndex)
    {
        var img = packVariants[packIndex].packObject.GetComponent<Image>();
        var btn = packVariants[packIndex].packObject.GetComponent<Button>();

        img.sprite = packVariants[packIndex].closedSprite;
        btn.interactable = false;

        gameManager.OnPackTableClicked();
    }

    public void ResetVisual()
    {
        currentPackIndex = -1;

        foreach (var variant in packVariants)
            variant.packObject.SetActive(false);
    }
}