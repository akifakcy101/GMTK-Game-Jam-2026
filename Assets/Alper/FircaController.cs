using UnityEngine;
using UnityEngine.InputSystem;

public class FircaController : MonoBehaviour
{
    private bool tutuluyorMu = false;

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
            
            if (temas != null && temas.gameObject == this.gameObject)
            {
                tutuluyorMu = true;
            }
        }

        // Farenin SOL TUŞU BIRAKILDIĞINDA
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            tutuluyorMu = false; 
        }

        // SADECE TUTULUYORKEN FAREYİ TAKİP ET
        if (tutuluyorMu)
        {
            transform.position = mousePosDunya;
        }
    }

    void OnTriggerStay2D(Collider2D temasEdenObje)
    {
        if (tutuluyorMu && temasEdenObje.CompareTag("Iz"))
        {
            Destroy(temasEdenObje.gameObject);
        }
    }
}