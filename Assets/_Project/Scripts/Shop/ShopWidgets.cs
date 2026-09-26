using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DogtorBurguer
{
    /// <summary>
    /// Low-level UGUI builders for the Shop screen, styled to the mock: the page scroll, cell
    /// rows (on the cream slab) and 3-column grids, HUD-palette section titles, authored boxes
    /// (cells, 9-sliced banners), wide-blank price pills with an inline currency icon, and the
    /// icon+text line those share.
    /// Pure construction — no purchase logic (ShopService) and no section composition (ShopSections).
    /// </summary>
    public static class ShopWidgets
    {
        private static readonly Vector2 Center = new(0.5f, 0.5f);
        private static readonly Vector2 TopCenter = new(0.5f, 1f);
        private static readonly Vector2 BottomCenter = new(0.5f, 0f);

        // --- page + rows ---

        /// <summary>The page: a vertical ScrollRect inset from the canvas edges (the page body).
        /// Returns the content transform sections are added to (vertical layout, auto height).</summary>
        public static RectTransform CreateVerticalScroll(Transform parent, float top, float bottom, float side)
        {
            GameObject scrollObj = new GameObject("Scroll");
            scrollObj.transform.SetParent(parent, false);
            RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = new Vector2(side, bottom);
            scrollRect.offsetMax = new Vector2(-side, -top);
            ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.scrollSensitivity = UIStyles.SHOP_SCROLL_SENSITIVITY;

            RectTransform viewport = CreateViewport(scrollObj.transform);

            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewport, false);
            RectTransform content = contentObj.AddComponent<RectTransform>();
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = Vector2.one;
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = Vector2.zero;

            VerticalLayoutGroup layout = contentObj.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(UIStyles.SHOP_CONTENT_PADDING, UIStyles.SHOP_CONTENT_PADDING,
                UIStyles.SHOP_CONTENT_PADDING, UIStyles.SHOP_CONTENT_BOTTOM_PADDING);
            layout.spacing = UIStyles.SHOP_SECTION_SPACING;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            contentObj.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewport;
            scroll.content = content;
            return content;
        }

        /// <summary>A horizontal, side-scrollable cell row (skins) on the 9-sliced cream slab, cells
        /// inset by SHOP_ROW_SLAB_PAD. Returns the row content transform cells are added to
        /// (horizontal layout, auto width).</summary>
        public static RectTransform CreateHorizontalRow(RectTransform pageContent, float cellHeight)
        {
            float pad = UIStyles.SHOP_ROW_SLAB_PAD;
            GameObject rowObj = new GameObject("Row");
            rowObj.transform.SetParent(pageContent, false);
            rowObj.AddComponent<RectTransform>();
            float rowHeight = cellHeight + 2f * pad;
            Image slab = rowObj.AddComponent<Image>();
            SetupSliced(slab, UiArt.Load("ui_shop_row_slab"), rowHeight);
            rowObj.AddComponent<LayoutElement>().preferredHeight = rowHeight;
            ShopRowScroll scroll = rowObj.AddComponent<ShopRowScroll>();
            scroll.horizontal = true;
            scroll.vertical = false;
            scroll.scrollSensitivity = UIStyles.SHOP_SCROLL_SENSITIVITY;

            RectTransform viewport = CreateViewport(rowObj.transform);
            viewport.offsetMin = new Vector2(pad, pad);
            viewport.offsetMax = new Vector2(-pad, -pad);

            GameObject contentObj = new GameObject("RowContent");
            contentObj.transform.SetParent(viewport, false);
            RectTransform content = contentObj.AddComponent<RectTransform>();
            content.anchorMin = Vector2.zero;
            content.anchorMax = new Vector2(0f, 1f);
            content.pivot = new Vector2(0f, 0.5f);
            content.sizeDelta = Vector2.zero;

            HorizontalLayoutGroup layout = contentObj.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = UIStyles.SHOP_CELL_SPACING;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperLeft;
            contentObj.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewport;
            scroll.content = content;
            return content;
        }

        /// <summary>A fixed 3-column grid of cells (power-ups, currency packs) — no side scroll;
        /// the page scroll carries it. Height follows the row count.</summary>
        public static RectTransform CreateGrid(RectTransform pageContent, float cellHeight)
        {
            GameObject gridObj = new GameObject("Grid");
            gridObj.transform.SetParent(pageContent, false);
            RectTransform grid = gridObj.AddComponent<RectTransform>();
            GridLayoutGroup layout = gridObj.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(UIStyles.SHOP_CELL_W, cellHeight);
            layout.spacing = new Vector2(UIStyles.SHOP_CELL_SPACING, UIStyles.SHOP_CELL_SPACING);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 3;
            layout.childAlignment = TextAnchor.UpperCenter;
            return grid;
        }

        // A clipped viewport with an invisible raycast surface so drags on empty space still scroll.
        private static RectTransform CreateViewport(Transform scrollObj)
        {
            GameObject viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollObj, false);
            RectTransform viewport = viewportObj.AddComponent<RectTransform>();
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.sizeDelta = Vector2.zero;
            Image surface = viewportObj.AddComponent<Image>();
            surface.color = Color.clear;
            viewportObj.AddComponent<RectMask2D>();
            return viewport;
        }

        // --- text ---

        /// <summary>"DOGTOR SKINS" — centered, HUD palette (cream + brown border). Returns the laid-out
        /// holder so a section can register it as a scroll anchor (the POWER-UPS deep link).</summary>
        public static RectTransform CreateSectionTitle(RectTransform pageContent, string text)
        {
            // The holder is what the page lays out: the gap BEFORE the section, then a band exactly
            // as tall as the caps. So the page reads section — gap — TITLE/section, the title hugging
            // what it names. (A plain text row carried the font's line box — ascender room above the
            // caps, descender room below — which no spacing knob could close.)
            GameObject holderObj = new GameObject("SectionTitle");
            holderObj.transform.SetParent(pageContent, false);
            RectTransform holder = holderObj.AddComponent<RectTransform>();
            holderObj.AddComponent<LayoutElement>().preferredHeight =
                UIStyles.SHOP_SECTION_GAP + UIStyles.SHOP_SECTION_TITLE_H;

            // The word: as wide as the section (minus the side inset) and generously tall, so
            // auto-size only ever shrinks for WIDTH; Capline centres the caps on the band.
            TextMeshProUGUI title = UIFactory.CreateText(holder, text, Vector2.zero, Vector2.zero,
                UIStyles.SHOP_SECTION_TITLE_SIZE, FontStyles.Bold, alignment: TextAlignmentOptions.Capline);
            UIFactory.StyleHudText(title);
            UIFactory.AutoFit(title, UIStyles.SHOP_SECTION_TITLE_SIZE_MIN, UIStyles.SHOP_SECTION_TITLE_SIZE);
            RectTransform rect = title.rectTransform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(-2f * UIStyles.SHOP_SECTION_TITLE_SIDE_INSET, UIStyles.SHOP_SECTION_TITLE_SIZE * 2f);
            rect.anchoredPosition = new Vector2(0f, UIStyles.SHOP_SECTION_TITLE_H * 0.5f);
            return holder;
        }

        /// <summary>The lime accent with the HUD border — cell names, pack amounts, THANK YOU.</summary>
        public static void StyleAccent(TextMeshProUGUI tmp) =>
            UIFactory.StyleFillAndBorder(tmp, UIStyles.SHOP_ACCENT, UIStyles.HUD_TEXT_BORDER, UIStyles.HUD_TEXT_BORDER_WIDTH);

        /// <summary>The shop's price text from a store label: keeps the store's own formatting
        /// (symbol, code, separators) but drops any character the game font cannot draw
        /// in-style — a currency outside the baked atlas (e.g. Thai baht) would render as a
        /// plain fallback glyph or a missing box, so those markets see digits only. (Blanket
        /// symbol stripping was a trial-font workaround, retired 2026-09-08.)</summary>
        public static string MoneyLabel(string priceLabel)
        {
            TMP_FontAsset font = TMP_Settings.defaultFontAsset;
            var sb = new System.Text.StringBuilder(priceLabel.Length);
            foreach (char c in priceLabel)
                if (char.IsDigit(c) || c == '.' || c == ',' || c == ' '
                    || (font != null && font.HasCharacter(c)))
                    sb.Append(c);
            return sb.ToString().Trim();
        }

        // --- boxes + cells ---

        /// <summary>The authored box arts a cell can sit on.</summary>
        public const string ItemBoxArt = "ui_shop_item_box";        // power-ups, currency packs (also the 9-sliced banner box)
        public const string SkinBoxArt = "ui_shop_skin_box";        // cream checker
        public const string SkinEquippedBoxArt = "ui_shop_skin_equipped"; // green checker

        /// <summary>A 9-sliced cream box (the item box art) — banners, the bundle row. The layout
        /// sets its width; <paramref name="height"/> is what it will be laid out at (the slice scale
        /// keys off it).</summary>
        public static Image CreateBox(Transform parent, string name, float height)
        {
            Image box = UIFactory.CreateImage(parent, name, null, Center, Vector2.zero, Vector2.one);
            SetupSliced(box, UiArt.Load(ItemBoxArt), height);
            box.gameObject.AddComponent<LayoutElement>().preferredHeight = height;
            return box;
        }

        // 9-slice at a sensible scale: UGUI draws sprite borders at their NATIVE pixel size (sprite
        // PPU 100 = canvas reference PPU), so a 2000px-wide art's borders would dwarf a 400px rect
        // and the center collapses. Scaling the slice by sprite-height / rect-height renders the
        // vertical edges exactly and stretches only the flat middle horizontally.
        private static void SetupSliced(Image image, Sprite sprite, float rectHeight)
        {
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = sprite.rect.height / rectHeight;
        }

        /// <summary>A cell's box size: the art at SHOP_CELL_W wide, native aspect.</summary>
        public static Vector2 BoxSize(string boxArt) => UIFactory.SizeByWidth(UiArt.Load(boxArt), UIStyles.SHOP_CELL_W);

        /// <summary>The transparent margin above a box art's visible top border, as a fraction of its
        /// height (alpha bbox, measured 2026-09-21) — the name line is anchored to the VISIBLE border,
        /// not the rect. Unknown art = 0 (rect top).</summary>
        public static float BoxTopMarginFrac(string boxArt) => boxArt switch
        {
            SkinBoxArt => 0.103f,
            SkinEquippedBoxArt => 0.113f,
            ItemBoxArt => 0.109f,
            _ => 0f,
        };

        /// <summary>Where the green pill's face begins, measured UP from the box's bottom edge: the
        /// pill rides over the box by SHOP_CELL_PILL_OVERLAP, minus the blank's margin + outline
        /// above its face (<see cref="ButtonFace"/>). A preview hanging down behind the pill is
        /// clipped here so the pill, not the cell's edge, is what cuts it.</summary>
        public static float PillFaceTopAboveBoxBottom()
        {
            Vector2 pillSize = new(UIStyles.SHOP_CELL_PILL_W, UIStyles.SHOP_CELL_PILL_H);
            float faceTopBelowPillTop = pillSize.y * 0.5f - ButtonFace.Of(UiArt.Load("ui_btn_green_wide"), pillSize).yMax;
            return UIStyles.SHOP_CELL_PILL_OVERLAP - faceTopBelowPillTop;
        }

        /// <summary>Height of a cell with/without its label line: half the label (it overlaps the
        /// box top edge) + box + pill, minus the pill's ride over the box bottom.</summary>
        public static float CellHeight(bool withLabel, string boxArt) =>
            (withLabel ? UIStyles.SHOP_CELL_LABEL_H * 0.5f : 0f) + BoxSize(boxArt).y
            - UIStyles.SHOP_CELL_PILL_OVERLAP + UIStyles.SHOP_CELL_PILL_H;

        /// <summary>A cell: [lime label] over an authored box over a green pill; one button for the
        /// whole thing. Sized for a row (LayoutElement) — a grid overrides with its cell size. Pass a
        /// null label to skip the label line (power-ups).</summary>
        public static ShopCell CreateCell(Transform parent, string name, string label, string boxArt, UnityAction onClick)
        {
            bool withLabel = label != null;
            float height = CellHeight(withLabel, boxArt);

            GameObject rootObj = new GameObject(name);
            rootObj.transform.SetParent(parent, false);
            RectTransform root = rootObj.AddComponent<RectTransform>();
            root.sizeDelta = new Vector2(UIStyles.SHOP_CELL_W, height);
            LayoutElement layout = rootObj.AddComponent<LayoutElement>();
            layout.preferredWidth = UIStyles.SHOP_CELL_W;
            layout.preferredHeight = height;

            float labelH = withLabel ? UIStyles.SHOP_CELL_LABEL_H : 0f;
            Vector2 boxSize = BoxSize(boxArt);

            TextMeshProUGUI labelText = null;
            if (withLabel)
            {
                labelText = UIFactory.CreateText(root, label, Vector2.zero,
                    new Vector2(UIStyles.SHOP_CELL_W + UIStyles.SHOP_CELL_SPACING, UIStyles.SHOP_CELL_LABEL_H),
                    UIStyles.SHOP_CELL_LABEL_SIZE, FontStyles.Bold);
                StyleAccent(labelText);
                // Localized skin names run long ("Formaggio grattugiato") — shrink rather than overflow.
                UIFactory.AutoFit(labelText, UIStyles.SHOP_CELL_LABEL_SIZE_MIN, UIStyles.SHOP_CELL_LABEL_SIZE);
                // Centred on the box's VISIBLE top border: the box rect top sits half a label down
                // from the root top, and the art's transparent margin is below that.
                AnchorTop(labelText.rectTransform, -BoxTopMarginFrac(boxArt) * boxSize.y);
            }
            Image box = UIFactory.CreateImage(root, "Box", UiArt.Load(boxArt), TopCenter,
                new Vector2(0f, -labelH * 0.5f - boxSize.y * 0.5f), boxSize);
            box.raycastTarget = true;

            Button button = rootObj.AddComponent<Button>();
            button.targetGraphic = box;
            if (onClick != null) button.onClick.AddListener(onClick);

            Button pill = CreatePill(root, "Pill", "ui_btn_green_wide", BottomCenter, Vector2.zero,
                UIStyles.SHOP_CELL_PILL_W, onClick, UIStyles.SHOP_CELL_PILL_H);
            RectTransform pillRect = pill.GetComponent<RectTransform>();
            pillRect.pivot = BottomCenter;

            // The name half-overlaps the box top and the pill rides over its bottom (mock);
            // keep the label above the box in draw order.
            if (labelText != null) labelText.transform.SetAsLastSibling();

            return new ShopCell(root, box.rectTransform, button, labelText, pill);
        }

        // --- pills + icon lines ---

        /// <summary>A wide-blank pill (ui_btn_green_wide / _red_wide / ui_btn_blue_watch …) sized by
        /// width, anchored at a point of its parent. Put a face on it with <see cref="SetPillLabel"/>.</summary>
        public static Button CreatePill(Transform parent, string name, string art, Vector2 anchor, Vector2 pos,
            float width, UnityAction onClick, float height = 0f)
        {
            Sprite blank = UiArt.Load(art);
            Vector2 size = height > 0f ? new Vector2(width, height) : UIFactory.SizeByWidth(blank, width);
            return UIFactory.CreateSpriteButton(parent, name, blank, anchor, pos, size, onClick);
        }

        /// <summary>(Re)writes a pill's face: a HUD-palette word/number and an optional currency
        /// icon after it, centered on the face. Replaces any previous face.</summary>
        public static void SetPillLabel(Button pill, string text, string iconArt)
            => SetPillLabel(pill, text, iconArt, 0f, UIStyles.SHOP_PILL_ICON_H);

        /// <param name="maxFontSize">A designer cap on the size; 0 = the word grows until its caps
        /// fill the face (UIFactory.CapFitFontSize), shrinking only for width.</param>
        public static void SetPillLabel(Button pill, string text, string iconArt, float maxFontSize, float iconHeight)
        {
            Transform old = pill.transform.Find("Face");
            if (old != null) Object.Destroy(old.gameObject);

            // Centred on the art's FACE (ButtonFace), like every button word; the side pad keeps
            // the line off the outline.
            RectTransform rect = pill.GetComponent<RectTransform>();
            Rect face = ButtonFace.Of(pill.image != null ? pill.image.sprite : null, rect.sizeDelta);
            float fontSize = UIFactory.CapFitFontSize(face.height);
            if (maxFontSize > 0f) fontSize = Mathf.Min(fontSize, maxFontSize);
            CreateIconLine(pill.transform, "Face", face.center, face.size, text, fontSize, iconArt, iconHeight,
                sidePad: face.width * UIStyles.BUTTON_LABEL_SIDE_PAD_FRAC);
        }

        /// <summary>A centered "text [icon]" line — the number-plus-currency-icon pattern the mock
        /// uses on pills and in the confirm dialog. Layout-driven, so the pair stays centered as one.
        /// The text is cap-height centred (Capline), so digits sit at the same height as the icon.</summary>
        public static TextMeshProUGUI CreateIconLine(Transform parent, string name, Vector2 pos, Vector2 size,
            string text, float fontSize, string iconArt, float iconHeight, float sidePad = 0f)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = Center;
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            HorizontalLayoutGroup layout = obj.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = UIStyles.SHOP_PILL_ICON_GAP;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            int pad = Mathf.RoundToInt(sidePad);
            layout.padding = new RectOffset(pad, pad, 0, 0);

            TextMeshProUGUI label = UIFactory.CreateText(rect, text, Vector2.zero, Vector2.zero, fontSize, FontStyles.Bold,
                alignment: TextAlignmentOptions.Capline);
            UIFactory.StyleHudText(label);
            // The layout narrows the label to the room left on the face, and auto-size fits the
            // word to that — but only down to its floor. With a cap-fit ceiling (tall pills offer
            // 70px+) CreateText's default 55% floor is still wider than the face for CANCEL, so
            // give it a real one.
            UIFactory.AutoFit(label, UIStyles.BUTTON_LABEL_MIN_FONT_SIZE, fontSize);

            if (iconArt != null)
            {
                Sprite icon = UiArt.Load(iconArt);
                Vector2 iconSize = UIFactory.SizeByHeight(icon, iconHeight);
                // The icon rides in a fixed-size holder so it can be nudged vertically against
                // the text line (a layout group allows no per-child offset) — digits render above
                // the baseline, so a centered icon looks low next to them.
                GameObject holder = new GameObject("IconHolder");
                holder.transform.SetParent(rect, false);
                holder.AddComponent<RectTransform>().sizeDelta = iconSize;
                LayoutElement element = holder.AddComponent<LayoutElement>();
                element.preferredWidth = iconSize.x;
                element.preferredHeight = iconSize.y;
                UIFactory.CreateImage(holder.transform, "Icon", icon, Center,
                    new Vector2(0f, UIStyles.SHOP_PILL_ICON_Y_NUDGE), iconSize);
            }
            return label;
        }

        // --- anchoring helpers ---

        public static void AnchorTop(RectTransform rect, float y)
        {
            rect.anchorMin = rect.anchorMax = TopCenter;
            rect.pivot = TopCenter;
            rect.anchoredPosition = new Vector2(0f, y);
        }

        /// <summary>Re-anchors a centered UIFactory text to the left edge of its parent.</summary>
        public static void AnchorLeft(RectTransform rect, Vector2 pos)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = pos;
        }
    }
}
