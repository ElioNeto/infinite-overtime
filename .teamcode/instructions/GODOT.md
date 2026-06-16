# Corporate Survivors — Lore e Arquitetura

## Stack

- **Engine:** Godot 4.6 (Mono / .NET)
- **Language:** C# (GDScript apenas para prototipação rápida)
- **Renderer:** Forward Plus
- **Physics:** Jolt Physics (usar `CharacterBody2D`, `Area2D` para 2D)
- **Platform target:** Windows / Linux / Web

---

## 📖 Lore — "Corporate Hell"

> Num grande prédio corporativo, a rotina de "só mais uma demanda" virou uma maldição literal.
> À noite, o sistema central reinicia, as luzes apagam, e tudo o que foi ignorado durante o expediente
> retorna deformado: e-mails ganham corpo, reuniões se materializam, metas viram monstros e o café
> passa a ser o único combustível capaz de manter alguém vivo.
>
> O protagonista é um funcionário preso no prédio depois do fechamento, tentando sobreviver até o
> amanhecer. A cada andar, o escritório se torna mais hostil: o setor financeiro vira um labirinto de
> contratos famintos, o RH convoca aparições burocráticas, a diretoria aparece como uma entidade final,
> e a sala de reuniões é onde a realidade começa a quebrar.
>
> O jogador não luta contra "inimigos aleatórios", mas contra os próprios excessos do ambiente
> corporativo. Isso dá humor, identidade e espaço para upgrades criativos.

### Tom e Estilo
- **Tom:** Sátira corporativa com tensão leve. O charme vem do absurdo de tratar ansiedade corporativa como apocalipse.
- **Visual:** Escritório comum → pesadelo neon → estética *corporate hell*.
- **Música/Som:** Ruídos de escritório distorcidos, notificações de e-mail como alertas, telefone tocando como aviso de chefes.

### Mecânicas Temáticas
| Elemento Original | Equivalente Corporativo |
|---|---|
| Vampiro | Funcionário preso no expediente |
| Armas automáticas | E-mails, planilhas, clipes, notebook superaquecido |
| Gemas de XP | Café (combustível) |
| Upgrades | Hábitos de sobrevivência corporativa |
| Ondas de inimigos | Departamentos liberando "demandas" |
| Chefes | Cargos/funções corrompidas |
| Mapa | Prédio corporativo (andares/departamentos) |
| Duração 30 min | Sobreviver até o amanhecer |

---

## 🏢 Andares / Fases (níveis do prédio)

1. **Térreo — Recepção & Hall** *(tutorial)*
   - Inimigos: E-mails perdidos, pastas esquecidas
   - Boss: O porteiro que nunca deixa ninguém sair

2. **2° Andar — Setor Financeiro**
   - Inimigos: Contratos famintos, planilhas devoradoras, metas trimestrais
   - Boss: A Auditoria Final

3. **3° Andar — Recursos Humanos**
   - Inimigos: Feedbacks construtivos (que constroem pilhas de papel), avaliações fantasma
   - Boss: O Processo Seletivo Eterno

4. **4° Andar — TI / Suporte**
   - Inimigos: Tickets esquecidos, servidores zumbis, cabos enredados
   - Boss: O Sistema Legado

5. **5° Andar — Sala de Reuniões**
   - Inimigos: Ata que nunca acaba, agendas conflitantes
   - Boss: O PowerPoint Vivo

6. **6° Andar — Diretoria / C-Level**
   - Inimigos: Metas inatingíveis, reuniões executivas, KPIs esmagadores
   - Boss: O CEO (entidade final)

---

## 🛠️ Upgrades (Hábitos de Sobrevivência Corporativa)

| Upgrade | Efeito | Descrição |
|---|---|---|
| ☕ **Café Extra Forte** | Aumenta velocidade | "Seu coração acelera, mas você aguenta mais um sprint." |
| 📎 **Clips Reforçados** | Dano + alcance | "Projéteis de escritório: agora com mais poder de fogo." |
| 🪪 **Crachá de Acesso** | Desbloqueia áreas | "Acesso liberado para andares superiores." |
| 📠 **Scanner de Mesa** | Visão expandida | "Escaneie o ambiente para enxergar ameaças através das paredes." |
| 💻 **Notebook Superaquecido** | AOE de fogo | "A ventoinha vai explodir. Literalmente." |
| 📊 **Planilha Explosiva** | Dano em área | "VLOOKUP que detona tudo num raio de 3 metros." |
| ✉️ **Mensagem Passivo-Agressiva** | Slow + dano contínuo | "Conforme conversamos, segue anexo o dano." |
| 🖨️ **Impressora Fantasma** | Spawna aliados | "Copia e cola funcionários fantasma pra lutar ao seu lado." |
| 🔋 **Carregador de Celular** | Regeneração lenta | "Bateria social recarregando... 5%." |
| 🧾 **Aditivo Contratual** | Vida máxima + | "Cláusula de tolerância estendida." |

---

## 👾 Inimigos (Manifestações Corporativas)

| Inimigo | Comportamento | Origem |
|---|---|---|
| **E-mail Não Lido** | Persegue lentamente, explode em spam ao morrer | Caixa de entrada |
| **Planilha Devoradora** | Avança em linha reta, deixa rastro de números | Setor Financeiro |
| **Avaliação Fantasma** | Telegrafa, aparece e desaparece | RH |
| **Ticket Esquecido** | Rápido, baixo HP, vem em grupos | TI |
| **Ata de Reunião** | Grande, lento, spawna minions "itens de pauta" | Sala de Reuniões |
| **Meta Inatingível** | Gigante, persegue implacavelmente | Diretoria |
| **Contrato Faminto** | Prende no lugar se encostar | Financeiro |
| **Café Derramado** | Cria poças de lentidão no chão | Copa |
| **Feedback Construtivo** | Ataca em ondas de texto | RH |

---

## 👑 Chefes (Cargos Corrompidos)

| Chefe | Andar | Mecânica |
|---|---|---|
| **Porteiro Eterno** | Térreo | Bloqueia a saída, spawna pastas perdidas |
| **Auditoria Final** | Financeiro | Invencível enquanto planilhas estiverem ativas |
| **Processo Seletivo Eterno** | RH | Invoca candidatos-fantasma em ondas |
| **Sistema Legado** | TI | Escudo de compatibilidade, só toma dano de upgrades específicos |
| **PowerPoint Vivo** | Reuniões | Slides que viram projéteis, transitions que teleportam |
| **CEO (Cargo Executivo Ominoso)** | Diretoria | Múltiplas fases: reunião, metas, demissão |

---

## 🗂️ Estrutura do Projeto

```
res://
├── Scenes/
│   ├── Game/
│   │   ├── Game.tscn              # Cena principal do jogo
│   │   └── Game.cs
│   ├── Player/
│   │   ├── Employee.tscn          # Funcionário protagonista
│   │   └── Employee.cs
│   ├── Enemies/
│   │   ├── Email.cs               # Script base para inimigos
│   │   └── (cada inimigo tem sua cena)
│   ├── Weapons/
│   │   ├── Coffee.cs              # Café (bebida que dá speed)
│   │   ├── Paperclip.cs           # Clipe (projétil)
│   │   ├── Spreadsheet.cs         # Planilha explosiva
│   │   ├── PassiveAggressive.cs   # Mensagem passivo-agressiva
│   │   └── LaptopOverheat.cs      # Notebook superaquecido
│   ├── Pickups/
│   │   ├── CoffeeDrop.tscn        # Café (XP)
│   │   └── CoffeeDrop.cs
│   ├── UI/
│   │   ├── HUD.tscn / HUD.cs
│   │   ├── MainMenu.tscn / MainMenu.cs
│   │   ├── UpgradeChoice.tscn / UpgradeChoice.cs
│   │   ├── GameOver.tscn / GameOver.cs
│   │   └── Victory.tscn / Victory.cs
│   └── Floors/
│       ├── Floor1_Reception.tscn
│       ├── Floor2_Finance.tscn
│       ├── Floor3_HR.tscn
│       ├── Floor4_IT.tscn
│       ├── Floor5_Meetings.tscn
│       └── Floor6_Boardroom.tscn
├── Scripts/
│   ├── Systems/
│   │   ├── WaveSpawner.cs         # Spawna inimigos por onda/departamento
│   │   ├── CoffeeSystem.cs        # Sistema de XP (café)
│   │   ├── UpgradeSystem.cs       # Sistema de upgrades/loot
│   │   ├── FloorManager.cs        # Gerencia transição entre andares
│   │   └── TimerSystem.cs         # Contagem regressiva até o amanhecer
│   ├── Data/
│   │   ├── WeaponData.cs          # Dados das armas corporativas
│   │   ├── EnemyData.cs           # Dados dos inimigos corporativos
│   │   ├── UpgradeData.cs         # Dados dos upgrades
│   │   ├── FloorData.cs           # Dados dos andares
│   │   └── EmployeeData.cs        # Dados do funcionário (stats)
│   └── Utils/
│       ├── MathUtils.cs
│       └── ScreenUtils.cs
├── Resources/
│   ├── Weapons/
│   ├── Enemies/
│   ├── Upgrades/
│   ├── Floors/
│   └── EmployeePresets/
├── Art/
│   ├── Sprites/
│   │   ├── Player/
│   │   ├── Enemies/
│   │   ├── Weapons/
│   │   ├── Pickups/
│   │   └── UI/
│   ├── Tilesets/
│   │   ├── Office/
│   │   └── CorporateHell/
│   └── Particles/
├── Audio/
│   ├── SFX/
│   │   ├── coffee_pickup.wav
│   │   ├── email_spawn.wav
│   │   ├── typewriter.wav
│   │   ├── keyboard_clack.wav
│   │   └── ...
│   └── Music/
│       ├── floor1_reception.ogg
│       ├── floor2_finance.ogg
│       └── boss_ceo.ogg
└── Autoload/
    ├── GameManager.cs             # Estado global do jogo
    └── AudioManager.cs            # Gerenciamento de áudio
```

---

## 📐 Boas práticas Godot + C#

### Nodes vs C#
- **C#** para toda lógica de jogo (performance, type safety, estrutura de dados complexa)
- **GDScript** apenas para prototipação rápida de cenas muito simples
- Sempre usar `[Export]` ou `GetNode<T>()` tipado, nunca `GetNode()` sem tipo

### Convenções de nomenclatura
- **Pastas/Arquivos:** `PascalCase` (ex: `Employee.tscn`, `WaveSpawner.cs`)
- **Métodos:** `PascalCase` (ex: `TakeDamage()`, `SpawnWave()`)
- **Variáveis privadas:** `_camelCase` (ex: `_coffeeLevel`, `_currentFloor`)
- **Propriedades públicas:** `PascalCase` (ex: `MaxHealth`, `MoveSpeed`)
- **Signals:** prefixo `On` + evento + sufixo opcional (ex: `OnDamageTaken`, `OnFloorCleared`)
- **Nodes na árvore:** usar naming consistente com a cena (ex: `%CoffeeLevelLabel`, `%Sprite2D`)

### Godot 4.6 específico
- `CharacterBody2D` para Employee e Enemies (movimento + colisão)
- `Area2D` para armas (Coffee, Paperclip, Spreadsheet), pickups (CoffeeDrop), área de efeito (LaptopOverheat)
- `TileMapLayer` (novo 4.x) para o cenário do escritório com tiles modulares
- `AnimationTree` para blend de animações (andar, idle, ataque)
- `GPUParticles2D` para efeitos visuais (faíscas de notebook, explosão de planilha)
- Sistema de dados via `Resource` (Godot Resources) para `WeaponData`, `EnemyData`, `FloorData`
- Usar **unique names** (`%NodeName`) para referências na cena ao invés de paths relativos

### Signals padrão do projeto
```csharp
// GameManager
signal OnFloorCleared(int floorIndex)
signal OnGameOver()
signal OnVictory()
signal OnMinutePassed(int minute)

// Employee (Player)
signal OnDamageTaken(float newHealth)
signal OnLeveledUp(int newLevel)
signal OnUpgradeChosen(UpgradeData upgrade)

// Enemy (base)
signal OnEnemyDied(Vector2 position, float coffeeValue)
```

### Exemplo — Employee.cs (Player)
```csharp
using Godot;
using System;

public partial class Employee : CharacterBody2D
{
    [Export] public float MoveSpeed = 200f;
    [Export] public float MaxHealth = 100f;
    [Export] public float CoffeeLevel = 0f;     // XP
    [Export] public int EmployeeLevel = 1;

    private float _currentHealth;
    private Vector2 _moveDirection;

    [Signal]
    public delegate void OnDamageTakenEventHandler(float newHealth);

    [Signal]
    public delegate void OnLeveledUpEventHandler(int newLevel);

    public override void _Ready()
    {
        _currentHealth = MaxHealth;
    }

    public override void _Process(double delta)
    {
        HandleMovement();
        MoveAndSlide();
    }

    private void HandleMovement()
    {
        _moveDirection = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        Velocity = _moveDirection * MoveSpeed;
    }

    public void TakeDamage(float amount)
    {
        _currentHealth = Mathf.Max(0, _currentHealth - amount);
        EmitSignal(SignalName.OnDamageTaken, _currentHealth);

        if (_currentHealth <= 0)
            Die();
    }

    public void CollectCoffee(float amount)
    {
        CoffeeLevel += amount;
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        float needed = 10f * Mathf.Pow(1.15f, EmployeeLevel - 1);
        while (CoffeeLevel >= needed)
        {
            CoffeeLevel -= needed;
            EmployeeLevel++;
            EmitSignal(SignalName.OnLeveledUp, EmployeeLevel);
            needed = 10f * Mathf.Pow(1.15f, EmployeeLevel - 1);
        }
    }

    private void Die()
    {
        // GameManager detecta e chama game over
        QueueFree();
    }
}
```

### Exemplo — EnemyData.cs (Resource)
```csharp
using Godot;
using System;

[GlobalClass]
public partial class EnemyData : Resource
{
    [Export] public string EnemyName { get; set; } = "E-mail Não Lido";
    [Export] public float MaxHealth { get; set; } = 30f;
    [Export] public float MoveSpeed { get; set; } = 80f;
    [Export] public float Damage { get; set; } = 8f;
    [Export] public float CoffeeDrop { get; set; } = 5f;
    [Export] public PackedScene Scene { get; set; }
    [Export] public Texture2D Icon { get; set; }
    [Export] public string Department { get; set; } = "TI";
}
```

---

## 🎮 Input Map

| Action | Primary Key | Secondary |
|---|---|---|
| `move_left` | A | Left Arrow |
| `move_right` | D | Right Arrow |
| `move_up` | W | Up Arrow |
| `move_down` | S | Down Arrow |
| `interact` | E | Space |
| `pause` | Escape | P |

---

## ⚙️ Sistema de Progressão

### Loop Principal
```
Sobreviver 30 min (6 min por andar, 5 andares + boss final)
  │
  ├─ Cada andar = 1 fase
  │   ├─ Spawn contínuo de inimigos do departamento
  │   ├─ Mini-boss ao final de cada andar
  │   └─ Transição ao eliminar o boss
  │
  ├─ Café (XP) drops de inimigos
  │   └─ Level-up → escolha de 3 upgrades aleatórios
  │
  └─ Game Over se HP = 0
      └─ Recomeça do andar atual ou do início (checkpoints por andar liberado)
```

### Dificuldade Progressiva (por minuto)
- A cada minuto: +5% spawn rate, +3% dano inimigo, +2% velocidade inimigo
- A cada novo andar: nova pool de inimigos do departamento
- Boss a cada 6 minutos (ao limpar o andar)

---

## ☕ Upgrade Pool (ref. para o UpgradeSystem)
```
Nível 1-3: ☕ Café, 📎 Clipes, 🖨️ Impressora
Nível 4-7: 💻 Notebook, 📊 Planilha, ✉️ Passivo-Agressiva, 📠 Scanner
Nível 8+: 🔋 Carregador, 🧾 Aditivo, combos evoluídos
```

---

## 🏃 Comandos Godot CLI

```bash
# Abrir editor
godot --editor

# Rodar o projeto
godot

# Build C#
dotnet build

# Validar scripts
godot --check-only

# Exportar
godot --export "Windows Desktop" --headless
godot --export "Linux/X11" --headless
```
