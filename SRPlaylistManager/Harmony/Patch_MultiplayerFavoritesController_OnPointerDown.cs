using HarmonyLib;
using System;
using Il2CppUtil.Controller;
using UnityEngine.EventSystems;

namespace SRPlaylistManager.Harmony
{

    [HarmonyPatch(typeof(MultiplayerFavoritesController), nameof(MultiplayerFavoritesController.OnPointerDown), new Type[] { typeof(PointerEventData) })]
    public class Patch_MultiplayerFavoritesController_OnPointerDown
    {
        public static bool Prefix(MultiplayerFavoritesController __instance, PointerEventData eventData)
        {
            // There isn't a SynthUIButton for this, it's handled directly...

            SRPlaylistManager.Instance?.Log("MP Favorites Toggle OnPointerDown");

            __instance.tooltip = "Select Playlists";

            // Treat the click as the end to the hover, to hide the tooltip
            //__instance.OnPointerExit(eventData);
            __instance.isHovered = false;

            SRPlaylistManager.Instance?.OnToggleMultiplayerPlaylistButton();

            // Stop normal favorites triggering
            return false;
        }
    }
}
