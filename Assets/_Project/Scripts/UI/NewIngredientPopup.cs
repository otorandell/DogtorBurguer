using DG.Tweening;
using TMPro;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// "NEW INGREDIENT!" — a world-space callout when a level-up adds an ingredient type to the
    /// run (2026-09-26, playtest feedback: the unlocks read as a sudden difficulty spike). The
    /// newcomer's sprite (active skin) pops in under the words on the yellow glow plate, holds so
    /// it can be recognised, then rises and fades. Cosmetic and non-blocking — the game keeps
    /// running. Knobs: UIStyles.NEW_INGREDIENT_*, AnimConfig.NEW_INGREDIENT_*.
    /// </summary>
    public static class NewIngredientPopup
    {
        public static void Spawn(IngredientType type)
        {
            GameObject root = new GameObject("NewIngredientPopup");
            root.transform.position = new Vector3(0f, UIStyles.NEW_INGREDIENT_Y, 0f);

            GameObject iconObj = new GameObject("Ingredient");
            iconObj.transform.SetParent(root.transform, false);
            SpriteRenderer icon = iconObj.AddComponent<SpriteRenderer>();
            icon.sprite = Theme.Ingredient(type);
            icon.sortingOrder = Constants.SORT_FLOATING_TEXT + 1;
            SpriteFit.Height(icon, UIStyles.NEW_INGREDIENT_ICON_H);

            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(root.transform, false);
            textObj.transform.localPosition = new Vector3(0f, UIStyles.NEW_INGREDIENT_TEXT_DY, 0f);
            TextMeshPro label = WorldTextFactory.Create(textObj, Loc.Get(LocKey.NewIngredient),
                UIStyles.NEW_INGREDIENT_TEXT_SIZE, UIStyles.HUD_TEXT_FILL, Constants.SORT_FLOATING_TEXT + 2,
                new Vector2(UIStyles.NEW_INGREDIENT_TEXT_W, 1f), FontStyles.Normal, UIStyles.OUTLINE_WIDTH_WORLD);
            label.enableAutoSizing = true; // long translations shrink to the plate
            label.fontSizeMin = UIStyles.NEW_INGREDIENT_TEXT_SIZE * 0.5f;
            label.fontSizeMax = UIStyles.NEW_INGREDIENT_TEXT_SIZE;
            SpriteRenderer plate = WorldTextFactory.AttachPlate(textObj, "ui_popup_plate_mult",
                UIStyles.NEW_INGREDIENT_PLATE_H, Constants.SORT_FLOATING_TEXT + 2, Vector2.zero);

            root.transform.localScale = Vector3.zero;
            Sequence seq = DOTween.Sequence().SetLink(root);
            seq.Append(root.transform.DOScale(1f, AnimConfig.NEW_INGREDIENT_POP_DURATION).SetEase(Ease.OutBack));
            seq.AppendInterval(AnimConfig.NEW_INGREDIENT_HOLD);
            seq.Append(root.transform.DOMoveY(UIStyles.NEW_INGREDIENT_Y + AnimConfig.NEW_INGREDIENT_RISE,
                AnimConfig.NEW_INGREDIENT_FADE_DURATION).SetEase(Ease.InQuad));
            seq.Join(icon.DOFade(0f, AnimConfig.NEW_INGREDIENT_FADE_DURATION));
            seq.Join(label.DOFade(0f, AnimConfig.NEW_INGREDIENT_FADE_DURATION));
            if (plate != null) seq.Join(plate.DOFade(0f, AnimConfig.NEW_INGREDIENT_FADE_DURATION));
            seq.OnComplete(() => Object.Destroy(root));
        }
    }
}
