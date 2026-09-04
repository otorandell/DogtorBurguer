using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace DogtorBurguer
{
    /// <summary>
    /// Manages wave preview indicators (blinking sprites above columns). A preview is reserved — and
    /// counts toward the next wave — the moment it's added, but its sprite stays hidden until the
    /// column's spawn zone is clear of falling pieces, so the ghost never overlaps a falling sprite.
    /// Placement (which column) is therefore unaffected by clearance; only the visual reveal is delayed.
    /// </summary>
    public class WavePreviewManager : MonoBehaviour
    {
        // One list of (preview, slot, revealed) entries — they can never desync (F-41).
        private readonly List<(GameObject preview, WaveSlot slot, bool revealed)> _entries = new();
        private Func<IngredientType, Sprite> _getSprite;

        public int Count => _entries.Count;

        public void Initialize(Func<IngredientType, Sprite> getSprite)
        {
            _getSprite = getSprite;
        }

        /// <summary>
        /// Adds a single preview, reserved immediately but hidden until revealed (queue refill).
        /// Returns false if no preview could be created (e.g. missing sprite) so callers don't spin.
        /// </summary>
        public bool AddPreview(WaveSlot slot)
        {
            GameObject preview = CreatePreview(slot.Type, slot.ColumnIndex);
            if (preview == null) return false;

            _entries.Add((preview, slot, false));
            return true;
        }

        /// <summary>
        /// Reveals (fades in + blinks) any reserved-but-hidden preview whose column is no longer blocked
        /// by a falling piece. This is purely visual — it never affects which columns are chosen.
        /// </summary>
        public void RevealCleared(HashSet<int> blockedColumns)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                if (entry.revealed || entry.preview == null) continue;
                if (blockedColumns.Contains(entry.slot.ColumnIndex)) continue;

                foreach (SpriteRenderer sr in entry.preview.GetComponentsInChildren<SpriteRenderer>())
                {
                    sr.color = new Color(1f, 1f, 1f, AnimConfig.PREVIEW_INITIAL_ALPHA);
                    sr.DOFade(AnimConfig.PREVIEW_FADE_MIN, AnimConfig.PREVIEW_FADE_DURATION)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.InOutSine);
                }
                _entries[i] = (entry.preview, entry.slot, true);
            }
        }

        /// <summary>True if a preview is already reserved above the given column.</summary>
        public bool HasPreviewInColumn(int columnIndex)
        {
            foreach (var (_, slot, _) in _entries)
            {
                if (slot.ColumnIndex == columnIndex)
                    return true;
            }
            return false;
        }

        /// <summary>Returns remaining (untapped) wave slots and clears all previews.</summary>
        public List<WaveSlot> ConsumeRemainingData()
        {
            var remaining = new List<WaveSlot>(_entries.Count);
            foreach (var (_, slot, _) in _entries)
                remaining.Add(slot);

            ClearPreviews();
            return remaining;
        }

        /// <summary>
        /// Tries to tap a preview at the given world position. Only revealed (visible) previews are
        /// tappable. Returns the tapped slot or null, removing the tapped preview.
        /// </summary>
        public WaveSlot? TryTap(Vector2 worldPos)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                if (!entry.revealed || entry.preview == null) continue;

                float dist = Vector2.Distance(worldPos, entry.preview.transform.position);
                if (dist < Constants.CELL_WIDTH * GameplayConfig.PREVIEW_TAP_RADIUS_MULT)
                {
                    DestroyPreview(entry.preview);
                    _entries.RemoveAt(i);
                    return entry.slot;
                }
            }
            return null;
        }

        public void ClearPreviews()
        {
            foreach (var (preview, _, _) in _entries)
            {
                if (preview != null)
                    DestroyPreview(preview);
            }
            _entries.Clear();
        }

        private void DestroyPreview(GameObject preview)
        {
            // Only the SpriteRenderers are tweened (DOFade); the transform never is.
            foreach (SpriteRenderer sr in preview.GetComponentsInChildren<SpriteRenderer>())
                sr.DOKill();
            Destroy(preview);
        }

        private GameObject CreatePreview(IngredientType type, int columnIndex)
        {
            Sprite sprite = _getSprite?.Invoke(type);
            if (sprite == null) return null;

            GameObject preview = new GameObject("WavePreview");
            float x = Constants.GRID_ORIGIN_X + (columnIndex * Constants.CELL_WIDTH);
            // Lowered a touch (PREVIEW_Y_OFFSET) so the ghost + its arrow back-picture fit the top band.
            float y = Constants.GRID_ORIGIN_Y + (Constants.MAX_ROWS * Constants.CELL_VISUAL_HEIGHT)
                      - Constants.PREVIEW_Y_OFFSET;
            preview.transform.position = new Vector3(x, y, 0);

            SpriteRenderer sr = preview.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = Constants.SORT_WAVE_PREVIEW;

            // Arrow back-picture (behind the ghost): yellow = regular, orange = bottom bun,
            // red = top bun — reads the piece kind at a glance before it drops.
            GameObject arrowObj = new GameObject("Arrow");
            arrowObj.transform.SetParent(preview.transform, false);
            SpriteRenderer arrow = arrowObj.AddComponent<SpriteRenderer>();
            arrow.sprite = UiArt.Load(ArrowArtFor(type));
            arrow.sortingOrder = Constants.SORT_WAVE_PREVIEW - 1;
            SpriteFit.Height(arrow, UIStyles.PREVIEW_ARROW_HEIGHT);

            // Start fully hidden (ghost + arrow); RevealCleared fades them in once the column's
            // spawn zone is clear of falling pieces.
            foreach (SpriteRenderer r in preview.GetComponentsInChildren<SpriteRenderer>())
                r.color = new Color(1f, 1f, 1f, 0f);

            return preview;
        }

        private static string ArrowArtFor(IngredientType type) => type switch
        {
            IngredientType.BunBottom => "ui_arrow_orange",
            IngredientType.BunTop => "ui_arrow_red",
            _ => "ui_arrow_yellow",
        };

        private void OnDestroy()
        {
            ClearPreviews();
        }

#if UNITY_EDITOR
        // Debug gizmo: the tap radius around each revealed (tappable) preview ghost.
        // Mirrors TryTap exactly — only revealed entries, same radius.
        private void OnDrawGizmos()
        {
            float radius = Constants.CELL_WIDTH * GameplayConfig.PREVIEW_TAP_RADIUS_MULT;
            Gizmos.color = GizmoStyles.PreviewTap;
            foreach (var (preview, _, revealed) in _entries)
            {
                if (!revealed || preview == null) continue;
                Gizmos.DrawWireSphere(preview.transform.position, radius);
            }
        }
#endif
    }
}
