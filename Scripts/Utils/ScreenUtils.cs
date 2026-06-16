using Godot;
using System;

namespace CorporateSurvivors.Utils;

/// <summary>
/// Utilitários para tela, câmera, viewport.
/// </summary>
public static class ScreenUtils
{
    private static Viewport _viewport;

    /// <summary>
    /// Inicializa com o viewport principal.
    /// </summary>
    public static void Initialize(Viewport viewport)
    {
        _viewport = viewport;
    }

    /// <summary>
    /// Retorna o tamanho da tela em pixels.
    /// </summary>
    public static Vector2 GetScreenSize()
    {
        if (_viewport == null)
            return new Vector2(1920, 1080); // fallback
        return _viewport.GetVisibleRect().Size;
    }

    /// <summary>
    /// Retorna os limites da tela no espaço global.
    /// </summary>
    public static Rect2 GetScreenRect()
    {
        Vector2 size = GetScreenSize();
        return new Rect2(Vector2.Zero, size);
    }

    /// <summary>
    /// Retorna uma posição aleatória dentro da tela.
    /// </summary>
    public static Vector2 RandomScreenPosition()
    {
        Vector2 size = GetScreenSize();
        return new Vector2(
            GD.Randf() * size.X,
            GD.Randf() * size.Y
        );
    }

    /// <summary>
    /// Retorna uma posição aleatória na borda da tela.
    /// </summary>
    public static Vector2 RandomEdgePosition()
    {
        Vector2 size = GetScreenSize();
        int edge = GD.Randi() % 4;
        return edge switch
        {
            0 => new Vector2(GD.Randf() * size.X, 0),                         // Topo
            1 => new Vector2(GD.Randf() * size.X, size.Y),                     // Base
            2 => new Vector2(0, GD.Randf() * size.Y),                          // Esquerda
            3 => new Vector2(size.X, GD.Randf() * size.Y),                      // Direita
            _ => Vector2.Zero
        };
    }

    /// <summary>
    /// Verifica se uma posição global está dentro dos limites da tela.
    /// Útil para spawn de inimigos (fora da tela).
    /// </summary>
    public static bool IsOnScreen(Vector2 globalPosition, Camera2D camera)
    {
        if (camera == null) return true;
        Rect2 screenRect = camera.GetViewportRect();
        return screenRect.HasPoint(camera.ToLocal(globalPosition));
    }

    /// <summary>
    /// Converte posição global para posição de tela (UI).
    /// </summary>
    public static Vector2 GlobalToScreen(Vector2 globalPosition, Camera2D camera)
    {
        if (camera == null) return globalPosition;
        return camera.UnprojectPosition(globalPosition);
    }

    /// <summary>
    /// Retorna a posição do mouse no espaço global.
    /// </summary>
    public static Vector2 GetMouseGlobalPosition()
    {
        if (_viewport == null) return Vector2.Zero;
        return _viewport.GetMousePosition();
    }
}
