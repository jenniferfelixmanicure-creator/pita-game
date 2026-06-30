using UnityEngine;

/// <summary>
/// Projétil genérico do jogador. Suporta perfuração, ricochete e rastreamento.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private bool destroyOnHit = true;
    [SerializeField] private GameObject hitFXPrefab;
    [SerializeField] private TrailRenderer trail;

    public int pierceLeft = 0;     // Quantos inimigos ainda pode atravessar
    public bool isHoming = false;  // Rastreamento de inimigo mais próximo
    public float homingStrength = 3f;

    private float damage;
    private PlayerController owner;
    private Rigidbody2D rb;
    private Vector2 direction;
    private float timer;
    private Transform homingTarget;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    public void Launch(Vector2 dir, float dmg, PlayerController shooter)
    {
        direction = dir.normalized;
        damage = dmg;
        owner = shooter;
        timer = lifetime;

        rb.linearVelocity = direction * speed;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

        if (isHoming)
            homingTarget = EnemyFinder.GetNearestEnemy(transform.position);
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f) ReturnToPool();

        if (isHoming && homingTarget != null)
        {
            Vector2 toTarget = ((Vector2)homingTarget.position - rb.position).normalized;
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, toTarget * speed, Time.deltaTime * homingStrength);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        var enemy = other.GetComponent<EnemyBase>();
        if (enemy == null || !enemy.IsAlive) return;

        // Dano com chance de crítico
        float finalDmg = damage;
        bool isCrit = CombatStats.Instance != null && CombatStats.Instance.RollCrit();
        if (isCrit) finalDmg *= CombatStats.Instance.GetCritMultiplier();

        Vector2 knockDir = (other.transform.position - transform.position).normalized;
        enemy.TakeDamage(finalDmg, knockDir);

        // Evento para vampirismo etc.
        CombatEvents.FireDamageDealt(finalDmg);

        if (isCrit)
            DamageNumberPool.Instance?.ShowCrit(other.transform.position, finalDmg);

        // Hit FX
        if (hitFXPrefab)
            Instantiate(hitFXPrefab, transform.position, Quaternion.identity);

        if (pierceLeft > 0)
        {
            pierceLeft--;
            AudioManager.Instance?.Play("pierce");
        }
        else if (destroyOnHit)
        {
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        if (trail) trail.Clear();
        ProjectilePool.Instance?.Return(gameObject);
    }
}

/// <summary>Projétil de inimigo — causa dano ao jogador</summary>
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 6f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private GameObject hitFXPrefab;

    private float damage;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb) rb.gravityScale = 0f;
    }

    public void Launch(Vector2 dir, float dmg)
    {
        damage = dmg;
        if (rb) rb.linearVelocity = dir.normalized * speed;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        other.GetComponent<PlayerController>()?.TakeDamage(damage);
        if (hitFXPrefab) Instantiate(hitFXPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
