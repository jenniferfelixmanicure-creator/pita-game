using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Controlador principal do jogador.
/// Gerencia movimento, animações, stats e integração com sistemas de habilidades.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    // --- Stats base ---
    [Header("Stats Base")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float defense = 0f;          // Redução de dano em %
    [SerializeField] private float attackSpeed = 1f;      // Multiplicador
    [SerializeField] private float damageMultiplier = 1f;
    [SerializeField] private float pickupRadius = 2f;

    // --- Estado atual ---
    public float CurrentHealth { get; private set; }
    public float MaxHealth => maxHealth + bonusMaxHealth;
    public bool IsAlive { get; private set; } = true;
    public bool IsInvincible { get; private set; }

    // --- Bônus acumulados por upgrades ---
    [HideInInspector] public float bonusMaxHealth = 0f;
    [HideInInspector] public float bonusMoveSpeed = 0f;
    [HideInInspector] public float bonusDefense = 0f;
    [HideInInspector] public float bonusAttackSpeed = 0f;
    [HideInInspector] public float bonusDamage = 0f;
    [HideInInspector] public float bonusPickupRadius = 0f;

    // --- Referências ---
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private VirtualJoystick joystick;

    // --- Movimento ---
    private Vector2 moveDirection;
    private bool isFacingRight = true;

    // --- Invencibilidade após dano ---
    [Header("Invencibilidade")]
    [SerializeField] private float invincibilityDuration = 0.5f;
    [SerializeField] private float flashInterval = 0.1f;

    // --- Eventos ---
    public System.Action<float, float> OnHealthChanged;
    public System.Action OnPlayerDied;
    public System.Action<float> OnDamageTaken;

    // --- Habilidades ativas ---
    private List<AbilityBase> activeAbilities = new List<AbilityBase>();

    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        CurrentHealth = MaxHealth;
        joystick = FindObjectOfType<VirtualJoystick>();
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    private void Update()
    {
        if (!IsAlive || GameManager.Instance.IsPaused) return;

        HandleMovementInput();
        UpdateAnimations();
        HandlePickupRadius();
    }

    private void FixedUpdate()
    {
        if (!IsAlive || GameManager.Instance.IsPaused) return;
        MovePlayer();
    }

    // --- Movimento ---

    private void HandleMovementInput()
    {
        if (joystick != null)
        {
            moveDirection = joystick.Direction;
        }
        else
        {
            // Fallback para teclado (PC/Editor)
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            moveDirection = new Vector2(h, v).normalized;
        }
    }

    private void MovePlayer()
    {
        float totalSpeed = (moveSpeed + bonusMoveSpeed);
        rb.linearVelocity = moveDirection * totalSpeed;
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        bool isMoving = moveDirection.magnitude > 0.1f;
        animator.SetBool("IsMoving", isMoving);
        animator.SetFloat("Speed", moveDirection.magnitude);

        // Flip sprite
        if (moveDirection.x > 0.1f && !isFacingRight) Flip();
        else if (moveDirection.x < -0.1f && isFacingRight) Flip();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(
            isFacingRight ? 1f : -1f,
            transform.localScale.y,
            transform.localScale.z
        );
    }

    // --- Saúde e dano ---

    public void TakeDamage(float rawDamage)
    {
        if (!IsAlive || IsInvincible) return;

        float totalDefense = Mathf.Clamp(defense + bonusDefense, 0f, 90f);
        float finalDamage = rawDamage * (1f - totalDefense / 100f);
        finalDamage = Mathf.Max(1f, finalDamage);

        CurrentHealth -= finalDamage;
        OnDamageTaken?.Invoke(finalDamage);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

        AudioManager.Instance?.Play("player_hurt");
        GameManager.Instance?.Vibrate();

        if (CurrentHealth <= 0f)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    public void Heal(float amount)
    {
        if (!IsAlive) return;
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void FullHeal()
    {
        CurrentHealth = MaxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    private void Die()
    {
        IsAlive = false;
        rb.linearVelocity = Vector2.zero;
        animator?.SetTrigger("Die");
        OnPlayerDied?.Invoke();
        GameManager.Instance?.TriggerGameOver();
        AudioManager.Instance?.Play("player_death");
    }

    private IEnumerator InvincibilityCoroutine()
    {
        IsInvincible = true;
        float elapsed = 0f;
        while (elapsed < invincibilityDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }
        spriteRenderer.enabled = true;
        IsInvincible = false;
    }

    // --- Coleta automática ---

    private void HandlePickupRadius()
    {
        float radius = pickupRadius + bonusPickupRadius;
        Collider2D[] items = Physics2D.OverlapCircleAll(transform.position, radius, LayerMask.GetMask("Pickup"));
        foreach (var item in items)
        {
            item.GetComponent<PickupItem>()?.Attract(transform);
        }
    }

    // --- Habilidades ---

    public void AddAbility(AbilityBase ability)
    {
        activeAbilities.Add(ability);
        ability.Initialize(this);
    }

    public bool HasAbility<T>() where T : AbilityBase
    {
        return activeAbilities.Exists(a => a is T);
    }

    public T GetAbility<T>() where T : AbilityBase
    {
        return activeAbilities.Find(a => a is T) as T;
    }

    // --- Stats calculados ---

    public float GetTotalDamage() => damageMultiplier + bonusDamage;
    public float GetTotalAttackSpeed() => attackSpeed + bonusAttackSpeed;
    public float GetTotalMoveSpeed() => moveSpeed + bonusMoveSpeed;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius + bonusPickupRadius);
    }
}
