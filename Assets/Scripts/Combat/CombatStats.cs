using UnityEngine;

/// <summary>Stats de combate globais como crítico — singleton por partida.</summary>
public class CombatStats : MonoBehaviour
{
    public static CombatStats Instance { get; private set; }

    private float critChance = 5f;      // %
    private float critMultiplier = 2f;  // x dano

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddCritChance(float amount) => critChance += amount;
    public void AddCritMultiplier(float amount) => critMultiplier += amount;
    public bool RollCrit() => Random.value * 100f < critChance;
    public float GetCritMultiplier() => critMultiplier;
    public float GetCritChance() => critChance;
}
