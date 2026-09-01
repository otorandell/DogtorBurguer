using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DogtorBurguer
{
    /// <summary>
    /// The parts of one shop cell built by <see cref="ShopWidgets.CreateCell"/>: an optional lime
    /// label line, the cream box (put icons/previews under <see cref="Box"/>), and the price pill.
    /// The whole cell is one button (<see cref="Button"/>, the box is its target graphic).
    /// </summary>
    public sealed class ShopCell
    {
        public RectTransform Root { get; }
        public RectTransform Box { get; }
        public Button Button { get; }
        public TextMeshProUGUI Label { get; }
        public Button Pill { get; }

        public ShopCell(RectTransform root, RectTransform box, Button button, TextMeshProUGUI label, Button pill)
        {
            Root = root;
            Box = box;
            Button = button;
            Label = label;
            Pill = pill;
        }

        /// <summary>Rewrites the pill's face: a word or number, optionally followed by a currency icon.</summary>
        public void SetPill(string text, string iconArt = null) => ShopWidgets.SetPillLabel(Pill, text, iconArt);
    }
}
