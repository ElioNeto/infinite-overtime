---
description: "Build C# e exportar o jogo"
---

Build do projeto C# e exportação do jogo Godot.

## Build C#
```bash
dotnet build
```

## Export (cria pasta export/ na raiz do projeto)
```bash
mkdir -p export
godot --export "Windows Desktop" --headless --path .
```

## Apenas checar erros de script
```bash
godot --check-only
```
