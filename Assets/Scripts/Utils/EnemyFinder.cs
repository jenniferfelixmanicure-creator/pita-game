using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Utilitário estático para localizar inimigos na cena.
/// Usado por habilidades, projéteis e mira automática.
/// </summary>
public static class EnemyFinder
{
    private static readonly Collider2D[] buffer = new Collider2D[200];

    public static Transform GetNearestEnemy(Vector3 from, float maxRadius = 50f)
    {
        int count = Physics2D.OverlapCircleNonAlloc(from, maxRadius, buffer, LayerMask.GetMask("Enemy"));
        Transform nearest = null;
        float nearestDist = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            var enemy = buffer[i].GetComponent<EnemyBase>();
            if (enemy == null || !enemy.IsAlive) continue;

            float dist = Vector2.Distance(from, buffer[i].transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = buffer[i].transform;
            }
        }
        return nearest;
    }

    public static Transform GetNearestEnemyExcept(Vector3 from, float maxRadius, Transform exclude)
    {
        int count = Physics2D.OverlapCircleNonAlloc(from, maxRadius, buffer, LayerMask.GetMask("Enemy"));
        Transform nearest = null;
        float nearestDist = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            if (buffer[i].transform == exclude) continue;
            var enemy = buffer[i].GetComponent<EnemyBase>();
            if (enemy == null || !enemy.IsAlive) continue;

            float dist = Vector2.Distance(from, buffer[i].transform.position);
            if (dist < nearestDist) { nearestDist = dist; nearest = buffer[i].transform; }
        }
        return nearest;
    }

    public static List<Transform> GetAllInRadius(Vector3 from, float radius)
    {
        int count = Physics2D.OverlapCircleNonAlloc(from, radius, buffer, LayerMask.GetMask("Enemy"));
        var result = new List<Transform>();
        for (int i = 0; i < count; i++)
        {
            if (buffer[i].GetComponent<EnemyBase>()?.IsAlive == true)
                result.Add(buffer[i].transform);
        }
        return result;
    }

    public static List<Transform> GetNEnemies(Vector3 from, int count, float maxRadius = 50f)
    {
        int found = Physics2D.OverlapCircleNonAlloc(from, maxRadius, buffer, LayerMask.GetMask("Enemy"));
        var result = new List<Transform>();
        var sorted = new List<(Transform t, float d)>();

        for (int i = 0; i < found; i++)
        {
            if (buffer[i].GetComponent<EnemyBase>()?.IsAlive != true) continue;
            float d = Vector2.Distance(from, buffer[i].transform.position);
            sorted.Add((buffer[i].transform, d));
        }

        sorted.Sort((a, b) => a.d.CompareTo(b.d));
        for (int i = 0; i < Mathf.Min(count, sorted.Count); i++)
            result.Add(sorted[i].t);

        return result;
    }
}
