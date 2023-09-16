using HarmonyLib;
using Il2CppSynth.SongSelection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Il2CppUtil.Controller;
using static MelonLoader.MelonLogger;
using Il2CppUtil.Data;

namespace SRPlaylistManager.Harmony
{
    [HarmonyPatch(typeof(PlaylistManagementController), nameof(PlaylistManagementController.Interface__TryAddSongsToSelectredPlaylist))]
    public class Patch_PlaylistManagementController_Interface__TryAddSongsToSelectredPlaylist
    {
        public static bool Prefix(PlaylistManagementController __instance)
        {
            SRPlaylistManager.Instance?.Log("HARMONY TryAddSongsToSelectredPlaylist");
            SRPlaylistManager.Instance?.Log($"" +
                $"{__instance.CurrentPlaylistIndex}, " + //-1
                $"{__instance.CurrentPlaylistSongIndex}, " + //0
                $"{__instance.CurrentSelectedPlaylist?.Name}, " +// New Playlist 8
                $"{__instance.CurrentSongSelectionType}, " +// MUSICPACKS
                $"{__instance.CurrentAddingSongsSelectedPlaylistIndex}, " + // 3 (all is 0, favs is 1)
                $"{SongSelectionManager.GetInstance?.IsOnPlaylistInterface}, " + // F
                $"{SongSelectionManager.GetInstance?.IsAddingSongToPlayList}, " + // F
                $"");
            return true;
            /*SRPlaylistManager.Instance?.OnTogglePlaylistButton();

            // Don't follow the normal "Add/Remove Favorites" logic
            return false;*/
        }

        public static void Postfix(PlaylistManagementController __instance)
        {
            SRPlaylistManager.Instance?.Log("HARMONY TryAddSongsToSelectredPlaylist POSTFIX");

            SRPlaylistManager.Instance?.Log($"" +
                $"{__instance.CurrentPlaylistIndex}, " + //-1
                $"{__instance.CurrentPlaylistSongIndex}, " + //0
                $"{__instance.CurrentSelectedPlaylist?.Name}, " +// New Playlist 8
                $"{__instance.CurrentSongSelectionType}, " +// MUSICPACKS
                $"{__instance.CurrentAddingSongsSelectedPlaylistIndex}, " + // 3 (all is 0, favs is 1)
                $"{SongSelectionManager.GetInstance?.IsOnPlaylistInterface}, " + // F
                $"{SongSelectionManager.GetInstance?.IsAddingSongToPlayList}, " + // F
                $"");
        }
    }

    [HarmonyPatch(typeof(PlaylistManagementController), nameof(PlaylistManagementController.Interface__OnSongSelectedForPlaylist))]
    public class Patch_PlaylistManagementController_Interface__OnSongSelectedForPlaylist
    {
        public static bool Prefix(PlaylistManagementController __instance)
        {
            SRPlaylistManager.Instance?.Log("HARMONY OnSongSelectedForPlaylist");
            SRPlaylistManager.Instance?.Log($"" +
                $"{__instance.CurrentPlaylistIndex}, " + //-1
                $"{__instance.CurrentPlaylistSongIndex}, " + //0
                $"{__instance.CurrentSelectedPlaylist?.Name}, " +// New Playlist 8
                $"{__instance.CurrentSongSelectionType}, " +// MUSICPACKS
                $"{__instance.CurrentAddingSongsSelectedPlaylistIndex}, " + // 3 (all is 0, favs is 1)
                $"{SongSelectionManager.GetInstance?.IsOnPlaylistInterface}, " + // F
                $"{SongSelectionManager.GetInstance?.IsAddingSongToPlayList}, " + // F
                $"");
            return true;
            /*SRPlaylistManager.Instance?.OnTogglePlaylistButton();

            // Don't follow the normal "Add/Remove Favorites" logic
            return false;*/
        }
    }

    [HarmonyPatch(typeof(PlaylistManagementController), nameof(PlaylistManagementController.OnPlaylistItemClicked))]
    public class Patch_PlaylistManagementController_Interface__OnPlaylistItemClicked
    {
        public static bool Prefix(PlaylistManagementController __instance, PlaylistItem playlist)
        {
            SRPlaylistManager.Instance?.Log("HARMONY OnPlaylistItemClicked");
            SRPlaylistManager.Instance?.Log($"Item {playlist.Name}");
            /*SRPlaylistManager.Instance?.Log($"" +
                $"{__instance.CurrentPlaylistIndex}, " + //-1
                $"{__instance.CurrentPlaylistSongIndex}, " + //0
                $"{__instance.CurrentSelectedPlaylist?.Name}, " +// New Playlist 8
                $"{__instance.CurrentSongSelectionType}, " +// MUSICPACKS
                $"{__instance.CurrentAddingSongsSelectedPlaylistIndex}, " + // 3 (all is 0, favs is 1)
                $"{SongSelectionManager.GetInstance?.IsOnPlaylistInterface}, " + // F
                $"{SongSelectionManager.GetInstance?.IsAddingSongToPlayList}, " + // F
                $"");*/
            return true;
            /*SRPlaylistManager.Instance?.OnTogglePlaylistButton();

            // Don't follow the normal "Add/Remove Favorites" logic
            return false;*/
        }
    }

    [HarmonyPatch(typeof(PlaylistManagementController), nameof(PlaylistManagementController.OnPlaylistSongClicked))]
    public class Patch_PlaylistManagementController_Interface__OnPlaylistSongClicked
    {
        public static bool Prefix(PlaylistManagementController __instance, PlaylistSong songClicked)
        {
            SRPlaylistManager.Instance?.Log("HARMONY OnPlaylistSongClicked");
            SRPlaylistManager.Instance?.Log($"Song {songClicked.name}");
            return true;
            /*SRPlaylistManager.Instance?.OnTogglePlaylistButton();

            // Don't follow the normal "Add/Remove Favorites" logic
            return false;*/
        }
    }
}
