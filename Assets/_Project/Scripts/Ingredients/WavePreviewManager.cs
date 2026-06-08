using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace DogtorBurguer
{
    /// <summary>
    /// Manages wave preview indicators (blinking sprites above columns).
    /// Handles display, cleanup, and tap-to-spawn detection for previews.
    /// </summary>
    public class WavePreviewManager : MonoBehaviour
    {
        // One list of (preview, slot) pairs — they can never desync (F-41).
        private readonly List<(GameObject preview, WaveSlot slot)> _entries = new();
        private Func<IngredientType, Sprite> _getSprite;

        public bool HasPreviews => _entries.Count > 0;

        public void Initialize(Func<IngredientType, Sprite> getSprite)
        {
            _getSprite = getSprite;
        }

        /// <summary>
        /// Shows blinking preview indicators for the given wave data.
        /// Takes ownership of the data until consumed or cleared.
        /// </summary>
        public void ShowPreviews(List<WaveSlot> waveData)
        {
            ClearPreviews();

            foreach (WaveSlot slot in waveData)
            {
                GameObject preview = CreatePreview(slot.Type, slot.ColumnIndex);
                if (preview == null) continue;

                _entries.Add((preview, slot));

                SpriteRenderer sr = preview.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.DOFade(AnimConfig.PREVIEW_FADE_MIN, AnimConfig.PREVIEW_FADE_DURATION)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.InOutSine);
                }
            }
        }

        /// <summary>Returns remaining (untapped) wave slots and clears all previews.</summary>
        public List<WaveSlot> ConsumeRemainingData()
        {
            var remaining = new List<WaveSlot>(_entries.Count);
            foreach (var (_, slot) in _entries)
                remaining.Add(slot);

            ClearPreviews();
            return remaining;
        }

        /// <summary>
        /// Tries to tap a preview at the given world position. Returns the tapped slot or
        /// null, removing the tapped preview.
        /// </summary>
        public WaveSlot? TryTap(Vector2 worldPos)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                GameObject preview = _entries[i].preview;
                if (preview == null) continue;

                float dist = Vector2.Distance(worldPos, preview.transform.position);
                if (dist < Constants.CELL_WIDTH * GameplayConfig.PREVIEW_TAP_RADIUS_MULT)
                {
                    WaveSlot slot = _entries[i].slot;
                    DestroyPreview(preview);
                    _entries.RemoveAt(i);
                    return slot;
                }
            }
            return null;
        }

        public void ClearPreviews()
        {
            foreach (var (preview, _) in _entries)
            {
                if (preview != null)
                    DestroyPreview(preview);
            }
            _entries.Clear();
        }

        private void DestroyPreview(GameObject preview)
        {
            // Only the SpriteRenderer is tweened (DOFade); the transform never is.
            SpriteRenderer sr = preview.GetComponent<SpriteRenderer>();
            if (sr != null) sr.DOKill();
            Destroy(preview);
        }

        private GameObject CreatePreview(IngredientType type, int columnIndex)
        {
            Sprite sprite = _getSprite?.Invoke(type);
            if (sprite == null) return null;

            GameObject preview = new GameObject("WavePreview");
            float x = Constants.GRID_ORIGIN_X + (columnIndex * Constants.CELL_WIDTH);
            float y = Constants.GRID_ORIGIN_Y + (Constants.MAX_ROWS * Constants.CELL_VISUAL_HEIGHT);
            preview.transform.position = new Vector3(x, y, 0);

            SpriteRenderer sr = preview.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = Constants.SORT_WAVE_PREVIEW;
            sr.color = new Color(1f, 1f, 1f, AnimConfig.PREVIEW_INITIAL_ALPHA);

            return preview;
        }

        private void OnDestroy()
        {
            ClearPreviews();
        }
    }
}
