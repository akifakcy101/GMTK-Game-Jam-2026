using UnityEngine;

public class CustomerController : MonoBehaviour
{
    public enum CustomerState
    {
        WalkingToCounter,
        AtCounter,
        WalkingToExit
    }

    [Header("Hareket Ayarları")]
    public float moveSpeed = 3f;

    [Header("Durum")]
    public CustomerState currentState = CustomerState.WalkingToCounter;

    private Vector3 targetCounterPosition;
    private Vector3 targetExitPosition;

    public void Setup(Vector3 counterPos, Vector3 exitPos)
    {
        this.targetCounterPosition = counterPos;
        this.targetExitPosition = exitPos;
        currentState = CustomerState.WalkingToCounter;
    }

    private void Update()
    {
        switch (currentState)
        {
            case CustomerState.WalkingToCounter:
                MoveTowardsPosition(targetCounterPosition);
                if (Vector3.Distance(transform.position, targetCounterPosition) < 0.05f)
                {
                    currentState = CustomerState.AtCounter;
                    Debug.Log("<color=cyan>[Customer]</color> Müşteri tezgaha ulaştı ve bekliyor.");
                }
                break;

            case CustomerState.AtCounter:
                // Müşteri tezgahta oyuncunun işlemini bekliyor
                break;

            case CustomerState.WalkingToExit:
                MoveTowardsPosition(targetExitPosition);
                if (Vector3.Distance(transform.position, targetExitPosition) < 0.05f)
                {
                    Debug.Log("<color=yellow>[Customer]</color> Müşteri çıkışa ulaştı ve yok edildi.");
                    Destroy(gameObject);
                }
                break;
        }
    }

    private void MoveTowardsPosition(Vector3 destination)
    {
        transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
    }

    // Müşteriyle iş bittiğinde çağrılacak fonksiyon
    public void CompleteAndLeave()
    {
        if (currentState == CustomerState.WalkingToExit) return;
        
        currentState = CustomerState.WalkingToExit;
        Debug.Log("<color=orange>[Customer]</color> Müşteri dükkandan ayrılıyor...");
    }
}
