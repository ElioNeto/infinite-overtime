# 🎮 Guia da Engine — Corporate Survivors

Guia completo de como usar a Godot Engine 4.6 (.NET / C#) com este projeto.

---

## Índice

- [1. Instalar Godot 4.6 .NET](#1-instalar-godot-46-net)
- [2. Abrir o Projeto](#2-abrir-o-projeto)
- [3. Estrutura do Jogador (Employee)](#3-estrutura-do-jogador-employee)
- [4. Como Testar o Jogo](#4-como-testar-o-jogo)
- [5. Ciclo de Vida de uma Partida](#5-ciclo-de-vida-de-uma-partida)
- [6. Debug e Console](#6-debug-e-console)
- [7. Como Modificar Atributos do Jogador](#7-como-modificar-atributos-do-jogador)
- [8. Recursos (Resources) — Dados do Jogo](#8-recursos-resources--dados-do-jogo)
- [9. Sistema de Armas (Auto-Attack)](#9-sistema-de-armas-auto-attack)
- [10. Projéteis (Projectile)](#10-projéteis-projectile)
- [11. Criar Novo Tipo de Inimigo](#11-criar-novo-tipo-de-inimigo)
- [12. Criar Nova Arma](#12-criar-nova-arma)
- [13. Criar Novo Upgrade](#13-criar-novo-upgrade)
- [14. Criar Novo Andar (FloorData)](#14-criar-novo-andar-floordata)
- [15. Comandos Úteis](#15-comandos-úteis)
- [16. Arquitetura Geral](#16-arquitetura-geral)
- [17. Status do Projeto](#17-status-do-projeto)

---

## 1. Instalar Godot 4.6 .NET

Baixe o **Godot 4.6 (.NET)** do site oficial:

```
https://godotengine.org/download/
```

Escolha a versão **.NET** (não a standard). Precisamos dela porque o projeto usa C#.

Alternativa via gerenciador de versões:

```bash
# Linux (flatpak)
flatpak install flathub org.godotengine.Godot

# Ou baixar o .NET build manualmente
wget https://github.com/godotengine/godot/releases/download/4.6-stable/Godot_v4.6-stable_mono_linux_x86_64.zip
unzip Godot_v4.6-stable_mono_linux_x86_64.zip
```

> ⚠️ **Importante:** O SDK do .NET 8 precisa estar instalado.
>
> ```bash
> # Verificar versão
> dotnet --version
> # Se não estiver instalado:
> # Windows: https://dotnet.microsoft.com/download/dotnet/8.0
> # Linux:   sudo apt install dotnet-sdk-8.0
> ```

---

## 2. Abrir o Projeto

```bash
# No terminal, navegue até a pasta e abra com Godot
godot4 --path /caminho/para/clone-vamp-surv

# Ou abra o editor Godot e clique em "Import" → selecione project.godot
```

Quando abrir pela primeira vez, o Godot vai detectar os scripts C# e pedir para **Build**.Clique em **Build** ou use o menu:

```
Project → Build
```

Ou atalho: `Ctrl + B`

Se houver erro de compilação, verifique:

```bash
dotnet build
```

---

## 3. Estrutura do Jogador (Employee)

O jogador é um **`CharacterBody2D`** — o tipo de nó padrão do Godot para personagens com física e movimento.

**Árvore de nós da cena `Employee.tscn`:**

```
Employee (CharacterBody2D)  ← script: Employee.cs
├── Sprite2D                ← arte do funcionário (SVG placeholder)
├── CollisionShape2D        ← hitbox (RectangleShape)
├── MagnetArea (Area2D)     ← área de atração de café (XP)
│   └── CollisionShape2D    ← CircleShape (criado via código em _Ready)
├── AnimationPlayer          ← animações (walk, idle — placeholders)
├── Camera2D                 ← câmera que segue o jogador
│   └── position_smoothing = true
└── Weapon2D (filho criado em código)  ← arma automática
    └── Timer (AttackTimer)
```

> ℹ️ A arma inicial é adicionada via código em `Game.StartGameplay()`:
> ```csharp
> var weaponNode = new Weapon2D();
> weaponNode.WeaponData = defaultWeapon;
> _playerInstance.AddChild(weaponNode);
> ```

### Fluxo de movimento:

```
_Process(delta)
    │
    ├── HandleMovement()
    │       │
    │       ├── Input.GetVector("move_left","move_right","move_up","move_down")
    │       │       │
    │       │       └── Lê as teclas WASD / Setas (configurado no project.godot)
    │       │
    │       ├── Velocity = direction × MoveSpeed
    │       │       │
    │       │       └── Define a velocidade que o CharacterBody2D usará
    │       │
    │       ├── AnimationPlayer → "walk" ou "idle"
    │       └── Sprite.FlipH = direction.X < 0
    │
    ├── HandleRegeneration(dt)    ← regeneração passiva de HP
    ├── HandleInvulnerability(dt) ← iframes após tomar dano
    │
    └── MoveAndSlide()
            │
            └── Método do CharacterBody2D que aplica Velocity com colisão
```

### Inputs configurados (`project.godot`):

| Ação | Tecla Primária | Tecla Secundária |
|------|---------------|------------------|
| `move_left` | A | ← |
| `move_right` | D | → |
| `move_up` | W | ↑ |
| `move_down` | S | ↓ |
| `pause` | ESC | — |
| `interact` | E | — |

---

## 4. Como Testar o Jogo

### No editor Godot:

```
1. Abra Scenes/UI/MainMenu.tscn
2. Clique em "Play Scene" (F6) para testar só o menu
   OU
3. Clique em "Play" (F5) — que usa a MainScene configurada (MainMenu.tscn)
```

### Fluxo completo de gameplay:

```
MainMenu.tscn
    │
    ├── [NOVO JOGO]
    │       │
    │       └── Game.tscn  ← cena principal
    │               │
    │               ├── GameManager.StartGame()
    │               ├── GameManager.InitializeFloors()
    │               │       └── Carrega FloorData de Resources/Floors/
    │               ├── Instancia Employee.tscn (jogador)
    │               ├── Adiciona Weapon2D (arma inicial Clipes Reforçados)
    │               ├── Instancia HUD.tscn
    │               ├── UpgradeSystem.InitializeUpgradePool()
    │               │       └── Carrega UpgradeData de Resources/Upgrades/
    │               ├── FloorManager.LoadFirstFloor()
    │               │       ├── Carrega dados do FloorData (Térreo)
    │               │       ├── Configura WaveSpawner com EnemyPool do andar
    │               │       └── (opcional) Carrega cena do andar
    │               └── WaveSpawner começa a spawnar inimigos
    │
    ├── [SAIR]
    │
    └── Player morre → GameOver.tscn
    └── Sobrevive 30min → Victory.tscn
```

---

## 5. Ciclo de Vida de uma Partida

Quando você clica **NOVO JOGO**:

1. **`MainMenu.OnStartPressed()`**
   ```csharp
   GetTree().ChangeSceneToFile("res://Scenes/Game/Game.tscn");
   ```

2. **`Game._Ready()`** (Game.tscn)
   - Conecta sinais do GameManager
   - Chama `StartGameplay()`

3. **`Game.StartGameplay()`**
   - Instancia `Employee` a partir de `Employee.tscn` no centro do mapa
   - Instancia `HUD` (CanvasLayer sobre a tela)
   - Inicializa `FloorManager` com o primeiro andar
   - Configura `WaveSpawner` com a pool de inimigos do andar
   - Chama **`GameManager.Instance.StartGame()`**

4. **`GameManager.StartGame()`**
   - Reseta: andar=0, tempo=0, nível=1, café=0
   - Estado muda para `Playing`
   - Começa a contar tempo no `_Process`

5. **Jogador se move** → WASD lê inputs, `MoveAndSlide()` aplica física

6. **Weapon2D (auto-attack)** → a cada `1/attackSpeed` segundos:
   - Busca o inimigo mais próximo dentro do `Range` da arma
   - Instancia `Projectile` da cena configurada no `WeaponData`
   - Projétil voa na direção do inimigo

7. **WaveSpawner** → a cada `1/spawnRate` segundos, spawna um inimigo nas bordas

8. **Inimigo morre** → droppa `CoffeeDrop`

9. **CoffeeDrop** → se entrar no `MagnetRadius` do jogador, é atraído e coletado

10. **Coleta de café** → `GameManager.AddCoffee()` → acumula XP → checa level up

11. **Level up!** → Jogo pausa → `UpgradeSystem.GetUpgradeChoices()` → HUD mostra 3 opções → jogador escolhe → `UpgradeSystem.ApplyUpgrade()` aplica efeitos

---

## 6. Debug e Console

Para ver logs do jogo em tempo real:

**No editor**: a aba **Output** (inferior) mostra todos os `GD.Print()`.

**No terminal** (execução debug):

```bash
godot4 --path /caminho/para/clone-vamp-surv
```

### Logs que aparecem durante o gameplay:

```
GameManager: Jogo iniciado!
Employee: Pronto! HP: 100/100, Speed: 200
Game: Jogador instanciado.
WaveSpawner: Pool configurada para Térreo — Recepção
WaveSpawner: Spawnou E-mail Não Lido em (450, 320)
BaseEnemy: E-mail Não Lido spawnado! HP: 30
Employee: Tomou 8.0 de dano! HP: 92.0/100
BaseEnemy: E-mail Não Lido derrotado!
GameManager: Level up! Nível 2
```

### Códigos de debug que você pode adicionar temporariamente:

```csharp
// No Employee._Process() — mostra posição e velocidade
GD.Print($"Pos: {GlobalPosition} | Vel: {Velocity} | HP: {_currentHealth}");

// No WaveSpawner — mostra contagem de inimigos
GD.Print($"Inimigos ativos: {GetTree().GetNodesInGroup("enemies").Count}");
```

---

## 7. Como Modificar Atributos do Jogador

No **editor Godot**, selecione `Employee.tscn` e veja o **Inspector**:

```
Employee (CharacterBody2D)
└── Atributos (script)
    ├── MoveSpeed: 200.0              ← velocidade do personagem
    ├── MaxHealth: 100.0              ← vida máxima
    ├── HealthRegenPerSecond: 0.0     ← regeneração passiva
    ├── DamageReduction: 0.0          ← redução de dano (0.0 a 0.9)
    ├── MagnetRadius: 100.0           ← raio de atração de café
    └── CoffeeGainMultiplier: 1.0     ← multiplicador de XP
```

Esses valores são **`[Export]`** — aparecem automaticamente no Inspector sem precisar de código extra.

Para alterar via código (ex: num upgrade):

```csharp
// Employee.cs
[Export] public float MoveSpeed { get; set; } = 200f;

// Em UpgradeSystem.cs
player.MoveSpeed += 20f;        // +20 de velocidade
player.MaxHealth *= 1.2f;       // +20% de vida máxima
player.ExpandMagnetRadius(50f); // +50 de raio de atração
```

---

## 8. Recursos (Resources) — Dados do Jogo

O projeto usa **arquivos `.tres`** (Godot Resource) como dados. Tudo que é configurável
(vida dos inimigos, dano das armas, upgrades, andares) fica em arquivos texto na pasta
`Resources/`, divididos por categoria:

```
Resources/
├── Enemies/        ← 6 EnemyData.tres (dados de cada inimigo)
│   ├── EmailNaoLido.tres
│   ├── PlanilhaMalignificada.tres
│   ├── AvaliacaoFantasma.tres
│   ├── CafeDerramado.tres
│   ├── AtaDeReuniao.tres
│   └── NotebookSuperaquecido.tres
├── Weapons/        ← 6 WeaponData.tres (armas do jogador)
│   ├── ClipesReforcados.tres            (disparo único, médio dano)
│   ├── NotebookSuperaquecido.tres       (burn — dano contínuo)
│   ├── PlanilhaExplosiva.tres           (explosão em área)
│   ├── CaféEspirrado.tres               (3 projéteis em cone)
│   ├── MensagemPassivoAgressiva.tres    (piercing + slow)
│   └── ImpressoraFantasma.tres          (8 projéteis 360°)
├── Upgrades/       ← 11 UpgradeData.tres (melhorias)
│   ├── CafeExtraFortado.tres       (velocidade)
│   ├── VidaExtra.tres              (vida máxima)
│   ├── Regeneracao.tres            (regeneração)
│   ├── ArmaduraCorporativa.tres    (redução de dano)
│   ├── RedeDeCafe.tres             (ganho de XP)
│   ├── ImãDeCafe.tres              (raio de atração)
│   ├── DanoDeArma.tres             (dano ×1.15)
│   ├── VelocidadeDeArma.tres       (attack speed ×1.10)
│   ├── AlcanceExtra.tres           (range ×1.20)
│   ├── DisparoDuplo.tres           (+1 projétil)
│   └── HoraExtra.tres              (-10% spawn rate)
└── Floors/         ← 6 FloorData.tres (andares/fases)
    ├── Terreo_Recepcao.tres
    ├── SegundoAndar_Financeiro.tres
    ├── TerceiroAndar_RH.tres
    ├── QuartoAndar_TI.tres
    ├── QuintoAndar_Reunioes.tres
    └── SextoAndar_Diretoria.tres
```

### Carregamento automático

Os Resources são carregados automaticamente em tempo de execução:

| Sistema | Pasta | Método |
|---------|-------|--------|
| `FloorManager` | `Resources/Floors/` | `GameManager.InitializeFloors()` — lê todos `.tres` da pasta |
| `UpgradeSystem` | `Resources/Upgrades/` | `InitializeUpgradePool()` — lê todos `.tres` da pasta |
| Arma inicial | `Resources/Weapons/ClipesReforcados.tres` | `GameManager.LoadDefaultWeapon()` |

### EnemyData

Arquivo: `Recursos/Enemies/<Nome>.tres`

```gdscript
[resource]
EnemyName = "E-mail Não Lido"
MaxHealth = 30.0
MoveSpeed = 80.0
Damage = 8.0
CoffeeDrop = 5.0
BehaviorType = 0          # 0=Chase, 1=Patrol, 2=Teleport, etc.
Scene = ExtResource(...)  # Aponta para Scene do inimigo
```

### WeaponData

Arquivo: `Resources/Weapons/<Nome>.tres`

```gdscript
[resource]
WeaponName = "Clipes Reforçados"
Damage = 10.0
AttackSpeed = 1.5         # ataques por segundo
ProjectileSpeed = 600.0
Range = 350.0
IsPiercing = false
ProjectileCount = 1
SpreadAngle = 0.0
EffectType = 0             # 0=None, 1=Slow, 2=Burn, 3=Explosion, etc.
MaxLevel = 5
DamagePerLevel = 4.0
AttackSpeedPerLevel = 0.05
ProjectileScene = ExtResource(...)  # Cena do projétil
ProjectileColor = Color(...)
```

### UpgradeData

Arquivo: `Resources/Upgrades/<Nome>.tres`

```gdscript
[resource]
UpgradeName = "Café Extra Forte"
MinLevel = 1              # nível mínimo do jogador para aparecer
MaxLevel = 99             # nível máximo
RarityWeight = 2.0        # peso na rolagem (maior = mais comum)
TargetType = 0            # 0=Player, 1=Weapon, 2=Global
ModifierType = 0          # 0=Additive, 1=Multiplicative
StatType = 0              # 0=MoveSpeed, 1=MaxHealth, etc.
Value = 20.0              # valor do efeito (ex: +20 velocidade)
IsStackable = true
MaxStacks = 5
```

### FloorData

Arquivo: `Resources/Floors/<Nome>.tres`

```gdscript
[resource]
FloorName = "Térreo — Recepção & Hall"
FloorIndex = 0
DurationMinutes = 6.0
HasBoss = false
EnemyPool = [{
    "Enemy": ExtResource(...),
    "Weight": 2.0,
    "MinFloorMinute": 0
}]
BaseSpawnRate = 1.5
MaxEnemiesAlive = 30
AmbientColor = Color(...)
```

---

## 9. Sistema de Armas (Auto-Attack)

O sistema de combate é **automático** (como em Vampire Survivors). O jogador não precisa
apertar botão para atacar — as armas disparam sozinhas.

### Weapon2D

**Script:** `Scenes/Weapons/Weapon2D.cs`

O `Weapon2D` é um `Node2D` que fica como filho do `Employee`. Cada arma tem:

```
Weapon2D (Node2D)
├── WeaponData (Resource)     ← dados da arma
├── AttackTimer (Timer)       ← controla o intervalo entre disparos
├── DamageMultiplier          ← modificador externo (ex: upgrades)
├── AttackSpeedMultiplier     ← modificador externo
├── RangeMultiplier           ← modificador externo
└── ExtraProjectiles          ← modificador externo
```

**Fluxo de ataque:**

```
OnAttackTimerTimeout()
    │
    ├── FindNearestEnemy()
    │       │
    │       ├── Itera sobre grupo "enemies"
    │       └── Retorna o mais próximo dentro de Range × RangeMultiplier
    │
    ├── (se target == null) → não atira
    │
    └── SpawnProjectile(direction)
            │
            ├── WeaponData.ProjectileScene.Instantiate<Projectile>()
            ├── projectile.Initialize(direction, WeaponData)
            ├── projectile.Damage *= DamageMultiplier
            └── AddChild(projectile) na cena atual
```

### Níveis da arma

Cada arma tem `MaxLevel` (padrão: 5). O dano e attack speed escalam:

```csharp
public float GetDamageForLevel(int level)
    => Damage + (DamagePerLevel × (level - 1));

public float GetAttackSpeedForLevel(int level)
    => AttackSpeed + (AttackSpeedPerLevel × (level - 1));
```

### Como adicionar arma ao jogador

```csharp
// Em código (ex: Game.cs ou UpgradeSystem)
var weaponNode = new Weapon2D();
weaponNode.WeaponData = GD.Load<WeaponData>("res://Resources/Weapons/MinhaArma.tres");
player.AddChild(weaponNode);
```

---

## 10. Projéteis (Projectile)

**Script:** `Scenes/Weapons/Projectile.cs`
**Cena:** `Scenes/Weapons/Projectile.tscn`

O projétil é um `Area2D` que se move em linha reta e detecta colisão com inimigos.

```
Projectile (Area2D)
├── Sprite2D                    ← cor configurada pelo WeaponData.ProjectileColor
├── CollisionShape2D (Circle)   ← hitbox do projétil
└── GpuParticles2D              ← trail particles (opcional, desligado por padrão)
```

**Comportamento:**

| Atributo | Efeito |
|----------|--------|
| `Damage` | Dano causado ao acertar |
| `Speed` | Velocidade de voo |
| `Lifetime` | Tempo até expirar (calculado = Range / Speed) |
| `IsPiercing` | Se `true`, atravessa inimigos sem desaparecer |
| `WeaponData.EffectType` | Efeito especial ao acertar |

**Efeitos especiais:**

| EffectType | Comportamento |
|------------|---------------|
| `None` | Apenas dano |
| `Slow` | (futuro) Lentidão no inimigo |
| `Burn` | (futuro) Dano contínuo |
| `Explosion` | Cria área de explosão que causa 50% de dano em raio |
| `Summon` | (futuro) Invoca aliado |
| `Heal` | (futuro) Cura o jogador ao acertar |
| `ArmorShred` | (futuro) Reduz defesa do inimigo |

---

## 11. Criar Novo Tipo de Inimigo

### Passo a passo:

**1. Crie o Resource de dados (`.tres`):**

```bash
touch Resources/Enemies/PlanilhaMalignificada.tres
```

Abra o arquivo no Godot (dá pra editar como texto ou no Inspector):

```gdscript
[gd_resource type=Resource script=ExtResource("res://Scripts/Data/EnemyData.cs")]
[resource]
EnemyName = "Planilha Malignificada"
MaxHealth = 60.0
MoveSpeed = 50.0
Damage = 12.0
CoffeeDrop = 8.0
BehaviorType = 1
```

> `BehaviorType` é um enum: 0=Chase, 1=Patrol, 2=Teleport, 3=Stationary, 4=Swarm, 5=Trap, 6=Spawner

**2. Crie a cena do inimigo (`.tscn`):**

```bash
touch Scenes/Enemies/PlanilhaMalignificada.tscn
```

**3. No editor Godot:**

- Abra `PlanilhaMalignificada.tscn`
- Nó raiz: `CharacterBody2D` com script `BaseEnemy.cs`
- Adicione: `Sprite2D`, `CollisionShape2D`, `AnimationPlayer`
- Adicione: `Area2D` (DamageArea) com `CollisionShape2D` para contato com jogador
- Adicione: `Area2D` (Hitbox) para receber dano de projéteis
- Conecte os nós nos exports do script

**4. Atribua o Resource:**

- No Inspector do `BaseEnemy`, arraste `PlanilhaMalignificada.tres` para `EnemyData`
- Configure `Scene` no Resource para apontar para `PlanilhaMalignificada.tscn`

**5. Adicione ao pool de spawn (no FloorData correspondente):**

Edite `Resources/Floors/Floor1_Recepcao.tres`:

```gdscript
[resource]
EnemyPool = [{
"Enemy": ExtResource("res://Resources/Enemies/PlanilhaMalignificada.tres"),
"Weight": 0.5,
"MinFloorMinute": 2
}]
```

---

## 12. Criar Nova Arma

### Passo a passo:

**1. Crie o Resource de dados (`.tres`):**

```bash
touch Resources/Weapons/NovaArma.tres
```

**2. Edite o arquivo:**

```gdscript
[gd_resource type="Resource" load_steps=3 format=3]
[ext_resource type="Script" path="res://Scripts/Data/WeaponData.cs" id="1"]
[ext_resource type="PackedScene" path="res://Scenes/Weapons/Projectile.tscn" id="2"]

[resource]
script = ExtResource("1")
WeaponName = "Nova Arma"
Damage = 12.0
AttackSpeed = 1.2
ProjectileSpeed = 500.0
Range = 300.0
IsPiercing = false
ProjectileCount = 1
SpreadAngle = 0.0
EffectType = 0
MaxLevel = 5
DamagePerLevel = 5.0
AttackSpeedPerLevel = 0.05
ProjectileScene = ExtResource("2")
ProjectileColor = Color(1.0, 1.0, 1.0, 1.0)
```

**3. A arma estará disponível para uso em código:**

```csharp
var weaponData = GD.Load<WeaponData>("res://Resources/Weapons/NovaArma.tres");
var weaponNode = new Weapon2D();
weaponNode.WeaponData = weaponData;
player.AddChild(weaponNode);
```

> A arma pode ser dropada como Upgrade ao subir de nível (adicione um UpgradeData
> com `TargetType = 1` para adicionar uma nova arma ao jogador).

---

## 13. Criar Novo Upgrade

### Passo a passo:

**1. Crie o Resource de dados (`.tres`):**

```bash
touch Resources/Upgrades/MeuUpgrade.tres
```

**2. Edite o arquivo:**

```gdscript
[gd_resource type="Resource" load_steps=2 format=3]
[ext_resource type="Script" path="res://Scripts/Data/UpgradeData.cs" id="1"]

[resource]
script = ExtResource("1")
UpgradeName = "Meu Upgrade"
Description = "Descrição do que ele faz."
Emoji = "⚡"
MinLevel = 1
MaxLevel = 99
RarityWeight = 1.5
TargetType = 0          # 0=Player, 1=Weapon, 2=Global
ModifierType = 0        # 0=Additivo, 1=Multiplicativo
StatType = 0            # Ver UpgradeStatType enum abaixo
Value = 10.0
IsStackable = true
MaxStacks = 3
TintColor = Color(0.5, 0.5, 0.5, 1.0)
```

### Enum `UpgradeStatType`

| Valor | Nome | Efeito |
|-------|------|--------|
| `0` | `MoveSpeed` | Velocidade do jogador |
| `1` | `MaxHealth` | Vida máxima |
| `2` | `HealthRegen` | Regeneração por segundo |
| `3` | `DamageReduction` | Redução de dano (%) |
| `4` | `CoffeeGain` | Multiplicador de XP |
| `5` | `CoffeeValue` | (reservado) |
| `6` | `MagnetRadius` | Raio de atração de café |
| `7` | `WeaponDamage` | Multiplicador de dano das armas |
| `8` | `WeaponSpeed` | Multiplicador de attack speed |
| `9` | `WeaponRange` | Multiplicador de alcance |
| `10` | `ExtraProjectiles` | +projéteis por ataque |
| `11` | `SpawnRateModifier` | Modificador de spawn de inimigos |

**3. O upgrade é carregado automaticamente** pelo `UpgradeSystem` na pasta `Resources/Upgrades/`.

---

## 14. Criar Novo Andar (FloorData)

### Passo a passo:

**1. Crie o Resource de dados (`.tres`):**

```bash
touch Resources/Floors/MeuAndar.tres
```

**2. Edite o arquivo:**

```gdscript
[gd_resource type="Resource" load_steps=... format=3]
[ext_resource type="Script" path="res://Scripts/Data/FloorData.cs" id="1"]
[ext_resource type="Resource" path="res://Resources/Enemies/AlgumInimigo.tres" id="2"]

[resource]
script = ExtResource("1")
FloorName = "Meu Novo Andar"
FloorIndex = 6              # índice único sequencial
Description = "Descrição do andar."
DurationMinutes = 6.0
HasBoss = true
BossName = "Chefe Final"
EnemyPool = [{
    "Enemy": ExtResource("2"),
    "Weight": 1.0,
    "MinFloorMinute": 0
}]
BaseSpawnRate = 2.0
MaxEnemiesAlive = 30
AmbientColor = Color(0.2, 0.2, 0.3, 1.0)
MusicTrack = ""
```

**3. O andar é carregado automaticamente** pelo `GameManager.InitializeFloors()` na pasta `Resources/Floors/`.

> ⚠️ `FloorIndex` deve ser único. Se houver dois andares com o mesmo índice,
> o último carregado sobrescreverá o anterior na ordenação.

---

## 15. Comandos Úteis

```bash
# Build do C#
dotnet build

# Verificar erros de compilação C#
dotnet build 2>&1 | grep -i error

# Executar Godot em modo debug (sem editor gráfico)
godot4 --path . --verbose

# Executar Godot com uma cena específica
godot4 --path . Scenes/UI/MainMenu.tscn

# Executar Godot headless (para testes em CI)
godot4 --path . --headless --quit

# Executar Godot com debug remoto
godot4 --path . --remote-debug port=6007

# Abrir o editor diretamente
godot4 --editor --path .

# Verificar se o projeto tem erros de recursos
godot4 --path . --check-only
```

### Atalhos do Editor Godot:

| Atalho | Ação |
|--------|------|
| `F5` | Rodar projeto (Main Scene) |
| `F6` | Rodar cena atual |
| `F8` | Parar execução |
| `Ctrl + B` | Build C# |
| `Ctrl + S` | Salvar cena |
| `Ctrl + Shift + A` | Adicionar nó |
| `Q` | Ferramenta de seleção |
| `W` | Ferramenta de mover |
| `E` | Ferramenta de rotacionar |
| `R` | Ferramenta de escalar |
| `F` | Foco no nó selecionado |

---

## 16. Arquitetura Geral

```
┌──────────────────────────────────────────────────────────────┐
│                     Godot Engine 4.6                         │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  Autoload (sempre carregados, independente de cena)          │
│  ├── GameManager  (estado do jogo, progressão, sinais)       │
│  └── AudioManager (música e SFX)                             │
│                                                               │
│  Scene: Game.tscn                                             │
│  ├── WorldContainer (Node2D)  ← onde tudo vive               │
│  │   ├── Employee (CharacterBody2D) ← jogador                │
│  │   │   └── Weapon2D (filho)  ← arma automática             │
│  │   ├── Inimigos (BaseEnemy)                                │
│  │   ├── Projéteis (Projectile)                              │
│  │   └── Pickups (CoffeeDrop, etc)                           │
│  ├── WaveSpawner (Node)       ← spawn de inimigos            │
│  ├── FloorManager (Node)      ← transição de andares         │
│  ├── UpgradeSystem (Node)     ← upgrades                     │
│  ├── TileMapLayer             ← mapa/colisão                 │
│  ├── WorldEnvironment         ← iluminação ambiente          │
│  ├── PauseMenu (CanvasLayer)  ← menu de pausa                │
│  └── HUD (CanvasLayer)        ← UI sobreposta                │
│                                                               │
│  Data (Resources / .tres)                                     │
│  ├── EmployeeData                                             │
│  ├── EnemyData + EnemySpawnEntry                              │
│  ├── WeaponData                                               │
│  ├── UpgradeData                                              │
│  └── FloorData                                                │
│                                                               │
│  Utils                                                        │
│  ├── ScreenUtils  (tela, viewport, câmera)                   │
│  └── MathUtils    (math helpers)                              │
│                                                               │
└──────────────────────────────────────────────────────────────┘
```

### Dependências entre os sistemas:

```
GameManager (singleton)
    ├── Game.cs          (orquestra a cena de gameplay)
    ├── Employee.cs      (jogador — se registra no GM)
    ├── WaveSpawner      (usa GM para dificuldade)
    ├── FloorManager     (ouve sinal OnFloorCleared)
    ├── UpgradeSystem    (ouve sinal OnLeveledUp)
    └── HUD              (ouve sinais do GM)

AudioManager (singleton)
    └── FloorManager     (toca música por andar)

Employee
    ├── Weapon2D (filho) → dispara Projectile → BaseEnemy.TakeDamage()
    └── MagnetArea → CoffeeDrop.Collect()

WaveSpawner
    ├── EnemyPool (FloorData → EnemySpawnEntry → EnemyData)
    └── Spawna BaseEnemy nas bordas da tela

UpgradeSystem
    ├── Escuta OnLeveledUp do GameManager
    └── Aplica UpgradeData no Employee / armas
```

---

---

## 17. Status do Projeto

### Funcionalidades Implementadas

| Funcionalidade | Status | Arquivos |
|---------------|--------|----------|
| Movimento do jogador (WASD) | ✅ Completo | `Employee.cs` |
| Menu principal | ✅ Completo | `MainMenu.tscn`, `MainMenu.cs` |
| HUD (vida, XP, nível, tempo) | ✅ Completo | `HUD.tscn`, `HUD.cs` |
| Pausa (ESC) | ✅ Completo | `Game.cs` |
| Game Over / Vitória | ✅ Completo | `GameOver.cs`, `Victory.cs` |
| Inimigos (6 tipos c/ 7 comportamentos) | ✅ Completo | `BaseEnemy.cs`, 6 scenes |
| Sistema de armas (auto-attack) | ✅ Completo | `Weapon2D.cs`, `Projectile.cs` |
| Projéteis com efeitos (explosão) | ✅ Completo | `Projectile.cs` |
| Sistema de upgrades (11 upgrades) | ✅ Completo | `UpgradeSystem.cs`, 11 `.tres` |
| 6 andares com EnemyPools | ✅ Completo | `FloorManager.cs`, 6 `.tres` |
| Coleta de café (XP) com magnet | ✅ Completo | `CoffeeDrop.cs`, `Employee.cs` |
| Level up com escolha de upgrades | ✅ Completo | `GameManager.cs`, `UpgradeSystem.cs` |
| Dificuldade progressiva por minuto | ✅ Completo | `GameManager.cs`, `WaveSpawner.cs` |
| Load automático de Resources | ✅ Completo | `GameManager.cs` |

### Por Fazer (Próximos Passos)

| Funcionalidade | Prioridade | Issue |
|---------------|-----------|-------|
| Sprites / arte final | Média | #11 |
| Música e SFX | Média | #10 |
| Tilemap / mapa | Média | #8 |
| Boss (CEO Espectral) | Alta | — |
| Mais armas (6+ tipos) | Alta | — |
| Animações dos inimigos | Baixa | — |
| Efeitos de partículas | Baixa | — |
| Polimento (transições, feedback) | Baixa | — |

### Build

```bash
dotnet build   # 0 erros, ~86 warnings (nullable)
```

SDK: .NET 8.0.128
Engine: Godot 4.6.3 (Forward Plus, Jolt Physics)

---

> 📁 Este guia está em `docs/GUIDE.md`. Mantenha-o atualizado conforme o projeto evolui!
