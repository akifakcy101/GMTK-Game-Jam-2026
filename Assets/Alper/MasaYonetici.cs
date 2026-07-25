using UnityEngine;

public class MasaYonetici : MonoBehaviour
{
    [Header("Sistem Bağlantıları")]
    public Spawner parmakIziUretici;
    public Transform esyaCikmaNoktasi; 

    [Header("Kamera Bağlantıları")]
    public GameObject anaOyunKamerasi; // Karakteri takip eden asıl kamera
    public GameObject masaKamerasi;    // Masa kamerası

    [Header("Test Ayarları")]
    [Tooltip("Envanter boşsa veya Prefab bulunamazsa test amaçlı doğurulacak varsayılan eşya Prefab'ı")]
    public GameObject testEsyaPrefab;

    private GameObject masadakiMevcutEsya;
    private bool masaAcikMi = false;
    private bool izlerBasildiMi = false;

    // 1. ANA OYUNDAN (Karakterden / Trigger'dan / Inspector'dan) ÇAĞIRILACAK OLAN AÇMA FONKSİYONU
    [ContextMenu("Test Masasını Aç")]
    public void MasayiAc()
    {
        if (masaAcikMi) return;

        // Kameraları değiştir: Ana oyunu kapat, masayı göster
        if (anaOyunKamerasi != null) anaOyunKamerasi.SetActive(false);
        if (masaKamerasi != null) masaKamerasi.SetActive(true);
        masaAcikMi = true;
        izlerBasildiMi = false;

        Vector3 spawnPos = esyaCikmaNoktasi != null ? esyaCikmaNoktasi.position : transform.position;
        spawnPos.z = 0f;

        masadakiMevcutEsya = CreateItemObjectOnDesk(spawnPos);

        if (masadakiMevcutEsya == null)
        {
            Debug.LogError("<color=red>[MasaYonetici]</color> Masaya koyulacak eşya oluşturulamadı! (Müşteri eşyasının Prefab/Sprite'ı veya Test Prefab eksik).");
            return;
        }

        if (parmakIziUretici == null)
        {
            Debug.LogError("<color=red>[MasaYonetici]</color> 'Parmak Izi Uretici' (Spawner) scripti MasaYonetici'ye atanmamış!");
            return;
        }

        // Üzerine parmak izlerini bas
        int izSayisi = parmakIziUretici.IzleriOlustur(masadakiMevcutEsya.transform);
        if (izSayisi > 0)
        {
            izlerBasildiMi = true;
            Debug.Log($"<color=green>[MasaYonetici]</color> Müşterinin eşyası '{masadakiMevcutEsya.name}' masada doğruldu ve {izSayisi} adet parmak izi basıldı.");
        }
        else
        {
            Debug.LogWarning("<color=yellow>[MasaYonetici]</color> Parmak izleri oluşturulamadı! Spawner scriptindeki 'Parmak Izi Prefab' alanını kontrol edin.");
        }
    }

    private GameObject CreateItemObjectOnDesk(Vector3 spawnPos)
    {
        // 1. Önce oyuncunun envanterindeki eşyaya (ItemData) bak
        if (PlayerInventory.Instance != null && PlayerInventory.Instance.hasItem && PlayerInventory.Instance.currentItem != null)
        {
            ItemData itemData = PlayerInventory.Instance.currentItem;

            // 1a. Eğer özel bir 2D/3D Prefab tanımlandıysa onu doğur
            if (itemData.itemPrefab != null)
            {
                return Instantiate(itemData.itemPrefab, spawnPos, Quaternion.identity);
            }
            // 1b. Prefab yoksa ama Sprite ikonu varsa, otomatik SpriteRenderer objesi oluştur
            else if (itemData.itemIcon != null)
            {
                GameObject dynamicItemObj = new GameObject($"Item_{itemData.itemName}");
                dynamicItemObj.transform.position = spawnPos;

                SpriteRenderer sr = dynamicItemObj.AddComponent<SpriteRenderer>();
                sr.sprite = itemData.itemIcon;
                sr.sortingOrder = 1;

                // Parmak izi konumlandırması için BoxCollider2D ekle
                BoxCollider2D col = dynamicItemObj.AddComponent<BoxCollider2D>();

                return dynamicItemObj;
            }
        }

        // 2. Envanterde eşya yoksa veya Sprite/Prefab atanmamışsa Test Prefab'ını doğur
        if (testEsyaPrefab != null)
        {
            return Instantiate(testEsyaPrefab, spawnPos, Quaternion.identity);
        }

        return null;
    }

    void Update()
    {
        // 2. OTOMATİK KAPANMA KONTROLÜ
        // YALNIZCA parmak izleri basıldıktan SONRA ve hiç parmak izi kalmadığında masayı kapat!
        if (masaAcikMi && izlerBasildiMi && masadakiMevcutEsya != null)
        {
            if (masadakiMevcutEsya.transform.childCount == 0)
            {
                MasayiKapat();
            }
        }
    }

    // 3. İŞ BİTİNCE MASAYI KAPATAN FONKSİYON
    public void MasayiKapat()
    {
        if (!masaAcikMi) return;

        masaAcikMi = false;
        izlerBasildiMi = false;

        // Masadaki temizlenmiş eşyayı yok et
        if (masadakiMevcutEsya != null)
        {
            Destroy(masadakiMevcutEsya);
        }

        // Kameraları eski haline çevir: Masayı kapat, ana oyuna dön
        if (masaKamerasi != null) masaKamerasi.SetActive(false);
        if (anaOyunKamerasi != null) anaOyunKamerasi.SetActive(true);

        // Envanterdeki eşyanın aşamasını ilerlet (Temizlendi -> Bir sonraki masaya hazır)
        if (PlayerInventory.Instance != null && PlayerInventory.Instance.hasItem)
        {
            PlayerInventory.Instance.AdvanceItemStage();
        }

        Debug.Log("<color=cyan>[MasaYonetici]</color> Tüm parmak izleri temizlendi! Masa kapatıldı ve oyuncu envanteri güncellendi.");
    }
}