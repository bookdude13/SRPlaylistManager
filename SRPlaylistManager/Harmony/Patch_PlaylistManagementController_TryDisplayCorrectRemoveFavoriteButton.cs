using HarmonyLib;
using Synth.SongSelection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Util.Controller;

namespace SRPlaylistManager.Harmony
{
    [HarmonyPatch(typeof(PlaylistManagementController), nameof(PlaylistManagementController.TryDisplayCorrectRemoveFavoriteButton))]
    public class Patch_PlaylistManagementController_TryDisplayCorrectRemoveFavoriteButton
    {
        public static void Postfix(GameObject ___pf_RemoveFromPlaylistButton, GameObject ___pf_SongAddFavoriteButton)
        {
            // Override the remove button text
            ___pf_RemoveFromPlaylistButton?.GetComponentInChildren<TMP_Text>(true)?.SetText("Playlist", true);

            // Hide icon. Still off-center, but not as obviously a different button :)
            ___pf_SongAddFavoriteButton?.transform.Find("Icon")?.gameObject.SetActive(false);
        }
    }
}
