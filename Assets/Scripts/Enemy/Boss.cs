using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Boss : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 20;
    public float speed = 1.5f;
    public int contactDamage = 10;
    public int attackDamage = 20;
    public float contactCooldown = 1f;

    [Header("Salir de la tierra")]
    public float riseDuration = 1.2f;         // duración del clip BossRise

    [Header("Ataque (embestida)")]
    public Transform attackPoint;             // hijo delante del boss
    public float attackRange = 1.2f;
    public float attackTriggerDistance = 2.5f;
    public float attackWindup = 0.6f;         // aviso antes de embestir
    public float dashSpeed = 6f;
    public float dashDuration = 0.35f;
    public float attackRecovery = 0.6f;       // se queda quieto tras embestir
    public float attackCooldown = 2.5f;

    [Header("Fase 2")]
    [Range(0.1f, 0.9f)] public float phase2Threshold = 0.5f;
    public float phase2SpeedMultiplier = 1.5f;
    public float phase2CooldownMultiplier = 0.6f;
    public Color bossTint = Color.white;
    public Color phase2Tint = new Color(1f, 0.6f, 0.6f);

    [Header("Animaciones (nombres de estado)")]
    public string riseState = "BossRise";
    public string walkState = "BossWalk";

    [Header("Muerte, victoria y UI")]
    public float deathDuration = 1.2f;
    public GameObject winZone;                // respaldo opcional, empieza desactivada
    public GameObject healthBarRoot;          // panel de la barra, empieza desactivado
    public Slider healthBar;

    [SerializeField] private string enemyLayerName = "Enemy";
    [SerializeField] private bool startFacingRight = false;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Transform player;

    private int health;
    private float currentSpeed;
    private float currentAttackCooldown;
    private Vector3 baseScale;

    private bool isRising = false;
    private bool isActive = false;
    private bool isAttacking = false;
    private bool isDead = false;
    private bool inPhase2 = false;
    private bool facingRight;

    private float lastAttackTime = -999f;
    private float lastContactTime = -999f;

    private void Awake()
    {
        int layer = LayerMask.NameToLayer(enemyLayerName);

        if (layer == -1)
            Debug.LogWarning("Boss: la capa '" + enemyLayerName + "' no existe.", this);
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

        health = maxHealth;
        currentSpeed = speed;
        currentAttackCooldown = attackCooldown;
        facingRight = startFacingRight;
        baseScale = transform.localScale;

        spriteRenderer.color = bossTint;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = maxHealth;
        }

        if (healthBarRoot != null) healthBarRoot.SetActive(false);
        if (winZone != null) winZone.SetActive(false);

        // Enterrado: invisible, quieto y sin colisiones
        spriteRenderer.enabled = false;
        if (animator != null) animator.speed = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        SetCollidersEnabled(false);
    }

    // La llama la zona de activación cuando el jugador entra en la arena
    public void Activate()
    {
        if (isRising || isActive || isDead) return;

        StartCoroutine(RiseRoutine());
    }

    private IEnumerator RiseRoutine()
    {
        isRising = true;

        FaceTarget();

        spriteRenderer.enabled = true;

        if (animator != null)
        {
            animator.speed = 1f;
            PlayState(riseState);
        }

        yield return new WaitForSeconds(riseDuration);

        if (isDead) yield break;

        rb.bodyType = RigidbodyType2D.Dynamic;
        SetCollidersEnabled(true);

        if (healthBarRoot != null) healthBarRoot.SetActive(true);

        PlayState(walkState);

        isRising = false;
        isActive = true;
    }

    private void FixedUpdate()
    {
        if (!isActive || isDead || isAttacking || player == null) return;

        float dx = player.position.x - transform.position.x;

        FaceTarget();

        // Cerca y con el ataque listo: embiste
        if (Mathf.Abs(dx) <= attackTriggerDistance &&
            Time.time >= lastAttackTime + currentAttackCooldown)
        {
            StartCoroutine(AttackRoutine());
            return;
        }

        // Muy pegado al jugador: se queda quieto
        if (Mathf.Abs(dx) <= 0.3f)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(
            Mathf.Sign(dx) * currentSpeed,
            rb.linearVelocity.y
        );
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // AVISO: parpadeo naranja y encogimiento
        Color baseColor = inPhase2 ? phase2Tint : bossTint;
        spriteRenderer.color = new Color(1f, 0.6f, 0.1f);
        transform.localScale = new Vector3(
            baseScale.x * Mathf.Sign(transform.localScale.x),
            baseScale.y * 0.9f,
            baseScale.z
        );

        yield return new WaitForSeconds(attackWindup);

        if (isDead) yield break;

        spriteRenderer.color = baseColor;
        transform.localScale = new Vector3(
            baseScale.x * Mathf.Sign(transform.localScale.x),
            baseScale.y,
            baseScale.z
        );

        // EMBESTIDA hacia el lado donde está el jugador
        float dir = player.position.x > transform.position.x ? 1f : -1f;
        bool alreadyHit = false;
        float t = 0f;

        while (t < dashDuration)
        {
            if (isDead) yield break;

            rb.linearVelocity = new Vector2(dir * dashSpeed, rb.linearVelocity.y);

            if (!alreadyHit && attackPoint != null)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);

                foreach (Collider2D hit in hits)
                {
                    if (!hit.CompareTag("Player")) continue;

                    Player p = hit.GetComponent<Player>();
                    if (p == null) continue;

                    p.TakeDamage(attackDamage);
                    p.ApplyKnockback(dir);
                    alreadyHit = true;
                    break;
                }
            }

            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // Recuperación: queda quieto y vulnerable
        yield return new WaitForSeconds(attackRecovery);

        if (isDead) yield break;

        lastAttackTime = Time.time;
        isAttacking = false;
    }

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
        // No recibe daño enterrado, saliendo ni muerto
        if (!isActive || isDead) return;

        health -= damage;

        if (healthBar != null)
            healthBar.value = Mathf.Max(health, 0);

        if (health <= 0)
        {
            StopAllCoroutines();
            StartCoroutine(DieRoutine());
            return;
        }

        if (!inPhase2 && health <= maxHealth * phase2Threshold)
            EnterPhase2();

        StartCoroutine(DamageFlash());
    }

    private void EnterPhase2()
    {
        inPhase2 = true;
        currentSpeed = speed * phase2SpeedMultiplier;
        currentAttackCooldown = attackCooldown * phase2CooldownMultiplier;
        spriteRenderer.color = phase2Tint;
    }

    private IEnumerator DamageFlash()
    {
        Color baseColor = inPhase2 ? phase2Tint : bossTint;

        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.12f);

        if (!isDead && !isAttacking)
            spriteRenderer.color = baseColor;
    }

    private IEnumerator DieRoutine()
    {
        isDead = true;
        isAttacking = false;

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        SetCollidersEnabled(false);

        if (healthBarRoot != null) healthBarRoot.SetActive(false);

        if (animator != null) animator.speed = 0f;

        // Se pone rojo y se desvanece
        float t = 0f;
        while (t < deathDuration)
        {
            float k = t / deathDuration;
            spriteRenderer.color = new Color(1f, 0.2f, 0.2f, 1f - k);
            t += Time.deltaTime;
            yield return null;
        }

        // ¡Victoria!
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ShowVictory();
        }
        else if (winZone != null)
        {
            winZone.SetActive(true); // respaldo
        }

        Destroy(gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!isActive || isDead) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        if (Time.time < lastContactTime + contactCooldown) return;
        lastContactTime = Time.time;

        Player p = collision.gameObject.GetComponent<Player>();
        if (p == null) return;

        p.TakeDamage(contactDamage);

        float dir = Mathf.Sign(collision.transform.position.x - transform.position.x);
        p.ApplyKnockback(dir);
    }

    private void PlayState(string stateName)
    {
        if (animator == null || string.IsNullOrEmpty(stateName)) return;
        animator.Play(stateName, 0, 0f);
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
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}