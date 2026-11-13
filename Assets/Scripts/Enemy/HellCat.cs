using System.Collections;
using UnityEngine;

public class HellCat : MonoBehaviour
{
    public float speed = 3f;
    public int damage = 10;
    public int health = 5;

    public float knockbackForce = 6f;
    public float knockbackDuration = 0.2f;
    public float deathDuration = 0.6f; // ajústalo a la duración de tu clip

    [SerializeField] private string enemyLayerName = "Enemy";

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private bool isActive = false; // empieza dormido
    private bool isKnockedBack = false;
    private bool isDead = false;

    private void Awake()
    {
        int layer = LayerMask.NameToLayer(enemyLayerName);

        if (layer == -1)
            Debug.LogWarning("HellCat: la capa '" + enemyLayerName + "' no existe.", this);
        else
            SetLayerRecursively(gameObject, layer);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();

        // Mientras duerme, la animación de correr queda congelada
        if (animator != null)
            animator.speed = 0f;
    }

    public void Activate()
    {
        if (isActive || isDead) return;

        isActive = true;

        if (animator != null)
            animator.speed = 1f;
    }

    private void FixedUpdate()
    {
        if (!isActive || isDead || isKnockedBack) return;

        // Siempre corre a la izquierda
        rb.linearVelocity = new Vector2(-speed, rb.linearVelocity.y);
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

        Activate(); // si lo golpean dormido, se despierta

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
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        if (animator == null)
        {
            Debug.LogWarning("HellCat: no se encontró ningún Animator", this);
        }
        else
        {
            animator.speed = 1f;
            animator.Play("CatDie", 0, 0f);
        }

        // Destruye todo el conjunto (padre, gato y zona) al terminar la animación
        GameObject root = transform.parent != null ? transform.parent.gameObject : gameObject;
        Destroy(root, deathDuration);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead || !isActive) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            player.TakeDamage(damage);

            float dir = Mathf.Sign(
                collision.transform.position.x -
                transform.position.x
            );

            player.ApplyKnockback(dir);
        }
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}