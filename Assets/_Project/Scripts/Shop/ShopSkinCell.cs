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
            // Dogtor cells sit a little lower in their row (room for the head that rises over the box);
            // the holder grows by the same amount so the slab grows with them.
            float drop = _skin.Slot == SkinSlot.ChefSkin ? UIStyles.SHOP_SKIN_CHEF_CELL_DROP : 0f;
            layout.preferredHeight = ShopWidgets.CellHeight(withLabel: true, ShopWidgets.SkinBoxArt) + drop;
            RectTransform cellRect = _cell.Root;
            cellRect.anchorMin = cellRect.anchorMax = new Vector2(0.5f, 1f);
            cellRect.pivot = new Vector2(0.5f, 1f);
            cellRect.anchoredPosition = new Vector2(0f, -drop);

            // Ingredient slots preview on a plate (like the Special Order stack); the chef doesn't.
            if (_skin.Slot != SkinSlot.ChefSkin)
            {
                Sprite plate = Theme.Plate;
                if (plate != null)
                    UIFactory.CreateImage(_cell.Box, "Plate", plate, new Vector2(0.5f, 0.5f),
                        new Vector2(0f, UIStyles.SHOP_SKIN_PLATE_Y),
                        UIFactory.SizeByWidth(plate, UIStyles.SHOP_SKIN_PLATE_W));
            }

            if (_skin.Slot == SkinSlot.BunSkin && _skin.SecondarySprite != null)
            {
                // Buns preview as the pair, both halves sized by WIDTH — equal heights made the
                // squatter bottom bun read smaller. Bottom seats on the plate, top floats above.
                Sprite bottom = _skin.SecondarySprite;
                Sprite top = _skin.Sprite;
                float w = UIStyles.SHOP_SKIN_BUN_W;
                float bottomH = w * bottom.rect.height / bottom.rect.width;
                float topH = w * top.rect.height / top.rect.width;
                float bottomY = UIStyles.SHOP_SKIN_BUN_BOTTOM_Y;
                AddPreviewSized(bottom, "PreviewBottom", bottomY, new Vector2(w, bottomH));
                AddPreviewSized(top, "PreviewTop",
                    bottomY + bottomH * 0.5f + UIStyles.SHOP_SKIN_BUN_GAP + topH * 0.5f, new Vector2(w, topH));
            }
            else if (_skin.Slot == SkinSlot.ChefSkin)
            {
                AddChefPreview(_skin.Preview);
            }
            else
            {
                AddPreviewSprite(_skin.Preview, "Preview", UIStyles.SHOP_SKIN_PREVIEW_Y, UIStyles.SHOP_SKIN_PREVIEW_H);
            }

            Refresh();
        }

        // A preview sprite sized by height at native aspect, clamped to the box width for wide art.
        private void AddPreviewSprite(Sprite sprite, string name, float y, float height)
        {
            Vector2 size = UIFactory.SizeByHeight(sprite, height);
            if (size.x > UIStyles.SHOP_SKIN_PREVIEW_MAX_W)
                size *= UIStyles.SHOP_SKIN_PREVIEW_MAX_W / size.x;
            AddPreviewSized(sprite, name, y, size);
        }

        private void AddPreviewSized(Sprite sprite, string name, float y, Vector2 size) =>
            UIFactory.CreateImage(_cell.Box, name, sprite, new Vector2(0.5f, 0.5f), new Vector2(0f, y), size);

        // The dogtor overflows its box: sized by WIDTH (the art is wider than tall), feet anchored at
        // SHOP_SKIN_CHEF_BOTTOM so every chef stands on the same line and hangs down behind the green
        // pill, which is what cuts it off — a RectMask2D ends exactly at the pill's face top, so no
        // feet poke out under the pill's transparent shadow margin. The clip is as wide as the chef
        // and reaches HEAD_ROOM above the box so the head can rise over the top border.
        private void AddChefPreview(Sprite sprite)
        {
            Vector2 boxSize = _cell.Box.sizeDelta;
            float boxBottom = -boxSize.y * 0.5f;
            float clipBottom = boxBottom + ShopWidgets.PillFaceTopAboveBoxBottom();
            float clipTop = boxSize.y * 0.5f + UIStyles.SHOP_SKIN_CHEF_HEAD_ROOM;
            Vector2 size = ChefPreviewSize(sprite);

            GameObject clipObj = new GameObject("PreviewClip");
            clipObj.transform.SetParent(_cell.Box, false);
            RectTransform clip = clipObj.AddComponent<RectTransform>();
            clip.anchorMin = clip.anchorMax = new Vector2(0.5f, 0.5f);
            clip.pivot = new Vector2(0.5f, 0.5f);
            clip.sizeDelta = new Vector2(size.x, clipTop - clipBottom);
            clip.anchoredPosition = new Vector2(0f, (clipTop + clipBottom) * 0.5f);
            clipObj.AddComponent<RectMask2D>();

            // Positioned in box space, then re-expressed relative to the clip's centre.
            float chefCenterY = boxBottom + UIStyles.SHOP_SKIN_CHEF_BOTTOM + size.y * 0.5f;
            UIFactory.CreateImage(clip, "Preview", sprite, new Vector2(0.5f, 0.5f),
                new Vector2(0f, chefCenterY - clip.anchoredPosition.y), size);
        }

        // Chef previews keep their IN-GAME proportions: each chef's PPU is tuned so the figure matches
        // the default in play, but the canvases carry different transparent margins, so sizing every
        // canvas to one width made the non-default dogtors ~20% too big. Scale the sprite's world size
        // by the factor that puts the default chef at SHOP_SKIN_CHEF_W.
        private static Vector2 ChefPreviewSize(Sprite sprite)
        {
            Skin defaultChef = Theme.Default(SkinSlot.ChefSkin);
            Sprite reference = defaultChef != null ? defaultChef.Sprite : sprite;
            float pxPerUnit = UIStyles.SHOP_SKIN_CHEF_W / (reference.rect.width / reference.pixelsPerUnit);
            return sprite.rect.size / sprite.pixelsPerUnit * pxPerUnit;
        }

        private void Refresh()
        {
            bool equipped = Theme.IsEquipped(_skin);
            // The two checker arts differ by a few px, so re-derive the size with the sprite.
            _box.sprite = UiArt.Load(equipped ? ShopWidgets.SkinEquippedBoxArt : ShopWidgets.SkinBoxArt);
            _box.rectTransform.sizeDelta = ShopWidgets.BoxSize(equipped ? ShopWidgets.SkinEquippedBoxArt : ShopWidgets.SkinBoxArt);

            if (equipped)
                _cell.SetPill(Loc.Get(LocKey.ShopEquipped));
            else if (ShopService.OwnsSkin(_skin))
                _cell.SetPill(Loc.Get(LocKey.ShopEquip));
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
