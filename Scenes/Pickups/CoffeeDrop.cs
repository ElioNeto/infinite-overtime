using Godot;
using System;
using CorporateSurvivors.Systems;

namespace CorporateSurvivors;

/// <summary>
/// Café — a moeda de experiência do jogo.
/// Inimigos derrotados dropam café que o jogador coleta automaticamente.
/// </summary>
public partial class CoffeeDrop : Area2D
{
    [ExportGroup("Atributos")]
    [Export] public float Amount { get; set; } = 5f;
    [Export] public float AttractSpeed { get; set; } = 400f;
    [Export] public float Lifetime { get; set; } = 15f; // Segundos até desaparecer

    [ExportGroup("Visual")]
    [Export] public Sprite2D Sprite { get; set; }
    [Export] public AnimationPlayer AnimationPlayer { get; set; }
    [Export] public GpuParticles2D FloatParticles { get; set; }

    private float _lifeTimer = 0f;
    private bool _isBeingAttracted = false;
    private Employee _targetPlayer = null;
    private bool _collected = false;

    public override void _Ready()
    {
        // Animação de flutuação
        if (AnimationPlayer != null)
        {
            AnimationPlayer.Play("float");
        }

        // Conexão para coleta pelo magnet do player
        BodyEntered += OnBodyEntered;

        AddToGroup("coffee_drops");
    }

    public override void _Process(double delta)
    {
        if (_collected) return;

        float dt = (float)delta;
        _lifeTimer += dt;

        // Atração magnética pelo jogador
        if (!_isBeingAttracted && GameManager.Instance?.Player != null)
        {
            float dist = GlobalPosition.DistanceTo(GameManager.Instance.Player.GlobalPosition);
            float magnetRadius = GameManager.Instance.Player.MagnetRadius;

            if (dist <= magnetRadius)
            {
                _isBeingAttracted = true;
                _targetPlayer = GameManager.Instance.Player;
            }
        }

        // Movimento de atração
        if (_isBeingAttracted && _targetPlayer != null)
        {
            Vector2 direction = (_targetPlayer.GlobalPosition - GlobalPosition).Normalized();
            GlobalPosition += direction * AttractSpeed * dt;

            // Verifica se chegou no player
            if (GlobalPosition.DistanceTo(_targetPlayer.GlobalPosition) < 10f)
            {
                Collect();
            }
        }

        // Desaparece após tempo de vida
        if (_lifeTimer >= Lifetime)
        {
            FadeOut();
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        // Coleta imediata se o player encostar
        if (body is Employee employee && !_collected)
        {
            if (employee == _targetPlayer || _targetPlayer == null)
            {
                Collect();
            }
        }
    }

    /// <summary>
    /// Coleta o café e retorna a quantidade para o GameManager adicionar.
    /// </summary>
    public float Collect()
    {
        if (_collected) return 0f;
        _collected = true;

        if (AnimationPlayer != null)
        {
            AnimationPlayer.Play("collect");
        }

        // Efeito sonoro
        AudioManager.Instance?.PlaySFXAtPosition(
            GD.Load<AudioStream>("res://Audio/SFX/coffee_pickup.wav"),
            GlobalPosition
        );

        // Pequeno delay antes de sumir pra tocar animação
        var timer = GetTree().CreateTimer(0.2f);
        timer.Timeout += () => QueueFree();

        return Amount;
    }

    private void FadeOut()
    {
        if (_collected) return;
        _collected = true;

        // Fade out visual
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 0), 0.5f);
        tween.TweenCallback(Callable.From(QueueFree));
    }
}
