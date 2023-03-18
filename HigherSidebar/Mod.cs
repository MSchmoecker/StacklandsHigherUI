using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HigherSidebar {
    [BepInPlugin(GUID, Name, Version)]
    [HarmonyPatch]
    public class Mod : BaseUnityPlugin {
        public const string Name = "Higher Sidebar";
        public const string GUID = "com.maxsch.stacklands.higherui";
        public const string Version = "0.3.0";

        private static Harmony harmony;

        private static ConfigEntry<float> infoBoxMinHeight;
        private static ConfigEntry<float> infoBoxMaxHeight;
        private static ConfigEntry<float> infoBoxHeight;
        private static ConfigEntry<bool> dynamicHeight;
        private static ConfigEntry<float> heightChangeCooldown;

        private static float lastHeight;
        private static float heightCooldown;

        private void Awake() {
            harmony = new Harmony(GUID);
            harmony.PatchAll();

            const string reload = "Reloads in-game when the config file is edited or a configuration manager is used.";
            string description = "";

            description = $"Dynamically resize the info box based on the preferred text height.{Environment.NewLine}{reload}";
            dynamicHeight = Config.Bind("Dynamic UI", "Use Dynamic Height", true, description);
            dynamicHeight.SettingChanged += (_1, _2) => UpdateSidebarHeights();

            description = $"Min height of the info box. Used if Dynamic Height is active.{Environment.NewLine}{reload}";
            infoBoxMinHeight = Config.Bind("Dynamic UI", "Info Box Min Height", 300f, description);
            infoBoxMinHeight.SettingChanged += (_1, _2) => UpdateSidebarHeights();

            description = $"Max height of the info box. Used if Dynamic Height is active.{Environment.NewLine}{reload}";
            infoBoxMaxHeight = Config.Bind("Dynamic UI", "Info Box Max Height", 700f, description);
            infoBoxMaxHeight.SettingChanged += (_1, _2) => UpdateSidebarHeights();

            description = $"Height of the info box. Used if Dynamic Height is not active. Vanilla is 300.{Environment.NewLine}{reload}";
            infoBoxHeight = Config.Bind("UI", "Info Box Height", 400f, description);
            infoBoxHeight.SettingChanged += (_1, _2) => UpdateSidebarHeights();

            description = $"Time before the dynamic height is shrunken again. Helps to box resizes when moving between cards.{Environment.NewLine}{reload}";
            heightChangeCooldown = Config.Bind("Dynamic UI", "Height Change Cooldown", 0.1f, description);
            heightChangeCooldown.SettingChanged += (_1, _2) => UpdateSidebarHeights();

            CreateConfigWatcher();
        }

        private void CreateConfigWatcher() {
            FileSystemWatcher watcher = new FileSystemWatcher(Paths.ConfigPath, Path.GetFileName(Config.ConfigFilePath));
            watcher.Changed += (_1, _2) => Config.Reload();
            watcher.Created += (_1, _2) => Config.Reload();
            watcher.Renamed += (_1, _2) => Config.Reload();
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        }

        [HarmonyPatch(typeof(GameScreen), nameof(GameScreen.LateUpdate)), HarmonyPostfix]
        private static void AfterGameScreenUpdate() {
            UpdateSidebarHeights();
        }

        [HarmonyPatch(typeof(GameScreen), nameof(GameScreen.Awake)), HarmonyPostfix]
        private static void AfterGameScreenAwake() {
            foreach (Image image in GameScreen.instance.InfoBox.GetComponentsInChildren<Image>()) {
                image.raycastTarget = false;
            }

            foreach (TextMeshProUGUI image in GameScreen.instance.InfoBox.GetComponentsInChildren<TextMeshProUGUI>()) {
                image.raycastTarget = false;
            }
        }

        private static float CalculateHeight() {
            if (!dynamicHeight.Value) {
                return infoBoxHeight.Value;
            }

            GameScreen.instance.InfoText.text = GameScreen.instance.InfoText.text.Trim();
            float preferredHeight = GameScreen.instance.Valuetext.preferredHeight + GameScreen.instance.InfoText.preferredHeight + 72f;
            return Mathf.Clamp(preferredHeight, infoBoxMinHeight.Value, infoBoxMaxHeight.Value);
        }

        private static void UpdateSidebarHeights() {
            if (!GameScreen.instance) {
                return;
            }

            float newHeight = CalculateHeight();

            if (Math.Abs(lastHeight - newHeight) < 1f) {
                heightCooldown = 0f;
                return;
            }

            if (newHeight < lastHeight && heightCooldown < heightChangeCooldown.Value) {
                heightCooldown += Time.deltaTime;
                return;
            }

            lastHeight = newHeight;
            heightCooldown = 0f;

            // InfoBox
            RectTransform infoBox = (RectTransform)GameScreen.instance.InfoBox.transform;
            Vector2 sizeDelta = infoBox.sizeDelta;
            sizeDelta.y = newHeight;
            infoBox.sizeDelta = sizeDelta;

            LayoutRebuilder.ForceRebuildLayoutImmediate(GameScreen.instance.SideTransform);
        }
    }
}
