using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Müşteri Prefab'ları")]
    [Tooltip("Doğurulacak müşteri Prefab'ları")]
    public GameObject[] customerPrefabs;

    [Header("Noktalar")]
    [Tooltip("Müşterinin ilk doğacağı nokta")]
    public Transform spawnPoint;
    [Tooltip("Müşterinin gelip duracağı tezgah/ön nokta")]
    public Transform counterPoint;
    [Tooltip("İş bitince müşterinin yürüyüp yok olacağı çıkış noktası")]
    public Transform exitPoint;

    [Header("Doğma Ayarları")]
    [Tooltip("Önceki müşteri gittikten kaç saniye sonra yeni müşteri gelsin?")]
    public float spawnDelay = 2f;
    [Tooltip("Otomatik yeni müşteri doğsun mu?")]
    public bool autoSpawn = true;

    private CustomerController currentCustomer;
    private bool isWaitingForNextSpawn = false;

    private void Start()
    {
        // Oyuna başlarken ilk müşteriyi doğur
        if (autoSpawn)
        {
            SpawnCustomer();
        }
    }

    private void Update()
    {
        // Eğer tezgahta müşteri varsa ve yok edildiyse / gittiyse yenisini doğurmayı planla
        if (autoSpawn && currentCustomer == null && !isWaitingForNextSpawn)
        {
            StartCoroutine(SpawnDelayRoutine());
        }
    }

    private System.Collections.IEnumerator SpawnDelayRoutine()
    {
        isWaitingForNextSpawn = true;
        yield return new WaitForSeconds(spawnDelay);
        SpawnCustomer();
        isWaitingForNextSpawn = false;
    }

    [ContextMenu("Müşteri Doğur")]
    public void SpawnCustomer()
    {
        // Halihazırda tezgahta bekleyen müşteri varsa yeni doğurma
        if (currentCustomer != null)
        {
            Debug.LogWarning("<color=yellow>[CustomerSpawner]</color> Tezgahta zaten bir müşteri var!");
            return;
        }

        if (customerPrefabs == null || customerPrefabs.Length == 0)
        {
            Debug.LogError("<color=red>[CustomerSpawner]</color> Müşteri Prefab'ı atanmamış!");
            return;
        }

        if (spawnPoint == null || counterPoint == null || exitPoint == null)
        {
            Debug.LogError("<color=red>[CustomerSpawner]</color> Spawn, Counter veya Exit noktaları atanmamış!");
            return;
        }

        // Rastgele bir müşteri seç
        int randomIndex = Random.Range(0, customerPrefabs.Length);
        GameObject newCustomerObj = Instantiate(customerPrefabs[randomIndex], spawnPoint.position, Quaternion.identity);

        currentCustomer = newCustomerObj.GetComponent<CustomerController>();
        if (currentCustomer == null)
        {
            currentCustomer = newCustomerObj.AddComponent<CustomerController>();
        }

        // Müşteriye hedeflerini ver
        currentCustomer.Setup(counterPoint.position, exitPoint.position);
        Debug.Log("<color=green>[CustomerSpawner]</color> Yeni müşteri doğruldu!");
    }

    // Müşterinin işi bittiğinde çağrılacak fonksiyon (Müşteriyi gönderir)
    [ContextMenu("Müşteriyi Gönder")]
    public void DismissCurrentCustomer()
    {
        if (currentCustomer != null)
        {
            currentCustomer.CompleteAndLeave();
            currentCustomer = null;
        }
    }
}
