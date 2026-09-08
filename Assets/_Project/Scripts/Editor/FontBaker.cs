using System.Text;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace DogtorBurguer
{
    /// <summary>
    /// One-click, deterministic replacement for the Font Asset Creator window (two manual runs
    /// went wrong: an ASCII-only charset, then an empty static asset): bakes the full
    /// localization character set from PantonDemo-Black INTO the existing SDF asset in place —
    /// same GUID and sub-asset fileIDs, so the TMP-Settings default font, the LiberationSans
    /// fallback and every cached material stay wired. Rerun whenever the charset or the source
    /// font changes.
    /// </summary>
    public static class FontBaker
    {
        private const string SourceFontPath = "Assets/_Project/Fonts/PantonDemo-Black.otf";
        private const string TargetAssetPath = "Assets/_Project/Fonts/PantonDemo-Black SDF.asset";

        // 96pt / 8px padding keeps the old 144/12 padding-to-point ratio, so the sticker
        // stroke + shadow (SDF-normalized material units) keep the same visual thickness — and
        // the ~190-character set fits ONE 2048 atlas (multi-atlas would break the custom
        // styled materials, which bind the first atlas texture only).
        private const int PointSize = 96;
        private const int Padding = 8;
        private const int AtlasSize = 2048;

        // The localization set: ASCII + Latin-1 supplement + the extras for ES/PT/DE/FR/IT/TR
        // (Ğğ İ Œœ Şş Žž), dashes, curly quotes, ellipsis, euro. Dotless ı (0x131) is excluded —
        // the font lacks it and caps-only Turkish never needs it.
        private static readonly (int from, int to)[] Ranges =
        {
            (0x20, 0x7E), (0xA1, 0xFF), (0x11E, 0x11F), (0x130, 0x130),
            (0x152, 0x153), (0x15E, 0x15F), (0x17D, 0x17E), (0x2013, 0x2014),
            (0x2018, 0x201A), (0x201C, 0x201E), (0x2026, 0x2026), (0x20AC, 0x20AC),
        };

        [MenuItem("Tools/Dogtor/Bake Panton Font Atlas")]
        public static void Bake()
        {
            Font source = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
            TMP_FontAsset target = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TargetAssetPath);
            if (source == null || target == null)
            {
                Debug.LogError("[FontBaker] Source font or target SDF asset not found");
                return;
            }

            // Generate in memory: dynamic mode accepts TryAddCharacters, then freeze to static.
            TMP_FontAsset baked = TMP_FontAsset.CreateFontAsset(source, PointSize, Padding,
                GlyphRenderMode.SDFAA, AtlasSize, AtlasSize, AtlasPopulationMode.Dynamic);
            if (!baked.TryAddCharacters(BuildCharacterSet(), out string missing))
                Debug.LogWarning($"[FontBaker] Characters the font lacks (should be none): {missing}");
            baked.atlasPopulationMode = AtlasPopulationMode.Static;

            // Copy the bake into the existing sub-assets so every GUID/fileID stays put.
            Texture2D targetTex = target.atlasTextures[0];
            Material targetMat = target.material;
            EditorUtility.CopySerialized(baked.atlasTextures[0], targetTex);
            EditorUtility.CopySerialized(baked.material, targetMat);
            EditorUtility.CopySerialized(baked, target);

            // The copy carried references to the in-memory bake — point them back home.
            SerializedObject so = new SerializedObject(target);
            so.FindProperty("m_Material").objectReferenceValue = targetMat;
            so.FindProperty("m_AtlasTextures").GetArrayElementAtIndex(0).objectReferenceValue = targetTex;
            so.ApplyModifiedPropertiesWithoutUndo();
            targetMat.SetTexture(ShaderUtilities.ID_MainTex, targetTex);
            target.name = "PantonDemo-Black SDF";
            targetTex.name = "PantonDemo-Black SDF Atlas";
            targetMat.name = "PantonDemo-Black SDF Material";

            int count = target.characterTable.Count;
            EditorUtility.SetDirty(target);
            EditorUtility.SetDirty(targetTex);
            EditorUtility.SetDirty(targetMat);
            AssetDatabase.SaveAssets();

            foreach (Texture2D tex in baked.atlasTextures)
                Object.DestroyImmediate(tex);
            Object.DestroyImmediate(baked.material);
            Object.DestroyImmediate(baked);

            Debug.Log($"[FontBaker] Baked {count} characters into {TargetAssetPath}");
        }

        private static string BuildCharacterSet()
        {
            StringBuilder sb = new StringBuilder();
            foreach ((int from, int to) in Ranges)
                for (int c = from; c <= to; c++)
                    sb.Append((char)c);
            return sb.ToString();
        }
    }
}
