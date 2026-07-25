using UnityEngine;
using UnityEngine.InputSystem;

public class FircaController : MonoBehaviour
{
    [Header("Sap ve Baş Collider Bağlantıları")]
    [Tooltip("Fırçayı elde tutmak/sürüklemek için tıklanacak Sap Collider'ı.")]
    public Collider2D tutmaCollider;

    [Tooltip("Parmak izlerini silecek Fırça Başı (uç) Trigger Collider'ı.")]
    public Collider2D temizlemeCollider;

    private bool tutuluyorMu = false;
    private Vector3 offset;
    private float originalZ;

    void Start()
    {
        originalZ = transform.position.z;
    }

    void Update()
    {
        if (Camera.main == null || Mouse.current == null) return;

        Vector2 mousePosEkran = Mouse.current.position.ReadValue();
        Vector3 mousePosDunya = Camera.main.ScreenToWorldPoint(new Vector3(mousePosEkran.x, mousePosEkran.y, Camera.main.nearClipPlane));
        mousePosDunya.z = 0f;

        // Farenin SOL TUŞUNA BASILDIĞI AN
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Collider2D temas = Physics2D.OverlapPoint(mousePosDunya);
            
            if (temas != null)
            {
                // Eğer tutmaCollider atandıysa ona tıklanıp tıklanmadığını kontrol et
                bool sapTiklandi = (tutmaCollider != null && temas == tutmaCollider) || 
                                   (tutmaCollider == null && (temas.gameObject == this.gameObject || temas.transform.IsChildOf(this.transform)));

                if (sapTiklandi)
                {
                    tutuluyorMu = true;
                    // Tutulduğu noktanın obje merkezine olan farkını (offset) hesapla
                    offset = transform.position - mousePosDunya;
                    offset.z = 0f;
                }
            }
        }

        // Farenin SOL TUŞU BIRAKILDIĞINDA
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            tutuluyorMu = false; 
        }

        // TUTULDUĞU NOKTADAN SÜRÜKLE
        if (tutuluyorMu)
        {
            Vector3 targetPos = mousePosDunya + offset;
            targetPos.z = originalZ;
            transform.position = targetPos;
        }
    }

    void OnTriggerStay2D(Collider2D temasEdenObje)
    {
        if (!tutuluyorMu) return;

        // Eğer temizlemeCollider atandıysa, temasın fırça başı ile yapıldığını doğrula
        if (temizlemeCollider != null && !temizlemeCollider.IsTouching(temasEdenObje))
        {
            return;
        }

        if (temasEdenObje.CompareTag("Iz"))
        {
            FingerprintInstance fpComp = temasEdenObje.GetComponent<FingerprintInstance>();
            if (fpComp != null)
            {
                fpComp.Clean();
            }
            else
            {
                Destroy(temasEdenObje.gameObject);
            }
        }
    }
}