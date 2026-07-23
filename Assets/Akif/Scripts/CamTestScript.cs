using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Hedef")]
    [Tooltip("Kameranın takip edeceği obje (Karakteriniz)")]
    public Transform target;

    [Header("Ayarlar")]
    [Tooltip("Kameranın takip etme yumuşaklığı (0 ile 1 arası)")]
    public float smoothSpeed = 0.125f;

    [Tooltip("Kameranın hedefe olan uzaklığı. 2D oyunlarda Z değeri genelde -10 olmalıdır.")]
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    // Fizik ve hareket işlemleri bittikten sonra kamerayı hareket ettirmek için LateUpdate kullanıyoruz.
    private void LateUpdate()
    {
        if (target != null)
        {
            // Kameranın gitmesi gereken nihai pozisyon
            Vector3 desiredPosition = target.position + offset;

            // Mevcut pozisyondan, gitmesi gereken pozisyona yumuşak bir geçiş (Lerp)
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            // Kameranın pozisyonunu uygula
            transform.position = smoothedPosition;
        }
    }
}