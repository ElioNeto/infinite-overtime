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

    // --- Deferred loading ---
    private bool _resourcesLoaded = false;
    private bool _diagnosticsPrinted = false;

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;

        // Tenta carregar recursos no _Ready. Se falhar (ResourceLoader pode
        // não estar totalmente inicializado para autoloads), faz nova tentativa
        // no primeiro _Process.
        TryLoadResources();
        if (!_resourcesLoaded)
        {
            GD.Print("GameManager: Recursos não carregaram no _Ready — tentando no _Process.");
            CallDeferred(nameof(TryLoadResources));
        }
    }

    private void TryLoadResources()
    {
        if (_resourcesLoaded) return;

        GD.Print("GameManager: Inicializando recursos...");
        InitializeFloors();
        LoadDefaultWeapon();

        PrintResourceDiagnostics();
        _diagnosticsPrinted = true;

        if (_floors.Count > 0)
        {
            _resourcesLoaded = true;
            GD.Print($"GameManager: {_floors.Count} andares carregados.");
        }
        else
        {
            GD.PrintErr("GameManager: Nenhum andar carregado de Resources/Floors/!");
        }
    }

    /// <summary>
    /// Imprime diagnóstico detalhado sobre o carregamento de recursos.
    /// Ajuda a identificar qual arquivo específico está falhando.
    /// </summary>
    private void PrintResourceDiagnostics()
    {
        GD.Print("=== DIAGNÓSTICO DE RECURSOS ===");

        // Testa carregamento de cada EnemyData.tres individualmente
        string[] enemyPaths = {
            "res://Resources/Enemies/EmailNaoLido.tres",
            "res://Resources/Enemies/CafeDerramado.tres",
            "res://Resources/Enemies/PlanilhaMalignificada.tres",
            "res://Resources/Enemies/NotebookSuperaquecido.tres",
            "res://Resources/Enemies/AvaliacaoFantasma.tres",
            "res://Resources/Enemies/AtaDeReuniao.tres"
        };
        foreach (string path in enemyPaths)
        {
            bool exists = ResourceLoader.Exists(path);
            var enemy = GD.Load<EnemyData>(path);
            GD.Print($"  EnemyData {path.GetFile()}: exists={exists}, loaded={(enemy != null)}, scene={(enemy?.Scene != null)}");
        }

        // Testa cada WeaponData
        string[] weaponPaths = {
            "res://Resources/Weapons/ClipesReforcados.tres",
            "res://Resources/Weapons/NotebookSuperaquecido.tres",
            "res://Resources/Weapons/PlanilhaExplosiva.tres",
            "res://Resources/Weapons/CafeEspirrado.tres",
            "res://Resources/Weapons/MensagemPassivoAgressiva.tres",
            "res://Resources/Weapons/ImpressoraFantasma.tres"
        };
        foreach (string path in weaponPaths)
        {
            bool exists = ResourceLoader.Exists(path);
            var weapon = GD.Load<WeaponData>(path);
            GD.Print($"  WeaponData {path.GetFile()}: exists={exists}, loaded={(weapon != null)}, scene={(weapon?.ProjectileScene != null)}");
        }

        // Testa scenes individuais
        string[] scenePaths = {
            "res://Scenes/Enemies/EmailNaoLido.tscn",
            "res://Scenes/Enemies/CafeDerramado.tscn",
            "res://Scenes/Enemies/PlanilhaMalignificada.tscn",
            "res://Scenes/Enemies/NotebookSuperaquecido.tscn",
            "res://Scenes/Enemies/AvaliacaoFantasma.tscn",
            "res://Scenes/Enemies/AtaDeReuniao.tscn",
            "res://Scenes/Weapons/Projectile.tscn",
            "res://Scenes/Pickups/CoffeeDrop.tscn"
        };
        foreach (string path in scenePaths)
        {
            bool exists = ResourceLoader.Exists(path);
            var scene = GD.Load<PackedScene>(path);
            GD.Print($"  Scene {path.GetFile()}: exists={exists}, loaded={(scene != null)}");
        }

        GD.Print("=== FIM DIAGNÓSTICO ===");
    }

    public override void _Process(double delta)
    {
        // Tenta carregar recursos se ainda não foi feito (fallback)
        if (!_resourcesLoaded && Instance != null)
        {
            TryLoadResources();
        }

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

        string floorsDir = "res://Resources/Floors/";
        if (!DirAccess.DirExistsAbsolute(floorsDir))
        {
            GD.PrintErr($"GameManager: Diretório não encontrado: {floorsDir}");
            return;
        }

        var dir = DirAccess.Open(floorsDir);
        if (dir == null)
        {
            GD.PrintErr($"GameManager: Falha ao abrir diretório: {floorsDir}");
            return;
        }

        dir.ListDirBegin();
        string fileName = dir.GetNext();
        int foundCount = 0;
        int loadedCount = 0;

        while (!string.IsNullOrEmpty(fileName))
        {
            if (fileName.EndsWith(".tres") || fileName.EndsWith(".res"))
            {
                foundCount++;
                string path = floorsDir + fileName;
                bool exists = ResourceLoader.Exists(path);
                var floorData = GD.Load<FloorData>(path);

                if (floorData != null)
                {
                    _floors.Add(floorData);
                    loadedCount++;
                    GD.Print($"GameManager: Andar carregado: {floorData.FloorName}");
                }
                else
                {
                    GD.PrintErr($"GameManager: Falha ao carregar andar: {path} (exists={exists})");
                }
            }
            fileName = dir.GetNext();
        }
        dir.ListDirEnd();

        GD.Print($"GameManager: Encontrados {foundCount} arquivos, carregados {loadedCount} andares.");

        // Ordena por FloorIndex
        var floorList = new System.Collections.Generic.List<FloorData>(_floors);
        floorList.Sort((a, b) => a.FloorIndex.CompareTo(b.FloorIndex));
        _floors = new Godot.Collections.Array<FloorData>(floorList);
    }

    /// <summary>
    /// Carrega a arma inicial padrão dos Resources.
    /// </summary>
    private void LoadDefaultWeapon()
    {
        string weaponPath = "res://Resources/Weapons/ClipesReforcados.tres";
        bool exists = ResourceLoader.Exists(weaponPath);
        _defaultWeapon = GD.Load<WeaponData>(weaponPath);

        if (_defaultWeapon != null)
        {
            GD.Print($"GameManager: Arma inicial carregada: {_defaultWeapon.WeaponName}");
        }
        else
        {
            GD.PrintErr($"GameManager: Arma inicial NÃO encontrada! path={weaponPath} exists={exists}");
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
