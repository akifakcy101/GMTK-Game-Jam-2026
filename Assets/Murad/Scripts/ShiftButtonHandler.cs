using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ShiftButtonHandler : MonoBehaviour
{
    private CustomerController ownerCustomer;

    private void Start()
    {
        ownerCustomer = GetComponentInParent<CustomerController>();
        GetComponent<Button>().onClick.AddListener(OnShiftClicked);
    }

    private void OnShiftClicked()
    {
        if (ownerCustomer == null)
        {
            Debug.LogError("<color=red>[ShiftButtonHandler]</color> Üst objede CustomerController bulunamadı!");
            return;
        }

        QueueShiftManager.Instance.SelectForShift(ownerCustomer);
    }
}