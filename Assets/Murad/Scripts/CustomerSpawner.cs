using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Müşteri Prefab'ları")]
    [Tooltip("Doğurulacak farklı müşteri görsel/Prefab türleri (Rastgele seçilir)")]
    public GameObject[] customerPrefabs;

    [Header("Noktalar")]
    [Tooltip("Müşterinin ilk doğacağı nokta")]
    public Transform spawnPoint;
    [Tooltip("Sıranın en başı (Tezgah/Ön nokta)")]
    public Transform counterPoint;
    [Tooltip("İş bitince müşterinin yürüyüp yok olacağı çıkış noktası")]
    public Transform exitPoint;

    [Header("Sıra Ayarları")]
    [Tooltip("Her bir müşterinin arkasında duracağı mesafe ve yön (Örn: X=0, Y=-1.5)")]
    public Vector3 queueOffset = new Vector3(0, -1.5f, 0);
    [Tooltip("Sırada maksimum kaç müşteri birikebilir?")]
    public int maxQueueSize = 5;
    [Tooltip("Kaç saniyede bir yeni müşteri doğsun?")]
    public float spawnInterval = 4f;
    [Tooltip("Otomatik yeni müşteri doğması açık mı?")]
    public bool autoSpawn = true;

    // ----------------------------------------------
    // YENİ EKLENEN
    [Header("Gün Ayarları")]
    [Tooltip("Bu gün toplam kaç müşteri gelecek (süre aralıkları buna göre hesaplanır)")]
    public int dayCustomerCount = 5;
    // ----------------------------------------------

    private List<CustomerController> customerQueue = new List<CustomerController>();

    // ----------------------------------------------
    // YENİ EKLENEN
    private Queue<float> pendingDurations = new Queue<float>();
    // ----------------------------------------------

    private float spawnTimer = 0f;

    private void Start()
    {
        spawnTimer = spawnInterval; // Başlar başlamaz ilk müşteriyi doğursun

        // ----------------------------------------------
        // YENİ EKLENEN
        PrepareDurations(dayCustomerCount);
        // ----------------------------------------------
    }

    private void Update()
    {
        if (!autoSpawn) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            if (customerQueue.Count < maxQueueSize)
            {
                SpawnCustomer();
            }
        }
    }

    [ContextMenu("Müşteri Doğur")]
    public void SpawnCustomer()
    {
        if (customerQueue.Count >= maxQueueSize)
        {
            Debug.LogWarning("<color=yellow>[CustomerSpawner]</color> Sıra dolu! Yeni müşteri gelemiyor.");
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

        // Rastgele bir müşteri türü seç
        int randomIndex = Random.Range(0, customerPrefabs.Length);
        GameObject newCustomerObj = Instantiate(customerPrefabs[randomIndex], spawnPoint.position, Quaternion.identity);

        CustomerController customer = newCustomerObj.GetComponent<CustomerController>();
        if (customer == null)
        {
            customer = newCustomerObj.AddComponent<CustomerController>();
        }

        // Sıradaki yerini hesapla
        int queueIndex = customerQueue.Count;
        Vector3 targetPos = GetQueuePosition(queueIndex);

        customer.Setup(targetPos, exitPoint.position);
        customerQueue.Add(customer);

        // ----------------------------------------------
        // YENİ EKLENEN
        // Bu müşteriye önceden hesaplanmış (karışık) süreyi ata
        CustomerCountdown countdown = newCustomerObj.GetComponentInChildren<CustomerCountdown>();
        if (countdown != null && pendingDurations.Count > 0)
        {
            countdown.SetDuration(pendingDurations.Dequeue());
            countdown.onExpired.AddListener(() =>
                Debug.LogWarning("<color=red>[CustomerSpawner]</color> Bir müşterinin süresi bitti! (Fail tetiklenecek)"));
        }
        // ----------------------------------------------

        Debug.Log($"<color=green>[CustomerSpawner]</color> Müşteri doğuruldu. Sıradaki yeri: {queueIndex + 1}");
    }

    // En öndeki müşterinin işi bittiğinde çağrılır (Öndekini gönderir ve arkadakileri 1 adım öne kaydırır)
    [ContextMenu("En Öndeki Müşteriyi Gönder")]
    public void DismissCurrentCustomer()
    {
        if (customerQueue.Count == 0)
        {
            Debug.LogWarning("<color=yellow>[CustomerSpawner]</color> Sıra zaten boş!");
            return;
        }

        // En öndeki müşteriyi al ve gönder
        CustomerController frontCustomer = customerQueue[0];
        customerQueue.RemoveAt(0);

        if (frontCustomer != null)
        {
            frontCustomer.CompleteAndLeave();
        }

        // Geride kalan tüm müşterileri 1 adım öne kaydır
        UpdateQueuePositions();
    }

    private void UpdateQueuePositions()
    {
        for (int i = 0; i < customerQueue.Count; i++)
        {
            if (customerQueue[i] != null)
            {
                Vector3 newPos = GetQueuePosition(i);
                customerQueue[i].UpdateTargetPosition(newPos);
            }
        }
    }

    private Vector3 GetQueuePosition(int index)
    {
        return counterPoint.position + (queueOffset * index);
    }

    // İleride en öndeki müşteriyi almak için (Sipariş kontrolü vb.)
    public CustomerController GetFrontCustomer()
    {
        return customerQueue.Count > 0 ? customerQueue[0] : null;
    }

    // ----------------------------------------------
    // YENİ EKLENEN
    // Süreleri hesapla ve karıştır (customer_count önceden belliyse buna göre)
    private void PrepareDurations(int count)
    {
        var durations = new List<float>();
        for (int i = 1; i <= count; i++)
        {
            durations.Add(Random.Range(10f * i, 20f * i));
        }

        // Fisher-Yates shuffle
        for (int i = durations.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (durations[i], durations[j]) = (durations[j], durations[i]);
        }

        pendingDurations = new Queue<float>(durations);
    }

    // İki müşterinin sıradaki yerini değiştirir (Shift mekaniği)
    public void SwapQueuePositions(CustomerController a, CustomerController b)
    {
        int indexA = customerQueue.IndexOf(a);
        int indexB = customerQueue.IndexOf(b);
        if (indexA < 0 || indexB < 0 || indexA == indexB) return;

        (customerQueue[indexA], customerQueue[indexB]) = (customerQueue[indexB], customerQueue[indexA]);
        UpdateQueuePositions();
    }
    // ----------------------------------------------
}