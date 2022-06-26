using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace StacklandsHigherUI {
    [BepInPlugin(GUID, Name, Version)]
    [HarmonyPatch]
    public class Mod : BaseUnityPlugin {
        public const string Name = "Higher UI";
        public const string GUID = "com.maxsch.stacklands.higherui";
        public const string Version = "0.1.0";

        private static Harmony harmony;

        private static ConfigEntry<float> sideBarHeight;
        private static ConfigEntry<float> infoBoxHeight;

        private void Awake() {
            harmony = new Harmony(GUID);
            harmony.PatchAll();

            // 420 is vanilla
            sideBarHeight = Config.Bind("UI", "Sidebar Height", 620f, "Height of the side bar");
            sideBarHeight.SettingChanged += (_1, _2) => UpdateSideBarHeight();

            // 200 is vanilla
            infoBoxHeight = Config.Bind("UI", "Info Box Height", 360f, "Height of the info box");
            infoBoxHeight.SettingChanged += (_1, _2) => UpdateInfoBoxHeight();
        }

        [HarmonyPatch(typeof(GameScreen), nameof(GameScreen.Awake)), HarmonyPostfix]
        private static void AfterGameScreenAwake() {
            UpdateSideBarHeight();
            UpdateInfoBoxHeight();
        }

        private static void UpdateSideBarHeight() {
            if (!GameScreen.instance) {
                return;
            }

            Vector2 sizeDelta = GameScreen.instance.SideTransform.sizeDelta;
            sizeDelta.y = sideBarHeight.Value;
            GameScreen.instance.SideTransform.sizeDelta = sizeDelta;
        }

        private static void UpdateInfoBoxHeight() {
            if (!GameScreen.instance) {
                return;
            }

            RectTransform infoBox = (RectTransform)GameScreen.instance.InfoBoxBackground.transform.parent;
            Vector2 sizeDelta = infoBox.sizeDelta;
            sizeDelta.y = infoBoxHeight.Value;
            infoBox.sizeDelta = sizeDelta;
        }
    }
}
