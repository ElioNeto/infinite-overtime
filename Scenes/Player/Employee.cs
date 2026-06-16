using Godot;
using System;
using System.Collections.Generic;
using CorporateSurvivors.Data;
using CorporateSurvivors.Systems;

namespace CorporateSurvivors;

/// <summary>
/// Funcionário protagonista — o jogador.
/// Controla movimento, vida, coleta de café e interação com upgrades.
/// </summary>
public partial class Employee : CharacterBody2D
{
    // --- Exports ---
    [ExportGroup("Atributos")]
    [Export] public float MoveSpeed { get; set; } = 200f;
    [Export] public float MaxHealth { get; set; } = 100f;
    [Export] public float HealthRegenPerSecond { get; set; } = 0f;
    [Export] public float DamageReduction { get; set; } = 0f; // 0.0 a 0.9

    [ExportGroup("Café (XP)")]
    [Export] public float MagnetRadius { get; set; } = 100f;
    [Export] public float CoffeeGainMultiplier { get; set; } = 1f;

    [ExportGroup("Nodes")]
    [Export] public Area2D MagnetArea { get; set; }
    [Export] public Sprite2D Sprite { get; set; }
    [Export] public AnimationPlayer AnimationPlayer { get; set; }

    // --- Privado ---
    private float _currentHealth;
    private Vector2 _moveDirection;
    private float _damageCooldown = 0f; // Tempo desde o último dano recebido
    private bool _isInvulnerable = false;
    private float _invulnerabilityTimer = 0f;

    // --- Sinais ---
    [Signal]
    public delegate void OnDamageTakenEventHandler(float currentHealth, float maxHealth);

    [Signal]
    public delegate void OnHealedEventHandler(float amount, float currentHealth);

    [Signal]
    public delegate void OnDiedEventHandler();

    public override void _Ready()
    {
        _currentHealth = MaxHealth;

        // Auto-conecta nós filhos caso não tenham sido atribuídos no Inspector
        MagnetArea ??= GetNode<Area2D>("MagnetArea");
        Sprite ??= GetNode<Sprite2D>("Sprite2D");
        AnimationPlayer ??= GetNode<AnimationPlayer>("AnimationPlayer");

        if (MagnetArea != null)
        {
            // Atualiza o raio do shape magnético (pode ter sido alterado por upgrades)
            UpdateMagnetShape();
            MagnetArea.BodyEntered += OnMagnetAreaEntered;
        }
        else
        {
            GD.PrintErr("Employee: MagnetArea não encontrado — coleta magnética desativada!");
        }

        // Registra no GameManager
        GameManager.Instance?.RegisterPlayer(this);

        GD.Print($"Employee: Pronto! HP: {_currentHealth}/{MaxHealth}, Speed: {MoveSpeed}");
    }

    public override void _Process(double delta)
    {
        if (GameManager.Instance?.CurrentState != GameManager.GameStateType.Playing)
            return;

        float dt = (float)delta;

        HandleMovement();
        HandleRegeneration(dt);
        HandleInvulnerability(dt);

        MoveAndSlide();
    }

    // --- Movimento ---

    private void HandleMovement()
    {
        _moveDirection = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        Velocity = _moveDirection * MoveSpeed;

        // Animações (placeholder — só toca se a animação existir)
        if (AnimationPlayer != null)
        {
            string anim = _moveDirection != Vector2.Zero ? "walk" : "idle";
            if (AnimationPlayer.HasAnimation(anim))
                AnimationPlayer.Play(anim);
        }

        // Flip sprite baseado na direção
        if (Sprite != null && _moveDirection.X != 0)
        {
            Sprite.FlipH = _moveDirection.X < 0;
        }
    }

    // --- Vida ---

    private void HandleRegeneration(float delta)
    {
        if (HealthRegenPerSecond > 0 && _currentHealth < MaxHealth)
        {
            float healAmount = HealthRegenPerSecond * delta;
            _currentHealth = Mathf.Min(MaxHealth, _currentHealth + healAmount);
        }
    }

    private void HandleInvulnerability(float delta)
    {
        if (!_isInvulnerable) return;

        _invulnerabilityTimer -= delta;
        if (_invulnerabilityTimer <= 0f)
        {
            _isInvulnerable = false;
            // Piscar sprite para indicar fim da invulnerabilidade
            if (Sprite != null)
                Sprite.Modulate = Colors.White;
        }
    }

    /// <summary>
    /// Aplica dano ao funcionário. Respeita cooldown de dano e redução.
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (_isInvulnerable || GameManager.Instance?.CurrentState != GameManager.GameStateType.Playing)
            return;

        float reducedDamage = amount * (1f - DamageReduction);
        _currentHealth = Mathf.Max(0f, _currentHealth - reducedDamage);

        // Feedback visual: piscar vermelho
        if (Sprite != null)
        {
            Sprite.Modulate = new Color(1f, 0.3f, 0.3f);
        }

        // Invulnerabilidade temporária (iframe)
        _isInvulnerable = true;
        _invulnerabilityTimer = 0.3f;

        EmitSignal(SignalName.OnDamageTaken, _currentHealth, MaxHealth);

        GD.Print($"Employee: Tomou {reducedDamage:F1} de dano! HP: {_currentHealth:F1}/{MaxHealth}");

        if (_currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        float oldHealth = _currentHealth;
        _currentHealth = Mathf.Min(MaxHealth, _currentHealth + amount);
        EmitSignal(SignalName.OnHealed, _currentHealth - oldHealth, _currentHealth);
    }

    private void Die()
    {
        GD.Print("Employee: Morreu!");
        EmitSignal(SignalName.OnDied);
        GameManager.Instance?.TriggerGameOver();
        QueueFree();
    }

    // --- Coleta de Café (XP) ---

    private void OnMagnetAreaEntered(Node2D body)
    {
        if (body is CoffeeDrop coffee)
        {
            float amount = coffee.Collect();
            GameManager.Instance?.AddCoffee(amount * CoffeeGainMultiplier);
        }
    }

    /// <summary>
    /// Aumenta o raio do ímã de café. Chamado por upgrades.
    /// </summary>
    public void ExpandMagnetRadius(float additionalRadius)
    {
        MagnetRadius += additionalRadius;
        UpdateMagnetShape();
    }

    /// <summary>
    /// Atualiza o CollisionShape2D do MagnetArea com o raio atual.
    /// </summary>
    private void UpdateMagnetShape()
    {
        if (MagnetArea == null) return;

        foreach (var child in MagnetArea.GetChildren())
        {
            if (child is CollisionShape2D shape && shape.Shape is CircleShape2D circle)
            {
                circle.Radius = MagnetRadius;
                return;
            }
        }
    }

    // --- Getters Públicos ---

    public float CurrentHealth => _currentHealth;
    public float HealthPercent => _currentHealth / MaxHealth;
    public bool IsInvulnerable => _isInvulnerable;
}
