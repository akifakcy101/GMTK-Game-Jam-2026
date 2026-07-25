using UnityEngine;

public class FingerprintInstance : MonoBehaviour
{
    public FingerprintData data;

    public void Clean()
    {
        if (data != null && PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.currentFingerprints.Remove(data);
        }
        Destroy(gameObject);
    }
}
