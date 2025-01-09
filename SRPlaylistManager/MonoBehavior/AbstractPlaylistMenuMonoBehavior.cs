using SRModCore;
using SRPlaylistManager.Models;
using SRPlaylistManager.Services;
using Il2CppSynth.SongSelection;
using System;
using UnityEngine;
using Il2CppUtil.Controller;
using MelonLoader;
using Il2CppPigeonCoopToolkit.Utillities;

namespace SRPlaylistManager.MonoBehavior
{
    [RegisterTypeInIl2Cpp]
    public abstract class AbstractPlaylistMenuMonoBehavior : MonoBehaviour
    {
        public AbstractPlaylistMenuMonoBehavior(IntPtr ptr) : base(ptr) { }

        protected SRLogger _logger;
        private PlaylistService playlistService;
        private ScrollablePanel playlistPanel;
        protected int songPlaylistIndexBeforeOpen = -1;

        public void Init(SRLogger logger, PlaylistService playlistService)
        {
            _logger = logger;
            this.playlistService = playlistService;
        }

        protected virtual GameObject GetToggledView()
        {
            return null;
        }
        protected virtual void OnMenuClose(GameObject toHide)
        {
            toHide.SetActive(true);
        }

        protected void LogVerbose(string message)
        {
            if (SRPlaylistManager.VERBOSE_LOGS)
            {
                _logger.Msg(message);
            }
        }

        private ScrollablePanel CreatePlaylistPanel(GameObject toHide)
        {
            // Add our playlist menu
            var panel = ScrollablePanel.Create("playlist_panel", () => OnMenuClose(toHide), _logger);

            // Use hidden view's parent as our own, since we basically replace it
            //_logger.Msg("Setting parent to " + toHide.transform.name + " parent, " + toHide.transform.parent.name);
            panel.Panel.transform.SetParent(toHide.transform.parent, true);
            //panel.Panel.transform.parent = toHide.transform.parent;

            // Add header
            panel.AddHeader("playlists_header", "Playlists");

            return panel;
        }

        protected virtual Vector3 GetPanelOffset() => Vector3.zero;

        public void OpenMenu()
        {
            var playlists = playlistService.GetPlaylists();

            // Get currently selected song
            var currentSong = GetSelectedTrack();
            if (currentSong == null)
            {
                _logger.Error("Current selected song null; not opening menu");
                return;
            }
            LogVerbose($"Current song: '{currentSong.SongDataName}' {currentSong.TrackName} {currentSong.Author}");

            // Stop any songs that are playing
            SongSelectionManager.GetInstance?.StopPreviewAudio();

            songPlaylistIndexBeforeOpen = currentSong.SearchIndex;
            LogVerbose($"Song search index before open: {songPlaylistIndexBeforeOpen}");
            LogVerbose($"Current plist song idx {PlaylistManagementController.GetInstance.CurrentPlaylistSongIndex}");
            LogVerbose($"Current plist song idx lookup {PlaylistManagementController.GetInstance.GetCurrentSelectedPlaylistSongIndex(currentSong.LeaderboardHash, currentSong.TrackName, currentSong.Author)}");

            // Get parent for panel
            var viewToHide = GetToggledView();// SongSelectionView.GetView();
            if (viewToHide == null)
            {
                _logger.Error("Failed to find view that will be toggled/hidden");
                return;
            }

            // Create panel if needed
            if (playlistPanel == null)
            {
                playlistPanel = CreatePlaylistPanel(viewToHide);

                // Offset panel as needed
                playlistPanel.Panel.transform.localPosition += GetPanelOffset();
            }

            // Add items
            LogVerbose($"Adding items");
            playlistPanel.ClearItems();
            foreach (var playlist in playlists)
            {
                var panelItem = new PlaylistPanelItem(playlist, currentSong, _logger);
                playlistPanel.AddItem(panelItem);
            }

            // Show menu, hide center view
            LogVerbose($"Showing");
            playlistPanel.SetVisibility(true);
            viewToHide.gameObject.SetActive(false);
        }

        private Il2CppSynth.Retro.Game_Track_Retro GetSelectedTrack()
        {
            return Il2CppSynth.SongSelection.SongSelectionManager.GetInstance?.SelectedGameTrack;
        }

        /// <summary>
        /// Updates the current playlist view with any playlist changes made.
        /// </summary>
        /// <returns>True if the refreshed view was a fixed view (all songs, favorites) or not found, false if it was a user playlist</returns>
        public bool RefreshCurrentPlaylistView()
        {
            var controller = PlaylistManagementController.GetInstance;


            LogVerbose("Current playlist idx: " + controller.CurrentPlaylistIndex);
            LogVerbose("Current selected playlist: " + controller.CurrentSelectedPlaylist?.Name);
            LogVerbose("Current selection type: " + controller.CurrentSongSelectionType);

            // Refresh currently selected playlist view
            if (controller.CurrentPlaylistIndex >= 0)
            {
                if (controller.CurrentSongSelectionType == Il2CppUtil.Data.SongSelectionType.FAVORITES)
                {
                    // Favorites playlist logic
                    LogVerbose("ShowFavorites");
                    // 0 is all songs, 1 is favorites
                    controller.Interface__ShowFavorites();
                    //controller.Interface__OnPlaylistScrollItemClick(1);
                    return true;
                }
                else if (controller.CurrentSelectedPlaylist.ShowAllSongs)
                {
                    LogVerbose("ShowAllSongs");
                    controller.Interface__OnPlaylistScrollItemClick(0);
                    return true;
                }
                else if (controller.CurrentSongSelectionType == Il2CppUtil.Data.SongSelectionType.EXPERIENCES)
                {
                    LogVerbose("ShowAllExperiences");
                    controller.Interface__ShowExperiencesShelf();
                    return true;
                }
                else
                {
                    // Not the special playlists, so just click it like normal
                    // Note that finding the current selected playlist was returning an index +1 too high, so the current index is simply used
                    // TODO use index - 1, or CurrentPlaylistIndex?
                    //var index = PlaylistPanelItem.FindMatchingPlaylistIndex(_logger, controller, controller.CurrentSelectedPlaylist);
                    //LogVerbose($"ItemClick playlist index {index}");
                    controller.Interface__OnPlaylistScrollItemClick(controller.CurrentPlaylistIndex);
                    return false;
                }
            }
            else
            {
                LogVerbose("Current playlist index is not a user playlist: " + controller.CurrentPlaylistIndex);
            }

            // This turns on song preview audio. Turn that off until we fully exit
            //SongSelectionManager.GetInstance.StopPreviewAudio();

            // Default to assuming a system view, not a user playlist
            return true;
        }
    }
}
