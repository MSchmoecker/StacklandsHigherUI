using System;
using System.IO;
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
        public const string Version = "0.1.4";

        private static Harmony harmony;

        private static ConfigEntry<float> infoBoxHeight;

        private void Awake() {
            harmony = new Harmony(GUID);
            harmony.PatchAll();

            // 300 is vanilla
            string description = $"Height of the info box. Vanilla is 300.{Environment.NewLine}" +
                                 $"Reloads in-game when the config file is edited or a configuration manager is used.";
            infoBoxHeight = Config.Bind("UI", "Info Box Height", 400f, description);
            infoBoxHeight.SettingChanged += (_1, _2) => UpdateSidebarHeights();

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

        [HarmonyPatch(typeof(GameScreen), nameof(GameScreen.Awake)), HarmonyPostfix]
        private static void AfterGameScreenAwake() {
            UpdateSidebarHeights();
        }

        private static void UpdateSidebarHeights() {
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
