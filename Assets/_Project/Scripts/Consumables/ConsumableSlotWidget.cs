using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DogtorBurguer
{
    /// <summary>
    /// One consumable inventory slot (screen-space UGUI): a round plate, the consumable icon, and a
    /// corner badge — the red num box with the live count, or the green plus box when empty. Built and
    /// driven by <see cref="ConsumableInventoryView"/>; hit-tested by the drag controller via Contains.
    /// </summary>
    public class ConsumableSlotWidget
    {
        public ConsumableType Type { get; }

        private readonly Camera _uiCamera;
        private readonly RectTransform _plateRect;
        private readonly Image _icon;
        private readonly Image _numBox;
        private readonly Image _plusBox;
        private readonly TextMeshProUGUI _count;

        public ConsumableSlotWidget(Transform parent, Camera uiCamera, ConsumableType type, Vector2 pos)
        {
            Type = type;
            _uiCamera = uiCamera; // the canvas is Screen Space - Camera; a null camera here would hit-test wrong

            Image plate = UIFactory.CreateImage(parent, $"Slot_{type}", UiArt.Load("ui_consumable_box"),
                new Vector2(0f, 1f), pos, UIStyles.CONSUMABLE_SLOT_SIZE);
            _plateRect = plate.rectTransform;

            // Consumable icon — the splashy slot version from the Consumables UI Kit (the plainer
            // Rewards badge stays on the fairy/ghost/faller). Sized by height preserving aspect.
            Sprite iconSprite = UiArt.Load("ui_consumable_" + type.ToString().ToLowerInvariant());
            float aspect = iconSprite != null ? iconSprite.rect.width / iconSprite.rect.height : 1f;
            Vector2 iconSize = new(UIStyles.CONSUMABLE_SLOT_ICON_H * aspect, UIStyles.CONSUMABLE_SLOT_ICON_H);
            _icon = UIFactory.CreateImage(plate.transform, "Icon", iconSprite,
                new Vector2(0.5f, 0.5f), UIStyles.CONSUMABLE_ICON_OFFSET, iconSize);

            // Corner badge (bottom-right): the num box + count when stocked, or the plus box when
            // empty. The plus box deep-links into the Shop (paused) — the standard "buy more" path.
            _numBox = MakeBadge(plate.transform, "NumBox", "ui_consumable_num");
            _plusBox = MakeBadge(plate.transform, "PlusBox", "ui_consumable_plus");
            Button plusButton = _plusBox.gameObject.AddComponent<Button>();
            plusButton.targetGraphic = _plusBox;
            plusButton.onClick.AddListener(() => ShopScreen.OpenInGame(scrollToPowerUps: true));

            _count = UIFactory.CreateText(_numBox.transform, "0", Vector2.zero,
                new Vector2(UIStyles.CONSUMABLE_BADGE_H, UIStyles.CONSUMABLE_BADGE_H),
                UIStyles.CONSUMABLE_COUNT_SIZE, FontStyles.Bold);
            UIFactory.StyleHudText(_count);
            _count.textWrappingMode = TextWrappingModes.NoWrap;
        }

        private static Image MakeBadge(Transform plate, string name, string art)
        {
            Sprite sprite = UiArt.Load(art);
            float aspect = sprite != null ? sprite.rect.width / sprite.rect.height : 1f;
            Vector2 size = new(UIStyles.CONSUMABLE_BADGE_H * aspect, UIStyles.CONSUMABLE_BADGE_H);
            return UIFactory.CreateImage(plate, name, sprite,
                new Vector2(0.5f, 0.5f), UIStyles.CONSUMABLE_BADGE_OFFSET, size);
        }

        /// <summary>Stocked → num box + count; empty → green plus box. The icon always shows (a carry
        /// hides it transiently; this restores it when the consume that ended the carry refreshes).</summary>
        public void Refresh(int count)
        {
            _icon.enabled = true;
            bool stocked = count > 0;
            _numBox.enabled = stocked;
            _count.enabled = stocked;
            _plusBox.enabled = !stocked;
            if (stocked) _count.text = count.ToString();
        }

        public void SetIconHidden(bool hidden) => _icon.enabled = !hidden;

        public bool Contains(Vector2 screenPos) =>
            RectTransformUtility.RectangleContainsScreenPoint(_plateRect, screenPos, _uiCamera);
    }
}
