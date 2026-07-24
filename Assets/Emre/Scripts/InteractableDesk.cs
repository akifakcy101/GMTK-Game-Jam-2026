using UnityEngine;
using UnityEngine.InputSystem;

public class InteractableDesk : MonoBehaviour
{
    [Header("Masa & Mekanik Bağlantısı")]
    [Tooltip("Bu masaya geçildiğinde açılacak Mekanik/UI Objesi")]
    public GameObject deskMechanicUI;

    [Header("Arayüz İpuçları")]
    [Tooltip("Masaya yaklaşınca çıkan 'E - Masaya Geç' yazısı veya görseli")]
    public GameObject interactPrompt;

    private bool isPlayerInRange = false;
    private bool isInteracting = false;
    private Movement playerMovement;

    private void Start()
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        if (deskMechanicUI != null)
            deskMechanicUI.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // Karakter masanın yanındaysa ve E tuşuna basarsa
        if (isPlayerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!isInteracting)
            {
                OpenDesk();
            }
            else
            {
                CloseDesk();
            }
        }
    }

    public void OpenDesk()
    {
        isInteracting = true;

        // İpucu yazısını gizle
        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        // Mekanik Arayüzünü Aç
        if (deskMechanicUI != null)
        {
            deskMechanicUI.SetActive(true);
            Debug.Log("<color=green>[InteractableDesk]</color> Masa açıldı: " + deskMechanicUI.name);
        }
        else
        {
            Debug.LogWarning("<color=red>[InteractableDesk]</color> DeskMechanicUI atanmamış!");
        }

        // Karakterin hareketini durdur
        if (playerMovement != null)
            playerMovement.enabled = false;
    }

    public void CloseDesk()
    {
        isInteracting = false;

        // Mekanik Arayüzünü Kapat
        if (deskMechanicUI != null)
        {
            deskMechanicUI.SetActive(false);
            Debug.Log("<color=yellow>[InteractableDesk]</color> Masa kapatıldı: " + deskMechanicUI.name);
        }

        // Karakter hâlâ masanın yanındaysa ipucunu tekrar göster
        if (isPlayerInRange && interactPrompt != null)
            interactPrompt.SetActive(true);

        // Karakterin hareketini tekrar aç
        if (playerMovement != null)
            playerMovement.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<Movement>() != null)
        {
            isPlayerInRange = true;
            playerMovement = other.GetComponent<Movement>();

            if (!isInteracting && interactPrompt != null)
            {
                interactPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<Movement>() != null)
        {
            isPlayerInRange = false;

            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }

            if (isInteracting)
            {
                CloseDesk();
            }

            playerMovement = null;
        }
    }
}
