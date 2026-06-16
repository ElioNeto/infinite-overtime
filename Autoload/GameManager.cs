using Godot;
using System;
using System.Collections.Generic;
using CorporateSurvivors.Data;

namespace CorporateSurvivors.Systems;

/// <summary>
/// Gerenciador global do jogo (Autoload).
/// Controla estado geral, transição de andares, progressão do jogador.
/// </summary>
public partial class GameManager : Node
{
    // --- Singleton ---
    public static GameManager Instance { get; private set; }

    // --- Signals ---
    [Signal]
    public delegate void OnFloorClearedEventHandler(int floorIndex);

    [Signal]
    public delegate void OnGameOverEventHandler();

    [Signal]
    public delegate void OnVictoryEventHandler();

    [Signal]
    public delegate void OnMinutePassedEventHandler(int totalMinute);

    [Signal]
    public delegate void OnLeveledUpEventHandler(int newLevel);

    [Signal]
    public delegate void OnUpgradeChosenEventHandler(UpgradeData upgrade);

    // --- Estado do Jogo ---
    public enum GameStateType
    {
        MainMenu,
        Playing,
        Paused,
        FloorTransition,
        GameOver,
        Victory
    }

    private GameStateType _currentState = GameStateType.MainMenu;

    // --- Progressão ---
    private int _currentFloorIndex = 0;
    private float _elapsedGameTime = 0f; // em segundos
    private int _lastReportedMinute = 0;

    // --- Jogador (referências) ---
    private Employee _player;
    private int _playerLevel = 1;
    private float _playerCoffee = 0f;
    private Godot.Collections.Array<WeaponData> _playerWeapons = new();
    private Godot.Collections.Array<UpgradeData> _playerUpgrades = new();

    // --- Config ---
    private float _totalGameDuration = 1800f; // 30 minutos em segundos
    private float _floorDuration = 360f; // 6 minutos por andar

    // --- Dados dos Andares ---
    private Godot.Collections.Array<FloorData> _floors = new();

    // --- Arma inicial ---
    private WeaponData _defaultWeapon;

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
        InitializeFloors();
        LoadDefaultWeapon();
    }

    public override void _Process(double delta)
    {
        if (_currentState != GameStateType.Playing) return;

        _elapsedGameTime += (float)delta;

        int currentMinute = Mathf.FloorToInt(_elapsedGameTime / 60f);
        if (currentMinute > _lastReportedMinute)
        {
            _lastReportedMinute = currentMinute;
            EmitSignal(SignalName.OnMinutePassed, _lastReportedMinute);
            CheckFloorProgression();
        }

        // Verifica vitória (30 min)
        if (_elapsedGameTime >= _totalGameDuration && _currentState == GameStateType.Playing)
        {
            TriggerVictory();
        }
    }

    // --- Inicialização ---

    private void InitializeFloors()
    {
        _floors.Clear();

        // Carrega todos os .tres da pasta Resources/Floors/
        string floorsDir = "res://Resources/Floors/";
        if (DirAccess.DirExistsAbsolute(floorsDir))
        {
            var dir = DirAccess.Open(floorsDir);
            if (dir != null)
            {
                dir.ListDirBegin();
                string fileName = dir.GetNext();
                while (!string.IsNullOrEmpty(fileName))
                {
                    if (fileName.EndsWith(".tres") || fileName.EndsWith(".res"))
                    {
                        string path = floorsDir + fileName;
                        var floorData = GD.Load<FloorData>(path);
                        if (floorData != null)
                        {
                            _floors.Add(floorData);
                            GD.Print($"GameManager: Andar carregado: {floorData.FloorName}");
                        }
                    }
                    fileName = dir.GetNext();
                }
                dir.ListDirEnd();
            }
        }

        // Ordena por FloorIndex
        var floorList = new System.Collections.Generic.List<FloorData>(_floors);
        floorList.Sort((a, b) => a.FloorIndex.CompareTo(b.FloorIndex));
        _floors = new Godot.Collections.Array<FloorData>(floorList);

        if (_floors.Count == 0)
            GD.PrintErr("GameManager: Nenhum andar carregado de Resources/Floors/!");
        else
            GD.Print($"GameManager: {_floors.Count} andares carregados.");
    }

    /// <summary>
    /// Carrega a arma inicial padrão dos Resources.
    /// </summary>
    private void LoadDefaultWeapon()
    {
        _defaultWeapon = GD.Load<WeaponData>("res://Resources/Weapons/ClipesReforcados.tres");
        if (_defaultWeapon != null)
        {
            GD.Print($"GameManager: Arma inicial carregada: {_defaultWeapon.WeaponName}");
        }
        else
        {
            GD.PrintErr("GameManager: Arma inicial não encontrada!");
        }
    }

    // --- Gerenciamento de Estado ---

    public void StartGame()
    {
        _currentFloorIndex = 0;
        _elapsedGameTime = 0f;
        _lastReportedMinute = 0;
        _playerLevel = 1;
        _playerCoffee = 0f;
        _playerWeapons.Clear();
        _playerUpgrades.Clear();
        _currentState = GameStateType.Playing;
        GD.Print("GameManager: Jogo iniciado!");
    }

    public void PauseGame()
    {
        if (_currentState == GameStateType.Playing)
        {
            _currentState = GameStateType.Paused;
            GetTree().Paused = true;
        }
    }

    public void ResumeGame()
    {
        if (_currentState == GameStateType.Paused)
        {
            _currentState = GameStateType.Playing;
            GetTree().Paused = false;
        }
    }

    public void TriggerGameOver()
    {
        _currentState = GameStateType.GameOver;
        EmitSignal(SignalName.OnGameOver);
        GD.Print("GameManager: Game Over!");
    }

    private void TriggerVictory()
    {
        _currentState = GameStateType.Victory;
        EmitSignal(SignalName.OnVictory);
        GD.Print("GameManager: Vitória! O amanhecer chegou!");
    }

    // --- Progressão ---

    private void CheckFloorProgression()
    {
        int floorTime = Mathf.FloorToInt(_elapsedGameTime / _floorDuration);
        if (floorTime > _currentFloorIndex && _currentFloorIndex < _floors.Count)
        {
            _currentFloorIndex = floorTime;
            EmitSignal(SignalName.OnFloorCleared, _currentFloorIndex);
            GD.Print($"GameManager: Andar {_currentFloorIndex} liberado!");
        }
    }

    public void AddCoffee(float amount)
    {
        if (_currentState != GameStateType.Playing) return;

        _playerCoffee += amount;
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        float needed = GetCoffeeNeededForLevel(_playerLevel);
        while (_playerCoffee >= needed)
        {
            _playerCoffee -= needed;
            _playerLevel++;
            EmitSignal(SignalName.OnLeveledUp, _playerLevel);
            GD.Print($"GameManager: Level up! Nível {_playerLevel}");
            needed = GetCoffeeNeededForLevel(_playerLevel);

            // Pausa para escolha de upgrade
            GetTree().Paused = true;
            // A UI de upgrade deve escutar OnLeveledUp e mostrar as opções
        }
    }

    private float GetCoffeeNeededForLevel(int level)
    {
        return 10f * Mathf.Pow(1.15f, level - 1);
    }

    public void ApplyUpgrade(UpgradeData upgrade)
    {
        _playerUpgrades.Add(upgrade);
        EmitSignal(SignalName.OnUpgradeChosen, upgrade);
        GD.Print($"GameManager: Upgrade aplicado: {upgrade.UpgradeName}");
        GetTree().Paused = false;
    }

    public void AddWeapon(WeaponData weapon)
    {
        _playerWeapons.Add(weapon);
        GD.Print($"GameManager: Nova arma: {weapon.WeaponName}");
    }

    // --- Getters ---

    public GameStateType CurrentState => _currentState;
    public int CurrentFloorIndex => _currentFloorIndex;
    public Godot.Collections.Array<FloorData> Floors => _floors;
    public WeaponData DefaultWeapon => _defaultWeapon;
    public float ElapsedGameTime => _elapsedGameTime;
    public int CurrentMinute => _lastReportedMinute;
    public int PlayerLevel => _playerLevel;
    public float PlayerCoffee => _playerCoffee;
    public Employee Player => _player;
    public Godot.Collections.Array<WeaponData> PlayerWeapons => _playerWeapons;
    public Godot.Collections.Array<UpgradeData> PlayerUpgrades => _playerUpgrades;

    public void RegisterPlayer(Employee player)
    {
        _player = player;
    }

    /// <summary>
    /// Retorna a dificuldade atual (multiplicador) baseada no tempo.
    /// </summary>
    public float GetDifficultyMultiplier()
    {
        return 1f + (_lastReportedMinute * 0.03f); // +3% por minuto
    }
}
