using UnityEngine;

public class RoofVisibilityController : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Görünürlüğü açılıp kapanacak Roof (Çatı) objesi")]
    public GameObject roof;

    [Header("Ayarlar")]
    [Tooltip("Karakterinizin sahip olduğu Tag (Etiket)")]
    public string playerTag = "Player";

    private SpriteRenderer roofRenderer;

    private void Start()
    {
        // Eğer roof objesi atandıysa, üzerindeki SpriteRenderer bileşenini alıyoruz.
        if (roof != null)
        {
            roofRenderer = roof.GetComponent<SpriteRenderer>();
        }
        else
        {
            Debug.LogWarning("Roof objesi atanmadı! Lütfen Inspector üzerinden atamasını yapın.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Collider'ın içine giren obje Player tag'ine sahipse
        if (collision.CompareTag(playerTag) && roofRenderer != null)
        {
            // Çatının görünürlüğünü kapat (Sadece SpriteRenderer kapanır, obje aktif kalır)
            roofRenderer.enabled = false;

            // Eğer objeyi tamamen kapatmak isterseniz üstteki satır yerine bunu kullanın:
            // roof.SetActive(false);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Collider'dan çıkan obje Player tag'ine sahipse
        if (collision.CompareTag(playerTag) && roofRenderer != null)
        {
            // Çatıyı tekrar görünür yap
            roofRenderer.enabled = true;

            // Eğer objeyi tamamen kapatmayı seçtiyseniz bunu kullanın:
            // roof.SetActive(true);
        }
    }
}