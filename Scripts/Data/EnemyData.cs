using Godot;
using System;

namespace CorporateSurvivors.Data;

/// <summary>
/// Dados de um inimigo corporativo.
/// Use este Resource para configurar cada tipo de manifestação do escritório.
/// </summary>
[GlobalClass]
public partial class EnemyData : Resource
{
    [ExportGroup("Identificação")]
    [Export] public string EnemyName { get; set; } = "E-mail Não Lido";
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "Um e-mail que você ignorou e agora ele te persegue.";
    [Export] public string Department { get; set; } = "TI"; // Qual andar/departamento esse inimigo pertence

    [ExportGroup("Atributos")]
    [Export] public float MaxHealth { get; set; } = 30f;
    [Export] public float MoveSpeed { get; set; } = 80f;
    [Export] public float Damage { get; set; } = 8f;
    [Export] public float DamageCooldown { get; set; } = 0.5f; // Intervalo entre danos no jogador
    [Export] public float CoffeeDrop { get; set; } = 5f; // Quanto de café (XP) dropa ao morrer

    [ExportGroup("Comportamento")]
    [Export] public EnemyBehaviorType BehaviorType { get; set; } = EnemyBehaviorType.Chase;
    [Export] public float DetectionRange { get; set; } = 400f;
    [Export] public float AttackRange { get; set; } = 20f;

    [ExportGroup("Spawn")]
    [Export] public float SpawnWeight { get; set; } = 1f; // Peso relativo no wave spawner
    [Export] public int MinWaveLevel { get; set; } = 1; // A partir de qual onda esse inimigo aparece

    [ExportGroup("Visual")]
    [Export] public string ScenePath { get; set; } = ""; // Caminho para o .tscn do inimigo (carregado lazy)
    [Export] public Texture2D Icon { get; set; }
    [Export] public Color GlowColor { get; set; } = new Color(1f, 0.2f, 0.2f);

    // Cache lazy da PackedScene para compatibilidade com código existente
    private PackedScene _cachedScene = null;
    /// <summary>
    /// Retorna a PackedScene carregada a partir de ScenePath.
    /// Carregada apenas no primeiro acesso (lazy load), fora do ciclo de ResourceLoader.
    /// </summary>
    public PackedScene Scene
    {
        get
        {
            if (_cachedScene == null && !string.IsNullOrEmpty(ScenePath))
            {
                _cachedScene = GD.Load<PackedScene>(ScenePath);
            }
            return _cachedScene;
        }
    }
}

public enum EnemyBehaviorType
{
    Chase,          // Persegue o jogador (padrão)
    Patrol,         // Anda em linha reta/faz patrol
    Teleport,       // Aparece e desaparece (Avaliação Fantasma)
    Stationary,     // Fica parado, ataca à distância
    Swarm,          // Pequeno, rápido, vem em grupo
    Trap,           // Cria poças/perigos no chão (Café Derramado)
    Spawner         // Gera outros inimigos (Ata de Reunião)
}
