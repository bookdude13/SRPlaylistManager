using HarmonyLib;
using System;
using Il2CppTMPro;
using Il2CppUtil.Controller;
using Il2Cpp;
using SRModCore;
using UnityEngine;
using static MelonLoader.MelonLogger;

namespace SRPlaylistManager.Harmony
{
    [HarmonyPatch(typeof(PlaylistManagementController), nameof(PlaylistManagementController.TryDisplayCorrectRemoveFavoriteButton))]
    public class Patch_PlaylistManagementController_TryDisplayCorrectRemoveFavoriteButton
    {
        public static void Prefix(PlaylistManagementController __instance)
        {
            SetupToggleButton(__instance.pf_RemoveFromPlaylistButton);
            SetupToggleButton(__instance.pf_SongAddFavoriteButton);
        }

        private static void SetupToggleButton(GameObject buttonGO)
        {
            if (buttonGO == null)
                return;

            // Override the remove button text
            var btnTexts = buttonGO.GetComponentsInChildren<TMP_Text>(true);
            foreach (var btnText in btnTexts)
            {
                btnText?.SetText("Select Playlists", true);
            }

            // Make sure the tooltip doesn't linger when we open up the playlist selection
            var button = buttonGO.GetComponentInChildren<SynthUIButton>();
            button.hideTooltipOnClick = true;
            button.stayHoveredwhenClicked = false;

            // Also add in our own behavior
            if (button != null)
            {
                button.WhenClicked = new UnityEngine.Events.UnityEvent();
                button.WhenClicked.AddListener(new Action(() => {
                    // Toggle playlist selection
                    SRPlaylistManager.Instance.OnToggleMainMenuPlaylistButton();
                }));
            }
        }
    }
}
