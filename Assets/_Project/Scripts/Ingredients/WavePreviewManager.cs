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
        private List<(IngredientType type, int columnIndex)> _data = new();
        private List<GameObject> _previews = new();
        private Func<IngredientType, Sprite> _getSprite;

        public bool HasPreviews => _previews.Count > 0;

        public void Initialize(Func<IngredientType, Sprite> getSprite)
        {
            _getSprite = getSprite;
        }

        /// <summary>
        /// Shows blinking preview indicators for the given wave data.
        /// Takes ownership of the data until consumed or cleared.
        /// </summary>
        public void ShowPreviews(List<(IngredientType type, int columnIndex)> waveData)
        {
            ClearPreviews();
            _data = new List<(IngredientType, int)>(waveData);

            foreach (var (type, colIdx) in _data)
            {
                Column col = GridManager.Instance?.GetColumn(colIdx);
                if (col == null) continue;

                GameObject preview = CreatePreview(type, col);
                if (preview != null)
                {
                    _previews.Add(preview);
                    SpriteRenderer sr = preview.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.DOFade(AnimConfig.PREVIEW_FADE_MIN, AnimConfig.PREVIEW_FADE_DURATION)
                            .SetLoops(-1, LoopType.Yoyo)
                            .SetEase(Ease.InOutSine);
                    }
                }
            }
        }

        /// <summary>
        /// Returns remaining (untapped) wave data and clears all previews.
        /// </summary>
        public List<(IngredientType type, int columnIndex)> ConsumeRemainingData()
        {
            var remaining = new List<(IngredientType, int)>(_data);
            ClearPreviews();
            return remaining;
        }

        /// <summary>
        /// Tries to tap a preview at the given world position.
        /// Returns the tapped entry or null. Removes the tapped preview.
        /// </summary>
        public (IngredientType type, int columnIndex)? TryTap(Vector2 worldPos)
        {
            for (int i = 0; i < _previews.Count; i++)
            {
                GameObject preview = _previews[i];
                if (preview == null) continue;

                float dist = Vector2.Distance(worldPos, preview.transform.position);
                if (dist < Constants.CELL_WIDTH * GameplayConfig.PREVIEW_TAP_RADIUS_MULT)
                {
                    var entry = _data[i];

                    preview.transform.DOKill();
                    SpriteRenderer sr = preview.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.DOKill();
                    Destroy(preview);

                    _previews.RemoveAt(i);
                    _data.RemoveAt(i);

                    return entry;
                }
            }
            return null;
        }

        public void ClearPreviews()
        {
            foreach (var preview in _previews)
            {
                if (preview != null)
                {
                    preview.transform.DOKill();
                    SpriteRenderer sr = preview.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.DOKill();
                    Destroy(preview);
                }
            }
            _previews.Clear();
            _data.Clear();
        }

        private GameObject CreatePreview(IngredientType type, Column column)
        {
            Sprite sprite = _getSprite?.Invoke(type);
            if (sprite == null) return null;

            GameObject preview = new GameObject("WavePreview");
            float x = Constants.GRID_ORIGIN_X + (column.ColumnIndex * Constants.CELL_WIDTH);
            float y = Constants.GRID_ORIGIN_Y + (Constants.MAX_ROWS * Constants.CELL_VISUAL_HEIGHT);
            preview.transform.position = new Vector3(x, y, 0);

            SpriteRenderer sr = preview.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 90;
            sr.color = new Color(1f, 1f, 1f, AnimConfig.PREVIEW_INITIAL_ALPHA);

            return preview;
        }

        private void OnDestroy()
        {
            ClearPreviews();
        }
    }
}
