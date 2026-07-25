using UnityEngine;

public class CustomerController : MonoBehaviour
{
    public enum CustomerState
    {
        WalkingToPosition,
        WaitingInQueue,
        WalkingToExit
    }

    [Header("Hareket Ayarları")]
    public float moveSpeed = 3f;

    [Header("Durum")]
    public CustomerState currentState = CustomerState.WalkingToPosition;

    [Header("Müşterinin Eşyası")]
    public ItemData assignedItem;

    private Vector3 currentTargetPosition;
    private Vector3 exitPosition;

    public void Setup(Vector3 initialTargetPos, Vector3 exitPos, ItemData item = null)
    {
        this.currentTargetPosition = initialTargetPos;
        this.exitPosition = exitPos;
        this.assignedItem = item;
        currentState = CustomerState.WalkingToPosition;
    }

    // Sıra ilerlediğinde müşteriye yeni hedefini verir
    public void UpdateTargetPosition(Vector3 newTargetPos)
    {
        this.currentTargetPosition = newTargetPos;
        currentState = CustomerState.WalkingToPosition;
    }

    private void Update()
    {
        switch (currentState)
        {
            case CustomerState.WalkingToPosition:
                MoveTowardsPosition(currentTargetPosition);
                if (Vector3.Distance(transform.position, currentTargetPosition) < 0.05f)
                {
                    currentState = CustomerState.WaitingInQueue;
                }
                break;

            case CustomerState.WaitingInQueue:
                // Sıradaki yerinde bekliyor
                break;

            case CustomerState.WalkingToExit:
                MoveTowardsPosition(exitPosition);
                if (Vector3.Distance(transform.position, exitPosition) < 0.05f)
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

    public ItemData GiveItemToPlayer()
    {
        ItemData itemToGive = assignedItem;
        Debug.Log($"<color=cyan>[Customer]</color> Müşteri eşyasını oyuncuya verdi: {(itemToGive != null ? itemToGive.itemName : "Bilinmeyen Eşya")}");
        return itemToGive;
    }

    // Müşteriyle iş bittiğinde çağrılacak fonksiyon
    public void CompleteAndLeave()
    {
        if (currentState == CustomerState.WalkingToExit) return;
        
        currentState = CustomerState.WalkingToExit;
        Debug.Log("<color=orange>[Customer]</color> Müşteri dükkandan ayrılıyor...");
    }
}
