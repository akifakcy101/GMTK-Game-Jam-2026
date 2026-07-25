using UnityEngine;

public class BarcodeManager : MasaYonetici
{
    [Header("Barkod Silici Sistem Bağlantısı")]
    public FausetSinkEraser silgiSistemi;

    [Header("Yeni Barkod (Sticker) Bağlantısı")]
    public StickerController yeniBarkodSticker;

    private bool isBarcodeErased = false;
    private bool isNewBarcodeAttached = false;

    private Transform initialStickerParent;
    private Vector3 initialStickerLocalPos;

    private void Start()
    {
        // Eğer Inspector'dan yeniBarkodSticker atanmadıysa çocuk objelerden bul
        if (yeniBarkodSticker == null)
        {
            yeniBarkodSticker = GetComponentInChildren<StickerController>();
        }

        if (yeniBarkodSticker != null)
        {
            initialStickerParent = yeniBarkodSticker.transform.parent;
            initialStickerLocalPos = yeniBarkodSticker.transform.localPosition;
        }
    }

    public override void MasayiAc()
    {
        if (masaAcikMi) return;

        // Kameraları değiştir: Ana oyunu kapat, masayı göster (eğer tanımlıysa)
        if (anaOyunKamerasi != null) anaOyunKamerasi.SetActive(false);
        if (masaKamerasi != null) masaKamerasi.SetActive(true);
        
        masaAcikMi = true;

        Vector3 spawnPos = esyaCikmaNoktasi != null ? esyaCikmaNoktasi.position : transform.position;
        spawnPos.z = 0f;

        masadakiMevcutEsya = CreateItemObjectOnDesk(spawnPos);

        if (masadakiMevcutEsya == null)
        {
            Debug.LogError("<color=red>[BarcodeManager]</color> Masaya koyulacak eşya oluşturulamadı! (Müşteri eşyasının Prefab/Sprite'ı veya Test Prefab eksik).");
            return;
        }

        // 1. Yeni Barkod Sticker'ını masadaki ilk yerine sıfırla/doğur
        if (yeniBarkodSticker != null)
        {
            if (initialStickerParent == null)
            {
                initialStickerParent = yeniBarkodSticker.transform.parent;
                initialStickerLocalPos = yeniBarkodSticker.transform.localPosition;
            }

            yeniBarkodSticker.ResetSticker(initialStickerParent, initialStickerLocalPos);
            yeniBarkodSticker.onStickerAttached.RemoveListener(OnStickerAttached);
            yeniBarkodSticker.onStickerAttached.AddListener(OnStickerAttached);
        }

        // 2. Oyuncu geçmişte barkodu tam sildiyse veya yeni barkod taktıysa durumları oku
        bool previouslyErased = PlayerInventory.Instance != null && PlayerInventory.Instance.isBarcodeErased;
        bool previouslyAttached = PlayerInventory.Instance != null && PlayerInventory.Instance.isNewBarcodeAttached;

        isBarcodeErased = previouslyErased;
        isNewBarcodeAttached = previouslyAttached;

        // 3. Barkod silme sistemini kur
        if (silgiSistemi != null)
        {
            SpriteRenderer barkodRenderer = BarkodRendererBul(masadakiMevcutEsya);
            if (barkodRenderer != null)
            {
                silgiSistemi.onEraseCompleted.RemoveListener(OnBarcodeErased);
                silgiSistemi.onEraseCompleted.AddListener(OnBarcodeErased);
                silgiSistemi.YeniBarkodKur(barkodRenderer);

                // Eğer barkod önceden tam silindiyse, masaya girildiğinde de silik kalsın
                if (previouslyErased)
                {
                    silgiSistemi.ClearRemainingPixels();
                    Debug.Log("<color=green>[BarcodeManager]</color> Eşyanın eski barkodu önceden tamamen silindiği için silik olarak yüklendi.");
                }
                else
                {
                    Debug.Log($"<color=green>[BarcodeManager]</color> '{masadakiMevcutEsya.name}' masaya koyuldu ve barkod silme sistemi hazırlandı.");
                }
            }
            else
            {
                Debug.LogWarning($"<color=yellow>[BarcodeManager]</color> '{masadakiMevcutEsya.name}' üzerinde barkod (Tag: 'Barkod' veya İsim: 'Barcode'/'Barkod') bulunamadı!");
            }
        }
        else
        {
            Debug.LogError("<color=red>[BarcodeManager]</color> FausetSinkEraser (silgiSistemi) atanmamış!");
        }
    }

    private void OnBarcodeErased()
    {
        isBarcodeErased = true;
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.isBarcodeErased = true;
        }

        Debug.Log("<color=green>[BarcodeManager]</color> Eski barkod başarıyla silindi!");
        CheckAndCompleteStage();
    }

    private void OnStickerAttached()
    {
        isNewBarcodeAttached = true;
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.isNewBarcodeAttached = true;
        }

        Debug.Log("<color=green>[BarcodeManager]</color> Yeni barkod başarıyla yapıştırıldı!");
        CheckAndCompleteStage();
    }

    private void CheckAndCompleteStage()
    {
        // HEM eski barkod silinmiş HEM DE yeni barkod yapıştırılmış olmalı
        if (isBarcodeErased && isNewBarcodeAttached)
        {
            if (PlayerInventory.Instance != null && PlayerInventory.Instance.hasItem && PlayerInventory.Instance.itemStage == 1)
            {
                PlayerInventory.Instance.AdvanceItemStage();
                Debug.Log("<color=cyan>[BarcodeManager]</color> Eski barkod silindi VE yeni barkod yapıştırıldı! Masa 2 (Aşama 2) tamamlandı.");
            }
        }
    }

    public override void MasayiKapat()
    {
        if (!masaAcikMi) return;

        masaAcikMi = false;

        // Dinleyicileri temizle
        if (silgiSistemi != null)
        {
            silgiSistemi.onEraseCompleted.RemoveListener(OnBarcodeErased);
            silgiSistemi.erasableSpriteRenderer = null;
        }

        // Sticker dinleyicisini temizle ve sticker'ı eşya yok edilmeden önce masa üzerine geri al
        if (yeniBarkodSticker != null)
        {
            yeniBarkodSticker.onStickerAttached.RemoveListener(OnStickerAttached);
            yeniBarkodSticker.transform.SetParent(initialStickerParent);
            yeniBarkodSticker.transform.localPosition = initialStickerLocalPos;
        }

        // Masadaki eşyayı yok et
        if (masadakiMevcutEsya != null)
        {
            Destroy(masadakiMevcutEsya);
            masadakiMevcutEsya = null;
        }

        // Tamamlanma kontrolü bildirimi
        if (PlayerInventory.Instance != null && PlayerInventory.Instance.hasItem)
        {
            if (PlayerInventory.Instance.itemStage == 1)
            {
                Debug.LogWarning($"<color=yellow>[BarcodeManager]</color> Masadan ayrılındı fakat işlemler henüz bitmedi. (Eski Barkod Silindi mi: {isBarcodeErased}, Yeni Barkod Takıldı mı: {isNewBarcodeAttached})");
            }
        }

        // Kameraları eski haline çevir (eğer tanımlıysa)
        if (masaKamerasi != null) masaKamerasi.SetActive(false);
        if (anaOyunKamerasi != null) anaOyunKamerasi.SetActive(true);
    }

    protected SpriteRenderer BarkodRendererBul(GameObject itemObj)
    {
        if (itemObj == null) return null;

        // 1. Tag ile "Barkod" objesini ara
        Transform barkodTransform = TagIleObjeBul(itemObj.transform, "Barkod");
        if (barkodTransform != null)
        {
            SpriteRenderer sr = barkodTransform.GetComponent<SpriteRenderer>();
            if (sr != null) return sr;
        }

        // 2. Isminde "Barkod" veya "Barcode" geçen çocuk objeleri ara
        SpriteRenderer[] renderers = itemObj.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in renderers)
        {
            string lowerName = sr.gameObject.name.ToLower();
            if (lowerName.Contains("barkod") || lowerName.Contains("barcode"))
            {
                return sr;
            }
        }

        // 3. Bulunamazsa child sprite renderer dene
        return itemObj.GetComponentInChildren<SpriteRenderer>();
    }

    private Transform TagIleObjeBul(Transform parent, string tag)
    {
        if (parent.CompareTag(tag)) return parent;

        foreach (Transform child in parent)
        {
            Transform sonuc = TagIleObjeBul(child, tag);
            if (sonuc != null) return sonuc;
        }
        return null;
    }
}
