using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Üretim Ayarları")]
    public GameObject parmakIziPrefab; 
    public int minIzSayisi = 1;
    public int maxIzSayisi = 6; 

    public int IzleriOlustur(Transform gelenEsya)
    {
        if (gelenEsya == null)
        {
            Debug.LogError("<color=red>[Spawner]</color> Gelen eşya null!");
            return 0;
        }

        if (parmakIziPrefab == null)
        {
            Debug.LogError("<color=red>[Spawner]</color> Parmak İzi Prefab'ı (parmakIziPrefab) Spawner scriptine atanmamış!");
            return 0;
        }

        SpriteRenderer esyaResmi = gelenEsya.GetComponent<SpriteRenderer>();
        if (esyaResmi == null)
        {
            // Eğer eşyanın kendisinde yoksa çocuklarında ara
            esyaResmi = gelenEsya.GetComponentInChildren<SpriteRenderer>();
        }

        if (esyaResmi == null)
        {
            Debug.LogError("<color=red>[Spawner]</color> Gelen eşyada veya çocuklarında SpriteRenderer bulunamadı! Parmak izleri oluşturulamadı.");
            return 0;
        }

        Bounds sinirlar = esyaResmi.bounds;
        int uretilecekMiktar = Random.Range(minIzSayisi, maxIzSayisi + 1);
        int olusturulanSayi = 0;

        for (int i = 0; i < uretilecekMiktar; i++)
        {
            // Eşya sınırlarının %10 ile %90'ı arasında güvenli rastgele pozisyon
            float rastgeleX = Mathf.Lerp(sinirlar.min.x, sinirlar.max.x, Random.Range(0.15f, 0.85f));
            float rastgeleY = Mathf.Lerp(sinirlar.min.y, sinirlar.max.y, Random.Range(0.15f, 0.85f));
            
            // Z koordinatını eşyanın biraz önünde tut ki arka planda kalmasın
            Vector3 dogmaNoktasi = new Vector3(rastgeleX, rastgeleY, gelenEsya.position.z - 0.1f);
            Quaternion rastgeleDonus = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            
            GameObject yeniIz = Instantiate(parmakIziPrefab, dogmaNoktasi, rastgeleDonus);
            yeniIz.transform.SetParent(gelenEsya);

            // Tag kontrolü: Tag "Iz" yapılmalı ki fırça silsin
            if (!yeniIz.CompareTag("Iz"))
            {
                yeniIz.tag = "Iz";
            }

            // Parmak izinin görsel katmanını (Order in Layer) eşyadan 1 yüksek yap
            SpriteRenderer izSprite = yeniIz.GetComponent<SpriteRenderer>();
            if (izSprite != null && esyaResmi != null)
            {
                izSprite.sortingOrder = esyaResmi.sortingOrder + 1;
            }

            olusturulanSayi++;
        }

        Debug.Log($"<color=green>[Spawner]</color> Eşya üzerine {olusturulanSayi} adet parmak izi oluşturuldu.");
        return olusturulanSayi;
    }
}