using HarmonyLib;
using Il2Cpp;
using Il2CppSynth.SongSelection;
using SRModCore;
using UnityEngine.Playables;
using static MelonLoader.MelonLogger;

namespace SRPlaylistManager.Harmony
{
    [HarmonyPatch(typeof(SongSelectionManager), "UpdateFavoriteButtonState")]
    public class Patch_SongSelectionManager_UpdateFavoriteButtonState
    {
        public static void Postfix(SongSelectionManager __instance)
        {
            SetupButton(__instance);
        }

        private static void SetupButton(SongSelectionManager __instance)
        {
            // Don't follow the normal text changes
            // TODO better text and do translation
            if (__instance.favoriteBtn == null)
            {
                SRPlaylistManager.Instance.Log("favoriteBtn was null!");
            }
            else
            {
                __instance.favoriteBtn.toolTipLabelNormal = "Select Playlists";
                __instance.favoriteBtn.toolTipLabelSelected = "Select Playlists";

                if (__instance.favoriteBtn.synthUIButton == null)
                {
                    SRPlaylistManager.Instance.Log("synthUIButton was null!");
                }
                else
                {
                    __instance.favoriteBtn.synthUIButton.SetText("Select Playlists");
                    __instance.favoriteBtn.synthUIButton.stayHoveredwhenClicked = false;
                    __instance.favoriteBtn.synthUIButton.hideTooltipOnClick = true;
                }

                //// TODO If favorited, use the filled pink heart.
                //// If not favorited but in a playlist, use the filled neutral heart.
                //// If not in either, use the empty heart.
                //SRPlaylistManager.Instance.GetCurrentSongPlaylistState(out var isInFavorites, out var isInPlaylist);
                //var buttonIcon = __instance.favoriteBtn;
                //var buttonAddFav = __instance.addToFavoriteButton;
            }
        }
    }
}
