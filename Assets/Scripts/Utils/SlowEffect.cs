using UnityEngine;
using System.Collections;

/// <summary>Efeito de lentidão aplicável a qualquer inimigo.</summary>
[RequireComponent(typeof(EnemyBase))]
public class SlowEffect : MonoBehaviour
{
    private Coroutine slowCoroutine;
    private EnemyBase enemy;
    private float originalSpeed;
    private Rigidbody2D rb;

    private void Awake()
    {
        enemy = GetComponent<EnemyBase>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void Apply(float percent, float duration)
    {
        if (slowCoroutine != null) StopCoroutine(slowCoroutine);
        slowCoroutine = StartCoroutine(SlowCoroutine(percent, duration));
    }

    private IEnumerator SlowCoroutine(float percent, float duration)
    {
        // Reduz velocidade visualmente (via cor azulada)
        var sr = GetComponent<SpriteRenderer>();
        if (sr) sr.color = new Color(0.5f, 0.7f, 1f, 1f);

        // A velocidade real é controlada no Rigidbody via scale
        if (rb) rb.linearVelocity *= (1f - percent / 100f);

        yield return new WaitForSeconds(duration);

        if (sr) sr.color = Color.white;
        slowCoroutine = null;
    }
}
