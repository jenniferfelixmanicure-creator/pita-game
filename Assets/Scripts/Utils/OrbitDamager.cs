using UnityEngine;

/// <summary>Componente das espadas orbitais — causa dano ao encostar em inimigos.</summary>
public class OrbitDamager : MonoBehaviour
{
    public float damage = 20f;
    public PlayerController owner;
    private float cooldown = 0.3f;
    private float timer;

    private void Update() { if (timer > 0f) timer -= Time.deltaTime; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (timer > 0f) return;
        if (!other.CompareTag("Enemy")) return;
        var e = other.GetComponent<EnemyBase>();
        if (e == null || !e.IsAlive) return;
        float d = owner != null ? damage * owner.GetTotalDamage() : damage;
        bool crit = CombatStats.Instance != null && CombatStats.Instance.RollCrit();
        if (crit) d *= CombatStats.Instance.GetCritMultiplier();
        e.TakeDamage(d);
        CombatEvents.FireDamageDealt(d);
        timer = cooldown;
        AudioManager.Instance?.Play("sword_hit");
    }
}
