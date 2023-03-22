using HarmonyLib;
using Synth.SongSelection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util.Controller;

namespace SRPlaylistManager.Harmony
{
    [HarmonyPatch(typeof(SongSelectionManager), nameof(SongSelectionManager.ToggleFavorite))]
    public class Patch_SongSelectionManager_ToggleFavorite
    {
        public static bool Prefix()
        {
            SRPlaylistManager.Instance?.OnTogglePlaylistButton();

            // Don't follow the normal "Add/Remove Favorites" logic
            return false;
        }
    }
}
