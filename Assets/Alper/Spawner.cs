using System.Collections.Generic;
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
            esyaResmi = gelenEsya.GetComponentInChildren<SpriteRenderer>();
        }

        if (esyaResmi == null)
        {
            Debug.LogError("<color=red>[Spawner]</color> Gelen eşyada veya çocuklarında SpriteRenderer bulunamadı! Parmak izleri oluşturulamadı.");
            return 0;
        }

        PlayerInventory inv = PlayerInventory.Instance;

        // 1. İLK DEFA MASAYA KOYULUYORSA: Yeni rastgele parmak izi verileri üret ve PlayerInventory'ye kaydet
        if (inv != null && !inv.isFingerprintsGenerated)
        {
            Bounds sinirlar = esyaResmi.bounds;
            int uretilecekMiktar = Random.Range(minIzSayisi, maxIzSayisi + 1);

            inv.currentFingerprints.Clear();

            for (int i = 0; i < uretilecekMiktar; i++)
            {
                float rastgeleX = Mathf.Lerp(sinirlar.min.x, sinirlar.max.x, Random.Range(0.15f, 0.85f));
                float rastgeleY = Mathf.Lerp(sinirlar.min.y, sinirlar.max.y, Random.Range(0.15f, 0.85f));
                
                Vector3 worldPos = new Vector3(rastgeleX, rastgeleY, gelenEsya.position.z - 0.1f);
                Vector3 relPos = gelenEsya.InverseTransformPoint(worldPos);
                float rot = Random.Range(0f, 360f);

                FingerprintData fpData = new FingerprintData(relPos, rot);
                inv.currentFingerprints.Add(fpData);
            }

            inv.isFingerprintsGenerated = true;
            Debug.Log($"<color=green>[Spawner]</color> Eşya için {inv.currentFingerprints.Count} adet yeni parmak izi verisi üretildi ve kaydedildi.");
        }

        // 2. MEVCUT KALAN PARMAK İZLERİNİ MASADA DOĞUR
        List<FingerprintData> fingerprintsToSpawn = inv != null ? new List<FingerprintData>(inv.currentFingerprints) : new List<FingerprintData>();

        int olusturulanSayi = 0;

        foreach (FingerprintData fpData in fingerprintsToSpawn)
        {
            Vector3 worldPos = gelenEsya.TransformPoint(fpData.relativePosition);
            Quaternion rotation = Quaternion.Euler(0, 0, fpData.zRotation);

            GameObject yeniIz = Instantiate(parmakIziPrefab, worldPos, rotation);
            yeniIz.transform.SetParent(gelenEsya);

            if (!yeniIz.CompareTag("Iz"))
            {
                yeniIz.tag = "Iz";
            }

            // FingerprintInstance bileşenini bağla
            FingerprintInstance fpComp = yeniIz.GetComponent<FingerprintInstance>();
            if (fpComp == null)
            {
                fpComp = yeniIz.AddComponent<FingerprintInstance>();
            }
            fpComp.data = fpData;

            SpriteRenderer izSprite = yeniIz.GetComponent<SpriteRenderer>();
            if (izSprite != null && esyaResmi != null)
            {
                izSprite.sortingOrder = esyaResmi.sortingOrder + 1;
            }

            olusturulanSayi++;
        }

        Debug.Log($"<color=green>[Spawner]</color> Eşya üzerine {olusturulanSayi} adet kalan parmak izi yerleştirildi.");
        return olusturulanSayi;
    }
}