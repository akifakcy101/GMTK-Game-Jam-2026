using UnityEngine;
using UnityEngine.UI;

public class BarcodeFrameAnimator : MonoBehaviour
{
    [Header("Frame Ayarları")]
    [Tooltip("Animasyon olarak dönecek sprite görselleri (Örn: 3 adet kare)")]
    public Sprite[] frames;

    [Tooltip("Saniyede kaç kare aksın? (FPS - Örn: 8, 10, 12)")]
    public float framesPerSecond = 8f;

    [Tooltip("Animasyon sürekli dönsün mü?")]
    public bool isLooping = true;

    private SpriteRenderer _spriteRenderer;
    private Image _uiImage;

    private int _currentFrameIndex = 0;
    private float _timer = 0f;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _uiImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        _currentFrameIndex = 0;
        _timer = 0f;
        ApplyCurrentFrame();
    }

    private void Update()
    {
        if (frames == null || frames.Length == 0) return;
        if (framesPerSecond <= 0f) return;

        _timer += Time.deltaTime;
        float frameTime = 1f / framesPerSecond;

        if (_timer >= frameTime)
        {
            _timer -= frameTime;

            if (isLooping)
            {
                _currentFrameIndex = (_currentFrameIndex + 1) % frames.Length;
            }
            else
            {
                _currentFrameIndex = Mathf.Min(_currentFrameIndex + 1, frames.Length - 1);
            }

            ApplyCurrentFrame();
        }
    }

    private void ApplyCurrentFrame()
    {
        if (frames == null || frames.Length == 0) return;
        if (_currentFrameIndex < 0 || _currentFrameIndex >= frames.Length) return;

        Sprite activeSprite = frames[_currentFrameIndex];
        if (activeSprite == null) return;

        if (_spriteRenderer != null)
        {
            _spriteRenderer.sprite = activeSprite;
        }

        if (_uiImage != null)
        {
            _uiImage.sprite = activeSprite;
        }
    }
}
