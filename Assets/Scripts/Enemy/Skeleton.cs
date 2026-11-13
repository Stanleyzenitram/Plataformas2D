using System.Collections;
using UnityEngine;

public class Skeleton : MonoBehaviour
{
    public float speed = 1.5f;
    public int damage = 10;
    public int health = 3;

    [Header("Retroceso al ser golpeado")]
    public float knockbackForce = 8f;       // empuje horizontal
    public float knockbackUpForce = 3f;     // pequeño salto al ser golpeado
    public float knockbackDuration = 0.25f; // tiempo que pierde el control

    [Header("Salir de la tierra")]
    public float detectionRange = 5f;       // distancia a la que despierta
    public float riseDuration = 1f;         // duración del clip SkeletonRise
    public string riseStateName = "SkeletonRise";

    [Header("Persecución y ataque")]
    public float stopDistance = 0.3f;       // evita que tiemble cuando está encima del jugador
    public float hitCooldown = 0.5f;        // mínimo entre dos golpes seguidos
    public float retreatDuration = 0.35f;   // cuánto se aleja tras golpear al jugador

    [SerializeField] private string enemyLayerName = "Enemy";
    [SerializeField] private bool startFacingRight = false; // hacia dónde mira el sprite original

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Transform player;

    private bool isRising = false;
    private bool isActive = false;   // ya terminó de salir y persigue
    private bool isKnockedBack = false;
    private bool isDead = false;
    private bool facingRight;
    private float lastHitTime = -999f;

    private void Awake()
    {
        int layer = LayerMask.NameToLayer(enemyLayerName);

        if (layer == -1)
            Debug.LogWarning("Skeleton: la capa '" + enemyLayerName + "' no existe.", this);
        else
            SetLayerRecursively(gameObject, layer);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        facingRight = startFacingRight;

        // Enterrado: invisible, quieto y sin colisiones
        spriteRenderer.enabled = false;
        if (animator != null) animator.speed = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        SetCollidersEnabled(false);
    }

    private void Update()
    {
        if (isRising || isActive || isDead || player == null) return;

        if (Vector2.Distance(transform.position, player.position) <= detectionRange)
            StartCoroutine(RiseRoutine());
    }

    private IEnumerator RiseRoutine()
    {
        isRising = true;

        // Antes de salir, mira hacia el jugador
        FaceTarget();

        spriteRenderer.enabled = true;

        if (animator != null)
        {
            animator.speed = 1f;
            animator.Play(riseStateName, 0, 0f);
        }

        yield return new WaitForSeconds(riseDuration);

        if (isDead) yield break;

        // Ya salió: se vuelve sólido y empieza a perseguir
        rb.bodyType = RigidbodyType2D.Dynamic;
        SetCollidersEnabled(true);

        isRising = false;
        isActive = true;
    }

    private void FixedUpdate()
    {
        // Durante el retroceso no se mueve por su cuenta
        if (!isActive || isDead || isKnockedBack || player == null) return;

        float dx = player.position.x - transform.position.x;

        // Muy cerca del jugador: se queda quieto en X
        if (Mathf.Abs(dx) <= stopDistance)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float dir = Mathf.Sign(dx);

        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);

        FaceTarget();
    }

    // Voltea el sprite hacia el jugador
    private void FaceTarget()
    {
        if (player == null) return;

        bool playerIsRight = player.position.x > transform.position.x;

        if (playerIsRight != facingRight)
        {
            facingRight = playerIsRight;

            transform.localScale = new Vector3(
                -transform.localScale.x,
                transform.localScale.y,
                transform.localScale.z
            );
        }
    }

    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        // Mientras está enterrado o saliendo no se le puede golpear
        if (isDead || !isActive) return;

        health -= damage;

        if (health <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(DamageFlash());

        // Retroceso: se empuja en horizontal, alejándose del jugador
        float pushDir = Mathf.Sign(hitDirection.x);

        if (hitDirection.x == 0f && player != null)
            pushDir = Mathf.Sign(transform.position.x - player.position.x);

        rb.linearVelocity = new Vector2(
            pushDir * knockbackForce,
            knockbackUpForce
        );

        StopCoroutine(nameof(KnockbackRoutine));
        StartCoroutine(KnockbackRoutine());
    }

    // Muerte instantánea (pinchos, etc.)
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
        SetCollidersEnabled(false);

        // Sin animación de muerte: se pone rojo y desaparece
        spriteRenderer.enabled = true;
        spriteRenderer.color = Color.red;
        if (animator != null) animator.speed = 0f;

        Destroy(gameObject, 0.25f);
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

    // Choque nuevo con el jugador: daña, igual que el fantasma
    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryHitPlayer(collision);
    }

    // Respaldo por si se queda pegado al jugador sin separarse
    private void OnCollisionStay2D(Collision2D collision)
    {
        TryHitPlayer(collision);
    }

    private void TryHitPlayer(Collision2D collision)
    {
        if (isDead || !isActive) return;

        if (!collision.gameObject.CompareTag("Player")) return;

        if (Time.time < lastHitTime + hitCooldown) return;

        Player p = collision.gameObject.GetComponent<Player>();
        if (p == null) return;

        lastHitTime = Time.time;

        p.TakeDamage(damage);

        float dir = Mathf.Sign(
            collision.transform.position.x - transform.position.x
        );

        p.ApplyKnockback(dir);

        // Se aleja un momento, para separarse y poder volver a embestir
        StopCoroutine(nameof(RetreatRoutine));
        StartCoroutine(RetreatRoutine(-dir));
    }

    private IEnumerator RetreatRoutine(float retreatDir)
    {
        isKnockedBack = true;

        rb.linearVelocity = new Vector2(retreatDir * speed, rb.linearVelocity.y);

        yield return new WaitForSeconds(retreatDuration);

        isKnockedBack = false;
    }

    private void SetCollidersEnabled(bool enabled)
    {
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
            col.enabled = enabled;
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}