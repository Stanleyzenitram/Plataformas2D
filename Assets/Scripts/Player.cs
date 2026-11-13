using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public float speed;
    public float jumpForce;
    public int health = 100;

    private Rigidbody2D rb2D;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveDirection;

    private bool isDead = false;
    private bool isKnockedBack = false;

    // --- Invulnerabilidad ---
    private bool isInvulnerable = false;
    [SerializeField] private float invulnerabilityTime = 1f;
    [SerializeField] private float blinkInterval = 0.1f;

    public bool IsInvulnerable => isInvulnerable;

    public InputActionReference move;
    public InputActionReference attack;
    public InputActionReference jump;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float checkRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private bool isGrounded;

    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.4f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int attackDamage = 1;

    private Vector3 attackPointStartPos;

    public Animator animator;

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        attackPointStartPos = attackPoint.localPosition;
    }

    void Update()
    {
        if (isDead) return;

        moveDirection = move.action.ReadValue<Vector2>();

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            checkRadius,
            groundLayer
        );

        if (moveDirection.x > 0)
        {
            spriteRenderer.flipX = false;

            attackPoint.localPosition = new Vector3(
                Mathf.Abs(attackPointStartPos.x),
                attackPointStartPos.y,
                attackPointStartPos.z
            );
        }
        else if (moveDirection.x < 0)
        {
            spriteRenderer.flipX = true;

            attackPoint.localPosition = new Vector3(
                -Mathf.Abs(attackPointStartPos.x),
                attackPointStartPos.y,
                attackPointStartPos.z
            );
        }

        animator.SetFloat("Speed", Mathf.Abs(moveDirection.x));
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("VerticalVelocity", rb2D.linearVelocity.y);
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        if (isKnockedBack) return;

        rb2D.linearVelocity = new Vector2(
            moveDirection.x * speed,
            rb2D.linearVelocity.y
        );
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvulnerable) return;

        health -= damage;
        AudioManager.Instance.PlaySound(AudioManager.Instance.hurtClip);

        Debug.Log("Vida actual: " + health);

        if (health <= 0)
        {
            StartCoroutine(Die());
        }
        else
        {
            animator.SetTrigger("Hurt");
            StartCoroutine(DamageFlash());
            StartCoroutine(InvulnerabilityRoutine());
        }
    }

    public void ApplyKnockback(float direction)
    {
        if (isDead || isInvulnerable) return;

        StartCoroutine(KnockbackRoutine());

        rb2D.linearVelocity = Vector2.zero;

        rb2D.AddForce(
            new Vector2(direction * 1.2f, 1f),
            ForceMode2D.Impulse
        );
    }

    private IEnumerator KnockbackRoutine()
    {
        isKnockedBack = true;

        yield return new WaitForSeconds(0.25f);

        isKnockedBack = false;
    }

    private IEnumerator DamageFlash()
    {
        Color originalColor = spriteRenderer.color;

        spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(0.15f);

        spriteRenderer.color = originalColor;
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;

        float elapsed = 0f;
        while (elapsed < invulnerabilityTime)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        spriteRenderer.enabled = true;
        isInvulnerable = false;
    }

    private IEnumerator Die()
    {
        isDead = true;

        rb2D.linearVelocity = Vector2.zero;
        AudioManager.Instance.PlaySound(AudioManager.Instance.deathClip);
        animator.SetTrigger("Death");

        yield return new WaitForSeconds(1f);

        GameUIManager.Instance.ShowGameOver();
    }

    void OnEnable()
    {
        attack.action.started += Attack;
        jump.action.started += Jump;
    }

    void OnDisable()
    {
        attack.action.started -= Attack;
        jump.action.started -= Jump;
    }

    private void Attack(InputAction.CallbackContext obj)
    {
        if (isDead || Time.timeScale == 0f) return;

        AudioManager.Instance.PlaySound(AudioManager.Instance.attackClip);
        animator.SetTrigger("Attack");

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayer
        );

        foreach (Collider2D enemy in hitEnemies)
        {
            Vector2 hitDirection =
                (enemy.transform.position - transform.position).normalized;

            enemy.GetComponentInParent<Enemy>()?.TakeDamage(attackDamage, hitDirection);
            enemy.GetComponentInParent<HellCat>()?.TakeDamage(attackDamage, hitDirection);
            enemy.GetComponentInParent<Skeleton>()?.TakeDamage(attackDamage, hitDirection);
            enemy.GetComponentInParent<Boss>()?.TakeDamage(attackDamage, hitDirection);
        }
    }

    private void Jump(InputAction.CallbackContext obj)
    {
        if (isDead || Time.timeScale == 0f) return;

        if (isGrounded)
        {
            AudioManager.Instance.PlaySound(AudioManager.Instance.jumpClip);

            rb2D.linearVelocity = new Vector2(
                rb2D.linearVelocity.x,
                jumpForce
            );
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            attackPoint.position,
            attackRange
        );
    }
}