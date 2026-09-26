using System.Collections.Generic;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Localized display names for skins: skin id -> LocKey. Lives in code rather than on the Skin
    /// asset because a serialized LocKey is stored as the enum's int, and LocKey gains entries
    /// mid-enum, which would silently re-point every stored name. Only shop-visible skins are
    /// listed (the background/plate/diner/cells defaults are never shown by name). A new skin =
    /// one LocKey + one line per Strings_XX table + one line here.
    /// </summary>
    public static class SkinNames
    {
        private static readonly Dictionary<string, LocKey> Keys = new()
        {
            ["chef_default"] = LocKey.SkinChefDefault,
            ["chef_burgerchain"] = LocKey.SkinChefBurgerchain,
            ["chef_european"] = LocKey.SkinChefEuropean,
            ["chef_japanese"] = LocKey.SkinChefJapanese,
            ["chef_mexican"] = LocKey.SkinChefMexican,
            ["chef_royale"] = LocKey.SkinChefRoyale,
            ["bun_default"] = LocKey.SkinBunDefault,
            ["bun_gourmet"] = LocKey.SkinBunGourmet,
            ["bun_gold"] = LocKey.SkinBunGold,
            ["bun_rustic"] = LocKey.SkinBunRustic,
            ["bun_black"] = LocKey.SkinBunBlack,
            ["meat_default"] = LocKey.SkinMeatDefault,
            ["meat_gourmet"] = LocKey.SkinMeatGourmet,
            ["meat_gold"] = LocKey.SkinMeatGold,
            ["meat_vegan"] = LocKey.SkinMeatVegan,
            ["meat_wagyu"] = LocKey.SkinMeatWagyu,
            ["cheese_default"] = LocKey.SkinCheeseDefault,
            ["cheese_gourmet"] = LocKey.SkinCheeseGourmet,
            ["cheese_gold"] = LocKey.SkinCheeseGold,
            ["cheese_shredded"] = LocKey.SkinCheeseShredded,
            ["cheese_blue"] = LocKey.SkinCheeseBlue,
            ["tomato_default"] = LocKey.SkinTomatoDefault,
            ["tomato_gourmet"] = LocKey.SkinTomatoGourmet,
            ["tomato_gold"] = LocKey.SkinTomatoGold,
            ["tomato_pico"] = LocKey.SkinTomatoPico,
            ["tomato_kumato"] = LocKey.SkinTomatoKumato,
            ["bacon_default"] = LocKey.SkinBaconDefault,
            ["bacon_gourmet"] = LocKey.SkinBaconGourmet,
            ["bacon_gold"] = LocKey.SkinBaconGold,
            ["bacon_pulledpork"] = LocKey.SkinBaconPulledpork,
            ["bacon_iberic"] = LocKey.SkinBaconIberic,
            ["onion_default"] = LocKey.SkinOnionDefault,
            ["onion_gourmet"] = LocKey.SkinOnionGourmet,
            ["onion_gold"] = LocKey.SkinOnionGold,
            ["onion_pickled"] = LocKey.SkinOnionPickled,
            ["onion_crispy"] = LocKey.SkinOnionCrispy,
            ["pickle_default"] = LocKey.SkinPickleDefault,
            ["pickle_gourmet"] = LocKey.SkinPickleGourmet,
            ["pickle_gold"] = LocKey.SkinPickleGold,
            ["pickle_bellpepper"] = LocKey.SkinPickleBellpepper,
            ["pickle_jalapeno"] = LocKey.SkinPickleJalapeno,
            ["lettuce_default"] = LocKey.SkinLettuceDefault,
            ["lettuce_gourmet"] = LocKey.SkinLettuceGourmet,
            ["lettuce_gold"] = LocKey.SkinLettuceGold,
            ["lettuce_avocado"] = LocKey.SkinLettuceAvocado,
            ["lettuce_purple"] = LocKey.SkinLettucePurple,
            ["egg_default"] = LocKey.SkinEggDefault,
            ["egg_boiled"] = LocKey.SkinEggBoiled,
            ["egg_gold"] = LocKey.SkinEggGold,
            ["egg_gourmet"] = LocKey.SkinEggGourmet,
            ["egg_omelet"] = LocKey.SkinEggOmelet,
        };

        /// <summary>The skin's name in the current language (loud error + the raw id if unmapped).</summary>
        public static string Get(Skin skin)
        {
            if (Keys.TryGetValue(skin.Id, out LocKey key)) return Loc.Get(key);

            Debug.LogError($"[SkinNames] no LocKey for skin '{skin.Id}' — add it to SkinNames");
            return skin.Id;
        }
    }
}
