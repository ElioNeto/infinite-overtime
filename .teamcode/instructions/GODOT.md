# Godot 4.6 — Vampire Survivors Clone

## Stack

- **Engine:** Godot 4.6 (Mono / .NET)
- **Language:** C# (GDScript também pode ser usado para prototipação rápida)
- **Renderer:** Forward Plus (`project.godot` config)
- **Physics:** Jolt Physics (configurado para 3D, mas o jogo é 2D — usar `CharacterBody2D`, `Area2D`, `RigidBody2D`)
- **Platform target:** Windows / Linux / Web

## Estrutura esperada do projeto (Vampire Survivors clone)

```
res://
├── Scenes/
│   ├── Game/
│   │   ├── Game.tscn          # Main game scene (world)
│   │   └── Game.cs
│   ├── Player/
│   │   ├── Player.tscn
│   │   └── Player.cs
│   ├── Enemy/
│   │   ├── Enemy.tscn
│   │   └── Enemy.cs
│   ├── Weapon/
│   │   ├── Weapon.tscn
│   │   └── Weapon.cs
│   └── UI/
│       ├── HUD.tscn
│       ├── HUD.cs
│       ├── MainMenu.tscn
│       ├── MainMenu.cs
│       ├── GameOver.tscn
│       └── GameOver.cs
├── Scripts/
│   ├── Systems/
│   │   ├── WaveSpawner.cs
│   │   ├── ExperienceSystem.cs
│   │   ├── UpgradesSystem.cs
│   │   └── TimerSystem.cs
│   ├── Data/
│   │   ├── WeaponData.cs
│   │   ├── EnemyData.cs
│   │   └── UpgradeData.cs
│   └── Utils/
│       ├── MathUtils.cs
│       └── ScreenUtils.cs
├── Resources/
│   ├── Weapons/
│   ├── Enemies/
│   └── Upgrades/
├── Art/
│   ├── Sprites/
│   ├── Tilesets/
│   └── Particles/
├── Audio/
│   ├── SFX/
│   └── Music/
└── Autoload/
    ├── GameManager.cs
    └── AudioManager.cs
```

## Boas práticas Godot + C#

### Nodes vs C#
- Prefira **C# scripts** para lógica de jogo (performance, type safety)
- Use **GDScript** apenas para prototipação rápida ou cenas muito simples
- Sempre referencie nós via `[Export]` ou `GetNode<>()` tipado, nunca `GetNode()` sem tipo

### Convenções de nomenclatura
- **Pastas/Arquivos:** `PascalCase` (ex: `Player.tscn`, `WeaponSystem.cs`)
- **Métodos C#:** `PascalCase` (ex: `TakeDamage()`, `SpawnEnemy()`)
- **Variáveis privadas:** `_camelCase` (ex: `_health`, `_speed`)
- **Variáveis públicas/propriedades:** `PascalCase` (ex: `MaxHealth`, `Speed`)
- **Signals:** prefixo `On` (ex: `OnEnemyDied`, `OnLevelUp`)
- **Nodes:** `PascalCase` ou `_camelCase` seguindo o padrão da cena

### Godot 4.6 específico
- Usar `CharacterBody2D` para Player e Enemies (movimento + colisão)
- Usar `Area2D` para armas, projéteis, área de efeito, pickups (experience gems)
- Usar `TileMapLayer` (novo no 4.x) para o cenário/chão
- Usar `AnimationTree` para blend de animações
- Usar `GPUParticles2D` para efeitos visuais
- Sistema de upgrade via `Resource` (Godot Resources) para dados de armas/inimigos

### Geração procedural
- O mapa deve ser gerado proceduralmente (ou tile-based grande o suficiente)
- Spawn de inimigos em ondas com dificuldade crescente
- Duração alvo: 30 minutos

### Padrão de Scripts (exemplo Player.cs)

```csharp
using Godot;
using System;

public partial class Player : CharacterBody2D
{
    [Export] public float Speed = 200f;
    [Export] public float MaxHealth = 100f;

    private float _currentHealth;
    private Vector2 _movementDirection;

    public override void _Ready()
    {
        _currentHealth = MaxHealth;
    }

    public override void _Process(double delta)
    {
        HandleMovementInput();
        MoveAndSlide();
    }

    private void HandleMovementInput()
    {
        _movementDirection = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        Velocity = _movementDirection * Speed;
    }

    public void TakeDamage(float amount)
    {
        _currentHealth -= amount;
        if (_currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        // Emit signal, show game over, etc.
        QueueFree();
    }
}
```

### Input Map (projetado para o jogo)

| Action         | Key         |
|----------------|-------------|
| `move_left`    | A / Left    |
| `move_right`   | D / Right   |
| `move_up`      | W / Up      |
| `move_down`    | S / Down    |

## Comandos Godot CLI (úteis para CI/dev)

```bash
# Abrir editor
godot --editor

# Rodar o projeto
godot --headless  # sem janela (servidor)
godot             # com janela

# Exportar
godot --export "Windows Desktop" --headless
godot --export "Linux/X11" --headless

# Build C# solution
dotnet build

# Verificar erros de script
godot --check-only
```

## Mecânicas Vampire Survivors (referência)

1. **Player** se move livremente no mapa
2. **Armas atacam automaticamente** (cooldown-based)
3. **Inimigos spawnam em ondas** do perímetro da tela
4. **Gemas de experiência** dropam de inimigos mortos
5. **Level-up** oferece escolha de upgrades aleatórios
6. **Duração**: sobreviver 30 minutos (ou até morrer)
7. **Dificuldade** escala com o tempo
