using Godot;
using System;
using CorporateSurvivors.Data;

namespace CorporateSurvivors;

/// <summary>
/// Projétil base para todas as armas corporativas.
/// Pode ser um clipe, uma planilha, uma mensagem, etc.
/// </summary>
public partial class Projectile : Area2D
{
    // (definido programaticamente pelo Weapon2D)
    public WeaponData WeaponData { get; set; }

    [ExportGroup("Atributos")]
    [Export] public float Damage { get; set; } = 10f;
    [Export] public float Speed { get; set; } = 500f;
    [Export] public float Lifetime { get; set; } = 3f; // Segundos até desaparecer
    [Export] public bool IsPiercing { get; set; } = false;

    [ExportGroup("Visual")]
    [Export] public Sprite2D Sprite { get; set; }
    [Export] public GpuParticles2D TrailParticles { get; set; }

    private Vector2 _direction;
    private float _lifeTimer = 0f;
    private Godot.Collections.Array<Node2D> _hitTargets = new();

    [Signal]
    public delegate void OnProjectileExpiredEventHandler();

    /// <summary>
    /// Inicializa o projétil com direção e dados da arma.
    /// </summary>
    public void Initialize(Vector2 direction, WeaponData data)
    {
        _direction = direction.Normalized();
        WeaponData = data;
        Damage = data.Damage;
        Speed = data.ProjectileSpeed;
        IsPiercing = data.IsPiercing;
        Lifetime = data.Range / data.ProjectileSpeed;

        // Aplica cor do projétil
        if (Sprite != null)
            Sprite.SelfModulate = data.ProjectileColor;

        Rotation = direction.Angle();

        // Conecta sinal de colisão
        BodyEntered += OnBodyEntered;
        AreaEntered += OnAreaEntered;

        AddToGroup("projectiles");
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        _lifeTimer += dt;

        // Movimento
        Position += _direction * Speed * dt;

        // Desaparece após tempo de vida
        if (_lifeTimer >= Lifetime)
        {
            Expire();
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (_hitTargets.Contains(body)) return;

        if (body is BaseEnemy enemy)
        {
            enemy.TakeDamage(Damage);
            _hitTargets.Add(body);

            if (!IsPiercing)
            {
                Hit();
            }
        }
    }

    private void OnAreaEntered(Area2D area)
    {
        // Para colisões com hitboxes de inimigos (se usarem Area2D)
        if (area.GetParent() is BaseEnemy enemy)
        {
            if (_hitTargets.Contains(enemy)) return;
            enemy.TakeDamage(Damage);
            _hitTargets.Add(enemy);

            if (!IsPiercing)
            {
                Hit();
            }
        }
    }

    /// <summary>
    /// Chamado quando o projétil acerta algo (ou expira).
    /// </summary>
    public void Hit()
    {
        // Efeito de impacto
        if (TrailParticles != null)
        {
            TrailParticles.Emitting = false;
        }

        // Efeito de explosão (se for arma de área)
        if (WeaponData?.EffectType == WeaponEffectType.Explosion)
        {
            Explode();
        }

        QueueFree();
    }

    private void Explode()
    {
        // Cria uma área de explosão temporária
        var explosionArea = new Area2D();
        var collisionShape = new CollisionShape2D();
        var circle = new CircleShape2D();
        circle.Radius = WeaponData?.Range * 0.3f ?? 50f;
        collisionShape.Shape = circle;
        explosionArea.AddChild(collisionShape);
        explosionArea.GlobalPosition = GlobalPosition;

        GetParent().AddChild(explosionArea);

        // Aplica dano a todos inimigos na área
        explosionArea.BodyEntered += (body) =>
        {
            if (body is BaseEnemy enemy)
            {
                enemy.TakeDamage(Damage * 0.5f); // 50% do dano em área
            }
        };

        // Remove após 0.1s
        var timer = GetTree().CreateTimer(0.1f);
        timer.Timeout += () => explosionArea.QueueFree();
    }

    private void Expire()
    {
        EmitSignal(SignalName.OnProjectileExpired);
        QueueFree();
    }
}
