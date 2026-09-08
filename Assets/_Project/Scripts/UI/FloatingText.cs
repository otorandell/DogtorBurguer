using UnityEngine;
using TMPro;
using DG.Tweening;

namespace DogtorBurguer
{
    public class FloatingText : MonoBehaviour
    {
        /// <summary>Rise-and-fade text; pass <paramref name="plateArt"/> (a popup blob from
        /// Resources/UI) to seat it on a glow plate — multipliers ride the yellow one. Pass
        /// <paramref name="iconArt"/> (a Resources/UI sprite name) to concatenate a small icon
        /// after the text — the fairy reward popups: "5 [gem]", "1 [ketchup]" (2026-09-08).</summary>
        public static void Spawn(Vector3 worldPos, string text, Color color,
            float fontSize = UIStyles.WORLD_FLOATING_TEXT_SIZE, string plateArt = null,
            string iconArt = null)
        {
            GameObject obj = new GameObject("FloatingText");
            obj.transform.position = worldPos;

            TextMeshPro tmp = WorldTextFactory.Create(obj, text, fontSize, color,
                Constants.SORT_FLOATING_TEXT, new Vector2(4f, 1f), FontStyles.Normal, UIStyles.OUTLINE_WIDTH_WORLD);

            SpriteRenderer plate = plateArt != null
                ? WorldTextFactory.AttachPlate(obj, plateArt, UIStyles.PLATE_FLOAT_H, Constants.SORT_FLOATING_TEXT, Vector2.zero)
                : null;

            // Inline icon: seated right after the text, the pair re-centered as one.
            SpriteRenderer icon = null;
            if (iconArt != null)
            {
                Sprite sprite = UiArt.Load(iconArt);
                if (sprite != null)
                {
                    float textW = tmp.GetPreferredValues(text).x;
                    GameObject iconObj = new GameObject("Icon");
                    iconObj.transform.SetParent(obj.transform, false);
                    icon = iconObj.AddComponent<SpriteRenderer>();
                    icon.sprite = sprite;
                    icon.sortingOrder = Constants.SORT_FLOATING_TEXT + 1;
                    SpriteFit.Height(icon, UIStyles.FLOAT_ICON_H);
                    float iconW = icon.bounds.size.x;
                    float shift = (iconW + UIStyles.FLOAT_ICON_GAP) * 0.5f;
                    tmp.transform.localPosition += Vector3.left * shift;
                    iconObj.transform.localPosition = new Vector3(
                        textW * 0.5f - shift + UIStyles.FLOAT_ICON_GAP + iconW * 0.5f,
                        UIStyles.FLOAT_ICON_Y, 0f);
                }
            }

            // Animate: float up and fade out
            Sequence seq = DOTween.Sequence();
            seq.Append(obj.transform.DOMoveY(worldPos.y + AnimConfig.FLOATING_TEXT_RISE, AnimConfig.FLOATING_TEXT_DURATION).SetEase(Ease.OutQuad));
            seq.Join(tmp.DOFade(0f, AnimConfig.FLOATING_TEXT_DURATION).SetDelay(AnimConfig.FLOATING_TEXT_FADE_DELAY));
            if (plate != null)
                seq.Join(plate.DOFade(0f, AnimConfig.FLOATING_TEXT_DURATION).SetDelay(AnimConfig.FLOATING_TEXT_FADE_DELAY));
            if (icon != null)
                seq.Join(icon.DOFade(0f, AnimConfig.FLOATING_TEXT_DURATION).SetDelay(AnimConfig.FLOATING_TEXT_FADE_DELAY));
            seq.OnComplete(() => Destroy(obj));
        }
    }
}
