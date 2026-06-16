using Godot;
using System;
using System.Collections.Generic;
using CorporateSurvivors.Data;
using CorporateSurvivors.Systems;

namespace CorporateSurvivors;

/// <summary>
/// Sistema de upgrades — gerencia a pool de upgrades, rolagem e aplicação.
/// Chamado quando o jogador sobe de nível.
/// </summary>
public partial class UpgradeSystem : Node
{
    [ExportGroup("Pool de Upgrades")]
    [Export] public Godot.Collections.Array<UpgradeData> AvailableUpgrades { get; set; } = new();

    [ExportGroup("Config")]
    [Export] public int ChoicesPerLevel { get; set; } = 3; // Quantas opções mostrar
    [Export] public bool AllowDuplicates { get; set; } = true;

    // Rastreia quantas vezes cada upgrade foi pego
    private Godot.Collections.Dictionary<string, int> _upgradeStacks = new();

    public override void _Ready()
    {
        // Conecta ao sinal de level up do GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLeveledUp += OnPlayerLeveledUp;
        }

        InitializeUpgradePool();
    }

    /// <summary>
    /// Inicializa a pool de upgrades carregando todos os .tres de Resources/Upgrades/.
    /// </summary>
    private void InitializeUpgradePool()
    {
        AvailableUpgrades.Clear();

        string upgradesDir = "res://Resources/Upgrades/";
        if (DirAccess.DirExistsAbsolute(upgradesDir))
        {
            var dir = DirAccess.Open(upgradesDir);
            if (dir != null)
            {
                dir.ListDirBegin();
                string fileName = dir.GetNext();
                while (!string.IsNullOrEmpty(fileName))
                {
                    if (fileName.EndsWith(".tres") || fileName.EndsWith(".res"))
                    {
                        string path = upgradesDir + fileName;
                        var upgrade = GD.Load<UpgradeData>(path);
                        if (upgrade != null)
                        {
                            AvailableUpgrades.Add(upgrade);
                            GD.Print($"UpgradeSystem: Upgrade carregado: {upgrade.UpgradeName}");
                        }
                    }
                    fileName = dir.GetNext();
                }
                dir.ListDirEnd();
            }
        }

        if (AvailableUpgrades.Count == 0)
            GD.PrintErr("UpgradeSystem: Nenhum upgrade carregado de Resources/Upgrades/!");
        else
            GD.Print($"UpgradeSystem: {AvailableUpgrades.Count} upgrades carregados.");
    }

    /// <summary>
    /// Chamado quando o jogador sobe de nível.
    /// Seleciona N upgrades aleatórios para o jogador escolher.
    /// </summary>
    private void OnPlayerLeveledUp(int newLevel)
    {
        var choices = GetUpgradeChoices(newLevel);
        if (choices.Count == 0)
        {
            GD.PrintErr("UpgradeSystem: Nenhum upgrade disponível para este nível!");
            return;
        }

        // Emite evento para a UI mostrar as escolhas
        // A UI deve chamar ApplyChosenUpgrade() quando o jogador selecionar
        GD.Print($"UpgradeSystem: {choices.Count} opções para nível {newLevel}");
    }

    /// <summary>
    /// Retorna uma lista de upgrades possíveis para o nível atual.
    /// Filtra por nível mínimo/máximo e faz rolagem ponderada.
    /// </summary>
    public Godot.Collections.Array<UpgradeData> GetUpgradeChoices(int playerLevel)
    {
        var candidates = new Godot.Collections.Array<UpgradeData>();

        foreach (var upgrade in AvailableUpgrades)
        {
            // Filtra por nível
            if (playerLevel < upgrade.MinLevel || playerLevel > upgrade.MaxLevel)
                continue;

            // Filtra por stack máximo
            if (!upgrade.IsStackable)
            {
                string key = upgrade.ResourcePath;
                if (_upgradeStacks.ContainsKey(key) && _upgradeStacks[key] >= 1)
                    continue;
            }
            else if (_upgradeStacks.ContainsKey(upgrade.ResourcePath))
            {
                if (_upgradeStacks[upgrade.ResourcePath] >= upgrade.MaxStacks)
                    continue;
            }

            candidates.Add(upgrade);
        }

        // Se não houver candidatos, libera o filtro de nível
        if (candidates.Count < ChoicesPerLevel)
        {
            foreach (var upgrade in AvailableUpgrades)
            {
                if (!candidates.Contains(upgrade))
                {
                    string key = upgrade.ResourcePath;
                    if (!upgrade.IsStackable && _upgradeStacks.ContainsKey(key) && _upgradeStacks[key] >= 1)
                        continue;
                    if (upgrade.IsStackable && _upgradeStacks.ContainsKey(key) && _upgradeStacks[key] >= upgrade.MaxStacks)
                        continue;

                    candidates.Add(upgrade);
                }
            }
        }

        // Rolagem ponderada
        var result = new Godot.Collections.Array<UpgradeData>();
        var pool = new Godot.Collections.Array<UpgradeData>(candidates);

        for (int i = 0; i < Mathf.Min(ChoicesPerLevel, pool.Count); i++)
        {
            float totalWeight = 0f;
            foreach (var u in pool)
                totalWeight += u.RarityWeight;

            float roll = GD.Randf() * totalWeight;
            for (int j = 0; j < pool.Count; j++)
            {
                roll -= pool[j].RarityWeight;
                if (roll <= 0f)
                {
                    result.Add(pool[j]);
                    pool.RemoveAt(j);
                    break;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Aplica o upgrade escolhido pelo jogador.
    /// Chamado pela UI quando o jogador seleciona uma opção.
    /// </summary>
    public void ApplyUpgrade(UpgradeData upgrade)
    {
        // Rastreia stack
        string key = upgrade.ResourcePath;
        if (!_upgradeStacks.ContainsKey(key))
            _upgradeStacks[key] = 0;
        _upgradeStacks[key]++;

        // Aplica o upgrade no GameManager
        GameManager.Instance?.ApplyUpgrade(upgrade);

        // Aplica efeitos no jogador
        ApplyUpgradeEffect(upgrade);

        GD.Print($"UpgradeSystem: {upgrade.UpgradeName} aplicado (stack {_upgradeStacks[key]})");
    }

    /// <summary>
    /// Aplica os efeitos do upgrade no jogador/armas.
    /// </summary>
    private void ApplyUpgradeEffect(UpgradeData upgrade)
    {
        var player = GameManager.Instance?.Player;
        if (player == null) return;

        int currentStack = _upgradeStacks[upgrade.ResourcePath];
        float value = upgrade.GetValueForStack(currentStack - 1);

        switch (upgrade.StatType)
        {
            case UpgradeStatType.MoveSpeed:
                if (upgrade.ModifierType == UpgradeModifierType.Additive)
                    player.MoveSpeed += value;
                else if (upgrade.ModifierType == UpgradeModifierType.Multiplicative)
                    player.MoveSpeed *= value;
                break;

            case UpgradeStatType.MaxHealth:
                if (upgrade.ModifierType == UpgradeModifierType.Additive)
                {
                    player.MaxHealth += value;
                    player.Heal(value); // Cura equivalente ao bônus
                }
                else if (upgrade.ModifierType == UpgradeModifierType.Multiplicative)
                {
                    player.MaxHealth *= value;
                }
                break;

            case UpgradeStatType.HealthRegen:
                if (upgrade.ModifierType == UpgradeModifierType.Additive)
                    player.HealthRegenPerSecond += value;
                break;

            case UpgradeStatType.DamageReduction:
                if (upgrade.ModifierType == UpgradeModifierType.Additive)
                    player.DamageReduction = Mathf.Min(0.9f, player.DamageReduction + value * 0.01f);
                break;

            case UpgradeStatType.CoffeeGain:
                if (upgrade.ModifierType == UpgradeModifierType.Additive)
                    player.CoffeeGainMultiplier += value * 0.01f;
                break;

            case UpgradeStatType.MagnetRadius:
                if (upgrade.ModifierType == UpgradeModifierType.Additive)
                    player.ExpandMagnetRadius(value);
                break;

            // Weapon stats e globais serão aplicados pelo GameManager
            default:
                GD.Print($"UpgradeSystem: Efeito {upgrade.StatType} não implementado ainda.");
                break;
        }
    }
}
