using UnityEngine;
using UnityEngine.InputSystem;

public class UVLightController : MonoBehaviour
{
    private bool tutuluyorMu = false;

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
            }
        }

        // 2. Farenin SOL TUŞU BIRAKILDIĞINDA
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            tutuluyorMu = false; 
        }

        // 3. IŞIK ELİMİZDEYKEN FAREYİ TAKİP ETSİN
        if (tutuluyorMu)
        {
            transform.position = mousePosDunya;
        }
    }
}