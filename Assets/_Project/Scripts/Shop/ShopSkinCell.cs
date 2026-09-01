using UnityEngine;
using UnityEngine.UI;

namespace DogtorBurguer
{
    /// <summary>
    /// One skin cell in a shop row (mock: lime name, preview on the authored checker box — green
    /// when equipped — and a green pill). Three states, refreshed after every transaction:
    /// equipped ("EQUIPPED"), owned
    /// ("EQUIP" — tap equips instantly, no dialog), or priced (cost + currency icon; tap buys and
    /// auto-equips, a failed buy shakes the cell).
    /// </summary>
    public class ShopSkinCell : MonoBehaviour
    {
        private Skin _skin;
        private ShopScreen _screen;
        private ShopCell _cell;
        private Image _box;

        public static void Create(RectTransform row, Skin skin, ShopScreen screen)
        {
            GameObject holder = new GameObject("Skin_" + skin.Id);
            holder.transform.SetParent(row, false);
            ShopSkinCell cell = holder.AddComponent<ShopSkinCell>();
            cell._skin = skin;
            cell._screen = screen;
            cell.Build();
            screen.RegisterRefresh(cell.Refresh);
        }

        private void Build()
        {
            // The cell is built as a child so ShopWidgets owns its layout; this holder only forwards
            // the LayoutElement size so the row lays the holder out like the cell.
            _cell = ShopWidgets.CreateCell(transform, "Cell", _skin.DisplayName, ShopWidgets.SkinBoxArt, OnClick);
            _box = _cell.Box.GetComponent<Image>();
            LayoutElement layout = gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = UIStyles.SHOP_CELL_W;
            layout.preferredHeight = ShopWidgets.CellHeight(withLabel: true, ShopWidgets.SkinBoxArt);
            RectTransform cellRect = _cell.Root;
            cellRect.anchorMin = cellRect.anchorMax = new Vector2(0.5f, 1f);
            cellRect.pivot = new Vector2(0.5f, 1f);
            cellRect.anchoredPosition = Vector2.zero;

            // The preview sized by height at native aspect (clamped to the box width for wide art) —
            // it may overflow the box top like the mock.
            Sprite preview = _skin.Preview;
            Vector2 size = UIFactory.SizeByHeight(preview, UIStyles.SHOP_SKIN_PREVIEW_H);
            if (size.x > UIStyles.SHOP_SKIN_PREVIEW_MAX_W)
                size *= UIStyles.SHOP_SKIN_PREVIEW_MAX_W / size.x;
            UIFactory.CreateImage(_cell.Box, "Preview", preview, new Vector2(0.5f, 0.5f),
                new Vector2(0f, UIStyles.SHOP_SKIN_PREVIEW_Y), size);

            Refresh();
        }

        private void Refresh()
        {
            bool equipped = Theme.IsEquipped(_skin);
            // The two checker arts differ by a few px, so re-derive the size with the sprite.
            _box.sprite = UiArt.Load(equipped ? ShopWidgets.SkinEquippedBoxArt : ShopWidgets.SkinBoxArt);
            _box.rectTransform.sizeDelta = ShopWidgets.BoxSize(equipped ? ShopWidgets.SkinEquippedBoxArt : ShopWidgets.SkinBoxArt);

            if (equipped)
                _cell.SetPill("EQUIPPED");
            else if (ShopService.OwnsSkin(_skin))
                _cell.SetPill("EQUIP");
            else
            {
                bool gems = _skin.Unlock == UnlockMethod.Gems;
                _cell.SetPill((gems ? _skin.GemCost : _skin.StarCost).ToString(), gems ? "ui_gem" : "ui_star");
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
