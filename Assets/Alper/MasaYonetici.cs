using UnityEngine;

public class MasaYonetici : MonoBehaviour
{
    [Header("Sistem Bağlantıları")]
    public Spawner parmakIziUretici;
    public Transform esyaCikmaNoktasi; 

    [Header("Kamera Bağlantıları")]
    public GameObject anaOyunKamerasi; // Karakteri takip eden asıl kamera
    public GameObject masaKamerasi;    // Az önce oluşturduğumuz masa kamerası

    private GameObject masadakiMevcutEsya;
    private bool masaAcikMi = false;

    // 1. ANA OYUNDAN (Karakterden) ÇAĞIRILACAK OLAN AÇMA FONKSİYONU
    public void MasayiAc(GameObject oyuncununElindekiEsyaPrefab)
    {
        if (masaAcikMi) return;

        // Kameraları değiştir: Ana oyunu kapat, masayı göster
        anaOyunKamerasi.SetActive(false);
        masaKamerasi.SetActive(true);
        masaAcikMi = true;

        // Karakterin elindeki eşyanın bir kopyasını masada oluştur
        masadakiMevcutEsya = Instantiate(oyuncununElindekiEsyaPrefab, esyaCikmaNoktasi.position, Quaternion.identity);

        // Üzerine parmak izlerini bas
        parmakIziUretici.IzleriOlustur(masadakiMevcutEsya.transform);
    }

    void Update()
    {
        // 2. OTOMATİK KAPANMA KONTROLÜ
        // Eğer masa açıksa ve eşya masadaysa sürekli parmak izlerini say
        if (masaAcikMi && masadakiMevcutEsya != null)
        {
            // Spawner kodumuz parmak izlerini eşyanın "çocuğu" (child) yapmıştı.
            // Eğer eşyanın hiç child objesi kalmadıysa, hepsi silinmiş demektir!
            if (masadakiMevcutEsya.transform.childCount == 0)
            {
                MasayiKapat();
            }
        }
    }

    // 3. İŞ BİTİNCE MASAYI KAPATAN FONKSİYON
    private void MasayiKapat()
    {
        masaAcikMi = false;

        // Masadaki temizlenmiş eşyayı yok et
        if (masadakiMevcutEsya != null)
        {
            Destroy(masadakiMevcutEsya);
        }

        // Kameraları eski haline çevir: Masayı kapat, ana oyuna dön
        masaKamerasi.SetActive(false);
        anaOyunKamerasi.SetActive(true);
        
        // (İsteğe bağlı) Buraya ileride: Karakterin elindeki eşyayı "temizlendi" olarak işaretleyen bir kod eklenebilir.
    }
}