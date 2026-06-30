using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// UI de seleção de habilidades ao subir de nível.
/// Apresenta 3 opções aleatórias (incluindo upgrades das já coletadas).
/// </summary>
public class AbilitySelectionUI : MonoBehaviour
{
    public static AbilitySelectionUI Instance { get; private set; }

    [Header("Referências")]
    [SerializeField] private GameObject panel;
    [SerializeField] private AbilityCard[] cards; // 3 cards

    [Header("Todas as habilidades disponíveis")]
    [SerializeField] private List<AbilityBase> allAbilityPrefabs;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        if (panel) panel.SetActive(false);
    }

    public void ShowAbilityChoices(int count = 3)
    {
        var options = GetRandomOptions(count);
        if (panel) panel.SetActive(true);

        for (int i = 0; i < cards.Length; i++)
        {
            if (i < options.Count)
            {
                cards[i].gameObject.SetActive(true);
                cards[i].Setup(options[i], OnAbilityChosen);
            }
            else
            {
                cards[i].gameObject.SetActive(false);
            }
        }
    }

    private List<AbilityBase> GetRandomOptions(int count)
    {
        var player = PlayerController.Instance;
        var options = new List<AbilityBase>();
        var pool = new List<AbilityBase>(allAbilityPrefabs);

        // Priorizar upgrades de habilidades que o jogador já tem
        foreach (var prefab in allAbilityPrefabs)
        {
            if (player != null && player.HasAbility<AbilityBase>())
            {
                // Move para o início do pool
                pool.Remove(prefab);
                pool.Insert(0, prefab);
            }
        }

        // Selecionar aleatoriamente
        while (options.Count < count && pool.Count > 0)
        {
            int idx = Random.Range(0, pool.Count);
            options.Add(pool[idx]);
            pool.RemoveAt(idx);
        }

        return options;
    }

    private void OnAbilityChosen(AbilityBase chosenPrefab)
    {
        var player = PlayerController.Instance;
        if (player == null) return;

        // Verificar se já tem essa habilidade (upgrade)
        var existing = player.GetAbility<AbilityBase>();
        if (existing != null && existing.GetType() == chosenPrefab.GetType())
        {
            existing.Upgrade();
        }
        else
        {
            // Nova habilidade
            var instance = player.gameObject.AddComponent(chosenPrefab.GetType()) as AbilityBase;
            player.AddAbility(instance);
        }

        if (panel) panel.SetActive(false);
        XPSystem.Instance?.OnAbilitySelected();
    }
}

/// <summary>Card de habilidade individual na tela de seleção</summary>
public class AbilityCard : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Button button;

    private System.Action<AbilityBase> onChosen;
    private AbilityBase abilityRef;

    public void Setup(AbilityBase ability, System.Action<AbilityBase> callback)
    {
        abilityRef = ability;
        onChosen = callback;

        if (icon && ability.icon) icon.sprite = ability.icon;
        if (nameText) nameText.text = ability.abilityName;
        if (descriptionText) descriptionText.text = ability.GetNextLevelDescription();

        int currentLevel = PlayerController.Instance?.GetAbility<AbilityBase>()?.CurrentLevel ?? 0;
        if (levelText) levelText.text = currentLevel > 0 ? $"Nível {currentLevel} → {currentLevel + 1}" : "NOVO";

        button?.onClick.RemoveAllListeners();
        button?.onClick.AddListener(() => onChosen?.Invoke(abilityRef));
    }
}
