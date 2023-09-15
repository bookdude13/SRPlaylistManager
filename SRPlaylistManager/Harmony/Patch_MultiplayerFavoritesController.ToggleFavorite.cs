using HarmonyLib;
using Il2CppSynth.SongSelection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppUtil.Controller;

namespace SRPlaylistManager.Harmony
{
    /*[HarmonyPatch(typeof(MultiplayerFavoritesController), nameof(MultiplayerFavoritesController.ToggleFavorite))]
    public class Patch_MultiplayerFavoritesController_ToggleFavorite
    {
        public static bool Prefix()
        {
            SRPlaylistManager.Instance?.OnTogglePlaylistButton();

            // Don't follow the normal "Add/Remove Favorites" logic
            return false;
        }
    }*/
}
