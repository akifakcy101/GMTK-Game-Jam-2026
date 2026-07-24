using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [Header("Speed Settings")]
    public float speed = 5f;

    [Header("Directional Sprites")]
    [Tooltip("Eğer boş bırakılırsa objedeki SpriteRenderer otomatik bulunur.")]
    public SpriteRenderer spriteRenderer;
    public Sprite upSprite;      // W (Yukarı)
    public Sprite downSprite;    // S (Aşağı)
    public Sprite leftSprite;    // A (Sol)
    public Sprite rightSprite;   // D (Sağ)

    private Rigidbody2D rb;
    private Vector2 movement;
    private InputAction moveAction; 

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

    void Update()
    {
        movement = moveAction.ReadValue<Vector2>();    
        UpdateSpriteDirection();
    }

    private void UpdateSpriteDirection()
    {
        if (spriteRenderer == null) return;

        // Dikey hareket baskınsa (Yukarı/Aşağı)
        if (Mathf.Abs(movement.y) > Mathf.Abs(movement.x))
        {
            if (movement.y > 0 && upSprite != null)
            {
                spriteRenderer.sprite = upSprite;
            }
            else if (movement.y < 0 && downSprite != null)
            {
                spriteRenderer.sprite = downSprite;
            }
        }
        // Yatay hareket baskınsa (Sağ/Sol)
        else if (Mathf.Abs(movement.x) > 0)
        {
            if (movement.x > 0 && rightSprite != null)
            {
                spriteRenderer.sprite = rightSprite;
            }
            else if (movement.x < 0 && leftSprite != null)
            {
                spriteRenderer.sprite = leftSprite;
            }
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement.normalized * speed * Time.fixedDeltaTime);
    }
}

