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
        // Conecta eventos do GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMinutePassed += UpdateTimer;
            GameManager.Instance.OnLeveledUp += OnLeveledUp;
            GameManager.Instance.OnFloorCleared += OnFloorChanged;

            // UpgradeSystem disponível na cena Game
            var upgradeSystem = GetNodeOrNull<UpgradeSystem>("../UpgradeSystem");
            if (upgradeSystem == null)
                upgradeSystem = GetNodeOrNull<UpgradeSystem>("/root/Game/UpgradeSystem");

            // Mostra upgrade panel quando sobe de nível
            GameManager.Instance.OnLeveledUp += (level) =>
            {
                if (UpgradePanel != null)
                {
                    UpgradePanel.Visible = true;
                    var choices = upgradeSystem?.GetUpgradeChoices(level);
                    SetupUpgradeChoices(choices);
                }
            };
        }

        // Esconde upgrade panel inicialmente
        if (UpgradePanel != null)
            UpgradePanel.Visible = false;

        // Inicializa labels
        UpdateTimer(0);
        UpdateLevel(1);
        UpdateHealth(100, 100);
        UpdateCoffee(0);
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

    private void OnLeveledUp(int level)
    {
        // Upgrade panel é exibido pelo evento conectado no _Ready
    }

    private void SetupUpgradeChoices(Godot.Collections.Array<Data.UpgradeData> choices)
    {
        var buttons = new[] { UpgradeOption1, UpgradeOption2, UpgradeOption3 };
        var upgradeSystem = GetNodeOrNull<UpgradeSystem>("../UpgradeSystem");

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;

            if (i < choices.Count)
            {
                var upgrade = choices[i];
                buttons[i].Visible = true;
                buttons[i].Text = $"{upgrade.Emoji} {upgrade.UpgradeName}\n{upgrade.Description}";

                // Remove handlers antigos
                buttons[i].Pressed -= () => { };
                int capturedIndex = i;
                buttons[i].Pressed += () =>
                {
                    upgradeSystem?.ApplyUpgrade(choices[capturedIndex]);
                    if (UpgradePanel != null)
                        UpgradePanel.Visible = false;
                };
            }
            else
            {
                buttons[i].Visible = false;
            }
        }
    }
}
