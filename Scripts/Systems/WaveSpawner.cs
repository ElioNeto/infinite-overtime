using Godot;
using System;
using System.Collections.Generic;
using CorporateSurvivors.Data;
using CorporateSurvivors.Systems;

namespace CorporateSurvivors;

/// <summary>
/// Sistema de spawn de inimigos corporativos.
/// Gera ondas baseadas no andar atual, tempo decorrido e dificuldade.
/// </summary>
public partial class WaveSpawner : Node
{
    [ExportGroup("Config")]
    [Export] public float BaseSpawnRate { get; set; } = 2f; // Inimigos por segundo (base)
    [Export] public int MaxEnemiesAlive { get; set; } = 50;
    [Export] public float SpawnRadius { get; set; } = 600f; // Distância do jogador para spawnar
    [Export] public float MinSpawnDistance { get; set; } = 400f; // Distância mínima do jogador
    [Export] public bool SpawnEnabled { get; set; } = true;

    // Pool interna construída a partir do FloorData (não usa sub-resources de .tres)
    private Godot.Collections.Array<EnemySpawnEntry> _enemyPool = new();

    private float _spawnTimer = 0f;
    private int _activeEnemyCount = 0;
    private Employee _player;

    public override void _Ready()
    {
        _player = GameManager.Instance?.Player;
    }

    public override void _Process(double delta)
    {
        if (!SpawnEnabled) return;
        if (GameManager.Instance?.CurrentState != GameManager.GameStateType.Playing) return;

        if (_player == null)
        {
            _player = GameManager.Instance?.Player;
            if (_player == null) return;
        }

        float dt = (float)delta;
        _spawnTimer += dt;

        float currentSpawnRate = GetAdjustedSpawnRate();
        float spawnInterval = 1f / currentSpawnRate;

        int activeCount = GetTree().GetNodesInGroup("enemies").Count;
        if (activeCount >= MaxEnemiesAlive) return;

        if (_spawnTimer >= spawnInterval)
        {
            _spawnTimer = 0f;
            SpawnEnemy();
        }
    }

    /// <summary>
    /// Retorna a taxa de spawn ajustada pela dificuldade temporal e do andar.
    /// </summary>
    private float GetAdjustedSpawnRate()
    {
        float timeMultiplier = 1f + (GameManager.Instance?.CurrentMinute ?? 0) * 0.05f;
        return BaseSpawnRate * timeMultiplier;
    }

    /// <summary>
    /// Spawnea um inimigo aleatório da pool em uma posição válida.
    /// </summary>
    private void SpawnEnemy()
    {
        if (_enemyPool.Count == 0)
        {
            GD.PrintErr("WaveSpawner: Pool de inimigos vazia!");
            return;
        }

        // Seleciona inimigo baseado no peso e minuto atual
        int currentMinute = GameManager.Instance?.CurrentMinute ?? 0;
        var availableEnemies = new Godot.Collections.Array<EnemySpawnEntry>();

        float totalWeight = 0f;
        foreach (var entry in _enemyPool)
        {
            if (entry.Enemy == null) continue;
            if (entry.MinFloorMinute > currentMinute) continue;

            availableEnemies.Add(entry);
            totalWeight += entry.Weight;
        }

        if (availableEnemies.Count == 0) return;

        // Rolagem ponderada
        float roll = GD.Randf() * totalWeight;
        EnemySpawnEntry selected = null;
        foreach (var entry in availableEnemies)
        {
            roll -= entry.Weight;
            if (roll <= 0f)
            {
                selected = entry;
                break;
            }
        }

        if (selected?.Enemy?.Scene == null) return;

        // Calcula posição de spawn (no perímetro ao redor do jogador)
        Vector2 spawnPos = GetSpawnPosition();
        if (spawnPos == Vector2.Zero) return;

        // Instancia o inimigo
        var enemy = selected.Enemy.Scene.Instantiate<BaseEnemy>();
        enemy.GlobalPosition = spawnPos;
        enemy.EnemyData = selected.Enemy;
        GetParent().AddChild(enemy);

        GD.Print($"WaveSpawner: Spawnou {selected.Enemy.EnemyName} em {spawnPos}");
    }

    /// <summary>
    /// Calcula uma posição de spawn válida no perímetro ao redor do jogador.
    /// Prioriza bordas da tela para não spawnar em cima do jogador.
    /// </summary>
    private Vector2 GetSpawnPosition()
    {
        if (_player == null) return Vector2.Zero;

        // Gera um ângulo aleatório
        float angle = GD.Randf() * Mathf.Pi * 2f;
        float distance = SpawnRadius;

        // Garante que não spawna muito perto
        distance = Mathf.Max(distance, MinSpawnDistance + GD.Randf() * 200f);

        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        Vector2 candidatePos = _player.GlobalPosition + offset;

        return candidatePos;
    }

    /// <summary>
    /// Configura a pool de inimigos para um andar específico.
    /// Constrói os EnemySpawnEntry internamente a partir dos arrays paralelos do FloorData,
    /// evitando o uso de sub-resources nos .tres (que falham no Godot 4 C# com GlobalClass).
    /// </summary>
    public void SetEnemyPoolForFloor(FloorData floor)
    {
        _enemyPool.Clear();

        int count = Mathf.Min(floor.EnemyPool.Count,
                    Mathf.Min(floor.EnemyWeights.Count, floor.EnemyMinMinutes.Count));
        for (int i = 0; i < count; i++)
        {
            var entry = new EnemySpawnEntry();
            entry.Enemy = floor.EnemyPool[i];
            entry.Weight = floor.EnemyWeights[i];
            entry.MinFloorMinute = floor.EnemyMinMinutes[i];
            _enemyPool.Add(entry);
        }

        BaseSpawnRate = floor.BaseSpawnRate;
        MaxEnemiesAlive = floor.MaxEnemiesAlive;
        GD.Print($"WaveSpawner: Pool configurada para {floor.FloorName} ({_enemyPool.Count} tipos)");
    }

}
