using HarmonyLib;
using Il2CppSynth.Multiplayer;

namespace SRPlaylistManager.Harmony
{
    // This is called during initialization (for the host),
    // and from Game_InfoProvider.OnEvent() with event code SetSelectedVersusTrack (for clients)
    [HarmonyPatch(typeof(MultiplayerRoomView), nameof(MultiplayerRoomView.FillSelectedTrackData))]
    public class Patch_MultiplayerRoomView_FillSelectedTrackData
    {
        public static void Postfix()
        {
            // Make sure the playlist button is properly set up!
            SRPlaylistManager.Instance.OnMultiplayerTrackFill();
        }
    }
}
