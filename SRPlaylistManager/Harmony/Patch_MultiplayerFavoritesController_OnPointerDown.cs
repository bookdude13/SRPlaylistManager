using HarmonyLib;
using System;
using Il2CppUtil.Controller;
using UnityEngine.EventSystems;

namespace SRPlaylistManager.Harmony
{
    // TODO update tooltip

    [HarmonyPatch(typeof(MultiplayerFavoritesController), nameof(MultiplayerFavoritesController.OnPointerDown), new Type[] { typeof(PointerEventData) })]
    public class Patch_MultiplayerFavoritesController_OnPointerDown
    {
        public static bool Prefix(PointerEventData eventData)
        {
            // There isn't a SynthUIButton for this, it's handled directly...

            SRPlaylistManager.Instance?.Log("MP Favorites Toggle OnPointerDown");

            SRPlaylistManager.Instance?.OnToggleMultiplayerPlaylistButton();

            // Stop normal favorites triggering
            return false;
        }
    }
}
