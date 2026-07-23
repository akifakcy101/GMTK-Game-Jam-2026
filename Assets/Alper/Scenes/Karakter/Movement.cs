using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [Header("Speed Settings")]
    public float speed = 5f;
    public float sprintSpeed = 8f;
    public float slowSpeed = 2.5f;    
    private float currentSpeed;     
    private Rigidbody2D rb;
    private Vector2 movement;
    private InputAction moveAction; 
    private InputAction sprintAction;
    private InputAction slowAction;

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
    }
    void OnEnable()
    {
        moveAction.Enable();
        sprintAction.Enable();
        slowAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        sprintAction.Disable();
        slowAction.Disable();
    }

    void Update()
    {
        movement = moveAction.ReadValue<Vector2>();    
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
        rb.MovePosition(rb.position + movement.normalized * currentSpeed * Time.fixedDeltaTime);
    }
}
