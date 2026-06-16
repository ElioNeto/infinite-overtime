---
description: "Rodar testes Godot"
---

Rodar testes para este projeto Godot.

## GUT (Godot Unit Test) — se configurado
```bash
godot --addons/gut/gut_cmdln.gd --path .
```

## Editor script test runner (se configurado com cenas de teste)
```bash
godot --script res://test_runner.gd --headless --path .
```

## Verificar erros de script
```bash
godot --check-only
```

## Build C# (sempre bom rodar antes)
```bash
dotnet build
```
