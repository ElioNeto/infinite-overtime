using Godot;
using System;

namespace CorporateSurvivors.Data;

/// <summary>
/// Dados de uma arma corporativa.
/// Cada arma é um hábito/ferramenta de escritório que ataca automaticamente.
/// </summary>
[GlobalClass]
public partial class WeaponData : Resource
{
    [ExportGroup("Identificação")]
    [Export] public string WeaponName { get; set; } = "Clipes Reforçados";
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "Projéteis de escritório: agora com mais poder de fogo.";
    [Export] public string Emoji { get; set; } = "📎";

    [ExportGroup("Atributos de Ataque")]
    [Export] public float Damage { get; set; } = 10f;
    [Export] public float AttackSpeed { get; set; } = 1.0f; // Ataques por segundo
    [Export] public float ProjectileSpeed { get; set; } = 500f;
    [Export] public float Range { get; set; } = 300f;
    [Export] public bool IsPiercing { get; set; } = false; // Atravessa inimigos?
    [Export] public int ProjectileCount { get; set; } = 1; // Quantos projéteis por ataque
    [Export] public float SpreadAngle { get; set; } = 0f; // Ângulo de dispersão (graus)

    [ExportGroup("Efeito Especial")]
    [Export] public WeaponEffectType EffectType { get; set; } = WeaponEffectType.None;
    [Export] public float EffectDuration { get; set; } = 2f;
    [Export] public float EffectValue { get; set; } = 0f; // Dano adicional, slow %, etc.

    [ExportGroup("Upgrade")]
    [Export] public int MaxLevel { get; set; } = 5;
    [Export] public float DamagePerLevel { get; set; } = 5f;
    [Export] public float AttackSpeedPerLevel { get; set; } = 0.1f;

    [ExportGroup("Visual")]
    [Export] public PackedScene ProjectileScene { get; set; }
    [Export] public Texture2D Icon { get; set; }
    [Export] public Color ProjectileColor { get; set; } = Colors.White;

    /// <summary>
    /// Retorna o dano calculado para um determinado nível da arma.
    /// </summary>
    public float GetDamageForLevel(int level)
    {
        return Damage + (DamagePerLevel * (level - 1));
    }

    /// <summary>
    /// Retorna a velocidade de ataque para um determinado nível.
    /// </summary>
    public float GetAttackSpeedForLevel(int level)
    {
        return AttackSpeed + (AttackSpeedPerLevel * (level - 1));
    }
}

public enum WeaponEffectType
{
    None,
    Slow,           // Lentidão (Mensagem Passivo-Agressiva)
    Burn,           // Dano contínuo (Notebook Superaquecido)
    Explosion,      // Dano em área (Planilha Explosiva)
    Summon,         // Invoca aliado (Impressora Fantasma)
    Heal,           // Cura ao acertar (Café)
    ArmorShred,     // Reduz defesa (Scanner)
}
