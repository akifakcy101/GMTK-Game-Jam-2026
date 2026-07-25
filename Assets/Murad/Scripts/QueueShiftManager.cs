using UnityEngine;

public class QueueShiftManager : MonoBehaviour
{
    public static QueueShiftManager Instance { get; private set; }

    [Header("Bağlantılar")]
    public CustomerSpawner customerSpawner;

    private CustomerController firstSelected;

    private void Awake()
    {
        Instance = this;
    }

    public void SelectForShift(CustomerController customer)
    {
        if (firstSelected == null)
        {
            firstSelected = customer;
            Debug.Log("<color=cyan>[QueueShiftManager]</color> İlk müşteri seçildi, ikinci Shift'e basın.");
            return;
        }

        if (firstSelected == customer)
        {
            firstSelected = null;
            Debug.Log("<color=cyan>[QueueShiftManager]</color> Seçim iptal edildi.");
            return;
        }

        customerSpawner.SwapQueuePositions(firstSelected, customer);
        firstSelected = null;
    }
}
