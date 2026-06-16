using Godot;
using System;

namespace CorporateSurvivors.Data;

/// <summary>
/// Dados do funcionário (jogador).
/// Define os atributos base que podem ser modificados por upgrades.
/// </summary>
[GlobalClass]
public partial class EmployeeData : Resource
{
    [ExportGroup("Atributos Base")]
    [Export] public float MaxHealth { get; set; } = 100f;
    [Export] public float MoveSpeed { get; set; } = 200f;
    [Export] public float HealthRegenPerSecond { get; set; } = 0f;
    [Export] public float DamageReduction { get; set; } = 0f; // 0.0 a 1.0

    [ExportGroup("Café / Progressão")]
    [Export] public float CoffeeNeededForLevel1 { get; set; } = 10f;
    [Export] public float CoffeeLevelMultiplier { get; set; } = 1.15f;
    [Export] public int MaxLevel { get; set; } = 99;

    [ExportGroup("Coleta")]
    [Export] public float MagnetRadius { get; set; } = 100f; // Raio de atração de café
    [Export] public float CoffeeGainMultiplier { get; set; } = 1f;

    [ExportGroup("Visual")]
    [Export] public string EmployeeName { get; set; } = "Funcionário(a)";
    [Export] public Texture2D Portrait { get; set; }

    /// <summary>
    /// Calcula quanto café é necessário para o próximo nível.
    /// </summary>
    public float GetCoffeeNeededForLevel(int currentLevel)
    {
        return CoffeeNeededForLevel1 * Mathf.Pow(CoffeeLevelMultiplier, currentLevel - 1);
    }
}
