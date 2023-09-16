using HarmonyLib;
using Il2CppSynth.SongSelection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Il2CppUtil.Controller;

namespace SRPlaylistManager.Harmony
{
    [HarmonyPatch(typeof(PlaylistManagementController), nameof(PlaylistManagementController.Interface__TryDeleteSongFromPlaylist))]
    public class Patch_PlaylistManagementController_Interface__TryDeleteSongFromPlaylist
    {
        public static bool Prefix()
        {
            SRPlaylistManager.Instance?.Log("HARMONY TryDeleteSongFromPlaylist");
            //return true;
            SRPlaylistManager.Instance?.OnTogglePlaylistButton();

            // Don't follow the normal "Add/Remove Favorites" logic
            return false;
        }
    }
}
