using Godot;
using System;
using CorporateSurvivors.Systems;

namespace CorporateSurvivors;

/// <summary>
/// Tela inicial do jogo.
/// </summary>
public partial class MainMenu : CanvasLayer
{
    [Export] public Button StartButton { get; set; }
    [Export] public Button QuitButton { get; set; }

    public override void _Ready()
    {
        StartButton ??= GetNode<Button>("%StartButton");
        QuitButton ??= GetNode<Button>("%QuitButton");

        StartButton.Pressed += OnStartPressed;
        QuitButton.Pressed += OnQuitPressed;
    }

    private void OnStartPressed()
    {
        // Inicia o jogo
        GetTree().ChangeSceneToFile("res://Scenes/Game/Game.tscn");
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }
}
