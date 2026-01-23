using UnityEngine;
using TMPro;
using DG.Tweening;

namespace DogtorBurguer
{
    public class FloatingText : MonoBehaviour
    {
        private static GameObject _prefab;

        public static void Spawn(Vector3 worldPos, string text, Color color, float fontSize = 4f)
        {
            GameObject obj = new GameObject("FloatingText");
            obj.transform.position = worldPos;

            TextMeshPro tmp = obj.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.sortingOrder = 100;

            RectTransform rect = tmp.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(4f, 1f);

            // Animate: float up and fade out
            Sequence seq = DOTween.Sequence();
            seq.Append(obj.transform.DOMoveY(worldPos.y + 1.5f, 0.8f).SetEase(Ease.OutQuad));
            seq.Join(tmp.DOFade(0f, 0.8f).SetDelay(0.3f));
            seq.OnComplete(() => Destroy(obj));
        }
    }
}
