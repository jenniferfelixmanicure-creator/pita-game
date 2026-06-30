using System;

/// <summary>Canal de eventos de combate — desacopla sistemas sem referências diretas.</summary>
public static class CombatEvents
{
    public static event Action<float> OnDamageDealt;
    public static event Action<float> OnDamageReceived;
    public static event Action OnEnemyKilled;
    public static event Action OnCriticalHit;

    public static void FireDamageDealt(float amount) => OnDamageDealt?.Invoke(amount);
    public static void FireDamageReceived(float amount) => OnDamageReceived?.Invoke(amount);
    public static void FireEnemyKilled() => OnEnemyKilled?.Invoke();
    public static void FireCriticalHit() => OnCriticalHit?.Invoke();
}
