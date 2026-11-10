using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{

    public float speed;
    public float jumpForce;
    public float health = 100f;
    private Rigidbody2D rb2D;

    private SpriteRenderer spriteRenderer;

    private Vector2 moveDirection;
    public InputActionReference move;
    public InputActionReference attack;
    public InputActionReference jump;

    private bool isGrounded;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float checkRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    

    public Animator animator;
    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }


    void Update()
    {
        moveDirection = move.action.ReadValue<Vector2>();
        
        isGrounded = Physics2D.OverlapCircle(
        groundCheck.position,
        checkRadius,
        groundLayer
    );

    if (moveDirection.x > 0)
    {
        spriteRenderer.flipX = false;
    }
    else if (moveDirection.x < 0)
    {
        spriteRenderer.flipX = true;
    }
    animator.SetFloat("Speed", Mathf.Abs(moveDirection.x)); 
    animator.SetBool("IsGrounded", isGrounded);
    animator.SetFloat("VerticalVelocity", rb2D.linearVelocity.y);

    }



    private void FixedUpdate()
    {
        rb2D.linearVelocity = new Vector2(moveDirection.x * speed, rb2D.linearVelocity.y );
    }


    void OnEnable()
    {
        attack.action.started+= Attack;
        jump.action.started += Jump;
    }

    void OnDisable()
    {
        attack.action.started-= Attack;
        jump.action.started -= Jump;
    }

    private void Attack(InputAction.CallbackContext obj)
    {
        Debug.Log("Attack");
    }

private void Jump(InputAction.CallbackContext obj)
{
    if (isGrounded)
    {
        rb2D.linearVelocity = new Vector2(
            rb2D.linearVelocity.x,
            jumpForce
        );
    }
}


}
