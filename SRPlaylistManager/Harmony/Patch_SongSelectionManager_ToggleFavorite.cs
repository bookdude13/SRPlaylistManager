using HarmonyLib;
using Il2CppSynth.SongSelection;

namespace SRPlaylistManager.Harmony
{
    [HarmonyPatch(typeof(SongSelectionManager), nameof(SongSelectionManager.ToggleFavorite))]
    public class Patch_SongSelectionManager_ToggleFavorite
    {
        public static bool Prefix(SongSelectionManager __instance)
        {
            SRPlaylistManager.Instance?.OnToggleMainMenuPlaylistButton();

            __instance.UpdateFavoriteButtonState();

            // Don't follow the normal "Add/Remove Favorites" logic
            return false;
        }
    }
}
