using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DogtorBurguer
{
    /// <summary>
    /// Low-level UGUI builders for the Shop screen, styled to the mock: the page scroll, cell
    /// rows and 3-column grids, HUD-palette section titles, 9-sliced cream boxes (cells, banners),
    /// wide-blank price pills with an inline currency icon, and the icon+text line those share.
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

        /// <summary>A horizontal, side-scrollable cell row (skins). Returns the row content
        /// transform cells are added to (horizontal layout, auto width).</summary>
        public static RectTransform CreateHorizontalRow(RectTransform pageContent, float height)
        {
            GameObject rowObj = new GameObject("Row");
            rowObj.transform.SetParent(pageContent, false);
            rowObj.AddComponent<RectTransform>();
            rowObj.AddComponent<LayoutElement>().preferredHeight = height;
            ShopRowScroll scroll = rowObj.AddComponent<ShopRowScroll>();
            scroll.horizontal = true;
            scroll.vertical = false;
            scroll.scrollSensitivity = UIStyles.SHOP_SCROLL_SENSITIVITY;

            RectTransform viewport = CreateViewport(rowObj.transform);

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

        /// <summary>"DOGTOR SKINS" — centered, HUD palette (cream + brown border).</summary>
        public static void CreateSectionTitle(RectTransform pageContent, string text)
        {
            TextMeshProUGUI title = UIFactory.CreateText(pageContent, text, Vector2.zero,
                Vector2.zero, UIStyles.SHOP_SECTION_TITLE_SIZE, FontStyles.Bold);
            UIFactory.StyleHudText(title);
            title.gameObject.AddComponent<LayoutElement>().preferredHeight = UIStyles.SHOP_SECTION_TITLE_H;
        }

        /// <summary>A smaller brown row label under a section title (one per ingredient type).</summary>
        public static void CreateSubTitle(RectTransform pageContent, string text)
        {
            TextMeshProUGUI sub = UIFactory.CreateText(pageContent, text, Vector2.zero,
                Vector2.zero, UIStyles.SHOP_SUBTITLE_SIZE, FontStyles.Bold, UIStyles.TOPBAR_NUMBER_COLOR);
            sub.gameObject.AddComponent<LayoutElement>().preferredHeight = UIStyles.SHOP_SUBTITLE_H;
        }

        /// <summary>The lime accent with the HUD border — cell names, pack amounts, THANK YOU.</summary>
        public static void StyleAccent(TextMeshProUGUI tmp) =>
            UIFactory.StyleFillAndBorder(tmp, UIStyles.SHOP_ACCENT, UIStyles.HUD_TEXT_BORDER, UIStyles.HUD_TEXT_BORDER_WIDTH);

        /// <summary>IAP price labels for display. The trial font maps "$" to a placeholder sliver
        /// glyph (see CLAUDE.md), so the currency symbol is dropped until the font is replaced —
        /// the real IAP SDK's localized strings will need the font fix anyway.</summary>
        public static string MoneyLabel(string priceLabel) => priceLabel.TrimStart('$');

        // --- boxes + cells ---

        /// <summary>A 9-sliced cream box (ui_consumable_box) at any size.</summary>
        public static Image CreateBox(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            Image box = UIFactory.CreateImage(parent, name, UiArt.Load("ui_consumable_box"), anchor, pos, size);
            box.type = Image.Type.Sliced;
            return box;
        }

        /// <summary>Height of a cell with/without its label line (box + pill + gaps).</summary>
        public static float CellHeight(bool withLabel) =>
            (withLabel ? UIStyles.SHOP_CELL_LABEL_H : 0f) + UIStyles.SHOP_CELL_BOX_H + UIStyles.SHOP_CELL_PILL_GAP
            + UIFactory.SizeByWidth(UiArt.Load("ui_btn_green_wide"), UIStyles.SHOP_CELL_PILL_W).y;

        /// <summary>A cell: [lime label] over a cream box over a green pill; one button for the whole
        /// thing. Sized for a row (LayoutElement) — a grid overrides with its cell size. Pass a null
        /// label to skip the label line (power-ups).</summary>
        public static ShopCell CreateCell(Transform parent, string name, string label, UnityAction onClick)
        {
            bool withLabel = label != null;
            float height = CellHeight(withLabel);

            GameObject rootObj = new GameObject(name);
            rootObj.transform.SetParent(parent, false);
            RectTransform root = rootObj.AddComponent<RectTransform>();
            root.sizeDelta = new Vector2(UIStyles.SHOP_CELL_W, height);
            LayoutElement layout = rootObj.AddComponent<LayoutElement>();
            layout.preferredWidth = UIStyles.SHOP_CELL_W;
            layout.preferredHeight = height;

            TextMeshProUGUI labelText = null;
            if (withLabel)
            {
                labelText = UIFactory.CreateText(root, label, Vector2.zero,
                    new Vector2(UIStyles.SHOP_CELL_W + UIStyles.SHOP_CELL_SPACING, UIStyles.SHOP_CELL_LABEL_H),
                    UIStyles.SHOP_CELL_LABEL_SIZE, FontStyles.Bold);
                StyleAccent(labelText);
                AnchorTop(labelText.rectTransform, 0f);
            }

            float labelH = withLabel ? UIStyles.SHOP_CELL_LABEL_H : 0f;
            Image box = CreateBox(root, "Box", TopCenter, new Vector2(0f, -labelH - UIStyles.SHOP_CELL_BOX_H * 0.5f),
                new Vector2(UIStyles.SHOP_CELL_W, UIStyles.SHOP_CELL_BOX_H));
            box.raycastTarget = true;

            Button button = rootObj.AddComponent<Button>();
            button.targetGraphic = box;
            if (onClick != null) button.onClick.AddListener(onClick);

            Button pill = CreatePill(root, "Pill", "ui_btn_green_wide", BottomCenter, Vector2.zero,
                UIStyles.SHOP_CELL_PILL_W, onClick);
            RectTransform pillRect = pill.GetComponent<RectTransform>();
            pillRect.pivot = BottomCenter;

            return new ShopCell(root, box.rectTransform, button, labelText, pill);
        }

        // --- pills + icon lines ---

        /// <summary>A wide-blank pill (ui_btn_green_wide / _red_wide / ui_btn_blue_watch …) sized by
        /// width, anchored at a point of its parent. Put a face on it with <see cref="SetPillLabel"/>.</summary>
        public static Button CreatePill(Transform parent, string name, string art, Vector2 anchor, Vector2 pos,
            float width, UnityAction onClick)
        {
            Sprite blank = UiArt.Load(art);
            return UIFactory.CreateSpriteButton(parent, name, blank, anchor, pos, UIFactory.SizeByWidth(blank, width), onClick);
        }

        /// <summary>(Re)writes a pill's face: a HUD-palette word/number and an optional currency
        /// icon after it, centered on the face. Replaces any previous face.</summary>
        public static void SetPillLabel(Button pill, string text, string iconArt)
        {
            Transform old = pill.transform.Find("Face");
            if (old != null) Object.Destroy(old.gameObject);

            RectTransform rect = pill.GetComponent<RectTransform>();
            CreateIconLine(pill.transform, "Face", UIStyles.SHOP_PILL_LABEL_NUDGE, rect.sizeDelta,
                text, UIStyles.SHOP_PILL_TEXT_SIZE, iconArt, UIStyles.SHOP_PILL_ICON_H);
        }

        /// <summary>A centered "text [icon]" line — the number-plus-currency-icon pattern the mock
        /// uses on pills and in the confirm dialog. Layout-driven, so the pair stays centered as one.</summary>
        public static TextMeshProUGUI CreateIconLine(Transform parent, string name, Vector2 pos, Vector2 size,
            string text, float fontSize, string iconArt, float iconHeight)
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

            TextMeshProUGUI label = UIFactory.CreateText(rect, text, Vector2.zero, Vector2.zero, fontSize, FontStyles.Bold);
            UIFactory.StyleHudText(label);

            if (iconArt != null)
            {
                Sprite icon = UiArt.Load(iconArt);
                Vector2 iconSize = UIFactory.SizeByHeight(icon, iconHeight);
                Image image = UIFactory.CreateImage(rect, "Icon", icon, Center, Vector2.zero, iconSize);
                LayoutElement element = image.gameObject.AddComponent<LayoutElement>();
                element.preferredWidth = iconSize.x;
                element.preferredHeight = iconSize.y;
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
