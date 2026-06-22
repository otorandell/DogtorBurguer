using System.Globalization;

namespace DogtorBurguer
{
    /// <summary>
    /// Compact number formatting for HUD readouts: exact up to 9,999 then K/M abbreviated, so large
    /// scores/currencies stay short (and legible) inside fixed-size boxes instead of shrinking tiny.
    /// </summary>
    public static class NumberFormat
    {
        public static string Abbreviate(int value)
        {
            if (value >= 1_000_000)
                return (value / 1_000_000f).ToString("0.#", CultureInfo.InvariantCulture) + "M";
            if (value >= 10_000)
                return (value / 1_000f).ToString("0.#", CultureInfo.InvariantCulture) + "K";
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
