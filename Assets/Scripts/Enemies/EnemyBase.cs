using UnityEngine;
using System.Collections;

/// <summary>
/// Classe base para todos os inimigos do jogo.
/// Herdar desta classe para criar novos tipos de inimigos.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class EnemyBase : MonoBehaviour
{
    // --- Stats ---
    [Header("Stats")]
    [SerializeField] protected float maxHealth = 30f;
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float attackRange = 1f;
    [SerializeField] protected float attackCooldown = 1f;

    // --- XP e drops ---
    [Header("Drops")]
    [SerializeField] protected int xpValue = 10;
    [SerializeField] protected int coinValue = 5;
    [SerializeField] [Range(0f, 1f)] protected float gemDropChance = 0.05f;
    [SerializeField] protected GameObject xpGemPrefab;
    [SerializeField] protected GameObject coinPrefab;
    [SerializeField] protected GameObject healthOrbPrefab;

    // --- Estado ---
    protected float currentHealth;
    public bool IsAlive { get; protected set; } = true;

    // --- Referências ---
    protected Rigidbody2D rb;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    protected Transform playerTransform;

    // --- Ataque ---
    protected float attackTimer;

    // --- Knockback ---
    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 3f;
    [SerializeField] private float knockbackDuration = 0.1f;
    private bool isKnockedBack;

    // --- Escalonamento de dificuldade ---
    protected float difficultyMultiplier = 1f;

    // --- Efeitos visuais ---
    [Header("Efeitos")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject deathEffectPrefab;
    [SerializeField] private Color hitColor = Color.red;
    private Color originalColor;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer) originalColor = spriteRenderer.color;
    }

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        playerTransform = PlayerController.Instance?.transform;
    }

    protected virtual void Update()
    {
        if (!IsAlive || GameManager.Instance.IsPaused) return;
        if (isKnockedBack) return;

        UpdateBehavior();

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f && playerTransform != null)
        {
            float dist = Vector2.Distance(transform.position, playerTransform.position);
            if (dist <= attackRange)
            {
                PerformAttack();
                attackTimer = attackCooldown;
            }
        }
    }

    protected virtual void FixedUpdate()
    {
        if (!IsAlive || isKnockedBack || GameManager.Instance.IsPaused) return;
        MoveTowardPlayer();
    }

    // --- Comportamento (override nos filhos) ---

    protected virtual void UpdateBehavior()
    {
        // Comportamento padrão: perseguir jogador
        if (playerTransform != null)
        {
            Vector2 dir = (playerTransform.position - transform.position).normalized;
            if (dir.x > 0) transform.localScale = new Vector3(1, 1, 1);
            else if (dir.x < 0) transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    protected virtual void MoveTowardPlayer()
    {
        if (playerTransform == null) return;
        Vector2 dir = (playerTransform.position - transform.position).normalized;
        rb.linearVelocity = dir * moveSpeed * difficultyMultiplier;
    }

    protected virtual void PerformAttack()
    {
        PlayerController.Instance?.TakeDamage(damage * difficultyMultiplier);
        animator?.SetTrigger("Attack");
        AudioManager.Instance?.Play("enemy_attack");
    }

    // --- Dano recebido ---

    public virtual void TakeDamage(float amount, Vector2 knockbackDir = default)
    {
        if (!IsAlive) return;

        currentHealth -= amount;

        // Efeito visual de hit
        StartCoroutine(FlashHit());
        if (hitEffectPrefab) Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

        // Knockback
        if (knockbackDir != Vector2.zero)
            StartCoroutine(KnockbackCoroutine(knockbackDir));

        // Dano flutuante
        DamageNumberPool.Instance?.ShowDamage(transform.position, amount);

        if (currentHealth <= 0f) Die();
    }

    protected virtual void Die()
    {
        IsAlive = false;
        rb.linearVelocity = Vector2.zero;

        // Efeito de morte
        if (deathEffectPrefab) Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        AudioManager.Instance?.Play("enemy_death");

        // Drops
        DropLoot();

        // XP para o jogador
        XPSystem.Instance?.AddXP(xpValue);
        GameManager.Instance?.RegisterEnemyKill();
        GameManager.Instance?.AddScore(xpValue * 10);

        // Missões
        MissionSystem.Instance?.RegisterKill(GetType().Name);

        animator?.SetTrigger("Die");
        Invoke(nameof(ReturnToPool), 0.5f);
    }

    protected virtual void DropLoot()
    {
        // XP Gem
        if (xpGemPrefab) Instantiate(xpGemPrefab, transform.position, Quaternion.identity);

        // Moeda
        if (coinPrefab && Random.value < 0.3f)
            Instantiate(coinPrefab, transform.position, Quaternion.identity);

        // Orbe de vida (raro)
        if (healthOrbPrefab && Random.value < 0.05f)
            Instantiate(healthOrbPrefab, transform.position, Quaternion.identity);

        // Gema
        if (Random.value < gemDropChance && coinPrefab)
            Instantiate(coinPrefab, transform.position, Quaternion.identity);
    }

    protected void ReturnToPool()
    {
        EnemyPool.Instance?.ReturnEnemy(gameObject);
    }

    // --- Escalonamento ---

    public void ApplyDifficultyScale(float multiplier)
    {
        difficultyMultiplier = multiplier;
        maxHealth *= multiplier;
        currentHealth = maxHealth;
        damage *= multiplier;
    }

    // --- Coroutines ---

    private IEnumerator FlashHit()
    {
        if (spriteRenderer) spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(0.08f);
        if (spriteRenderer) spriteRenderer.color = originalColor;
    }

    private IEnumerator KnockbackCoroutine(Vector2 dir)
    {
        isKnockedBack = true;
        rb.linearVelocity = dir * knockbackForce;
        yield return new WaitForSeconds(knockbackDuration);
        isKnockedBack = false;
    }

    // --- Colisão com jogador ---

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!IsAlive) return;
        if (other.CompareTag("Player") && attackTimer <= 0f)
        {
            PerformAttack();
            attackTimer = attackCooldown;
        }
    }

    // --- Getters públicos ---

    public float HealthPercent => currentHealth / maxHealth;
    public float GetDamage() => damage;
}
