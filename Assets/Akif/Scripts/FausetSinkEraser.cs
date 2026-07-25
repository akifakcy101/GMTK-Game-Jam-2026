using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class FausetSinkEraser : MonoBehaviour
{
    [Header("Fırça Sistemi")]
    public BrushController aktifFirca;

    [Header("Silme Ayarları")]
    [Tooltip("Artık burayı boş bırakabilirsin, TezgahManager otomatik atayacak.")]
    public SpriteRenderer erasableSpriteRenderer;
    public float brushRadius = 15f;
    [Range(0f, 1f)] public float brushHardness = 0.5f;

    [Header("Tamamlanma Ayarları")]
    [Range(0.1f, 1f)] public float completionThreshold = 0.90f;
    public UnityEvent onEraseCompleted;

    [Header("Bilgi Ekranı")]
    public float suAnkiSilinmeYuzdesi = 0f;

    private Texture2D _editableTexture;
    private Color32[] _texturePixels;
    private int _textureWidth;
    private int _textureHeight;
    private Vector2 _pivotPixels;
    private float _pixelsPerUnit;

    private bool _isCompleted = false;
    public bool IsCompleted => _isCompleted;
    private int _totalPixels;
    private float _checkTimer = 0f;
    private float _checkInterval = 0.2f;

    // YENİ: Başka bir scriptin (Tezgah) yeni barkod göndermesini sağlayan fonksiyon
    public void YeniBarkodKur(SpriteRenderer yeniBarkodRenderer)
    {
        // Varsa eski oluşturduğumuz dokuyu hafızadan sil (Memory Leak olmaması için)
        if (_editableTexture != null)
        {
            Destroy(_editableTexture);
        }

        erasableSpriteRenderer = yeniBarkodRenderer;
        _isCompleted = false;
        suAnkiSilinmeYuzdesi = 0f;

        if (erasableSpriteRenderer == null || erasableSpriteRenderer.sprite == null)
            return;

        Sprite originalSprite = erasableSpriteRenderer.sprite;
        _pixelsPerUnit = originalSprite.pixelsPerUnit;
        _pivotPixels = originalSprite.pivot;

        int rectX = Mathf.FloorToInt(originalSprite.rect.x);
        int rectY = Mathf.FloorToInt(originalSprite.rect.y);
        int rectWidth = Mathf.FloorToInt(originalSprite.rect.width);
        int rectHeight = Mathf.FloorToInt(originalSprite.rect.height);

        Color[] pixels = originalSprite.texture.GetPixels(rectX, rectY, rectWidth, rectHeight);

        _editableTexture = new Texture2D(rectWidth, rectHeight, TextureFormat.RGBA32, false);
        _editableTexture.filterMode = FilterMode.Bilinear;
        _editableTexture.SetPixels(pixels);
        _editableTexture.Apply();

        Vector2 normalizedPivot = new Vector2(_pivotPixels.x / rectWidth, _pivotPixels.y / rectHeight);

        erasableSpriteRenderer.sprite = Sprite.Create(_editableTexture, new Rect(0, 0, rectWidth, rectHeight), normalizedPivot, _pixelsPerUnit);

        _textureWidth = rectWidth;
        _textureHeight = rectHeight;
        _texturePixels = _editableTexture.GetPixels32();

        _totalPixels = _texturePixels.Length;
    }

    private void Update()
    {
        // Barkod yoksa veya bittiyse çalışma
        if (_isCompleted || erasableSpriteRenderer == null) return;

        if (aktifFirca != null && aktifFirca.isHolding)
        {
            EraseAtTipPosition();

            _checkTimer += Time.deltaTime;
            if (_checkTimer >= _checkInterval)
            {
                _checkTimer = 0f;
                CheckErasePercentage();
            }
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            CheckErasePercentage();
        }
    }

    private void EraseAtTipPosition()
    {
        Vector2 tipWorldPosition = aktifFirca.fircaUcu.position;

        // YENİ: Raycast yerine RaycastAll kullanıyoruz. (Altındaki tüm objeleri delip geçer)
        RaycastHit2D[] hits = Physics2D.RaycastAll(tipWorldPosition, Vector2.zero);

        // Fırçanın değdiği tüm objeleri sırayla kontrol et
        foreach (RaycastHit2D hit in hits)
        {
            // Eğer değdiğimiz obje bizim silinecek barkodumuzsa işlemi yap
            if (hit.collider != null && hit.collider.gameObject == erasableSpriteRenderer.gameObject)
            {
                Vector2 localPoint = erasableSpriteRenderer.transform.InverseTransformPoint(hit.point);

                int pixelX = Mathf.RoundToInt(localPoint.x * _pixelsPerUnit + _pivotPixels.x);
                int pixelY = Mathf.RoundToInt(localPoint.y * _pixelsPerUnit + _pivotPixels.y);

                ErasePixels(pixelX, pixelY);

                break; // Barkodu bulup sildiğimiz için diğer objelere bakmaya gerek yok, döngüden çık.
            }
        }
    }

    private void ErasePixels(int centerX, int centerY)
    {
        bool textureChanged = false;
        int radius = Mathf.RoundToInt(brushRadius);

        for (int x = -radius; x < radius; x++)
        {
            for (int y = -radius; y < radius; y++)
            {
                int pixelX = centerX + x;
                int pixelY = centerY + y;

                if (pixelX >= 0 && pixelX < _textureWidth && pixelY >= 0 && pixelY < _textureHeight)
                {
                    float distance = Mathf.Sqrt(x * x + y * y);

                    if (distance < brushRadius)
                    {
                        int index = pixelY * _textureWidth + pixelX;

                        float normalizedDistance = distance / brushRadius;
                        float alphaFactor = Mathf.Clamp01((1.0f - normalizedDistance) / (1.0f - brushHardness));
                        float targetAlpha = (1.0f - alphaFactor) * 255.0f;

                        Color32 pixelColor = _texturePixels[index];

                        if (pixelColor.a > targetAlpha)
                        {
                            pixelColor.a = (byte)targetAlpha;
                            _texturePixels[index] = pixelColor;
                            textureChanged = true;
                        }
                    }
                }
            }
        }

        if (textureChanged)
        {
            _editableTexture.SetPixels32(_texturePixels);
            _editableTexture.Apply();
        }
    }

    private void CheckErasePercentage()
    {
        if (_isCompleted || erasableSpriteRenderer == null) return;

        int erasedCount = 0;
        byte alphaThreshold = 20;

        for (int i = 0; i < _totalPixels; i++)
        {
            if (_texturePixels[i].a < alphaThreshold)
            {
                erasedCount++;
            }
        }

        float currentPercentage = (float)erasedCount / _totalPixels;
        suAnkiSilinmeYuzdesi = currentPercentage * 100f;

        if (currentPercentage >= completionThreshold)
        {
            _isCompleted = true;
            Debug.Log("BARKOD BAŞARIYLA SİLİNDİ! Kalan parçalar temizleniyor...");

            ClearRemainingPixels();

            suAnkiSilinmeYuzdesi = 100f;
            onEraseCompleted?.Invoke();
        }
    }

    public void ClearRemainingPixels()
    {
        if (_editableTexture == null || _texturePixels == null) return;

        for (int i = 0; i < _totalPixels; i++)
        {
            Color32 pixelColor = _texturePixels[i];
            pixelColor.a = 0;
            _texturePixels[i] = pixelColor;
        }

        _editableTexture.SetPixels32(_texturePixels);
        _editableTexture.Apply();

        _isCompleted = true;
        suAnkiSilinmeYuzdesi = 100f;
    }
}