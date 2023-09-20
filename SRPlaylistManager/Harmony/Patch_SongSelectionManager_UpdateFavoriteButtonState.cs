using HarmonyLib;
using Il2CppSynth.SongSelection;

namespace SRPlaylistManager.Harmony
{
    [HarmonyPatch(typeof(SongSelectionManager), "UpdateFavoriteButtonState")]
    public class Patch_SongSelectionManager_UpdateFavoriteButtonState
    {
        public static bool Prefix(SongSelectionManager __instance)
        {
            // Don't follow the normal text changes
            // TODO better text and do translation
            __instance.favoriteBtnLabel.SetText("Playlist", true);

            return false;
        }
    }
}
