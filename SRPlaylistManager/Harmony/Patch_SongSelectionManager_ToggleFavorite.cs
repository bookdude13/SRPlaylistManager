using HarmonyLib;
using Il2CppSynth.SongSelection;

namespace SRPlaylistManager.Harmony
{
    [HarmonyPatch(typeof(SongSelectionManager), nameof(SongSelectionManager.ToggleFavorite))]
    public class Patch_SongSelectionManager_ToggleFavorite
    {
        public static bool Prefix()
        {
            SRPlaylistManager.Instance?.OnToggleMainMenuPlaylistButton();

            // Don't follow the normal "Add/Remove Favorites" logic
            return false;
        }
    }
}
