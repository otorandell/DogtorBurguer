using UnityEngine;
using UnityEngine.UI;

namespace DogtorBurguer
{
    /// <summary>
    /// One skin cell in a shop row (mock: lime name, preview on a pastel checker in the cream box,
    /// green pill). Three states, refreshed after every transaction: equipped ("EQUIPPED"), owned
    /// ("EQUIP" — tap equips instantly, no dialog), or priced (cost + currency icon; tap buys and
    /// auto-equips, a failed buy shakes the cell).
    /// </summary>
    public class ShopSkinCell : MonoBehaviour
    {
        private Skin _skin;
        private ShopScreen _screen;
        private ShopCell _cell;

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
            _cell = ShopWidgets.CreateCell(transform, "Cell", _skin.DisplayName, OnClick);
            LayoutElement layout = gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = UIStyles.SHOP_CELL_W;
            layout.preferredHeight = ShopWidgets.CellHeight(withLabel: true);
            RectTransform cellRect = _cell.Root;
            cellRect.anchorMin = cellRect.anchorMax = new Vector2(0.5f, 1f);
            cellRect.pivot = new Vector2(0.5f, 1f);
            cellRect.anchoredPosition = Vector2.zero;

            // Pastel checker inside the box, then the preview sized by height at native aspect
            // (clamped to the box width for wide art) — it may overflow the box top like the mock.
            float inset = UIStyles.SHOP_CELL_CHECKER_INSET;
            Vector2 checkerSize = new(UIStyles.SHOP_CELL_W - 2f * inset, UIStyles.SHOP_CELL_BOX_H - 2f * inset);
            int cells = UIStyles.SHOP_CELL_CHECKER_CELLS;
            Image checker = UIFactory.CreateImage(_cell.Box, "Checker",
                SpriteFactory.Checker(cells, Mathf.Max(1, Mathf.RoundToInt(cells * checkerSize.y / checkerSize.x))),
                new Vector2(0.5f, 0.5f), Vector2.zero, checkerSize);
            checker.color = UIStyles.SHOP_CELL_CHECKER;

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
            if (Theme.IsEquipped(_skin))
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
