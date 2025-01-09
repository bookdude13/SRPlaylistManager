using HarmonyLib;
using System;
using Il2CppUtil.Controller;
using UnityEngine.EventSystems;

namespace SRPlaylistManager.Harmony
{
    [HarmonyPatch(typeof(MultiplayerFavoritesController), nameof(MultiplayerFavoritesController.OnPointerEnter), new Type[] { typeof(PointerEventData) })]
    public class Patch_MultiplayerFavoritesController_OnPointerEnter
    {
        public static void Prefix(MultiplayerFavoritesController __instance, PointerEventData eventData)
        {
            // Override tooltip text before any logic runs
            __instance.tooltip = "Select Playlists";
        }

        public static void Postfix(MultiplayerFavoritesController __instance, PointerEventData eventData)
        {
            // Make sure the tooltip stays overwritten after logic runs
            __instance.tooltip = "Select Playlists";
        }
    }
}
