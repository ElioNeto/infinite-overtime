using Godot;
using System;

namespace CorporateSurvivors.Data;

/// <summary>
/// Upgrade escolhido ao subir de nível.
/// Representa hábitos de sobrevivência corporativa.
/// </summary>
[GlobalClass]
public partial class UpgradeData : Resource
{
    [ExportGroup("Identificação")]
    [Export] public string UpgradeName { get; set; } = "Café Extra Forte";
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "Seu coração acelera, mas você aguenta mais um sprint.";
    [Export] public string Emoji { get; set; } = "☕";

    [ExportGroup("Requisitos")]
    [Export] public int MinLevel { get; set; } = 1; // Nível mínimo do funcionário para aparecer
    [Export] public int MaxLevel { get; set; } = 99; // Nível máximo para aparecer
    [Export] public float RarityWeight { get; set; } = 1f; // Peso na rolagem (maior = mais comum)

    [ExportGroup("Efeito")]
    [Export] public UpgradeTargetType TargetType { get; set; } = UpgradeTargetType.Player;
    [Export] public UpgradeModifierType ModifierType { get; set; } = UpgradeModifierType.Additive;
    [Export] public UpgradeStatType StatType { get; set; } = UpgradeStatType.MoveSpeed;
    [Export] public float Value { get; set; } = 20f; // Quanto aumenta (ex: +20 de speed)

    [ExportGroup("Stacking")]
    [Export] public bool IsStackable { get; set; } = true;
    [Export] public int MaxStacks { get; set; } = 3;

    [ExportGroup("Visual")]
    [Export] public Texture2D Icon { get; set; }
    [Export] public Color TintColor { get; set; } = Colors.White;

    /// <summary>
    /// Retorna o valor calculado considerando quantas vezes já foi pego.
    /// </summary>
    public float GetValueForStack(int currentStack)
    {
        if (!IsStackable) return Value;
        return Value * Mathf.Min(currentStack + 1, MaxStacks);
    }
}

public enum UpgradeTargetType
{
    Player,         // Modifica atributos do jogador
    Weapon,         // Modifica uma arma específica
    Global          // Modifica o jogo (ex: spawn rate reduzido)
}

public enum UpgradeModifierType
{
    Additive,       // Soma: +20 de speed
    Multiplicative, // Multiplica: *1.2 de dano
    Override        // Substitui: define um valor específico
}

public enum UpgradeStatType
{
    // Player stats
    MoveSpeed,
    MaxHealth,
    HealthRegen,
    DamageReduction,
    CoffeeGain,     // Café recebido por drop

    // Weapon stats
    WeaponDamage,
    WeaponSpeed,
    WeaponRange,
    ProjectileCount,

    // Global
    MagnetRadius,   // Raio de atração de café
    ExperienceGain, // Ganho de XP geral
    SpawnRate,      // Reduz spawn rate (mais seguro)
}
