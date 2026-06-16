using Godot;
using System;
using System.Collections.Generic;
using CorporateSurvivors.Data;
using CorporateSurvivors.Systems;

namespace CorporateSurvivors;

/// <summary>
/// Gerencia a transição entre andares e carregamento dos departamentos.
/// Cada andar representa uma fase com tema, inimigos e boss próprios.
/// </summary>
public partial class FloorManager : Node
{
    [ExportGroup("Andares")]
    [Export] public Godot.Collections.Array<FloorData> Floors { get; set; } = new();

    [ExportGroup("Nodes")]
    [Export] public WaveSpawner WaveSpawner { get; set; }
    [Export] public Node2D FloorContainer { get; set; } // Onde a cena do andar é instanciada
    [Export] public TileMapLayer FloorTilemap { get; set; }

    private int _currentFloorIndex = -1;
    private Node2D _currentFloorInstance;

    public override void _Ready()
    {
        // Conecta ao GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnFloorCleared += OnFloorCleared;

            // Se nenhum andar foi atribuído no Inspector, carrega do GameManager
            if (Floors.Count == 0 && GameManager.Instance.Floors.Count > 0)
            {
                Floors = GameManager.Instance.Floors;
                GD.Print($"FloorManager: {Floors.Count} andares carregados do GameManager.");
            }
        }
    }

    /// <summary>
    /// Carrega o primeiro andar (índice 0).
    /// </summary>
    public void LoadFirstFloor()
    {
        LoadFloor(0);
    }

    /// <summary>
    /// Carrega um andar específico pelo índice.
    /// </summary>
    public void LoadFloor(int floorIndex)
    {
        if (floorIndex < 0 || floorIndex >= Floors.Count)
        {
            GD.PrintErr($"FloorManager: Andar {floorIndex} não existe!");
            return;
        }

        // Limpa andar anterior
        ClearCurrentFloor();

        _currentFloorIndex = floorIndex;
        FloorData floorData = Floors[floorIndex];

        GD.Print($"FloorManager: Carregando {floorData.FloorName}...");

        // Carrega cena do andar (se existir)
        string scenePath = $"res://Scenes/Floors/Floor{floorIndex + 1}_{SanitizeName(floorData.FloorName)}.tscn";
        var floorScene = GD.Load<PackedScene>(scenePath);

        if (floorScene != null && FloorContainer != null)
        {
            _currentFloorInstance = floorScene.Instantiate<Node2D>();
            FloorContainer.AddChild(_currentFloorInstance);
        }

        // Configura o WaveSpawner para este andar
        if (WaveSpawner != null)
        {
            WaveSpawner.SetEnemyPoolForFloor(floorData);
        }

        // Configura iluminação ambiente
        // (a ser implementado com WorldEnvironment)

        // Configura música
        if (!string.IsNullOrEmpty(floorData.MusicTrack))
        {
            var musicStream = GD.Load<AudioStream>($"res://Audio/Music/{floorData.MusicTrack}");
            if (musicStream != null)
            {
                AudioManager.Instance?.PlayMusic(musicStream);
            }
        }

        // Notifica o GameManager
        GD.Print($"FloorManager: Andar {floorData.FloorName} carregado!");
    }

    /// <summary>
    /// Chamado quando o GameManager detecta que um andar foi limpo (boss derrotado).
    /// </summary>
    private void OnFloorCleared(int floorIndex)
    {
        if (floorIndex < Floors.Count - 1)
        {
            // Próximo andar
            LoadFloor(floorIndex + 1);
        }
        else
        {
            // Último andar limpo → vitória (já é tratado pelo GameManager)
            GD.Print("FloorManager: Todos os andares foram limpos!");
        }
    }

    /// <summary>
    /// Limpa a cena do andar atual.
    /// </summary>
    private void ClearCurrentFloor()
    {
        if (_currentFloorInstance != null)
        {
            _currentFloorInstance.QueueFree();
            _currentFloorInstance = null;
        }

        // Limpa inimigos restantes
        var enemies = GetTree().GetNodesInGroup("enemies");
        foreach (var enemy in enemies)
        {
            (enemy as Node)?.QueueFree();
        }
    }

    /// <summary>
    /// Sanitiza nome para usar em paths.
    /// </summary>
    private string SanitizeName(string name)
    {
        return name.Replace(" ", "").Replace("—", "").Replace("/", "_");
    }

    public FloorData CurrentFloor => _currentFloorIndex >= 0 ? Floors[_currentFloorIndex] : null;
    public int CurrentFloorIndex => _currentFloorIndex;
}
