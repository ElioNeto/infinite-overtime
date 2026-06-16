---
description: "Checar erros de script Godot"
---

Validar o projeto Godot em busca de erros nos scripts.

```bash
godot --check-only

# Com rebuild C# antes
dotnet build && godot --check-only
```