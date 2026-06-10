using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Colors for the editor-only input-area debug gizmos — one per interaction, so each
    /// clickable zone reads at a glance. Drawn by the component that owns each hit-test
    /// (IngredientSpawner = falling, WavePreviewManager = preview, TouchInputHandler = chef).
    /// </summary>
    public static class GizmoStyles
    {
        public static readonly Color FallingTap = Color.green;   // tap a falling piece → fast-drop
        public static readonly Color PreviewTap = Color.yellow;  // tap a revealed preview ghost → spawn
        public static readonly Color ChefFlip = Color.magenta;   // tap near the chef → swap plates
        public static readonly Color ChefDrag = Color.cyan;      // tap a side / swipe → move the chef
    }
}
