using UnityEngine;
using UnityEngine.InputSystem;
using TMPro; // TextMeshPro (UI) kütüphanesi
using System.Collections;

public class WorkBenchTest : MonoBehaviour
{
    [Header("Kamera Ayarları")]
    public Camera anaKamera;
    public Transform tezgahKameraKonumu;

    [Header("Arayüz (UI) Ayarları")]
    [Tooltip("Butonların ve yazıların içinde olduğu ana Canvas objesi")]
    public GameObject secimArayuzu;
    [Tooltip("Alttaki 'Seçilen : Yok' yazısı")]
    public TextMeshProUGUI secilenText;
    [Tooltip("Uyarı vermek için kullanacağımız yazı (E Bas yazısını veya yeni bir text kullanabilirsin)")]
    public TextMeshProUGUI uyariText;

    [Header("Eşya ve Tezgah Ayarları")]
    public Transform esyaDogmaNoktasi;
    public GameObject[] esyaPrefablari;
    [Tooltip("UI'da gözükecek isimleri sırasıyla yazın (Örn: Gun, Harrow, Hammer)")]
    public string[] esyaIsimleri;

    public FausetSinkEraser silgiSistemi;

    private int _seciliEsyaIndex = -1; // -1: Hiçbir şey seçilmedi demek
    private bool _tezgahtaMi = false;
    private Vector3 _baslangicKameraPozisyonu;
    private GameObject _masadakiEsya;

    private void Start()
    {
        if (anaKamera != null)
        {
            // Geri dönebilmek için ilk pozisyonu hafızaya al
            _baslangicKameraPozisyonu = anaKamera.transform.position;
        }

        // Başlangıç arayüz ayarları
        if (secilenText != null) secilenText.text = "Seçilen : Yok";
        if (uyariText != null) uyariText.gameObject.SetActive(false); // Uyarıyı başta gizle
    }

    // YENİ: UI Butonlarına tıkladığımızda çalışacak fonksiyon
    public void EsyaSec(int index)
    {
        _seciliEsyaIndex = index;

        if (secilenText != null && index < esyaIsimleri.Length)
        {
            secilenText.text = "Seçilen : " + esyaIsimleri[index];
        }
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // E tuşuna basıldığında
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!_tezgahtaMi)
            {
                // Tezgahta değiliz ve E'ye bastık. Eşya seçilmiş mi kontrol et:
                if (_seciliEsyaIndex == -1)
                {
                    // Eşya seçilmemiş! Uyarı ver.
                    if (uyariText != null) StartCoroutine(UyariGosterRoutine());
                    else Debug.LogWarning("Önce bir eşya seçmelisin!");
                }
                else
                {
                    // Eşya seçilmiş, tezgaha geç!
                    TezgahaGec();
                }
            }
            else
            {
                // Zaten tezgahtaydık, demek ki geri çıkmak istiyoruz.
                TezgahtanCik();
            }
        }
    }

    // Uyarı yazısını 2 saniye gösterip gizleyen sistem
    private IEnumerator UyariGosterRoutine()
    {
        uyariText.text = "Lütfen önce bir eşya seçin!";
        uyariText.color = Color.red;
        uyariText.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        uyariText.gameObject.SetActive(false);
    }

    private void TezgahaGec()
    {
        _tezgahtaMi = true;

        // Arayüzü gizle
        if (secimArayuzu != null) secimArayuzu.SetActive(false);

        // Kamerayı ANINDA tezgaha ışınla (Akıcı geçiş iptal edildi)
        Vector3 hedefPozisyon = tezgahKameraKonumu.position;
        hedefPozisyon.z = anaKamera.transform.position.z; // Z derinliğini bozmamak için
        anaKamera.transform.position = hedefPozisyon;

        // Eşyayı masaya koy
        if (_masadakiEsya != null) Destroy(_masadakiEsya);
        _masadakiEsya = Instantiate(esyaPrefablari[_seciliEsyaIndex], esyaDogmaNoktasi.position, Quaternion.identity);

        Transform barkodObjesi = TagIleObjeBul(_masadakiEsya.transform, "Barkod");
        if (barkodObjesi != null)
        {
            SpriteRenderer barkodRenderer = barkodObjesi.GetComponent<SpriteRenderer>();
            if (barkodRenderer != null) silgiSistemi.YeniBarkodKur(barkodRenderer);
        }
    }

    private void TezgahtanCik()
    {
        _tezgahtaMi = false;

        // Arayüzü tekrar göster
        if (secimArayuzu != null) secimArayuzu.SetActive(true);

        // Kamerayı ANINDA ilk yerine ışınla
        anaKamera.transform.position = _baslangicKameraPozisyonu;

        // Masayı temizle
        if (_masadakiEsya != null) Destroy(_masadakiEsya);
        silgiSistemi.erasableSpriteRenderer = null;
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