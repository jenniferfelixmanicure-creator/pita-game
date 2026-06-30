using UnityEngine;

/// <summary>
/// Classe base abstrata para todas as habilidades do jogo.
/// Cada habilidade herda desta classe e implementa seu comportamento único.
/// </summary>
public abstract class AbilityBase : MonoBehaviour
{
    [Header("Info da Habilidade")]
    public string abilityName = "Habilidade";
    [TextArea] public string description = "";
    public Sprite icon;
    public int maxLevel = 5;

    [Header("Tipo")]
    public AbilityType abilityType;

    public enum AbilityType { Weapon, Passive, Active }

    // --- Estado ---
    public int CurrentLevel { get; protected set; } = 0;
    public bool IsMaxLevel => CurrentLevel >= maxLevel;
    protected PlayerController player;
    protected bool isInitialized;

    public virtual void Initialize(PlayerController owner)
    {
        player = owner;
        isInitialized = true;
        OnInitialize();
    }

    protected virtual void OnInitialize() { }

    public void Upgrade()
    {
        if (IsMaxLevel) return;
        CurrentLevel++;
        OnUpgrade(CurrentLevel);
        AudioManager.Instance?.Play("ability_upgrade");
    }

    protected virtual void OnUpgrade(int newLevel) { }

    public virtual string GetNextLevelDescription() => description;
}

// ==========================================
// HABILIDADES DE ARMAS (30+)
// ==========================================

/// <summary>Bola de Fogo — projétil que explode ao impactar</summary>
public class FireballAbility : AbilityBase
{
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private float baseInterval = 1.5f;
    [SerializeField] private float baseDamage = 25f;

    private float timer;
    private float damage;
    private float interval;

    protected override void OnInitialize()
    {
        abilityName = "Bola de Fogo";
        description = "Dispara bolas de fogo que explodem nos inimigos.";
        damage = baseDamage;
        interval = baseInterval;
    }

    protected override void OnUpgrade(int level)
    {
        damage += 10f;
        interval = Mathf.Max(0.3f, interval - 0.1f);
    }

    private void Update()
    {
        if (!isInitialized || GameManager.Instance.IsPaused) return;
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Fire();
            timer = interval / player.GetTotalAttackSpeed();
        }
    }

    private void Fire()
    {
        var target = EnemyFinder.GetNearestEnemy(player.transform.position);
        if (target == null) return;

        Vector2 dir = (target.position - player.transform.position).normalized;
        var ball = ProjectilePool.Instance.Get(fireballPrefab, player.transform.position, Quaternion.identity);
        ball?.GetComponent<Projectile>()?.Launch(dir, damage * player.GetTotalDamage(), player);
        AudioManager.Instance?.Play("fireball");
    }
}

/// <summary>Raio — dano em cadeia entre inimigos próximos</summary>
public class LightningAbility : AbilityBase
{
    [SerializeField] private float baseInterval = 2f;
    [SerializeField] private float baseDamage = 40f;
    [SerializeField] private int chainCount = 3;
    [SerializeField] private float chainRadius = 3f;
    [SerializeField] private GameObject lightningFXPrefab;

    private float timer;
    private float damage;
    private float interval;
    private int chains;

    protected override void OnInitialize()
    {
        abilityName = "Raio em Cadeia";
        description = "Lança raios que saltam entre inimigos próximos.";
        damage = baseDamage;
        interval = baseInterval;
        chains = chainCount;
    }

    protected override void OnUpgrade(int level)
    {
        damage += 15f;
        chains++;
        interval = Mathf.Max(0.5f, interval - 0.15f);
    }

    private void Update()
    {
        if (!isInitialized || GameManager.Instance.IsPaused) return;
        timer -= Time.deltaTime;
        if (timer <= 0f) { Strike(); timer = interval / player.GetTotalAttackSpeed(); }
    }

    private void Strike()
    {
        var first = EnemyFinder.GetNearestEnemy(player.transform.position);
        if (first == null) return;

        var hit = first.GetComponent<EnemyBase>();
        hit?.TakeDamage(damage * player.GetTotalDamage());

        Transform current = first;
        for (int i = 1; i < chains; i++)
        {
            var next = EnemyFinder.GetNearestEnemyExcept(current.position, chainRadius, current);
            if (next == null) break;
            DrawLightning(current.position, next.position);
            next.GetComponent<EnemyBase>()?.TakeDamage(damage * player.GetTotalDamage() * 0.7f);
            current = next;
        }

        AudioManager.Instance?.Play("lightning");
    }

    private void DrawLightning(Vector3 from, Vector3 to)
    {
        if (lightningFXPrefab)
        {
            var fx = Instantiate(lightningFXPrefab, from, Quaternion.identity);
            var lr = fx.GetComponent<LineRenderer>();
            if (lr) { lr.SetPosition(0, from); lr.SetPosition(1, to); }
            Destroy(fx, 0.2f);
        }
    }
}

/// <summary>Aura de Gelo — dano contínuo em área ao redor do jogador</summary>
public class IceAuraAbility : AbilityBase
{
    [SerializeField] private float baseRadius = 2f;
    [SerializeField] private float baseDamagePerSec = 10f;
    [SerializeField] private float slowPercent = 30f;

    private float radius;
    private float dps;
    private float tickTimer;

    protected override void OnInitialize()
    {
        abilityName = "Aura de Gelo";
        description = "Aura de gelo que congela e dana inimigos próximos.";
        radius = baseRadius;
        dps = baseDamagePerSec;
    }

    protected override void OnUpgrade(int level)
    {
        radius += 0.5f;
        dps += 5f;
    }

    private void Update()
    {
        if (!isInitialized || GameManager.Instance.IsPaused) return;
        tickTimer -= Time.deltaTime;
        if (tickTimer <= 0f) { ApplyAura(); tickTimer = 0.5f; }
    }

    private void ApplyAura()
    {
        var enemies = EnemyFinder.GetAllInRadius(player.transform.position, radius);
        foreach (var e in enemies)
        {
            e.GetComponent<EnemyBase>()?.TakeDamage(dps * 0.5f * player.GetTotalDamage());
            e.GetComponent<SlowEffect>()?.Apply(slowPercent, 1f);
        }
    }
}

/// <summary>Espada Giratória — orbita ao redor do jogador</summary>
public class OrbitingSwordsAbility : AbilityBase
{
    [SerializeField] private GameObject swordPrefab;
    [SerializeField] private float orbitRadius = 2f;
    [SerializeField] private float orbitSpeed = 120f;
    [SerializeField] private float baseDamage = 20f;

    private GameObject[] swords;
    private float angle;
    private int count = 2;
    private float damage;

    protected override void OnInitialize()
    {
        abilityName = "Espadas Orbitais";
        description = "Espadas giram ao redor do jogador causando dano.";
        damage = baseDamage;
        SpawnSwords();
    }

    protected override void OnUpgrade(int level)
    {
        damage += 8f;
        count++;
        foreach (var s in swords) if (s) Destroy(s);
        SpawnSwords();
    }

    private void SpawnSwords()
    {
        swords = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            swords[i] = swordPrefab ? Instantiate(swordPrefab) : GameObject.CreatePrimitive(PrimitiveType.Sphere);
            swords[i].transform.SetParent(null);
            var dmg = swords[i].AddComponent<OrbitDamager>();
            dmg.damage = damage;
            dmg.owner = player;
        }
    }

    private void Update()
    {
        if (!isInitialized || GameManager.Instance.IsPaused) return;
        angle += orbitSpeed * Time.deltaTime;
        for (int i = 0; i < swords.Length; i++)
        {
            if (swords[i] == null) continue;
            float a = (angle + (360f / swords.Length) * i) * Mathf.Deg2Rad;
            swords[i].transform.position = (Vector2)player.transform.position
                + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * orbitRadius;
        }
    }

    private void OnDestroy()
    {
        foreach (var s in swords) if (s) Destroy(s);
    }
}

/// <summary>Flecha Perfurante — atravessa múltiplos inimigos</summary>
public class PiercingArrowAbility : AbilityBase
{
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private float baseDamage = 35f;
    [SerializeField] private float baseInterval = 1.2f;
    [SerializeField] private int pierceCount = 2;

    private float damage, interval;
    private int pierce;
    private float timer;

    protected override void OnInitialize()
    {
        abilityName = "Flecha Perfurante";
        description = "Flecha que atravessa vários inimigos.";
        damage = baseDamage; interval = baseInterval; pierce = pierceCount;
    }

    protected override void OnUpgrade(int l) { damage += 12f; pierce++; interval -= 0.1f; }

    private void Update()
    {
        if (!isInitialized || GameManager.Instance.IsPaused) return;
        timer -= Time.deltaTime;
        if (timer <= 0f) { Shoot(); timer = interval / player.GetTotalAttackSpeed(); }
    }

    private void Shoot()
    {
        var target = EnemyFinder.GetNearestEnemy(player.transform.position);
        if (target == null) return;
        Vector2 dir = (target.position - player.transform.position).normalized;
        var arrow = ProjectilePool.Instance.Get(arrowPrefab, player.transform.position, Quaternion.identity);
        var proj = arrow?.GetComponent<Projectile>();
        if (proj) { proj.pierceLeft = pierce; proj.Launch(dir, damage * player.GetTotalDamage(), player); }
        AudioManager.Instance?.Play("arrow");
    }
}
