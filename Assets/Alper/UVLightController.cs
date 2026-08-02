using UnityEngine;
using UnityEngine.InputSystem;

public class UVLightController : MonoBehaviour
{
    [Header("Collider & Obje Bağlantıları")]
    [Tooltip("UV Lambayı tutmak için tıklanacak Sap Collider'ı (Boş bırakılırsa tüm lamba tıklanabilir olur)")]
    public Collider2D tutmaCollider;

    [Tooltip("Lamba sapından tutulduğunda açılacak olan UV Işık Objesi (UV Light)")]
    public GameObject uvLightObject;

    private bool tutuluyorMu = false;
    private Vector3 offset;
    private float originalZ;

    void Start()
    {
        originalZ = transform.position.z;

        // UV Işık objesi Inspector'da atanmadıysa alt objeler arasında otomatik ara
        if (uvLightObject == null)
        {
            Transform lightChild = transform.Find("UVLight");
            if (lightChild == null) lightChild = transform.Find("UV Light");
            if (lightChild == null) lightChild = transform.Find("Light");
            if (lightChild == null && transform.childCount > 0) lightChild = transform.GetChild(0);

            if (lightChild != null)
            {
                uvLightObject = lightChild.gameObject;
            }
        }

        // Başlangıçta lamba tutulmadığı için ışığı kapat
        SetLightState(false);
    }

    void Update()
    {
        if (Camera.main == null || Mouse.current == null) return;

        Vector2 mousePosEkran = Mouse.current.position.ReadValue();
        Vector3 mousePosDunya = Camera.main.ScreenToWorldPoint(new Vector3(mousePosEkran.x, mousePosEkran.y, Camera.main.nearClipPlane));
        mousePosDunya.z = 0f;

        // 1. Farenin SOL TUŞUNA BASILDIĞI AN
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Collider2D temas = Physics2D.OverlapPoint(mousePosDunya);
            
            if (temas != null)
            {
                bool sapTiklandi = (tutmaCollider != null && temas == tutmaCollider) || 
                                   (tutmaCollider == null && (temas.gameObject == this.gameObject || temas.transform.IsChildOf(this.transform)));

                if (sapTiklandi)
                {
                    tutuluyorMu = true;
                    // Tutulduğu noktanın obje merkezine olan farkını (offset) hesapla
                    offset = transform.position - mousePosDunya;
                    offset.z = 0f;

                    // Sap tutulduğunda UV ışığını göster/aç
                    SetLightState(true);
                }
            }
        }

        // 2. Farenin SOL TUŞU BIRAKILDIĞINDA
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (tutuluyorMu)
            {
                tutuluyorMu = false;
                // Sap bırakıldığında UV ışığını gizle/kapat
                SetLightState(false);
            }
        }

        // 3. IŞIK TUTULDUĞU NOKTADAN SÜRÜKLENSİN
        if (tutuluyorMu)
        {
            Vector3 targetPos = mousePosDunya + offset;
            targetPos.z = originalZ;
            transform.position = targetPos;
        }
    }

    private void SetLightState(bool active)
    {
        if (uvLightObject != null)
        {
            uvLightObject.SetActive(active);
        }
    }
}