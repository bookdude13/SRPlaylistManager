using HarmonyLib;
using Il2CppSynth.SongSelection;
using Il2CppUtil.Controller;

namespace SRPlaylistManager.Harmony
{
    [HarmonyPatch(typeof(SongSelectionManager), nameof(SongSelectionManager.PlayPreviewAudio))]
    public class Patch_SongSelectionManager_PlayPreviewAudio
    {
        public static void Prefix()
        {
            // Force some UI refreshing on first load. This was the first hook I could make work.
            PlaylistManagementController.GetInstance.TryDisplayCorrectRemoveFavoriteButton();
        }
    }
}
