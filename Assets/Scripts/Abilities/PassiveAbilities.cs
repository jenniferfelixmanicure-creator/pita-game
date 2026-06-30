using UnityEngine;

// ==========================================
// HABILIDADES PASSIVAS (20+)
// ==========================================

/// <summary>Vampirismo — recupera vida ao causar dano</summary>
public class VampirismAbility : AbilityBase
{
    [SerializeField] private float healPercent = 5f;
    private float totalHeal;

    protected override void OnInitialize()
    {
        abilityName = "Vampirismo";
        description = "Recupera % da vida ao causar dano.";
        CombatEvents.OnDamageDealt += OnDamageDealt;
    }

    protected override void OnUpgrade(int l) => healPercent += 3f;

    private void OnDamageDealt(float dmg) => player?.Heal(dmg * (healPercent / 100f));
    private void OnDestroy() => CombatEvents.OnDamageDealt -= OnDamageDealt;
}

/// <summary>Armadura de Espinhos — devolve % do dano recebido</summary>
public class ThornsAbility : AbilityBase
{
    [SerializeField] private float returnPercent = 20f;

    protected override void OnInitialize()
    {
        abilityName = "Armadura de Espinhos";
        description = "Devolve % do dano recebido ao atacante.";
        player.OnDamageTaken += OnDamageTaken;
    }

    protected override void OnUpgrade(int l) => returnPercent += 10f;

    private void OnDamageTaken(float dmg)
    {
        float reflected = dmg * (returnPercent / 100f);
        var nearest = EnemyFinder.GetNearestEnemy(player.transform.position);
        nearest?.GetComponent<EnemyBase>()?.TakeDamage(reflected);
    }

    private void OnDestroy() => player.OnDamageTaken -= OnDamageTaken;
}

/// <summary>Velocidade do Vento — aumenta velocidade de movimento</summary>
public class WindSpeedAbility : AbilityBase
{
    [SerializeField] private float speedBonus = 1.5f;

    protected override void OnInitialize()
    {
        abilityName = "Velocidade do Vento";
        description = "Aumenta velocidade de movimento significativamente.";
        player.bonusMoveSpeed += speedBonus;
    }

    protected override void OnUpgrade(int l) => player.bonusMoveSpeed += speedBonus * 0.5f;
}

/// <summary>Coração de Ferro — aumenta HP máximo</summary>
public class IronHeartAbility : AbilityBase
{
    [SerializeField] private float hpBonus = 30f;

    protected override void OnInitialize()
    {
        abilityName = "Coração de Ferro";
        description = "Aumenta o HP máximo do personagem.";
        player.bonusMaxHealth += hpBonus;
        player.Heal(hpBonus);
    }

    protected override void OnUpgrade(int l)
    {
        player.bonusMaxHealth += hpBonus;
        player.Heal(hpBonus);
    }
}

/// <summary>Escudo de Bolso — absorve o próximo hit a cada X segundos</summary>
public class BubbleShieldAbility : AbilityBase
{
    [SerializeField] private float cooldown = 8f;
    private float timer;
    private bool shieldActive;

    protected override void OnInitialize()
    {
        abilityName = "Escudo de Bolso";
        description = "Bloqueia o próximo hit periodicamente.";
        timer = cooldown;
        ActivateShield();
    }

    protected override void OnUpgrade(int l) => cooldown = Mathf.Max(3f, cooldown - 1.5f);

    private void Update()
    {
        if (!isInitialized || shieldActive) return;
        timer -= Time.deltaTime;
        if (timer <= 0f) ActivateShield();
    }

    private void ActivateShield()
    {
        shieldActive = true;
        player.OnDamageTaken += AbsorbDamage;
        UIManager.Instance?.ShowShieldIndicator(true);
    }

    private void AbsorbDamage(float dmg)
    {
        shieldActive = false;
        timer = cooldown;
        player.OnDamageTaken -= AbsorbDamage;
        UIManager.Instance?.ShowShieldIndicator(false);
        AudioManager.Instance?.Play("shield_break");
        player.IsInvincible = false; // Força a invencibilidade a não acumular
    }
}

/// <summary>Ímã — aumenta o raio de coleta de drops</summary>
public class MagnetAbility : AbilityBase
{
    [SerializeField] private float radiusBonus = 2f;

    protected override void OnInitialize()
    {
        abilityName = "Ímã";
        description = "Atrai itens de maior distância.";
        player.bonusPickupRadius += radiusBonus;
    }

    protected override void OnUpgrade(int l) => player.bonusPickupRadius += radiusBonus * 0.5f;
}

/// <summary>Sangue Frio — aumenta dano crítico</summary>
public class ColdBloodAbility : AbilityBase
{
    [SerializeField] private float critChance = 10f;
    [SerializeField] private float critMultiplier = 1.5f;

    protected override void OnInitialize()
    {
        abilityName = "Sangue Frio";
        description = "Aumenta chance e multiplicador de crítico.";
        CombatStats.Instance?.AddCritChance(critChance);
        CombatStats.Instance?.AddCritMultiplier(critMultiplier - 1f);
    }

    protected override void OnUpgrade(int l)
    {
        CombatStats.Instance?.AddCritChance(5f);
        CombatStats.Instance?.AddCritMultiplier(0.2f);
    }
}

/// <summary>Adrenalina — aumenta ataque ao ficar com pouca vida</summary>
public class AdrenalineAbility : AbilityBase
{
    [SerializeField] private float threshold = 30f;
    [SerializeField] private float damageBonus = 50f;
    private bool isActive;

    protected override void OnInitialize()
    {
        abilityName = "Adrenalina";
        description = "Aumenta dano quando HP está abaixo de 30%.";
    }

    private void Update()
    {
        if (!isInitialized) return;
        bool shouldBeActive = (player.CurrentHealth / player.MaxHealth * 100f) < threshold;
        if (shouldBeActive != isActive)
        {
            isActive = shouldBeActive;
            player.bonusDamage += isActive ? damageBonus / 100f : -damageBonus / 100f;
        }
    }

    protected override void OnUpgrade(int l) => damageBonus += 15f;
}

/// <summary>Regeneração — recupera HP ao longo do tempo</summary>
public class RegenerationAbility : AbilityBase
{
    [SerializeField] private float hpPerSec = 2f;
    private float timer;

    protected override void OnInitialize()
    {
        abilityName = "Regeneração";
        description = "Recupera HP passivamente ao longo do tempo.";
    }

    protected override void OnUpgrade(int l) => hpPerSec += 1.5f;

    private void Update()
    {
        if (!isInitialized) return;
        timer -= Time.deltaTime;
        if (timer <= 0f) { player?.Heal(hpPerSec); timer = 1f; }
    }
}

/// <summary>Ganância — ganha mais moedas dos inimigos</summary>
public class GreedAbility : AbilityBase
{
    [SerializeField] private float coinBonus = 50f;

    protected override void OnInitialize()
    {
        abilityName = "Ganância";
        description = "Ganha mais moedas ao matar inimigos.";
        CurrencySystem.Instance?.AddCoinBonusPercent(coinBonus);
    }

    protected override void OnUpgrade(int l) => CurrencySystem.Instance?.AddCoinBonusPercent(25f);
}
