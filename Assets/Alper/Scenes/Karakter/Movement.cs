using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [Header("Speed Settings")]
    public float speed = 5f;

    [Header("Directional Animations (4'er frame)")]
    [Tooltip("Eğer boş bırakılırsa objedeki SpriteRenderer otomatik bulunur.")]
    public SpriteRenderer spriteRenderer;

    [Tooltip("Yukarı hareket animasyonu frameleri (W)")]
    public Sprite[] upSprites;
    [Tooltip("Aşağı hareket animasyonu frameleri (S)")]
    public Sprite[] downSprites;
    [Tooltip("Sol hareket animasyonu frameleri (A)")]
    public Sprite[] leftSprites;
    [Tooltip("Sağ hareket animasyonu frameleri (D)")]
    public Sprite[] rightSprites;

    [Header("Animation Settings")]
    [Tooltip("Animasyon hızı (Kare/saniye)")]
    public float frameRate = 8f;
    [Tooltip("Sadece hareket ederken mi animasyon oynatılsın?")]
    public bool animateOnlyWhenMoving = true;
    [Tooltip("Duvara takılma tespiti için minimum hareket hızı eşiği.")]
    public float minMovementThreshold = 0.1f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private InputAction moveAction;

    private int currentFrame;
    private float animationTimer;
    private Vector2 lastDirection = Vector2.down;

    private Vector2 lastPosition;
    private float currentSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        moveAction = new InputAction("Move");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
    }

    void OnEnable()
    {
        moveAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
    }

    void Start()
    {
        lastPosition = rb.position;
    }

    void Update()
    {
        movement = moveAction.ReadValue<Vector2>();    
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (spriteRenderer == null) return;

        bool hasInput = movement.sqrMagnitude > 0.01f;

        // Tuşa basılıyor mu VE karakter fiziksel olarak gerçekten hareket ediyor mu?
        bool isMoving = hasInput && (currentSpeed >= minMovementThreshold);

        // Tuş girdisi varsa bakılan yönü güncelle
        if (hasInput)
        {
            if (Mathf.Abs(movement.y) > Mathf.Abs(movement.x))
            {
                lastDirection = movement.y > 0 ? Vector2.up : Vector2.down;
            }
            else
            {
                lastDirection = movement.x > 0 ? Vector2.right : Vector2.left;
            }
        }

        // Mevcut yöne uygun Sprite dizisini al
        Sprite[] currentArray = GetSpriteArrayForDirection(lastDirection);

        if (currentArray == null || currentArray.Length == 0) return;

        if (isMoving || !animateOnlyWhenMoving)
        {
            animationTimer += Time.deltaTime;
            if (animationTimer >= 1f / frameRate)
            {
                animationTimer = 0f;
                currentFrame = (currentFrame + 1) % currentArray.Length;
            }
        }
        else
        {
            // Dururken veya duvara takılınca ilk karede (Idle) kal
            currentFrame = 0;
            animationTimer = 0f;
        }

        // SpriteRenderer'ı güncelle
        if (currentFrame < currentArray.Length && currentArray[currentFrame] != null)
        {
            spriteRenderer.sprite = currentArray[currentFrame];
        }
    }

    private Sprite[] GetSpriteArrayForDirection(Vector2 dir)
    {
        if (dir == Vector2.up) return upSprites;
        if (dir == Vector2.down) return downSprites;
        if (dir == Vector2.left) return leftSprites;
        if (dir == Vector2.right) return rightSprites;
        return downSprites;
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement.normalized * speed * Time.fixedDeltaTime);

        // Karakterin fiziksel yer değiştirmesinden gerçek hızı hesapla
        currentSpeed = (rb.position - lastPosition).magnitude / Time.fixedDeltaTime;
        lastPosition = rb.position;
    }
}

