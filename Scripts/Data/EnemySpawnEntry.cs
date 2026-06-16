using Godot;

namespace CorporateSurvivors.Data;

/// <summary>
/// Define uma entrada na pool de spawn de um andar.
/// Associa um tipo de inimigo com peso e minuto mínimo de aparição.
/// </summary>
[GlobalClass]
public partial class EnemySpawnEntry : Resource
{
    [Export] public EnemyData Enemy { get; set; }
    [Export] public float Weight { get; set; } = 1f; // Peso relativo no spawn pool
    [Export] public int MinFloorMinute { get; set; } = 0; // A partir de qual minuto spawna
}
