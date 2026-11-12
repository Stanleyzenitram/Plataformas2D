using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 1f;
    public int damage = 10;
    public int health = 5;

    public float knockbackForce = 8f;
    public float knockbackDuration = 0.2f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private bool movingRight = true;
    private bool isKnockedBack = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        if (isKnockedBack) return;

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
        health -= damage;

        StartCoroutine(DamageFlash());

        rb.linearVelocity = Vector2.zero;

        rb.AddForce(
            hitDirection * knockbackForce,
            ForceMode2D.Impulse
        );

        StartCoroutine(KnockbackRoutine());

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("TurnPoint"))
        {
            Flip();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject
                .GetComponent<Player>()
                .TakeDamage(damage);

            float dir = Mathf.Sign(
                collision.transform.position.x -
                transform.position.x
            );

            collision.gameObject
                .GetComponent<Player>()
                .ApplyKnockback(dir);

            Flip();
        }
    }
}   