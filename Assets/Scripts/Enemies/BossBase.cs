using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Classe base para todos os chefes (Bosses).
/// Suporta múltiplas fases, padrões de ataque e ataques especiais.
/// </summary>
public class BossBase : EnemyBase
{
    [System.Serializable]
    public class BossPhase
    {
        public string phaseName;
        [Range(0f, 1f)] public float triggerAtHP = 0.5f;
        public float speedMultiplier = 1.2f;
        public float damageMultiplier = 1.3f;
        public GameObject phaseEffect;
    }

    [Header("Boss Config")]
    public string bossName = "Chefe";
    [SerializeField] private List<BossPhase> phases;
    [SerializeField] private float enrageThreshold = 0.2f;
    [SerializeField] private bool isEnraged;

    [Header("Ataques Especiais")]
    [SerializeField] private float specialAttackInterval = 8f;
    private float specialAttackTimer;
    private int currentPhaseIndex = -1;

    protected override void Start()
    {
        base.Start();
        UIManager.Instance?.ShowBossHP(bossName, 1f);
        AudioManager.Instance?.PlayMusic("boss_music");
        specialAttackTimer = specialAttackInterval;
    }

    private new void Update()
    {
        base.Update();
        if (!IsAlive) return;

        UIManager.Instance?.ShowBossHP(bossName, HealthPercent);
        CheckPhaseTransition();

        specialAttackTimer -= Time.deltaTime;
        if (specialAttackTimer <= 0f)
        {
            StartCoroutine(SpecialAttack());
            specialAttackTimer = specialAttackInterval;
        }
    }

    private void CheckPhaseTransition()
    {
        for (int i = 0; i < phases.Count; i++)
        {
            if (HealthPercent <= phases[i].triggerAtHP && currentPhaseIndex < i)
            {
                EnterPhase(i);
            }
        }

        if (!isEnraged && HealthPercent <= enrageThreshold) Enrage();
    }

    private void EnterPhase(int index)
    {
        currentPhaseIndex = index;
        var phase = phases[index];
        moveSpeed *= phase.speedMultiplier;
        damage *= phase.damageMultiplier;

        if (phase.phaseEffect)
            Instantiate(phase.phaseEffect, transform.position, Quaternion.identity);

        AudioManager.Instance?.Play("boss_phase");
        StartCoroutine(PhaseTransitionEffect());
    }

    private void Enrage()
    {
        isEnraged = true;
        moveSpeed *= 1.5f;
        damage *= 1.5f;
        attackCooldown *= 0.5f;
        GetComponent<SpriteRenderer>()?.material?.SetFloat("_Enrage", 1f);
        AudioManager.Instance?.Play("boss_enrage");
    }

    protected virtual IEnumerator SpecialAttack()
    {
        // Override nos filhos para ataques únicos
        yield return null;
    }

    private IEnumerator PhaseTransitionEffect()
    {
        GetComponent<SpriteRenderer>().color = Color.white;
        for (int i = 0; i < 5; i++)
        {
            GetComponent<SpriteRenderer>().color = Color.red;
            yield return new WaitForSeconds(0.1f);
            GetComponent<SpriteRenderer>().color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
    }

    protected override void Die()
    {
        UIManager.Instance?.HideBossHP();
        AudioManager.Instance?.PlayMusic("main_game");

        // Drop especial de boss
        CurrencySystem.Instance?.AddGems(10 + (int)(difficultyMultiplier * 5));
        CurrencySystem.Instance?.AddCoins(200 + (int)(difficultyMultiplier * 100));
        ChestSystem.Instance?.DropChest(transform.position, ChestType.Boss);

        base.Die();
    }
}

// ==========================================
// BOSS 1 — Guardião das Sombras
// ==========================================
public class ShadowGuardianBoss : BossBase
{
    [SerializeField] private GameObject shadowOrbPrefab;
    [SerializeField] private int orbCount = 8;

    protected override IEnumerator SpecialAttack()
    {
        AudioManager.Instance?.Play("boss_special");
        yield return new WaitForSeconds(0.5f);

        // Dispara orbes em círculo
        for (int i = 0; i < orbCount; i++)
        {
            float angle = (360f / orbCount) * i * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            if (shadowOrbPrefab)
            {
                var orb = Instantiate(shadowOrbPrefab, transform.position, Quaternion.identity);
                orb.GetComponent<EnemyProjectile>()?.Launch(dir, GetDamage() * 0.8f);
            }
            yield return new WaitForSeconds(0.05f);
        }
    }
}

// ==========================================
// BOSS 2 — Titã de Gelo
// ==========================================
public class IceTitanBoss : BossBase
{
    [SerializeField] private float freezeRadius = 5f;
    [SerializeField] private float freezeDuration = 2f;

    protected override IEnumerator SpecialAttack()
    {
        AudioManager.Instance?.Play("ice_blast");
        yield return new WaitForSeconds(0.3f);

        // Congela jogador
        PlayerController.Instance?.StartCoroutine(FreezePlayer());
    }

    private IEnumerator FreezePlayer()
    {
        // Efeito visual de congelamento
        yield return new WaitForSeconds(freezeDuration);
    }
}

// ==========================================
// BOSS 3 — Rei dos Mortos
// ==========================================
public class UndeadKingBoss : BossBase
{
    [SerializeField] private GameObject skeletonPrefab;
    [SerializeField] private int summonCount = 4;

    protected override IEnumerator SpecialAttack()
    {
        AudioManager.Instance?.Play("summon");
        for (int i = 0; i < summonCount; i++)
        {
            Vector2 pos = (Vector2)transform.position + Random.insideUnitCircle * 3f;
            if (skeletonPrefab) Instantiate(skeletonPrefab, pos, Quaternion.identity);
            yield return new WaitForSeconds(0.3f);
        }
    }
}
