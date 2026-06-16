using Godot;
using System;
using CorporateSurvivors.Systems;

namespace CorporateSurvivors;

/// <summary>
/// HUD do jogo — exibe vida, tempo, nível, armas e café.
/// </summary>
public partial class HUD : CanvasLayer
{
    [ExportGroup("Labels")]
    [Export] public Label TimerLabel { get; set; }
    [Export] public Label FloorLabel { get; set; }
    [Export] public Label LevelLabel { get; set; }
    [Export] public Label CoffeeLabel { get; set; }
    [Export] public Label HealthLabel { get; set; }

    [ExportGroup("Bars")]
    [Export] public ProgressBar HealthBar { get; set; }
    [Export] public ProgressBar CoffeeBar { get; set; }

    [ExportGroup("Upgrade Panel")]
    [Export] public Control UpgradePanel { get; set; }
    [Export] public Button UpgradeOption1 { get; set; }
    [Export] public Button UpgradeOption2 { get; set; }
    [Export] public Button UpgradeOption3 { get; set; }

    public override void _Ready()
    {
        // Auto-conecta nós filhos por nome (fallback caso não atribuídos no Inspector)
        TimerLabel ??= GetNode<Label>("TimerLabel");
        FloorLabel ??= GetNode<Label>("FloorLabel");
        LevelLabel ??= GetNode<Label>("LevelLabel");
        CoffeeLabel ??= GetNode<Label>("CoffeeLabel");
        HealthLabel ??= GetNode<Label>("HealthLabel");
        HealthBar ??= GetNode<ProgressBar>("HealthBar");
        CoffeeBar ??= GetNode<ProgressBar>("CoffeeBar");
        UpgradePanel ??= GetNode<Control>("UpgradePanel");
        UpgradeOption1 ??= GetNode<Button>("UpgradePanel/UpgradeOption1");
        UpgradeOption2 ??= GetNode<Button>("UpgradePanel/UpgradeOption2");
        UpgradeOption3 ??= GetNode<Button>("UpgradePanel/UpgradeOption3");

        // Conecta eventos do GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMinutePassed += UpdateTimer;
            GameManager.Instance.OnFloorCleared += OnFloorChanged;

            // Mostra upgrade panel quando sobe de nível
            GameManager.Instance.OnLeveledUp += ShowUpgradePanel;
        }

        // Conecta botões de upgrade (uma vez, evita acúmulo)
        if (UpgradeOption1 != null) UpgradeOption1.Pressed += () => OnUpgradeButtonPressed(0);
        if (UpgradeOption2 != null) UpgradeOption2.Pressed += () => OnUpgradeButtonPressed(1);
        if (UpgradeOption3 != null) UpgradeOption3.Pressed += () => OnUpgradeButtonPressed(2);

        // Esconde upgrade panel inicialmente
        if (UpgradePanel != null)
            UpgradePanel.Visible = false;

        // Inicializa labels
        UpdateTimer(0);
        UpdateLevel(1);
        UpdateHealth(100, 100);
        UpdateCoffee(0);
    }

    private void ShowUpgradePanel(int newLevel)
    {
        if (UpgradePanel == null) return;

        var upgradeSystem = GetNodeOrNull<UpgradeSystem>("../UpgradeSystem");
        if (upgradeSystem == null)
            upgradeSystem = GetNodeOrNull<UpgradeSystem>("/root/Game/UpgradeSystem");

        UpgradePanel.Visible = true;
        var choices = upgradeSystem?.GetUpgradeChoices(newLevel);
        SetupUpgradeChoices(choices);
    }

    public override void _Process(double delta)
    {
        // Atualiza vida e café em tempo real
        var player = GameManager.Instance?.Player;
        if (player != null)
        {
            UpdateHealth(player.CurrentHealth, player.MaxHealth);
            UpdateCoffee(GameManager.Instance.PlayerCoffee);
            UpdateLevel(GameManager.Instance.PlayerLevel);
        }
    }

    private void UpdateTimer(int minute)
    {
        if (TimerLabel != null)
        {
            int totalSeconds = Mathf.FloorToInt(GameManager.Instance?.ElapsedGameTime ?? 0);
            int mins = totalSeconds / 60;
            int secs = totalSeconds % 60;
            TimerLabel.Text = $"{mins:D2}:{secs:D2}";
        }
    }

    private void UpdateLevel(int level)
    {
        if (LevelLabel != null)
            LevelLabel.Text = $"Nível {level}";
    }

    private void UpdateHealth(float current, float max)
    {
        if (HealthLabel != null)
            HealthLabel.Text = $"HP: {current:F0}/{max:F0}";

        if (HealthBar != null)
            HealthBar.Value = max > 0 ? (current / max) * 100.0 : 0;
    }

    private void UpdateCoffee(float coffee)
    {
        if (CoffeeLabel != null)
            CoffeeLabel.Text = $"Café: {coffee:F1}";

        if (CoffeeBar != null)
        {
            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                float needed = 10f * Mathf.Pow(1.15f, gameManager.PlayerLevel - 1);
                CoffeeBar.Value = needed > 0 ? (coffee / needed) * 100.0 : 0;
            }
        }
    }

    private void OnFloorChanged(int floorIndex)
    {
        if (FloorLabel != null)
        {
            string[] floorNames = {
                "Térreo — Recepção",
                "2° Andar — Financeiro",
                "3° Andar — RH",
                "4° Andar — TI",
                "5° Andar — Reuniões",
                "6° Andar — Diretoria"
            };

            if (floorIndex >= 0 && floorIndex < floorNames.Length)
                FloorLabel.Text = floorNames[floorIndex];
            else
                FloorLabel.Text = $"Andar {floorIndex + 1}";
        }
    }

    // OnLeveledUp é tratado por ShowUpgradePanel

    // Armazena as escolhas atuais para evitar reconexão de sinais
    private Godot.Collections.Array<Data.UpgradeData> _currentChoices = new();

    private void SetupUpgradeChoices(Godot.Collections.Array<Data.UpgradeData> choices)
    {
        _currentChoices = choices;
        var buttons = new[] { UpgradeOption1, UpgradeOption2, UpgradeOption3 };
        var upgradeSystem = GetNodeOrNull<UpgradeSystem>("../UpgradeSystem");

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;

            if (i < _currentChoices.Count)
            {
                var upgrade = _currentChoices[i];
                buttons[i].Visible = true;
                buttons[i].Text = $"{upgrade.Emoji} {upgrade.UpgradeName}\n{upgrade.Description}";
                buttons[i].Disabled = false;
            }
            else
            {
                buttons[i].Visible = false;
                buttons[i].Disabled = true;
            }
        }
    }

    /// <summary>
    /// Chamado pelos botões de upgrade (conectado no Inspector ou via código).
    /// Cada botão chama este método com seu índice como argumento.
    /// </summary>
    private void OnUpgradeButtonPressed(int index)
    {
        var upgradeSystem = GetNodeOrNull<UpgradeSystem>("../UpgradeSystem");
        if (upgradeSystem == null)
            upgradeSystem = GetNodeOrNull<UpgradeSystem>("/root/Game/UpgradeSystem");

        if (index >= 0 && index < _currentChoices.Count)
        {
            upgradeSystem?.ApplyUpgrade(_currentChoices[index]);
        }

        if (UpgradePanel != null)
            UpgradePanel.Visible = false;
    }
}
