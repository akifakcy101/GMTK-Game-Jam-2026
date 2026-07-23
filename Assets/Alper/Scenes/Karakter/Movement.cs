using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [Header("Speed Settings")]
    public float speed = 5f;
    public float sprintSpeed = 8f;
    public float slowSpeed = 2.5f;
    
    [Header("Rotation Settings")]
    public float rotationSpeed = 10f; 
    
    private float currentSpeed;     
    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 mousePosition;
    
    // Ufak bir isimlendirme düzeltmesi: moveAcction -> moveAction
    private InputAction moveAction; 
    private InputAction sprintAction;
    private InputAction slowAction;
    private InputAction mousePointAction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        moveAction = new InputAction("Move");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
            
        sprintAction = new InputAction("Sprint", binding: "<Keyboard>/leftShift");
        slowAction = new InputAction("Slow", binding: "<Keyboard>/leftCtrl");
        mousePointAction = new InputAction("MousePoint", binding: "<Mouse>/position");
    }

    void OnEnable()
    {
        moveAction.Enable();
        sprintAction.Enable();
        slowAction.Enable();
        mousePointAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        sprintAction.Disable();
        slowAction.Disable();
        mousePointAction.Disable();
    }

    void Update()
    {
        movement = moveAction.ReadValue<Vector2>();    
        mousePosition = mousePointAction.ReadValue<Vector2>();

        if(sprintAction.IsPressed())
        {
            currentSpeed = sprintSpeed;
        }
        else if(slowAction.IsPressed())
        {
            currentSpeed = slowSpeed;
        }
        else
        {
            currentSpeed = speed;
        }
    }

    void FixedUpdate()
    {
        // 1. KRİTİK DÜZELTME: Çapraz Hızlanma
        // movement.normalized kullanarak W ve D tuşlarına aynı anda basıldığında
        // karakterin çapraz yönde (matematiksel olarak) daha hızlı gitmesini engelledik.
        rb.MovePosition(rb.position + movement.normalized * currentSpeed * Time.fixedDeltaTime);
        
        HandleRotation();
    }

    private void HandleRotation()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.nearClipPlane));
        Vector3 targetDirection = mouseWorldPos - transform.position;

        if (targetDirection.magnitude > 0.1f)
        {
            // 2. KRİTİK DÜZELTME: 2D Dönüş Matematiği (Atan2)
            // Quaternion.LookRotation 3D için tasarlandığından 2D objelerin kaybolmasına yol açabilir.
            // Bunun yerine 2D dünyası (X ve Y ekseni) için en garanti yol olan Trigonometri (Atan2) kullanıyoruz.
            float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
            
            // Not: Eğer karakterin fareye yan dönüyorsa, çizilen sprite'ın yönüne göre
            // aşağıdaki açı değerine +90f veya -90f ekleyebilirsin. (Örn: angle - 90f)
            Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
            
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }
}