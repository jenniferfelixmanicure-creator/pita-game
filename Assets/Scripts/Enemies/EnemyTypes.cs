using UnityEngine;
using System.Collections;

// ==========================================
// TIPOS DE INIMIGOS (30+)
// ==========================================

/// <summary>Inimigo corpo a corpo básico — persegue e ataca de perto</summary>
public class MeleeEnemy : EnemyBase { /* Comportamento padrão da EnemyBase */ }

/// <summary>Inimigo atirador — mantém distância e atira projéteis</summary>
public class RangedEnemy : EnemyBase
{
    [Header("Ranged")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float preferredDistance = 6f;
    [SerializeField] private float shootDamage = 15f;
    [SerializeField] private float shootInterval = 2f;
    private float shootTimer;

    protected override void UpdateBehavior()
    {
        if (playerTransform == null) return;
        float dist = Vector2.Distance(transform.position, playerTransform.position);

        // Manter distância
        if (dist < preferredDistance * 0.8f)
        {
            Vector2 away = (transform.position - playerTransform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position,
                (Vector2)transform.position + away, moveSpeed * Time.deltaTime);
        }

        // Atirar
        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0f && dist <= preferredDistance * 1.5f)
        {
            Shoot();
            shootTimer = shootInterval;
        }
    }

    protected override void MoveTowardPlayer()
    {
        // Não mover diretamente para o jogador — sobrescrito em UpdateBehavior
        rb.linearVelocity = Vector2.zero;
    }

    private void Shoot()
    {
        if (playerTransform == null || projectilePrefab == null) return;
        Vector2 dir = (playerTransform.position - transform.position).normalized;
        var proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        proj.GetComponent<EnemyProjectile>()?.Launch(dir, shootDamage * difficultyMultiplier);
        AudioManager.Instance?.Play("enemy_shoot");
    }
}

/// <summary>Inimigo tanque — muito HP, lento, dano alto</summary>
public class TankEnemy : EnemyBase
{
    protected override void Start()
    {
        base.Start();
        maxHealth *= 3f;
        currentHealth = maxHealth;
        moveSpeed *= 0.5f;
        damage *= 2f;
    }
}

/// <summary>Inimigo rápido — baixo HP mas muito veloz</summary>
public class FastEnemy : EnemyBase
{
    protected override void Start()
    {
        base.Start();
        maxHealth *= 0.5f;
        currentHealth = maxHealth;
        moveSpeed *= 2.5f;
        damage *= 0.7f;
        attackCooldown *= 0.4f;
    }
}

/// <summary>Inimigo explosivo — explode ao morrer causando dano em área</summary>
public class ExplosiveEnemy : EnemyBase
{
    [Header("Explosão")]
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float explosionDamage = 40f;
    [SerializeField] private GameObject explosionFXPrefab;

    protected override void Die()
    {
        Explode();
        base.Die();
    }

    private void Explode()
    {
        if (explosionFXPrefab) Instantiate(explosionFXPrefab, transform.position, Quaternion.identity);
        AudioManager.Instance?.Play("explosion");

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
                hit.GetComponent<PlayerController>()?.TakeDamage(explosionDamage * difficultyMultiplier);
        }
    }
}

/// <summary>Inimigo divisor — se divide em versões menores ao morrer</summary>
public class SplitEnemy : EnemyBase
{
    [Header("Divisão")]
    [SerializeField] private GameObject smallEnemyPrefab;
    [SerializeField] private int splitCount = 2;
    [SerializeField] private bool alreadySplit = false;

    protected override void Die()
    {
        if (!alreadySplit && smallEnemyPrefab)
        {
            for (int i = 0; i < splitCount; i++)
            {
                Vector2 offset = Random.insideUnitCircle * 0.5f;
                var small = Instantiate(smallEnemyPrefab, (Vector2)transform.position + offset, Quaternion.identity);
                small.GetComponent<SplitEnemy>()?.SetAlreadySplit();
            }
        }
        base.Die();
    }

    public void SetAlreadySplit() => alreadySplit = true;
}

/// <summary>Inimigo curandeiro — cura inimigos próximos</summary>
public class HealerEnemy : EnemyBase
{
    [Header("Cura")]
    [SerializeField] private float healRadius = 4f;
    [SerializeField] private float healPerSec = 5f;
    [SerializeField] private float healInterval = 1f;
    private float healTimer;

    private void Update()
    {
        base.Update();
        healTimer -= Time.deltaTime;
        if (healTimer <= 0f) { HealNearby(); healTimer = healInterval; }
    }

    private void HealNearby()
    {
        var nearby = EnemyFinder.GetAllInRadius(transform.position, healRadius);
        foreach (var e in nearby)
        {
            var enemy = e.GetComponent<EnemyBase>();
            if (enemy != null && enemy != this)
                enemy.TakeDamage(-healPerSec); // Negativo = cura
        }
    }
}

/// <summary>Inimigo fantasma — atravessa obstáculos, temporariamente invulnerável</summary>
public class GhostEnemy : EnemyBase
{
    [Header("Fantasma")]
    [SerializeField] private float phaseInterval = 3f;
    [SerializeField] private float phaseDuration = 1f;
    private bool isPhasing;
    private float phaseTimer;

    protected override void Start()
    {
        base.Start();
        phaseTimer = phaseInterval;
    }

    private void Update()
    {
        base.Update();
        phaseTimer -= Time.deltaTime;
        if (phaseTimer <= 0f && !isPhasing) StartCoroutine(Phase());
    }

    private IEnumerator Phase()
    {
        isPhasing = true;
        GetComponent<Collider2D>().enabled = false;
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.3f);
        yield return new WaitForSeconds(phaseDuration);
        GetComponent<Collider2D>().enabled = true;
        GetComponent<SpriteRenderer>().color = Color.white;
        isPhasing = false;
        phaseTimer = phaseInterval;
    }

    public override void TakeDamage(float amount, Vector2 knockbackDir = default)
    {
        if (!isPhasing) base.TakeDamage(amount, knockbackDir);
    }
}

/// <summary>Inimigo escudeiro — tem escudo frontal que bloqueia dano</summary>
public class ShieldEnemy : EnemyBase
{
    [Header("Escudo")]
    [SerializeField] private float shieldHP = 50f;
    [SerializeField] private GameObject shieldSprite;
    private float currentShieldHP;
    private bool shieldBroken;

    protected override void Start()
    {
        base.Start();
        currentShieldHP = shieldHP;
    }

    public override void TakeDamage(float amount, Vector2 knockbackDir = default)
    {
        if (!shieldBroken)
        {
            currentShieldHP -= amount;
            if (currentShieldHP <= 0f)
            {
                shieldBroken = true;
                if (shieldSprite) shieldSprite.SetActive(false);
                AudioManager.Instance?.Play("shield_break");
            }
            return;
        }
        base.TakeDamage(amount, knockbackDir);
    }
}

/// <summary>Elite — versão reforçada com aura especial</summary>
public class EliteEnemy : EnemyBase
{
    [Header("Elite")]
    [SerializeField] private GameObject auraPrefab;
    [SerializeField] private float eliteScale = 1.5f;

    protected override void Start()
    {
        base.Start();
        maxHealth *= 2.5f;
        currentHealth = maxHealth;
        damage *= 1.5f;
        xpValue *= 3;
        coinValue *= 3;
        transform.localScale *= eliteScale;
        if (auraPrefab) Instantiate(auraPrefab, transform);
    }
}
