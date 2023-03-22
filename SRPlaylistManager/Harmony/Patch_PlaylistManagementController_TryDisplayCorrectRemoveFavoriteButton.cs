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
        public static void Postfix(GameObject ___pf_RemoveFromPlaylistButton)
        {
            // Override the remove button to just open our list as well
            ___pf_RemoveFromPlaylistButton?.GetComponentInChildren<TMP_Text>(true)?.SetText("Playlist", true);
        }
    }
}
