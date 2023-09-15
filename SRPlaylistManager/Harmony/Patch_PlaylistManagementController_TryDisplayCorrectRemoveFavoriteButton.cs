using HarmonyLib;
using Il2CppSynth.SongSelection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppTMPro;
using UnityEngine;
using Il2CppUtil.Controller;

namespace SRPlaylistManager.Harmony
{
    [HarmonyPatch(typeof(PlaylistManagementController), nameof(PlaylistManagementController.TryDisplayCorrectRemoveFavoriteButton))]
    public class Patch_PlaylistManagementController_TryDisplayCorrectRemoveFavoriteButton
    {
        public static void Postfix(PlaylistManagementController __instance)
        {
            // Override the remove button text
            __instance.pf_RemoveFromPlaylistButton?.GetComponentInChildren<TMP_Text>(true)?.SetText("Playlist", true);

            // Hide icon. Still off-center, but not as obviously a different button :)
            __instance.pf_SongAddFavoriteButton?.transform.Find("Icon")?.gameObject.SetActive(false);
        }
    }
}
