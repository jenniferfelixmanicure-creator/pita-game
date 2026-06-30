using UnityEngine;

/// <summary>
/// Primeiro script a rodar — garante que todos os singletons existem
/// antes de qualquer outra cena carregar.
/// Coloque este componente num GameObject "Bootstrapper" na cena Splash.
/// </summary>
[DefaultExecutionOrder(-1000)]
public class GameBootstrapper : MonoBehaviour
{
    [Header("Prefabs dos Sistemas (arrastar no Inspector)")]
    [SerializeField] private GameObject gameManagerPrefab;
    [SerializeField] private GameObject audioManagerPrefab;
    [SerializeField] private GameObject saveSystemPrefab;
    [SerializeField] private GameObject currencySystemPrefab;
    [SerializeField] private GameObject missionSystemPrefab;
    [SerializeField] private GameObject achievementSystemPrefab;
    [SerializeField] private GameObject dailyRewardSystemPrefab;
    [SerializeField] private GameObject seasonPassPrefab;
    [SerializeField] private GameObject shopSystemPrefab;
    [SerializeField] private GameObject rankingSystemPrefab;
    [SerializeField] private GameObject adManagerPrefab;
    [SerializeField] private GameObject firebaseManagerPrefab;
    [SerializeField] private GameObject permanentUpgradesPrefab;

    private static bool initialized = false;

    private void Awake()
    {
        if (initialized) { Destroy(gameObject); return; }
        initialized = true;
        DontDestroyOnLoad(gameObject);

        Spawn(saveSystemPrefab);
        Spawn(audioManagerPrefab);
        Spawn(currencySystemPrefab);
        Spawn(missionSystemPrefab);
        Spawn(achievementSystemPrefab);
        Spawn(dailyRewardSystemPrefab);
        Spawn(seasonPassPrefab);
        Spawn(shopSystemPrefab);
        Spawn(rankingSystemPrefab);
        Spawn(adManagerPrefab);
        Spawn(firebaseManagerPrefab);
        Spawn(permanentUpgradesPrefab);
        Spawn(gameManagerPrefab);

        Debug.Log("[Bootstrapper] Todos os sistemas inicializados.");
    }

    private void Spawn(GameObject prefab)
    {
        if (prefab == null) return;
        var obj = Instantiate(prefab);
        obj.name = prefab.name;
        DontDestroyOnLoad(obj);
    }
}
