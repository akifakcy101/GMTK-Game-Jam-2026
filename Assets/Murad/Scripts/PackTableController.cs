using UnityEngine;
using UnityEngine.UI;

public class PackTableController : MonoBehaviour
{
    [System.Serializable]
    public class PackVariant
    {
        public string packName;       // sadece Inspector'da tanımak için (Carton/Container/Foil)
        public GameObject packObject; // Pack Carton / Pack Container / Pack Foil objesinin kendisi
        public Sprite openSprite;
        public Sprite closedSprite;
    }

    [Header("Sıra Packs butonlarıyla aynı olmalı: 0=Carton, 1=Container, 2=Foil")]
    public PackVariant[] packVariants;

    public PackingGameManager gameManager;

    private int currentPackIndex = -1;

    // GameManager, bir Pack seçildiğinde çağırır
    public void SetPack(int packIndex)
    {
        currentPackIndex = packIndex;

        for (int i = 0; i < packVariants.Length; i++)
        {
            bool isSelected = (i == packIndex);
            packVariants[i].packObject.SetActive(isSelected);

            if (isSelected)
            {
                var img = packVariants[i].packObject.GetComponent<Image>();
                var btn = packVariants[i].packObject.GetComponent<Button>();
                if (img != null)
                {
                    img.sprite = packVariants[i].openSprite;
                    img.color = Color.white;
                }
                if (btn != null)
                {
                    ColorBlock cb = btn.colors;
                    cb.disabledColor = Color.white;
                    btn.colors = cb;
                    btn.interactable = true;
                }
            }
        }
    }

    // İlgili child'ın Button OnClick() listesine bağlanacak (Carton=0, Container=1, Foil=2)
    public void OnTableClicked(int packIndex)
    {
        var img = packVariants[packIndex].packObject.GetComponent<Image>();
        var btn = packVariants[packIndex].packObject.GetComponent<Button>();

        if (img != null)
        {
            img.sprite = packVariants[packIndex].closedSprite;
            img.color = Color.white;
        }

        if (btn != null)
        {
            ColorBlock cb = btn.colors;
            cb.disabledColor = Color.white;
            btn.colors = cb;
            btn.interactable = false;   // tekrar tıklanmasın
        }

        gameManager.OnPackTableClicked();
    }

    // Trash Bin / reset sırasında GameManager tarafından çağrılır
    public void ResetVisual()
    {
        currentPackIndex = -1;

        foreach (var variant in packVariants)
            variant.packObject.SetActive(false);
    }
}