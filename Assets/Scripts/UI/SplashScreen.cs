using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Splash screen épica da Pita Game com:
/// - Logo central com scale-in + glow pulsante
/// - Raios elétricos animados (azul e roxo)
/// - Partículas faiscantes
/// - Fade-in/out suave para o menu principal
/// - Suporte a logo fornecida pelo usuário
/// </summary>
public class SplashScreen : MonoBehaviour
{
    [Header("Referências de UI")]
    [SerializeField] private Image logoImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("Sprites")]
    [SerializeField] private Sprite logoSprite;   // splash_logo.webp

    [Header("Partículas")]
    [SerializeField] private ParticleSystem lightningLeft;
    [SerializeField] private ParticleSystem lightningRight;
    [SerializeField] private ParticleSystem sparkParticles;
    [SerializeField] private ParticleSystem glowParticles;

    [Header("Configurações de Timing")]
    [SerializeField] private float fadeInDuration  = 0.8f;
    [SerializeField] private float holdDuration    = 2.2f;
    [SerializeField] private float fadeOutDuration = 0.6f;
    [SerializeField] private string nextScene       = "MainMenu";

    [Header("Animações do Logo")]
    [SerializeField] private float logoPunchScale  = 1.15f;
    [SerializeField] private float glowFrequency   = 1.8f;
    [SerializeField] private float glowMinAlpha    = 0.6f;

    // Imagem de glow atrás do logo
    [SerializeField] private Image logoGlowImage;

    private void Start()
    {
        if (logoImage && logoSprite)
            logoImage.sprite = logoSprite;

        StartCoroutine(PlaySplash());
    }

    private IEnumerator PlaySplash()
    {
        // Estado inicial
        if (canvasGroup) canvasGroup.alpha = 0f;
        if (logoImage) logoImage.transform.localScale = Vector3.one * 0.4f;
        if (logoGlowImage) logoGlowImage.color = new Color(0.3f, 0.5f, 1f, 0f);

        // 1. Fade-in do fundo
        yield return StartCoroutine(FadeCanvas(0f, 1f, fadeInDuration * 0.5f));

        // 2. Logo escala com punch (0.4 → 1.15 → 1.0)
        yield return StartCoroutine(ScaleLogo());

        // 3. Ativa partículas
        lightningLeft?.Play();
        lightningRight?.Play();
        sparkParticles?.Play();
        glowParticles?.Play();

        // 4. Glow pulsante + texto loading
        float elapsed = 0f;
        float dotTimer = 0f;
        int dots = 0;
        while (elapsed < holdDuration)
        {
            elapsed  += Time.deltaTime;
            dotTimer += Time.deltaTime;

            // Glow pulsante azul/roxo
            if (logoGlowImage)
            {
                float g = Mathf.Lerp(glowMinAlpha, 1f,
                    (Mathf.Sin(elapsed * glowFrequency * Mathf.PI * 2f) + 1f) * 0.5f);
                float hue = (elapsed * 0.1f) % 1f;   // 0=azul → roxo
                Color c = Color.HSVToRGB(Mathf.Lerp(0.62f, 0.78f, (Mathf.Sin(elapsed * 0.8f) + 1f) * 0.5f), 0.9f, 1f);
                logoGlowImage.color = new Color(c.r, c.g, c.b, g);
            }

            // Logo micro oscila
            if (logoImage)
            {
                float s = 1f + Mathf.Sin(elapsed * 2.5f) * 0.015f;
                logoImage.transform.localScale = Vector3.one * s;
            }

            // Animação dos dots no texto
            if (dotTimer >= 0.45f)
            {
                dotTimer = 0f;
                dots = (dots + 1) % 4;
                if (loadingText) loadingText.text = "CARREGANDO" + new string('.', dots);
            }

            yield return null;
        }

        // 5. Fecha partículas
        lightningLeft?.Stop();
        lightningRight?.Stop();
        sparkParticles?.Stop();
        glowParticles?.Stop();

        if (loadingText) loadingText.text = "PRONTO!";

        // 6. Fade-out total
        yield return StartCoroutine(FadeCanvas(1f, 0f, fadeOutDuration));

        // 7. Carrega cena do menu
        SceneManager.LoadScene(nextScene);
    }

    private IEnumerator ScaleLogo()
    {
        float t = 0f;
        float dur = 0.55f;
        Vector3 from = Vector3.one * 0.4f;
        Vector3 punch = Vector3.one * logoPunchScale;

        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float ease = EaseOutBack(t);
            if (logoImage) logoImage.transform.localScale = Vector3.LerpUnclamped(from, punch, ease);
            yield return null;
        }

        // Volta para escala 1
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.2f;
            if (logoImage) logoImage.transform.localScale = Vector3.Lerp(punch, Vector3.one, t);
            yield return null;
        }
    }

    private IEnumerator FadeCanvas(float from, float to, float duration)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            if (canvasGroup) canvasGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }
    }

    private static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }
}
