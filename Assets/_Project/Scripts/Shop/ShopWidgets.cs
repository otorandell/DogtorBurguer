using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DogtorBurguer
{
    /// <summary>
    /// Low-level UGUI builders for the Shop screen: the vertical page scroll, horizontal cell
    /// rows, section titles, offer bars and price buttons. Pure construction —
    /// no purchase logic (that's ShopService) and no section composition (that's ShopSections).
    /// </summary>
    public static class ShopWidgets
    {
        /// <summary>The page: a vertical ScrollRect filling the canvas below the header. Returns
        /// the content transform sections are added to (vertical layout, auto height).</summary>
        public static RectTransform CreateVerticalScroll(Transform parent, float topOffset)
        {
            GameObject scrollObj = new GameObject("Scroll");
            scrollObj.transform.SetParent(parent, false);
            RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = new Vector2(0f, -topOffset);
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
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            contentObj.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewport;
            scroll.content = content;
            return content;
        }

        /// <summary>A horizontal, side-scrollable cell row (skins, consumable cards). Returns the
        /// row content transform cells are added to (horizontal layout, auto width).</summary>
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
            layout.childAlignment = TextAnchor.MiddleLeft;
            contentObj.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewport;
            scroll.content = content;
            return content;
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

        public static void CreateSectionTitle(RectTransform pageContent, string text)
        {
            TextMeshProUGUI title = UIFactory.CreateText(pageContent, text, Vector2.zero,
                Vector2.zero, UIStyles.SHOP_SECTION_TITLE_SIZE, FontStyles.Bold,
                UIStyles.SHOP_SECTION_TITLE_COLOR, TextAlignmentOptions.BottomLeft);
            title.gameObject.AddComponent<LayoutElement>().preferredHeight = UIStyles.SHOP_SECTION_TITLE_H;
        }

        /// <summary>A smaller row label sitting under a section title (one per ingredient type).</summary>
        public static void CreateSubTitle(RectTransform pageContent, string text)
        {
            TextMeshProUGUI sub = UIFactory.CreateText(pageContent, text, Vector2.zero,
                Vector2.zero, UIStyles.SHOP_SUBTITLE_SIZE, FontStyles.Bold,
                UIStyles.SHOP_SUBTEXT_COLOR, TextAlignmentOptions.BottomLeft);
            sub.gameObject.AddComponent<LayoutElement>().preferredHeight = UIStyles.SHOP_SUBTITLE_H;
        }

        /// <summary>A full-width offer bar (currency packs, remove-ads): background + LayoutElement.
        /// Fill it with <see cref="CreateBarTexts"/> and <see cref="CreatePriceButton"/>.</summary>
        public static RectTransform CreateBar(RectTransform pageContent, float height, Color background)
        {
            GameObject barObj = new GameObject("Bar");
            barObj.transform.SetParent(pageContent, false);
            RectTransform bar = barObj.AddComponent<RectTransform>();
            barObj.AddComponent<Image>().color = background;
            barObj.AddComponent<LayoutElement>().preferredHeight = height;
            return bar;
        }

        /// <summary>The bar's left side: an icon plus title/subtitle, and an optional gold badge
        /// tag sitting above the price button on the right.</summary>
        public static void CreateBarTexts(RectTransform bar, string iconArt, string title, string subtitle, string badge)
        {
            Sprite icon = UiArt.Load(iconArt);
            float aspect = icon != null ? icon.rect.width / icon.rect.height : 1f;
            UIFactory.CreateImage(bar, "Icon", icon, new Vector2(0f, 0.5f),
                new Vector2(UIStyles.SHOP_OFFER_ICON_X, 0f),
                new Vector2(UIStyles.SHOP_OFFER_ICON_H * aspect, UIStyles.SHOP_OFFER_ICON_H));

            TextMeshProUGUI titleText = UIFactory.CreateText(bar, title, Vector2.zero,
                UIStyles.SHOP_OFFER_TITLE_RECT, UIStyles.SHOP_OFFER_TITLE_SIZE, FontStyles.Bold,
                UIStyles.TEXT_UI, TextAlignmentOptions.MidlineLeft);
            AnchorLeft(titleText.rectTransform, new Vector2(UIStyles.SHOP_OFFER_TEXT_X, 13f));

            if (!string.IsNullOrEmpty(subtitle))
            {
                TextMeshProUGUI subText = UIFactory.CreateText(bar, subtitle, Vector2.zero,
                    UIStyles.SHOP_OFFER_SUB_RECT, UIStyles.SHOP_OFFER_SUB_SIZE, FontStyles.Normal,
                    UIStyles.SHOP_SUBTEXT_COLOR, TextAlignmentOptions.MidlineLeft);
                AnchorLeft(subText.rectTransform, new Vector2(UIStyles.SHOP_OFFER_TEXT_X, -14f));
            }

            if (!string.IsNullOrEmpty(badge))
            {
                TextMeshProUGUI badgeText = UIFactory.CreateText(bar, badge, Vector2.zero,
                    UIStyles.SHOP_BADGE_RECT, UIStyles.SHOP_BADGE_SIZE, FontStyles.Bold,
                    UIStyles.SHOP_BADGE_COLOR);
                RectTransform rect = badgeText.rectTransform;
                rect.anchorMin = rect.anchorMax = new Vector2(1f, 0.5f);
                rect.pivot = new Vector2(1f, 0.5f);
                rect.anchoredPosition = UIStyles.SHOP_BADGE_POS;
            }
        }

        /// <summary>A buy button, anchored right-center of its parent by default. Pass a currency
        /// icon ("ui_star"/"ui_gem") to prefix the label, or null for money prices.</summary>
        public static Button CreatePriceButton(RectTransform parent, Vector2 anchor, Vector2 pos,
            Vector2 size, Color color, string iconArt, string label, UnityEngine.Events.UnityAction onClick)
        {
            (GameObject obj, Button button, TextMeshProUGUI text) =
                UIFactory.CreateButton(parent, label, Vector2.zero, size, color, UIStyles.SHOP_PRICE_TEXT_SIZE, onClick);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;

            if (iconArt != null)
            {
                Sprite icon = UiArt.Load(iconArt);
                float aspect = icon != null ? icon.rect.width / icon.rect.height : 1f;
                UIFactory.CreateImage(rect, "CurrencyIcon", icon, new Vector2(0f, 0.5f),
                    new Vector2(UIStyles.SHOP_PRICE_ICON_X, 0f),
                    new Vector2(UIStyles.SHOP_PRICE_ICON_H * aspect, UIStyles.SHOP_PRICE_ICON_H));
                text.margin = new Vector4(UIStyles.SHOP_PRICE_TEXT_MARGIN, 0f, 0f, 0f);
            }

            return button;
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
