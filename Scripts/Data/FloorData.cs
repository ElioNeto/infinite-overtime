using Godot;
using System;
using System.Collections.Generic;

namespace CorporateSurvivors.Data;

/// <summary>
/// Dados de um andar/fase do prédio corporativo.
/// </summary>
[GlobalClass]
public partial class FloorData : Resource
{
    [ExportGroup("Identificação")]
    [Export] public string FloorName { get; set; } = "Térreo — Recepção & Hall";
    [Export] public int FloorIndex { get; set; } = 0;
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "A recepção parece normal... até você tentar sair.";

    [ExportGroup("Duração")]
    [Export] public float DurationMinutes { get; set; } = 6f; // Minutos para sobreviver neste andar
    [Export] public bool HasBoss { get; set; } = true;

    [ExportGroup("Spawn")]
    [Export] public Godot.Collections.Array<EnemySpawnEntry> EnemyPool { get; set; } = new();
    [Export] public float BaseSpawnRate { get; set; } = 2f; // Inimigos por segundo (base)
    [Export] public int MaxEnemiesAlive { get; set; } = 50;

    [ExportGroup("Visual")]
    [Export] public Color AmbientColor { get; set; } = new Color(0.15f, 0.15f, 0.2f);
    [Export] public Texture2D FloorPreview { get; set; }
    [Export] public string MusicTrack { get; set; } = "";

    [ExportGroup("Boss")]
    [Export] public string BossName { get; set; } = "Porteiro Eterno";
    [Export] public string BossScenePath { get; set; } = ""; // Caminho para o .tscn do boss (carregado lazy)

    /// <summary>
    /// Retorna a taxa de spawn ajustada para o minuto atual dentro deste andar.
    /// </summary>
    public float GetSpawnRateForMinute(int minute)
    {
        // A cada minuto, +5% de spawn rate
        return BaseSpawnRate * (1f + 0.05f * minute);
    }
}
