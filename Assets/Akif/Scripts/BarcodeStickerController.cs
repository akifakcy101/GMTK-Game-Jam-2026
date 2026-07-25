using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events; // YENİ: Event sistemi için kütüphane eklendi

public class StickerController : MonoBehaviour
{
    [Header("Ayarlar")]
    public Collider2D tutmaCollideri;
    public SpriteRenderer stickerGorseli;
    [Tooltip("Bırakıldığında masadaki eski yerine dönme hızı")]
    public float donusHizi = 15f;

    [Header("Oyun Mantığı (Eventler)")]
    [Tooltip("Sticker başarıyla yapıştırıldığında çalışacak şeyler (Ses çal, menü aç vs.)")]
    public UnityEvent onStickerAttached; // YENİ: Yapıştırma tamamlandı çıktısı

    [Header("Renkler")]
    public Color yapistirilabilirRenk = Color.green;
    public Color yapistirilamazRenk = Color.red;
    public Color normalRenk = Color.white;

    private Vector3 _baslangicPozisyonu;
    private bool _isHolding = false;
    private bool _yerineDonuyor = false;
    private bool _yapistirildi = false;

    private Vector3 _tutmaFarki;
    private Transform _gecerliHedef = null;

    void Start()
    {
        _baslangicPozisyonu = transform.position;
        stickerGorseli.color = normalRenk;
    }

    void Update()
    {
        if (_yapistirildi || Mouse.current == null) return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        // Tıklama Anı
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
            if (hit.collider != null && hit.collider == tutmaCollideri)
            {
                _isHolding = true;
                _yerineDonuyor = false;
                _tutmaFarki = transform.position - mouseWorldPos;

                HedefiKontrolEt();
            }
        }

        // Sürükleme Anı
        if (Mouse.current.leftButton.isPressed && _isHolding)
        {
            transform.position = mouseWorldPos + _tutmaFarki;
            HedefiKontrolEt();
        }

        // Bırakma Anı
        if (Mouse.current.leftButton.wasReleasedThisFrame && _isHolding)
        {
            _isHolding = false;

            if (_gecerliHedef != null)
            {
                Yapistir();
            }
            else
            {
                _yerineDonuyor = true;
                stickerGorseli.color = normalRenk;
            }
        }

        // Yerine Dönme Animasyonu (Lerp)
        if (_yerineDonuyor && !_isHolding)
        {
            transform.position = Vector3.Lerp(transform.position, _baslangicPozisyonu, donusHizi * Time.deltaTime);
            if (Vector3.Distance(transform.position, _baslangicPozisyonu) < 0.01f)
            {
                transform.position = _baslangicPozisyonu;
                _yerineDonuyor = false;
            }
        }
    }

    private void HedefiKontrolEt()
    {
        Collider2D[] hitColliders = Physics2D.OverlapPointAll(transform.position);
        _gecerliHedef = null;

        foreach (var col in hitColliders)
        {
            if (col.CompareTag("YapiskanHedefi"))
            {
                _gecerliHedef = col.transform;
                break;
            }
        }

        if (_gecerliHedef != null)
        {
            stickerGorseli.color = yapistirilabilirRenk;
        }
        else
        {
            stickerGorseli.color = yapistirilamazRenk;
        }
    }

    private void Yapistir()
    {
        _yapistirildi = true;
        stickerGorseli.color = normalRenk;

        transform.SetParent(_gecerliHedef);

        Debug.Log("Sticker başarıyla yapıştırıldı!");

        // YENİ: Başarı çıktısını (sinyalini) oyunun diğer kısımlarına yolla
        onStickerAttached?.Invoke();
    }

    public bool IsAttached => _yapistirildi;

    public void ResetSticker(Transform parent, Vector3 localPos)
    {
        _yapistirildi = false;
        _isHolding = false;
        _yerineDonuyor = false;
        _gecerliHedef = null;

        if (parent != null) transform.SetParent(parent);
        transform.localPosition = localPos;
        _baslangicPozisyonu = transform.position;

        if (stickerGorseli != null) stickerGorseli.color = normalRenk;
        gameObject.SetActive(true);
    }
}