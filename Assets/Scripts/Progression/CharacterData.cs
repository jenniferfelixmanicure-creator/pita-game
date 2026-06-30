using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Dados de todos os personagens jogáveis (30+).
/// Cada personagem tem stats únicos, habilidade passiva especial e aparência.
/// </summary>
[CreateAssetMenu(fileName = "CharacterData", menuName = "PitaGame/CharacterData")]
public class CharacterData : ScriptableObject
{
    [System.Serializable]
    public class Character
    {
        public int id;
        public string characterName;
        [TextArea] public string lore;
        public Sprite portrait;
        public Sprite sprite;
        public RuntimeAnimatorController animator;

        [Header("Stats Base")]
        public float maxHealth = 100f;
        public float moveSpeed = 5f;
        public float attackSpeed = 1f;
        public float damage = 1f;
        public float defense = 0f;

        [Header("Habilidade Passiva Única")]
        public string passiveAbilityName;
        [TextArea] public string passiveDescription;

        [Header("Habilidade Inicial")]
        public AbilityBase.AbilityType startingAbilityType;

        [Header("Desbloqueio")]
        public bool startsUnlocked = false;
        public int unlockCostCoins = 0;
        public int unlockCostGems = 0;
        public string unlockRequirement = "";
    }

    public List<Character> characters = new List<Character>();

    public Character GetCharacter(int id) =>
        characters.Find(c => c.id == id);

    public List<Character> GetUnlockedCharacters()
    {
        var unlocked = new List<Character>();
        var save = SaveSystem.Instance?.Data;
        if (save == null) return unlocked;

        foreach (var c in characters)
        {
            if (c.id < save.unlockedCharacters.Length && save.unlockedCharacters[c.id])
                unlocked.Add(c);
        }
        return unlocked;
    }
}

// ==========================================
// 30 PERSONAGENS DEFINIDOS POR SCRIPT
// ==========================================

/// <summary>
/// Registro estático com os dados de todos os 30 personagens.
/// Em produção, use CharacterData ScriptableObject no editor.
/// </summary>
public static class CharacterRegistry
{
    public static readonly string[] Names = new string[]
    {
        /* 00 */ "Pita",          // Personagem inicial — equilibrado
        /* 01 */ "Zara",          // Maga — alto dano mágico, baixo HP
        /* 02 */ "Brak",          // Guerreiro — HP alto, defesa forte
        /* 03 */ "Lyra",          // Arqueira — velocidade e projéteis
        /* 04 */ "Vex",           // Assassino — crítico alto, invisibilidade
        /* 05 */ "Nox",           // Necromante — invoca mortos-vivos
        /* 06 */ "Terra",         // Druida — regeneração e cura em área
        /* 07 */ "Blitz",         // Eletromante — dano em cadeia
        /* 08 */ "Frost",         // Criomante — ralentamento e congelamento
        /* 09 */ "Pyra",          // Piromaníaca — queimadura e AoE
        /* 10 */ "Gale",          // Andarilho do Vento — velocidade máxima
        /* 11 */ "Titan",         // Colosso — enorme, lento, imune a knockback
        /* 12 */ "Specter",       // Fantasma — atravessa paredes
        /* 13 */ "Wren",          // Engenheiro — cria torretas
        /* 14 */ "Kira",          // Samurai — combos de espada
        /* 15 */ "Dusk",          // Vampiro — vampirismo aumentado
        /* 16 */ "Gloom",         // Sombra — aura de dano
        /* 17 */ "Arin",          // Paladina — escudo sagrado e cura
        /* 18 */ "Rex",           // Berserker — mais forte com HP baixo
        /* 19 */ "Mirage",        // Ilusionista — clone distrai inimigos
        /* 20 */ "Comet",         // Astronauta — meteoros do céu
        /* 21 */ "Echo",          // Bardo — buffa aliados e enfraquece inimigos
        /* 22 */ "Rift",          // Viajante do tempo — reverte ações
        /* 23 */ "Leech",         // Parasita — rouba força dos inimigos
        /* 24 */ "Surge",         // Centelha — velocidade de ataque máxima
        /* 25 */ "Void",          // Nulificador — campo de anulação
        /* 26 */ "Storm",         // Tempestade — controla o clima
        /* 27 */ "Inferno",       // Demônio — corrompido, dano duplo
        /* 28 */ "Serene",        // Monge — meditação e curas poderosas
        /* 29 */ "Omega",         // Misterioso — habilidades aleatórias
    };

    public static readonly float[] BaseHP = new float[]
    {
        100, 70, 180, 80, 75, 90, 120, 75, 85, 80,
        70, 220, 80, 95, 90, 85, 80, 130, 110, 80,
        85, 100, 90, 95, 70, 90, 100, 120, 110, 100
    };

    public static readonly float[] BaseSpeed = new float[]
    {
        5.0f, 4.5f, 3.5f, 5.5f, 7.0f, 4.0f, 4.5f, 5.0f, 4.5f, 4.5f,
        8.0f, 3.0f, 5.5f, 4.0f, 5.5f, 5.0f, 5.5f, 4.5f, 4.0f, 5.0f,
        6.0f, 4.5f, 5.0f, 5.0f, 7.5f, 4.5f, 5.0f, 4.5f, 4.5f, 5.0f
    };

    public static readonly int[] UnlockCostCoins = new int[]
    {
        0,    500,  800,  600,  1200, 900,  700,  1000, 900,  800,
        1500, 1200, 1300, 1100, 1400, 1600, 1300, 900,  1000, 1700,
        1800, 2000, 2200, 1900, 1600, 2500, 2000, 2800, 1500, 5000
    };
}
