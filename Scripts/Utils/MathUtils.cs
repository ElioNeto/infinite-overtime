using Godot;
using System;

namespace CorporateSurvivors.Utils;

/// <summary>
/// Utilitários matemáticos para o jogo.
/// </summary>
public static class MathUtils
{
    /// <summary>
    /// Mapeia um valor de uma faixa para outra.
    /// </summary>
    public static float Map(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        float t = (value - fromMin) / (fromMax - fromMin);
        return Mathf.Lerp(toMin, toMax, t);
    }

    /// <summary>
    /// Retorna um ângulo apontando para um alvo.
    /// </summary>
    public static float AngleTo(Vector2 from, Vector2 to)
    {
        return (to - from).Angle();
    }

    /// <summary>
    /// Gera um ponto aleatório dentro de um círculo.
    /// </summary>
    public static Vector2 RandomPointInCircle(float radius)
    {
        float angle = GD.Randf() * Mathf.Pi * 2f;
        float r = Mathf.Sqrt(GD.Randf()) * radius; // Distribuição uniforme
        return new Vector2(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r);
    }

    /// <summary>
    /// Gera um ponto aleatório no perímetro de um círculo.
    /// </summary>
    public static Vector2 RandomPointOnCircle(float radius)
    {
        float angle = GD.Randf() * Mathf.Pi * 2f;
        return new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
    }

    /// <summary>
    /// Retorna uma direção aleatória (vetor unitário).
    /// </summary>
    public static Vector2 RandomDirection()
    {
        float angle = GD.Randf() * Mathf.Pi * 2f;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    /// <summary>
    /// Retorna a distância entre dois pontos ignorando a altura (2D).
    /// </summary>
    public static float Distance2D(Vector2 a, Vector2 b)
    {
        return a.DistanceTo(b);
    }

    /// <summary>
    /// Amortece um valor em direção a um alvo (para movimentação suave).
    /// </summary>
    public static float Damp(float current, float target, float smoothing, float delta)
    {
        return Mathf.Lerp(current, target, 1f - Mathf.Exp(-smoothing * delta));
    }

    /// <summary>
    /// Versão 2D do Damp.
    /// </summary>
    public static Vector2 Damp(Vector2 current, Vector2 target, float smoothing, float delta)
    {
        return new Vector2(
            Damp(current.X, target.X, smoothing, delta),
            Damp(current.Y, target.Y, smoothing, delta)
        );
    }
}
