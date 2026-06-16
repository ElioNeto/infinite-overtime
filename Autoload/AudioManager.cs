using Godot;
using System;

namespace CorporateSurvivors.Systems;

/// <summary>
/// Gerenciador de áudio global (Autoload).
/// Controla música ambiente por andar e efeitos sonoros.
/// </summary>
public partial class AudioManager : Node
{
    public static AudioManager Instance { get; private set; }

    [Export] public AudioStreamPlayer MusicPlayer { get; set; }
    [Export] public AudioStreamPlayer SFXPlayer { get; set; }

    private string _currentMusicTrack = "";

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;

        MusicPlayer = new AudioStreamPlayer();
        SFXPlayer = new AudioStreamPlayer();
        AddChild(MusicPlayer);
        AddChild(SFXPlayer);
    }

    public void PlayMusic(AudioStream stream, float fadeTime = 1.0f)
    {
        if (MusicPlayer.Stream == stream) return;

        MusicPlayer.Stream = stream;
        MusicPlayer.Play();
    }

    public void PlaySFX(AudioStream stream)
    {
        SFXPlayer.Stream = stream;
        SFXPlayer.Play();
    }

    public void PlaySFXAtPosition(AudioStream stream, Vector2 position)
    {
        // Para efeitos 3D/posicionais, usar AudioStreamPlayer2D
        var player2D = new AudioStreamPlayer2D();
        player2D.Stream = stream;
        player2D.GlobalPosition = position;
        AddChild(player2D);
        player2D.Play();

        // Limpa após terminar
        player2D.Finished += () => player2D.QueueFree();
    }

    public void SetMusicVolume(float db)
    {
        MusicPlayer.VolumeDb = db;
    }

    public void SetSFXVolume(float db)
    {
        SFXPlayer.VolumeDb = db;
    }

    public void StopMusic(float fadeTime = 0.5f)
    {
        MusicPlayer.Stop();
    }
}
