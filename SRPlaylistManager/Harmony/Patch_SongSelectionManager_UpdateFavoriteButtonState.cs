using HarmonyLib;
using Il2CppSynth.SongSelection;
using SRModCore;

namespace SRPlaylistManager.Harmony
{
    [HarmonyPatch(typeof(SongSelectionManager), "UpdateFavoriteButtonState")]
    public class Patch_SongSelectionManager_UpdateFavoriteButtonState
    {
        public static bool Prefix(SongSelectionManager __instance)
        {
            // Don't follow the normal text changes
            // TODO better text and do translation
            __instance.favoriteBtn.toolTipLabelNormal = "Select Playlists";
            __instance.favoriteBtn.toolTipLabelSelected = "Select Playlists";

            __instance.favoriteBtn.synthUIButton.SetText("Select Playlists");

            // TODO set state based on if the current song is in _any_ playlist, not just favorites?

            return true;
        }
    }
}
