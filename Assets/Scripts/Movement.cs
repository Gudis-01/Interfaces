using UnityEngine;
using UnityEngine.InputSystem;
 
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [Tooltip("Velocidad horizontal del personaje")]
    public float moveSpeed = 6f;
 
    [Tooltip("Suavizado del movimiento (más alto = para/arranca más rápido)")]
    public float acceleration = 12f;
 
    [Header("Salto")]
    [Tooltip("Fuerza del salto")]
    public float jumpForce = 10f;
 
    [Tooltip("Multiplicador de gravedad al caer (salto se siente más natural)")]
    public float fallMultiplier = 2.5f;
 
    [Tooltip("Multiplicador de gravedad si sueltas el botón antes de tiempo")]
    public float lowJumpMultiplier = 2f;
 
    [Header("Detección de suelo")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;
 
    private Rigidbody2D rb;
    private float inputX;
    private bool isGrounded;
    private bool jumpPressed;
    private bool jumpHeld;
    private Vector3 originalScale;
 
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
    }
 
    void Update()
    {
        // Leer input (A = izquierda, D = derecha) usando el nuevo Input System
        var keyboard = Keyboard.current;
        inputX = 0f;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed) inputX = -1f;
            if (keyboard.dKey.isPressed) inputX = 1f;
        }
 
        // Detectar suelo
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
 
        // Salto (se guarda el press para no perderlo entre FixedUpdate)
        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame && isGrounded)
            jumpPressed = true;
 
        jumpHeld = keyboard != null && keyboard.spaceKey.isPressed;
 
        // Voltear el sprite según dirección, conservando su tamaño original
        if (inputX != 0)
            transform.localScale = new Vector3(Mathf.Sign(inputX) * Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
    }
 
    void FixedUpdate()
    {
        // Movimiento horizontal suavizado
        float targetSpeed = inputX * moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float movement = speedDiff * acceleration;
        rb.AddForce(new Vector2(movement, 0f), ForceMode2D.Force);
 
        // Ejecutar salto
        if (jumpPressed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // resetea velocidad vertical
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpPressed = false;
        }
 
        // Gravedad extra para que el salto no se sienta "flotante"
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !jumpHeld)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }
 
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
 