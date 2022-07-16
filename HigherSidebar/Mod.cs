using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace HigherSidebar {
    [BepInPlugin(GUID, Name, Version)]
    [HarmonyPatch]
    public class Mod : BaseUnityPlugin {
        public const string Name = "Higher Sidebar";
        public const string GUID = "com.maxsch.stacklands.higherui";
        public const string Version = "0.1.0";

        private static Harmony harmony;

        private static ConfigEntry<float> infoBoxHeight;

        private void Awake() {
            harmony = new Harmony(GUID);
            harmony.PatchAll();

            // 200 is vanilla
            infoBoxHeight = Config.Bind("UI", "Info Box Height", 400f, "Height of the info box");
            infoBoxHeight.SettingChanged += (_1, _2) => UpdateHeights();
        }

        [HarmonyPatch(typeof(GameScreen), nameof(GameScreen.Awake)), HarmonyPostfix]
        private static void AfterGameScreenAwake() {
            UpdateHeights();
        }

        private static void UpdateHeights() {
            if (!GameScreen.instance) {
                return;
            }

            // InfoBox
            RectTransform infoBox = (RectTransform)GameScreen.instance.ValueParent.transform.parent;
            Vector2 sizeDelta = infoBox.sizeDelta;
            sizeDelta.y = infoBoxHeight.Value;
            infoBox.sizeDelta = sizeDelta;

            // SideBar
            sizeDelta = GameScreen.instance.SideTransform.sizeDelta;
            sizeDelta.y = - infoBoxHeight.Value - 15;
            GameScreen.instance.SideTransform.sizeDelta = sizeDelta;
        }
    }
}
