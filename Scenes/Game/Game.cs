using Godot;
using System;
using CorporateSurvivors.Data;
using CorporateSurvivors.Systems;

namespace CorporateSurvivors;

/// <summary>
/// Cena principal do jogo — gerencia a execução da gameplay.
/// Instancia o jogador, o spawner, o HUD e coordena o loop do jogo.
/// </summary>
public partial class Game : Node
{
    [ExportGroup("Referências")]
    [Export] public PackedScene EmployeeScene { get; set; } // Player scene
    [Export] public PackedScene HUDScene { get; set; }

    [ExportGroup("Sistemas")]
    [Export] public WaveSpawner WaveSpawner { get; set; }
    [Export] public FloorManager FloorManager { get; set; }
    [Export] public UpgradeSystem UpgradeSystem { get; set; }
    [Export] public Node2D WorldContainer { get; set; } // Onde o jogador e inimigos ficam

    private Node _hudInstance;
    private Employee _playerInstance;

    public override void _Ready()
    {
        if (GameManager.Instance == null)
        {
            GD.PrintErr("Game: GameManager não encontrado!");
            return;
        }

        // Escuta eventos do GameManager
        GameManager.Instance.OnGameOver += OnGameOver;
        GameManager.Instance.OnVictory += OnVictory;
        GameManager.Instance.OnLeveledUp += OnPlayerLeveledUp;
        GameManager.Instance.OnMinutePassed += OnMinutePassed;

        StartGameplay();
    }

    private void StartGameplay()
    {
        // Instancia o jogador
        if (EmployeeScene != null && WorldContainer != null)
        {
            _playerInstance = EmployeeScene.Instantiate<Employee>();
            _playerInstance.Position = Vector2.Zero; // Centro do mapa
            WorldContainer.AddChild(_playerInstance);

            // Adiciona aos grupos
            _playerInstance.AddToGroup("player");

            GD.Print("Game: Jogador instanciado.");
        }
        else
        {
            GD.PrintErr("Game: EmployeeScene ou WorldContainer não configurados!");
        }

        // Instancia o HUD
        if (HUDScene != null)
        {
            _hudInstance = HUDScene.Instantiate();
            AddChild(_hudInstance);
            GD.Print("Game: HUD instanciado.");
        }

        // Inicializa FloorManager
        if (FloorManager != null)
        {
            FloorManager.WaveSpawner = WaveSpawner;
            FloorManager.FloorContainer = WorldContainer;
            FloorManager.LoadFirstFloor();
        }

        // Inicia oficialmente o jogo no GameManager
        GameManager.Instance.StartGame();
    }

    private void OnGameOver()
    {
        GD.Print("Game: Game Over — carregando cena de game over...");
        // Trocar para cena GameOver.tscn
        // GetTree().ChangeSceneToFile("res://Scenes/UI/GameOver.tscn");
    }

    private void OnVictory()
    {
        GD.Print("Game: Vitória! O amanhecer chegou!");
        // Trocar para cena Victory.tscn
        // GetTree().ChangeSceneToFile("res://Scenes/UI/Victory.tscn");
    }

    private void OnPlayerLeveledUp(int newLevel)
    {
        GD.Print($"Game: Jogador atingiu nível {newLevel}!");
        // O UpgradeSystem já gerencia a escolha
    }

    private void OnMinutePassed(int totalMinute)
    {
        GD.Print($"Game: Minuto {totalMinute} — Dificuldade aumentada!");
        // Pode tocar efeitos sonoros ou avisos na UI
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("pause"))
        {
            if (GameManager.Instance?.CurrentState == GameManager.GameStateType.Playing)
                GameManager.Instance?.PauseGame();
            else if (GameManager.Instance?.CurrentState == GameManager.GameStateType.Paused)
                GameManager.Instance?.ResumeGame();
        }
    }

    public override void _ExitTree()
    {
        // Desconecta sinais para evitar vazamentos
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= OnGameOver;
            GameManager.Instance.OnVictory -= OnVictory;
            GameManager.Instance.OnLeveledUp -= OnPlayerLeveledUp;
            GameManager.Instance.OnMinutePassed -= OnMinutePassed;
        }
    }
}
