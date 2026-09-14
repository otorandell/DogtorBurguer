using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// One-click Play Store build — Tools → Dogtor → Build Android App Bundle. Guards the two
    /// footguns of a release build (the Test Build switch left on, an unsigned bundle), reads the
    /// upload-key password from the gitignored Keys/KEYSTORE-INFO.txt and writes the .aab into
    /// Builds/Android/. Tester APKs keep the manual route in Docs/build-and-share.md.
    /// </summary>
    public static class BuildMenu
    {
        private const string MENU_SCENE_PATH = "Assets/Scenes/MainMenu.unity";
        private const string KEYSTORE_INFO_PATH = "Keys/KEYSTORE-INFO.txt";
        private const string OUTPUT_DIR = "Builds/Android";
        private const string PASSWORD_LINE_PREFIX = "Password:";

        [MenuItem("Tools/Dogtor/Build Android App Bundle (Play upload)")]
        private static void BuildAppBundle()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                EditorUtility.DisplayDialog("Build App Bundle",
                    "Switch the active platform to Android first (File → Build Profiles → Android → Switch Platform).",
                    "OK");
                return;
            }

            if (!EnsureTestBuildOff()) return;

            string password = ReadKeystorePassword();
            if (password == null) return;

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystorePass = password;
            PlayerSettings.Android.keyaliasPass = password;

            bool previousBundleSetting = EditorUserBuildSettings.buildAppBundle;
            EditorUserBuildSettings.buildAppBundle = true;

            string fileName = $"DogtorBurguer-{PlayerSettings.bundleVersion}-vc{PlayerSettings.Android.bundleVersionCode}.aab";
            string outputPath = Path.Combine(OUTPUT_DIR, fileName);
            Directory.CreateDirectory(OUTPUT_DIR);

            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None,
            };

            BuildReport report;
            try
            {
                report = BuildPipeline.BuildPlayer(options);
            }
            finally
            {
                EditorUserBuildSettings.buildAppBundle = previousBundleSetting;
            }

            if (report.summary.result == BuildResult.Succeeded)
            {
                long sizeMb = new FileInfo(outputPath).Length / (1024 * 1024);
                Debug.Log($"[BuildMenu] App bundle built: {outputPath} ({sizeMb} MB)");
                EditorUtility.RevealInFinder(Path.GetFullPath(outputPath));
            }
            else
            {
                Debug.LogError($"[BuildMenu] Build {report.summary.result}: {report.summary.totalErrors} error(s) — see the Console.");
            }
        }

        /// <summary>
        /// The menu scene's MainMenuUI carries the Test Build switch (mock store, free ads, all
        /// skins). A store build must ship with it off; offer to flip + save it right here.
        /// </summary>
        private static bool EnsureTestBuildOff()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;

            var scene = EditorSceneManager.OpenScene(MENU_SCENE_PATH, OpenSceneMode.Single);
            MainMenuUI menu = scene.GetRootGameObjects()
                .Select(go => go.GetComponentInChildren<MainMenuUI>(true))
                .FirstOrDefault(m => m != null);
            if (menu == null)
            {
                Debug.LogError($"[BuildMenu] No MainMenuUI found in {MENU_SCENE_PATH}.");
                return false;
            }

            var serialized = new SerializedObject(menu);
            SerializedProperty testBuild = serialized.FindProperty("_testBuild");
            if (testBuild == null)
            {
                Debug.LogError("[BuildMenu] MainMenuUI has no _testBuild field — update BuildMenu.");
                return false;
            }
            if (!testBuild.boolValue) return true;

            bool fix = EditorUtility.DisplayDialog("Test Build is ON",
                "MainMenu.unity has the Test Build switch ticked (mock store, free ads, every skin owned). " +
                "A Play upload must ship with it OFF.\n\nUntick it and save the scene?",
                "Untick and build", "Cancel");
            if (!fix) return false;

            testBuild.boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[BuildMenu] Test Build switched off in MainMenu.unity.");
            return true;
        }

        private static string ReadKeystorePassword()
        {
            if (!File.Exists(KEYSTORE_INFO_PATH))
            {
                EditorUtility.DisplayDialog("Build App Bundle",
                    $"{KEYSTORE_INFO_PATH} not found — the upload keystore password lives there (gitignored).", "OK");
                return null;
            }

            string line = File.ReadLines(KEYSTORE_INFO_PATH)
                .FirstOrDefault(l => l.TrimStart().StartsWith(PASSWORD_LINE_PREFIX, StringComparison.OrdinalIgnoreCase));
            // The password is the first token after the colon — the line may carry a note in
            // parentheses after it.
            string password = line?.Substring(line.IndexOf(':') + 1).Trim().Split(' ', '\t')[0];
            if (string.IsNullOrEmpty(password))
            {
                EditorUtility.DisplayDialog("Build App Bundle",
                    $"No '{PASSWORD_LINE_PREFIX} …' line in {KEYSTORE_INFO_PATH}.", "OK");
                return null;
            }
            return password;
        }
    }
}
