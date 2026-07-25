using UnityEngine;
using UnityEngine.InputSystem;

public class UVLightController : MonoBehaviour
{
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

        // 1. Farenin SOL TUŞUNA BASILDIĞI AN
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Collider2D temas = Physics2D.OverlapPoint(mousePosDunya);
            
            if (temas != null && temas.gameObject == this.gameObject)
            {
                tutuluyorMu = true;
                // Tutulduğu noktanın obje merkezine olan farkını (offset) hesapla
                offset = transform.position - mousePosDunya;
                offset.z = 0f;
            }
        }

        // 2. Farenin SOL TUŞU BIRAKILDIĞINDA
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            tutuluyorMu = false;
        }

        // 3. IŞIK TUTULDUĞU NOKTADAN SÜRÜKLENSİN
        if (tutuluyorMu)
        {
            Vector3 targetPos = mousePosDunya + offset;
            targetPos.z = originalZ;
            transform.position = targetPos;
        }
    }
}