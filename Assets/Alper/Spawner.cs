using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Üretim Ayarları")]
    public GameObject parmakIziPrefab; 
    public int minIzSayisi = 1;
    public int maxIzSayisi = 6; 

    public void IzleriOlustur(Transform gelenEsya)
    {
        if (gelenEsya == null) return;

        SpriteRenderer esyaResmi = gelenEsya.GetComponent<SpriteRenderer>();
        
        if (esyaResmi == null)
        {
            Debug.LogWarning("Gelen eşyada SpriteRenderer yok! İzler oluşturulamadı.");
            return;
        }

        Bounds sinirlar = esyaResmi.bounds;

        int uretilecekMiktar = Random.Range(minIzSayisi, maxIzSayisi + 1);

        for (int i = 0; i < uretilecekMiktar; i++)
        {

            float rastgeleX = Random.Range(sinirlar.min.x + 0.2f, sinirlar.max.x - 0.2f);
            float rastgeleY = Random.Range(sinirlar.min.y + 0.2f, sinirlar.max.y - 0.2f);
            
            Vector2 dogmaNoktasi = new Vector2(rastgeleX, rastgeleY);
            Quaternion rastgeleDonus = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            
            GameObject yeniIz = Instantiate(parmakIziPrefab, dogmaNoktasi, rastgeleDonus);
            yeniIz.transform.SetParent(gelenEsya);
        }
    }
}