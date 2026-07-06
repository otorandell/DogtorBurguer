using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DogtorBurguer
{
    /// <summary>
    /// One skin cell in a shop row. Three states, refreshed after every transaction:
    /// equipped (highlight + "EQUIPPED"), owned (tap to equip, instant — no dialog), or priced
    /// (currency icon + cost; tap buys and auto-equips, a failed buy shakes the cell).
    /// </summary>
    public class ShopSkinCell : MonoBehaviour
    {
        private Skin _skin;
        private ShopScreen _screen;
        private Image _background;
        private Image _priceIcon;
        private TextMeshProUGUI _stateLabel;

        public static void Create(RectTransform row, Skin skin, ShopScreen screen)
        {
            GameObject obj = new GameObject("Skin_" + skin.Id);
            obj.transform.SetParent(row, false);
            ShopSkinCell cell = obj.AddComponent<ShopSkinCell>();
            cell._skin = skin;
            cell._screen = screen;
            cell.Build();
            screen.RegisterRefresh(cell.Refresh);
        }

        private void Build()
        {
            Vector2 cellSize = UIStyles.SHOP_SKIN_CELL_SIZE;
            LayoutElement layout = gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = cellSize.x;
            layout.preferredHeight = cellSize.y;

            _background = gameObject.AddComponent<Image>();
            Button button = gameObject.AddComponent<Button>();
            button.targetGraphic = _background;
            button.onClick.AddListener(OnClick);

            // Preview sized by height at native aspect, shrunk to fit the cell if the art is wide.
            Sprite preview = _skin.Preview;
            float aspect = preview != null ? preview.rect.width / preview.rect.height : 1f;
            float height = UIStyles.SHOP_SKIN_PREVIEW_H;
            float width = height * aspect;
            float maxWidth = cellSize.x - 2f * UIStyles.SHOP_SKIN_PREVIEW_MARGIN;
            if (width > maxWidth)
            {
                width = maxWidth;
                height = width / aspect;
            }
            UIFactory.CreateImage(transform, "Preview", preview, new Vector2(0.5f, 1f),
                new Vector2(0f, -UIStyles.SHOP_SKIN_PREVIEW_MARGIN - UIStyles.SHOP_SKIN_PREVIEW_H * 0.5f),
                new Vector2(width, height));

            TextMeshProUGUI name = UIFactory.CreateText(transform, _skin.DisplayName, Vector2.zero,
                new Vector2(cellSize.x - 8f, 34f), UIStyles.SHOP_SKIN_NAME_SIZE,
                FontStyles.Bold, UIStyles.TEXT_UI);
            RectTransform nameRect = name.rectTransform;
            nameRect.anchorMin = nameRect.anchorMax = new Vector2(0.5f, 1f);
            nameRect.pivot = new Vector2(0.5f, 1f);
            nameRect.anchoredPosition = new Vector2(0f, UIStyles.SHOP_SKIN_NAME_Y);

            // Bottom state row: currency icon (priced state only) + label.
            _priceIcon = UIFactory.CreateImage(transform, "PriceIcon", null, new Vector2(0.5f, 0f),
                new Vector2(UIStyles.SHOP_SKIN_PRICE_ICON_X, UIStyles.SHOP_SKIN_STATE_Y), Vector2.one);
            _stateLabel = UIFactory.CreateText(transform, "", Vector2.zero,
                new Vector2(cellSize.x - 24f, 26f), UIStyles.SHOP_SKIN_STATE_SIZE, FontStyles.Bold);
            RectTransform stateRect = _stateLabel.rectTransform;
            stateRect.anchorMin = stateRect.anchorMax = new Vector2(0.5f, 0f);
            stateRect.pivot = new Vector2(0.5f, 0.5f);
            stateRect.anchoredPosition = new Vector2(UIStyles.SHOP_SKIN_STATE_X, UIStyles.SHOP_SKIN_STATE_Y);

            Refresh();
        }

        private void Refresh()
        {
            if (Theme.IsEquipped(_skin))
            {
                _background.color = UIStyles.SHOP_CARD_BG_EQUIPPED;
                _priceIcon.enabled = false;
                _stateLabel.text = "EQUIPPED";
                _stateLabel.color = UIStyles.SHOP_EQUIPPED_COLOR;
            }
            else if (ShopService.OwnsSkin(_skin))
            {
                _background.color = UIStyles.SHOP_CARD_BG;
                _priceIcon.enabled = false;
                _stateLabel.text = "EQUIP";
                _stateLabel.color = UIStyles.TEXT_UI;
            }
            else
            {
                bool gems = _skin.Unlock == UnlockMethod.Gems;
                Sprite icon = UiArt.Load(gems ? "ui_gem" : "ui_star");
                float aspect = icon != null ? icon.rect.width / icon.rect.height : 1f;
                _background.color = UIStyles.SHOP_CARD_BG;
                _priceIcon.enabled = true;
                _priceIcon.sprite = icon;
                _priceIcon.rectTransform.sizeDelta =
                    new Vector2(UIStyles.SHOP_SKIN_PRICE_ICON_H * aspect, UIStyles.SHOP_SKIN_PRICE_ICON_H);
                _stateLabel.text = (gems ? _skin.GemCost : _skin.StarCost).ToString();
                _stateLabel.color = UIStyles.TEXT_UI;
            }
        }

        private void OnClick()
        {
            if (Theme.IsEquipped(_skin)) return;

            if (ShopService.OwnsSkin(_skin))
            {
                ShopService.TryEquip(_skin);
                _screen.NotifyChanged();
                return;
            }

            if (ShopService.TryBuySkin(_skin)) _screen.NotifyChanged();
            else ShopScreen.Deny(transform);
        }
    }
}
