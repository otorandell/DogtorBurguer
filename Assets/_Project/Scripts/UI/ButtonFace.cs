using System.Collections.Generic;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// The FACE of each kit button blank: the coloured body inside the dark outline, excluding the
    /// canvas's transparent margins and the outline itself. A word belongs centred on the face, not
    /// on the canvas — every "shadow is bottom-right" label nudge the screens used to carry was a
    /// hand-measured version of this table (2026-09-17). Insets are fractions of the canvas
    /// (x = left, y = top, z = right, w = bottom), measured by scratchpad/measure_button_faces.py —
    /// re-run it and paste when a blank changes. The bottom BEVEL is part of the body and is taken
    /// off uniformly by <see cref="UIStyles.BUTTON_FACE_LIP_FRAC"/>, so every blank places its word
    /// the same way (a per-art darker-band test mis-read two-tone faces like the red CREDITS blank
    /// and put its word higher than SHOP's). Unknown art = the whole rect.
    /// </summary>
    public static class ButtonFace
    {
        private static readonly Dictionary<string, Vector4> _insets = new()
        {
            ["ui_play_button"] = new Vector4(0.084f, 0.128f, 0.057f, 0.169f),
            ["ui_btn_blue_wide"] = new Vector4(0.031f, 0.093f, 0.044f, 0.117f),
            ["ui_btn_green_wide"] = new Vector4(0.031f, 0.093f, 0.044f, 0.117f),
            ["ui_menu_btn_credits"] = new Vector4(0.075f, 0.122f, 0.066f, 0.156f),
            ["ui_menu_btn_shop"] = new Vector4(0.057f, 0.128f, 0.059f, 0.147f),
            ["ui_btn_green_big"] = new Vector4(0.144f, 0.162f, 0.183f, 0.185f),
            ["ui_btn_green"] = new Vector4(0.075f, 0.122f, 0.066f, 0.156f),
            ["ui_btn_yellow"] = new Vector4(0.057f, 0.128f, 0.059f, 0.147f),
            ["ui_btn_confirm_buy"] = new Vector4(0.080f, 0.154f, 0.092f, 0.196f),
            ["ui_btn_confirm_cancel"] = new Vector4(0.097f, 0.132f, 0.111f, 0.158f),
            // The baked TV icon on the left is dark, so the measured face is the area RIGHT of it —
            // which is where the Watch label belongs anyway.
            ["ui_btn_blue_watch"] = new Vector4(0.358f, 0.106f, 0.064f, 0.170f),
            ["ui_btn_cream"] = new Vector4(0.071f, 0.083f, 0.068f, 0.166f),
        };

        /// <summary>The face rect, in the button's local px (centred origin), for <paramref name="art"/>
        /// shown at <paramref name="size"/>.</summary>
        public static Rect Of(Sprite art, Vector2 size)
        {
            Vector4 m = art != null && _insets.TryGetValue(art.name, out Vector4 v) ? v : Vector4.zero;
            float top = size.y * 0.5f - m.y * size.y;
            float bottom = -size.y * 0.5f + m.w * size.y;
            bottom += (top - bottom) * UIStyles.BUTTON_FACE_LIP_FRAC; // the bevel under the face
            return Rect.MinMaxRect(-size.x * 0.5f + m.x * size.x, bottom, size.x * 0.5f - m.z * size.x, top);
        }
    }
}
