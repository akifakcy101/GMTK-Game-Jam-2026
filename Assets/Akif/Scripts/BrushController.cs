using UnityEngine;
using UnityEngine.InputSystem;

public class BrushController : MonoBehaviour
{
    [Header("Fırça Noktaları")]
    public Transform fircaUcu;
    public Transform fircaSapi;

    [Header("Fizik (Tutma Yeri)")]
    public Collider2D tutmaCollideri;

    [Header("Dönüş Ayarları")]
    [Tooltip("Fırça bırakıldığında eski yerine ne kadar hızlı kayarak dönecek?")]
    public float donusHizi = 15f; // <--- YENİ: Kayma hızı ayarı

    [HideInInspector]
    public bool isHolding = false;

    private Vector3 _tutmaFarki;

    // YENİ: Dönüş için gereken değişkenler
    private Vector3 _baslangicPozisyonu;
    private bool _yerineDonuyor = false;

    private void Start()
    {
        // Oyun başladığında fırçanın masadaki ilk durduğu yeri kaydet
        _baslangicPozisyonu = transform.position;
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        mouseWorldPosition.z = 0f;

        // Fare sol tuşuna tıklandığında
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPosition, Vector2.zero);

            if (hit.collider != null && hit.collider == tutmaCollideri)
            {
                isHolding = true;
                _yerineDonuyor = false; // Havada dönerken yakalarsak dönüşü iptal et

                if (fircaSapi != null)
                {
                    _tutmaFarki = transform.position - fircaSapi.position;
                }
                else
                {
                    _tutmaFarki = Vector3.zero;
                }
            }
        }

        // Fırça elimizdeyken fareyi hareket ettirirsek
        if (Mouse.current.leftButton.isPressed && isHolding)
        {
            transform.position = mouseWorldPosition + _tutmaFarki;
        }

        // Fareyi bıraktığımızda
        if (Mouse.current.leftButton.wasReleasedThisFrame && isHolding)
        {
            isHolding = false;
            _yerineDonuyor = true; // YENİ: Dönüş hareketini tetikle
        }

        // YENİ: Fırça elimizde değilse ve yerine dönüyorsa kayarak git
        if (_yerineDonuyor && !isHolding)
        {
            // Vector3.Lerp, mevcut pozisyondan hedef pozisyona pürüzsüzce kaymayı sağlar
            transform.position = Vector3.Lerp(transform.position, _baslangicPozisyonu, donusHizi * Time.deltaTime);

            // Hedefe çok yaklaştığında tam milimetrik olarak oturt ve hareketi bitir
            if (Vector3.Distance(transform.position, _baslangicPozisyonu) < 0.01f)
            {
                transform.position = _baslangicPozisyonu;
                _yerineDonuyor = false;
            }
        }
    }
}