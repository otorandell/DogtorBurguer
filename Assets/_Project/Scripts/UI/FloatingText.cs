using UnityEngine;
using TMPro;
using DG.Tweening;

namespace DogtorBurguer
{
    public class FloatingText : MonoBehaviour
    {
        /// <summary>Rise-and-fade text; pass <paramref name="plateArt"/> (a popup blob from
        /// Resources/UI) to seat it on a glow plate — multipliers ride the yellow one.</summary>
        public static void Spawn(Vector3 worldPos, string text, Color color,
            float fontSize = UIStyles.WORLD_FLOATING_TEXT_SIZE, string plateArt = null)
        {
            GameObject obj = new GameObject("FloatingText");
            obj.transform.position = worldPos;

            TextMeshPro tmp = WorldTextFactory.Create(obj, text, fontSize, color,
                Constants.SORT_FLOATING_TEXT, new Vector2(4f, 1f), FontStyles.Normal, UIStyles.OUTLINE_WIDTH_WORLD);

            SpriteRenderer plate = plateArt != null
                ? WorldTextFactory.AttachPlate(obj, plateArt, UIStyles.PLATE_FLOAT_H, Constants.SORT_FLOATING_TEXT, Vector2.zero)
                : null;

            // Animate: float up and fade out
            Sequence seq = DOTween.Sequence();
            seq.Append(obj.transform.DOMoveY(worldPos.y + AnimConfig.FLOATING_TEXT_RISE, AnimConfig.FLOATING_TEXT_DURATION).SetEase(Ease.OutQuad));
            seq.Join(tmp.DOFade(0f, AnimConfig.FLOATING_TEXT_DURATION).SetDelay(AnimConfig.FLOATING_TEXT_FADE_DELAY));
            if (plate != null)
                seq.Join(plate.DOFade(0f, AnimConfig.FLOATING_TEXT_DURATION).SetDelay(AnimConfig.FLOATING_TEXT_FADE_DELAY));
            seq.OnComplete(() => Destroy(obj));
        }
    }
}
