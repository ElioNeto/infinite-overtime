using Godot;
using System;
using CorporateSurvivors.Systems;

namespace CorporateSurvivors;

/// <summary>
/// Tela de Vitória — "O Amanhecer Chegou"
/// </summary>
public partial class Victory : CanvasLayer
{
    [Export] public Label StatsLabel { get; set; }
    [Export] public Button MainMenuButton { get; set; }

    public override void _Ready()
    {
        StatsLabel ??= GetNode<Label>("%StatsLabel");
        MainMenuButton ??= GetNode<Button>("%MainMenuButton");

        // Mostra estatísticas
        if (GameManager.Instance != null)
        {
            int minutes = GameManager.Instance.CurrentMinute;
            int level = GameManager.Instance.PlayerLevel;
            int floors = GameManager.Instance.CurrentFloorIndex + 1;
            StatsLabel.Text = $"Sobreviveu: {minutes} min\nAndares liberados: {floors}\nNível final: {level}";
        }

        MainMenuButton.Pressed += () =>
        {
            GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
        };
    }
}
