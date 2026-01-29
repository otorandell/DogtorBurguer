using UnityEngine;
using TMPro;
using DG.Tweening;

namespace DogtorBurguer
{
    public class FloatingText : MonoBehaviour
    {
        private static GameObject _prefab;

        public static void Spawn(Vector3 worldPos, string text, Color color, float fontSize = UIStyles.WORLD_FLOATING_TEXT_SIZE)
        {
            GameObject obj = new GameObject("FloatingText");
            obj.transform.position = worldPos;

            TextMeshPro tmp = obj.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.sortingOrder = 100;
            tmp.outlineWidth = UIStyles.OUTLINE_WIDTH_WORLD;
            tmp.outlineColor = UIStyles.OUTLINE_COLOR;

            RectTransform rect = tmp.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(4f, 1f);

            // Animate: float up and fade out
            Sequence seq = DOTween.Sequence();
            seq.Append(obj.transform.DOMoveY(worldPos.y + AnimConfig.FLOATING_TEXT_RISE, AnimConfig.FLOATING_TEXT_DURATION).SetEase(Ease.OutQuad));
            seq.Join(tmp.DOFade(0f, AnimConfig.FLOATING_TEXT_DURATION).SetDelay(AnimConfig.FLOATING_TEXT_FADE_DELAY));
            seq.OnComplete(() => Destroy(obj));
        }
    }
}
