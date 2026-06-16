using Godot;
using System;
using CorporateSurvivors.Data;
using CorporateSurvivors.Systems;

namespace CorporateSurvivors;

/// <summary>
/// Arma automática que fica acoplada ao jogador.
/// Dispara projéteis periodicamente na direção do inimigo mais próximo dentro do alcance.
/// </summary>
[GlobalClass]
public partial class Weapon2D : Node2D
{
    [ExportGroup("Dados")]
    [Export] public WeaponData WeaponData { get; set; }

    [ExportGroup("Config")]
    [Export] public float DamageMultiplier { get; set; } = 1f;
    [Export] public float AttackSpeedMultiplier { get; set; } = 1f;
    [Export] public float RangeMultiplier { get; set; } = 1f;
    [Export] public int ExtraProjectiles { get; set; } = 0;

    /// <summary>
    /// Nível atual da arma (1 = base).
    /// </summary>
    public int WeaponLevel { get; set; } = 1;

    private Timer _attackTimer;
    private Employee _owner;

    public override void _Ready()
    {
        _owner = GetParent() as Employee;

        _attackTimer = new Timer();
        _attackTimer.OneShot = false;
        _attackTimer.Autostart = true;
        AddChild(_attackTimer);
        _attackTimer.Timeout += OnAttackTimerTimeout;

        UpdateTimerInterval();
    }

    /// <summary>
    /// Atualiza o intervalo do timer com base no AttackSpeed da arma e multiplicadores.
    /// </summary>
    private void UpdateTimerInterval()
    {
        if (WeaponData == null) return;
        float baseInterval = 1f / WeaponData.GetAttackSpeedForLevel(WeaponLevel);
        _attackTimer.WaitTime = baseInterval / AttackSpeedMultiplier;
    }

    /// <summary>
    /// Chamado a cada tick do timer de ataque.
    /// Encontra o inimigo mais próximo e dispara projéteis.
    /// </summary>
    private void OnAttackTimerTimeout()
    {
        if (_owner == null) return;
        if (WeaponData?.ProjectileScene == null) return;
        if (GameManager.Instance?.CurrentState != GameManager.GameStateType.Playing) return;

        Node2D target = FindNearestEnemy();
        if (target == null) return;

        int totalProjectiles = WeaponData.ProjectileCount + ExtraProjectiles;
        float baseAngle = GlobalPosition.AngleToPoint(target.GlobalPosition);
        float totalSpread = WeaponData.SpreadAngle;

        for (int i = 0; i < totalProjectiles; i++)
        {
            float offset = 0f;
            if (totalProjectiles > 1)
            {
                float fraction = (float)i / (totalProjectiles - 1) - 0.5f;
                offset = fraction * totalSpread;
            }

            float angle = baseAngle + Mathf.DegToRad(offset);
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            SpawnProjectile(direction);
        }
    }

    /// <summary>
    /// Retorna o inimigo mais próximo dentro do alcance da arma.
    /// </summary>
    private Node2D FindNearestEnemy()
    {
        float effectiveRange = WeaponData.Range * RangeMultiplier;
        float nearestDist = effectiveRange;
        Node2D nearest = null;

        var enemies = GetTree().GetNodesInGroup("enemies");
        foreach (var node in enemies)
        {
            if (node is not Node2D enemy) continue;
            float dist = GlobalPosition.DistanceTo(enemy.GlobalPosition);
            if (dist <= nearestDist)
            {
                nearestDist = dist;
                nearest = enemy;
            }
        }

        return nearest;
    }

    /// <summary>
    /// Instancia e dispara um projétil na direção especificada.
    /// </summary>
    private void SpawnProjectile(Vector2 direction)
    {
        var projectile = WeaponData.ProjectileScene.Instantiate<Projectile>();
        projectile.GlobalPosition = GlobalPosition;
        projectile.Initialize(direction, WeaponData);

        // Aplica multiplicadores de dano
        projectile.Damage *= DamageMultiplier;

        GetTree().CurrentScene.AddChild(projectile);
    }

    /// <summary>
    /// Aumenta o nível da arma e atualiza o timer.
    /// </summary>
    public void LevelUp()
    {
        WeaponLevel++;
        UpdateTimerInterval();
    }
}
