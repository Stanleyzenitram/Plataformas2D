using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 1f;
    public int damage = 10;
    public int health = 5;

    public float knockbackForce = 8f;
    public float knockbackDuration = 0.2f;
    public float deathDuration = 0.6f; // ajústalo a la duración de tu clip

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private bool movingRight = true;
    private bool isKnockedBack = false;
    private bool isDead = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
    }

    private void FixedUpdate()
    {
        if (isDead || isKnockedBack) return;

        rb.linearVelocity = new Vector2(
            movingRight ? speed : -speed,
            rb.linearVelocity.y
        );
    }

    private void Flip()
    {
        movingRight = !movingRight;

        transform.localScale = new Vector3(
            -transform.localScale.x,
            transform.localScale.y,
            transform.localScale.z
        );
    }

    private IEnumerator KnockbackRoutine()
    {
        isKnockedBack = true;
        yield return new WaitForSeconds(knockbackDuration);
        isKnockedBack = false;
    }

    private IEnumerator DamageFlash()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.color = originalColor;
    }

    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        if (isDead) return;

        health -= damage;

        if (health <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(DamageFlash());

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(hitDirection * knockbackForce, ForceMode2D.Impulse);

        StartCoroutine(KnockbackRoutine());
    }

    // Muerte instantánea (pinchos, zona de muerte, etc.)
    public void Kill()
    {
        if (isDead) return;

        health = 0;
        Die();
    }

    private void Die()
    {
        isDead = true;

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        foreach (Collider2D col in GetComponents<Collider2D>())
            col.enabled = false;

        if (animator == null)
        {
            Debug.LogWarning("Enemy: no se encontró ningún Animator", this);
        }
        else
        {
            animator.Play("EnemyDeath", 0, 0f); // fuerza el estado, sin depender de la transición
        }

        Destroy(gameObject, deathDuration);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("TurnPoint"))
        {
            Flip();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            player.TakeDamage(damage);

            float dir = Mathf.Sign(
                collision.transform.position.x -
                transform.position.x
            );

            player.ApplyKnockback(dir);

            Flip();
        }
    }
}