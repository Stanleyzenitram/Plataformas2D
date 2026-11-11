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

        // Busca el sprite aunque esté en un hijo
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("No se encontró SpriteRenderer en el enemigo");
        }
    }

    private void FixedUpdate()
    {
        if (isKnockedBack)
            return;

        if (movingRight)
        {
            rb.linearVelocity = new Vector2(
                speed,
                rb.linearVelocity.y
            );
        }
        else
        {
            rb.linearVelocity = new Vector2(
                -speed,
                rb.linearVelocity.y
            );
        }
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
        if (spriteRenderer == null)
            yield break;

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

            Rigidbody2D playerRb =
                collision.gameObject.GetComponent<Rigidbody2D>();

            float dir = Mathf.Sign(
                collision.transform.position.x -
                transform.position.x
            );

            playerRb.linearVelocity = Vector2.zero;

            playerRb.AddForce(
                new Vector2(dir * 5f, 2f),
                ForceMode2D.Impulse
            );

            Flip();
        }
    }
}