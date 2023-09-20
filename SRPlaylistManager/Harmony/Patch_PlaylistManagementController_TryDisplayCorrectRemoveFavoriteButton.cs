using HarmonyLib;
using System;
using Il2CppTMPro;
using Il2CppUtil.Controller;
using Il2Cpp;

namespace SRPlaylistManager.Harmony
{
    [HarmonyPatch(typeof(PlaylistManagementController), nameof(PlaylistManagementController.TryDisplayCorrectRemoveFavoriteButton))]
    public class Patch_PlaylistManagementController_TryDisplayCorrectRemoveFavoriteButton
    {
        public static void Postfix(PlaylistManagementController __instance)
        {
            // Override the remove button text
            __instance.pf_RemoveFromPlaylistButton?.GetComponentInChildren<TMP_Text>(true)?.SetText("Playlist", true);

            // Also add in our own behavior
            var button = __instance.pf_RemoveFromPlaylistButton?.GetComponentInChildren<SynthUIButton>();
            button.WhenClicked = new UnityEngine.Events.UnityEvent();
            button.WhenClicked.AddListener(new Action(() => { SRPlaylistManager.Instance.OnToggleMainMenuPlaylistButton(); }));

            // Hide icon. Still off-center, but not as obviously a different button :)
            __instance.pf_SongAddFavoriteButton?.transform.Find("Icon")?.gameObject.SetActive(false);
        }
    }
}
