using System;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HigherSidebar {
    [HarmonyPatch]
    public class HigherSidebarMod : Mod {
        public const string Name = "Higher Sidebar";
        public const string GUID = "com.maxsch.stacklands.higherui";
        public const string Version = "0.4.0";

        private static Harmony harmony;

        private static ConfigEntry infoBoxMinHeight;
        private static ConfigEntry infoBoxMaxHeight;
        private static ConfigEntry infoBoxHeight;
        private static ConfigEntry dynamicHeight;
        private static ConfigEntry heightChangeCooldown;

        private static float lastHeight;
        private static float heightCooldown;

        private void Awake() {
            harmony = new Harmony(GUID);
            harmony.PatchAll();
        }

        private void OnDestroy() {
            harmony.UnpatchSelf();
        }

        public override void Ready() {
            const string reload = "Reloads in-game when the config file is edited or a configuration manager is used.";
            string description = "";

            description = $"Dynamically resize the info box based on the preferred text height.{Environment.NewLine}{reload}";
            dynamicHeight = Config.GetEntry<bool>("Use Dynamic Height", true);
            dynamicHeight.ExtraData["tooltip"] = description;
            // dynamicHeight.SettingChanged += (_1, _2) => UpdateSidebarHeights();

            description = $"Min height of the info box. Used if Dynamic Height is active.{Environment.NewLine}{reload}";
            infoBoxMinHeight = Config.GetEntry<float>("Info Box Min Height", 300f);
            infoBoxMinHeight.ExtraData["tooltip"] = description;
            // infoBoxMinHeight.SettingChanged += (_1, _2) => UpdateSidebarHeights();

            description = $"Max height of the info box. Used if Dynamic Height is active.{Environment.NewLine}{reload}";
            infoBoxMaxHeight = Config.GetEntry<float>("Info Box Max Height", 700f);
            infoBoxMaxHeight.ExtraData["tooltip"] = description;
            // infoBoxMaxHeight.SettingChanged += (_1, _2) => UpdateSidebarHeights();

            description = $"Height of the info box. Used if Dynamic Height is not active. Vanilla is 300.{Environment.NewLine}{reload}";
            infoBoxHeight = Config.GetEntry<float>("Info Box Height", 400f);
            infoBoxHeight.ExtraData["tooltip"] = description;
            // infoBoxHeight.SettingChanged += (_1, _2) => UpdateSidebarHeights();

            description = $"Time before the dynamic height is shrunken again. Helps to box resizes when moving between cards.{Environment.NewLine}{reload}";
            heightChangeCooldown = Config.GetEntry<float>("Height Change Cooldown", 0.1f);
            heightChangeCooldown.ExtraData["tooltip"] = description;
            // heightChangeCooldown.SettingChanged += (_1, _2) => UpdateSidebarHeights();
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
            if (!dynamicHeight.GetBool()) {
                return infoBoxHeight.GetFloat();
            }

            GameScreen.instance.InfoText.text = GameScreen.instance.InfoText.text.Trim();
            float preferredHeight = GameScreen.instance.Valuetext.preferredHeight + GameScreen.instance.InfoText.preferredHeight + 72f;
            return Mathf.Clamp(preferredHeight, infoBoxMinHeight.GetFloat(), infoBoxMaxHeight.GetFloat());
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

            if (newHeight < lastHeight && heightCooldown < heightChangeCooldown.GetFloat()) {
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
