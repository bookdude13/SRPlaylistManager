using HarmonyLib;
using System;
using Il2CppUtil.Controller;
using UnityEngine.EventSystems;

namespace SRPlaylistManager.Harmony
{
    [HarmonyPatch(typeof(MultiplayerFavoritesController), nameof(MultiplayerFavoritesController.OnPointerEnter), new Type[] { typeof(PointerEventData) })]
    public class Patch_MultiplayerFavoritesController_OnPointerEnter
    {
        public static void Postfix(MultiplayerFavoritesController __instance, PointerEventData eventData)
        {
            // Override tooltip text
            __instance.tooltip = "Select Playlists";
        }
    }
}
