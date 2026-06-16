using Godot;
using System;
using CorporateSurvivors.Data;
using CorporateSurvivors.Systems;

namespace CorporateSurvivors;

/// <summary>
/// Classe base para todos os inimigos corporativos.
/// Cada tipo de inimigo (Email, Planilha, etc.) estende esta classe.
/// </summary>
public partial class BaseEnemy : CharacterBody2D
{
    // --- Exports ---
    // (definido programaticamente pelo WaveSpawner)
    public EnemyData EnemyData { get; set; }

    [ExportGroup("Nodes")]
    [Export] public Sprite2D Sprite { get; set; }
    [Export] public AnimationPlayer AnimationPlayer { get; set; }
    [Export] public Area2D Hitbox { get; set; }
    [Export] public Area2D DamageArea { get; set; }  // Área que causa dano ao jogador
    [Export] public CollisionShape2D CollisionShape { get; set; }
    [Export] public GpuParticles2D DeathParticles { get; set; }

    // --- Privado ---
    private float _currentHealth;
    private Employee _player;
    private float _damageCooldownTimer = 0f;
    private bool _isDead = false;
    private Vector2 _spawnPosition;
    private float _aliveTime = 0f;

    // --- Sinais ---
    [Signal]
    public delegate void OnEnemyDiedEventHandler(Vector2 position, float coffeeValue);

    [Signal]
    public delegate void OnEnemyDamagedEventHandler(float health, float maxHealth);

    public override void _Ready()
    {
        if (EnemyData == null)
        {
            GD.PrintErr("BaseEnemy: EnemyData não atribuído!");
            QueueFree();
            return;
        }

        _currentHealth = EnemyData.MaxHealth;
        _spawnPosition = GlobalPosition;

        // Encontra o jogador
        _player = GetTree().GetFirstNodeInGroup("player") as Employee;
        if (_player == null)
        {
            // Tenta pelo GameManager
            _player = GameManager.Instance?.Player;
        }

        // Conecta sinais
        if (DamageArea != null)
        {
            DamageArea.BodyEntered += OnDamageAreaEntered;
            DamageArea.BodyExited += OnDamageAreaExited;
        }

        AddToGroup("enemies");

        GD.Print($"BaseEnemy: {EnemyData.EnemyName} spawnado! HP: {_currentHealth}");
    }

    public override void _Process(double delta)
    {
        if (_isDead) return;
        if (GameManager.Instance?.CurrentState != GameManager.GameStateType.Playing) return;

        float dt = (float)delta;
        _aliveTime += dt;
        _damageCooldownTimer = Mathf.Max(0f, _damageCooldownTimer - dt);

        switch (EnemyData.BehaviorType)
        {
            case EnemyBehaviorType.Chase:
                BehaviorChase(dt);
                break;
            case EnemyBehaviorType.Patrol:
                BehaviorPatrol(dt);
                break;
            case EnemyBehaviorType.Teleport:
                BehaviorTeleport(dt);
                break;
            case EnemyBehaviorType.Stationary:
                BehaviorStationary(dt);
                break;
            case EnemyBehaviorType.Swarm:
                BehaviorChase(dt); // Swarm usa chase mas é mais rápido e frágil
                break;
            case EnemyBehaviorType.Trap:
                BehaviorTrap(dt);
                break;
            case EnemyBehaviorType.Spawner:
                BehaviorSpawner(dt);
                break;
        }

        MoveAndSlide();
    }

    // --- Comportamentos ---

    private void BehaviorChase(float delta)
    {
        if (_player == null) return;

        float distance = GlobalPosition.DistanceTo(_player.GlobalPosition);
        if (distance > EnemyData.DetectionRange) return;

        Vector2 direction = (_player.GlobalPosition - GlobalPosition).Normalized();
        Velocity = direction * EnemyData.MoveSpeed;

        // Aplica dificuldade
        Velocity *= GameManager.Instance?.GetDifficultyMultiplier() ?? 1f;

        // Flip sprite
        if (Sprite != null)
            Sprite.FlipH = direction.X < 0;
    }

    private void BehaviorPatrol(float delta)
    {
        // Patrulha em linha reta, inverte ao bater na parede
        if (IsOnWall())
        {
            Velocity = -Velocity;
        }

        if (Velocity == Vector2.Zero)
        {
            Velocity = Vector2.Right * EnemyData.MoveSpeed;
        }
    }

    private float _teleportTimer = 0f;
    private void BehaviorTeleport(float delta)
    {
        _teleportTimer -= delta;
        if (_teleportTimer <= 0f && _player != null)
        {
            // Teleporta para perto do jogador
            Vector2 targetPos = _player.GlobalPosition + new Vector2(
                GD.Randf() * 100f - 50f,
                GD.Randf() * 100f - 50f
            );
            GlobalPosition = targetPos;
            _teleportTimer = 2f + GD.Randf() * 1f;

            // Efeito visual
            if (Sprite != null)
            {
                Sprite.Modulate = new Color(1f, 1f, 1f, 0.3f);
                var tween = CreateTween();
                tween.TweenProperty(Sprite, "modulate", Colors.White, 0.3f);
            }
        }
    }

    private void BehaviorStationary(float delta)
    {
        Velocity = Vector2.Zero;
        // Atira projéteis no jogador (a ser implementado por subclasse)
    }

    private void BehaviorTrap(float delta)
    {
        Velocity = Vector2.Zero;
        // Cria poças de lentidão (a ser implementado por subclasse)
    }

    private void BehaviorSpawner(float delta)
    {
        Velocity = Vector2.Zero;
        // Spawna inimigos menores (a ser implementado por subclasse)
    }

    // --- Dano ---

    public virtual void TakeDamage(float amount)
    {
        if (_isDead) return;

        _currentHealth -= amount;
        EmitSignal(SignalName.OnEnemyDamaged, _currentHealth, EnemyData.MaxHealth);

        // Feedback visual
        if (Sprite != null)
        {
            Sprite.Modulate = new Color(1f, 0.5f, 0.5f);
            var tween = CreateTween();
            tween.TweenProperty(Sprite, "modulate", Colors.White, 0.1f);
        }

        if (_currentHealth <= 0f)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (_isDead) return;
        _isDead = true;

        // Spawna café
        if (EnemyData.CoffeeDrop > 0)
        {
            var coffeeScene = GD.Load<PackedScene>("res://Scenes/Pickups/CoffeeDrop.tscn");
            if (coffeeScene != null)
            {
                var coffee = coffeeScene.Instantiate<CoffeeDrop>();
                coffee.GlobalPosition = GlobalPosition;
                coffee.Amount = EnemyData.CoffeeDrop;
                GetParent().AddChild(coffee);
            }
        }

        // Partículas de morte
        if (DeathParticles != null)
        {
            var particles = DeathParticles.Duplicate() as GpuParticles2D;
            if (particles != null)
            {
                particles.GlobalPosition = GlobalPosition;
                GetParent().AddChild(particles);
                particles.Emitting = true;
            }
        }

        EmitSignal(SignalName.OnEnemyDied, GlobalPosition, EnemyData.CoffeeDrop);
        GD.Print($"BaseEnemy: {EnemyData.EnemyName} derrotado!");

        QueueFree();
    }

    // --- Colisão com Jogador ---

    private void OnDamageAreaEntered(Node2D body)
    {
        if (body is Employee employee && _damageCooldownTimer <= 0f)
        {
            employee.TakeDamage(EnemyData.Damage);
            _damageCooldownTimer = EnemyData.DamageCooldown;
        }
    }

    private void OnDamageAreaExited(Node2D body)
    {
        // Placeholder para efeitos de saída
    }

    public void OnHitboxEntered(Node2D body)
    {
        // Projéteis chamam este método ao colidir
        if (body is Projectile projectile)
        {
            TakeDamage(projectile.Damage);
            if (!projectile.WeaponData.IsPiercing)
            {
                projectile.Hit();
            }
        }
    }

    // --- Utilitários ---

    public float HealthPercent => _currentHealth / EnemyData.MaxHealth;
    public float AliveTime => _aliveTime;
}
