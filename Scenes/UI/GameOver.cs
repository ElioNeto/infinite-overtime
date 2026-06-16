using Godot;
using System;
using CorporateSurvivors.Systems;

namespace CorporateSurvivors;

/// <summary>
/// Tela de Game Over — "Horário Comercial Excedido"
/// </summary>
public partial class GameOver : CanvasLayer
{
    [Export] public Label TitleLabel { get; set; }
    [Export] public Label StatsLabel { get; set; }
    [Export] public Button RestartButton { get; set; }
    [Export] public Button MainMenuButton { get; set; }

    public override void _Ready()
    {
        TitleLabel ??= GetNode<Label>("TitleLabel");
        StatsLabel ??= GetNode<Label>("StatsLabel");
        RestartButton ??= GetNode<Button>("RestartButton");
        MainMenuButton ??= GetNode<Button>("MainMenuButton");

        // Mostra estatísticas
        if (GameManager.Instance != null)
        {
            int minutes = GameManager.Instance.CurrentMinute;
            int floor = GameManager.Instance.CurrentFloorIndex + 1;
            int level = GameManager.Instance.PlayerLevel;
            StatsLabel.Text = $"Sobreviveu: {minutes} min\nAndar: {floor}° | Nível: {level}";
        }

        RestartButton.Pressed += OnRestartPressed;
        MainMenuButton.Pressed += OnMainMenuPressed;
    }

    private void OnRestartPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/Game/Game.tscn");
    }

    private void OnMainMenuPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
    }
}
