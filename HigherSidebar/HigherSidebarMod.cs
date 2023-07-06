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
        }

        private void OnDestroy() {
            harmony.UnpatchSelf();
        }

        public override void Ready() {
            dynamicHeight = Config.GetEntry<bool>("Use Dynamic Height", true, new ConfigUI {
                Tooltip = $"Dynamically resize the info box based on the preferred text height",
            });
            dynamicHeight.OnChanged += (_) => UpdateSidebarHeights();

            infoBoxMinHeight = Config.GetEntry<float>("Info Box Min Height", 300f, new ConfigUI {
                Tooltip = $"Min height of the info box. Used if Dynamic Height is active",
            });
            infoBoxMinHeight.OnChanged += (_) => UpdateSidebarHeights();

            infoBoxMaxHeight = Config.GetEntry<float>("Info Box Max Height", 700f, new ConfigUI {
                Tooltip = $"Max height of the info box. Used if Dynamic Height is active",
            });
            infoBoxMaxHeight.OnChanged += (_) => UpdateSidebarHeights();

            infoBoxHeight = Config.GetEntry<float>("Info Box Height", 400f, new ConfigUI {
                Tooltip = $"Height of the info box. Used if Dynamic Height is not active. Vanilla is 300.",
            });
            infoBoxHeight.OnChanged += (_) => UpdateSidebarHeights();

            heightChangeCooldown = Config.GetEntry<float>("Height Change Cooldown", 0.1f, new ConfigUI {
                Tooltip = $"Time before the dynamic height is shrunken again. Helps to box resizes when moving between cards",
            });
            heightChangeCooldown.OnChanged += (_) => UpdateSidebarHeights();
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

            foreach (GameObject divider in GameScreen.instance.dividerList) {
                if (divider.activeInHierarchy && divider.TryGetComponent(out VerticalLayoutGroup verticalLayoutGroup)) {
                    preferredHeight += verticalLayoutGroup.preferredHeight + verticalLayoutGroup.spacing;
                }
            }

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
