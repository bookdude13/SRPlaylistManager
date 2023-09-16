using SRModCore;
using SRPlaylistManager.Models;
using SRPlaylistManager.Services;
using Il2CppSynth.SongSelection;
using System;
using UnityEngine;
using Il2CppUtil.Controller;
using MelonLoader;

namespace SRPlaylistManager.MonoBehavior
{
    [RegisterTypeInIl2Cpp]
    class PlaylistMenuMonoBehavior : MonoBehaviour
    {
        private static SRLogger _logger;
        private PlaylistService playlistService;
        private ScrollablePanel playlistPanel;
        private int songPlaylistIndexBeforeOpen = -1;

        public PlaylistMenuMonoBehavior () { }

        public void Init(SRLogger logger, PlaylistService playlistService)
        {
            _logger = logger;
            this.playlistService = playlistService;
        }

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
            _logger.Msg($"Current song: '{currentSong.name}'");

            // Stop any songs that are playing
            SongSelectionManager.GetInstance?.StopPreviewAudio();

            songPlaylistIndexBeforeOpen = currentSong.SearchIndex;
            _logger.Msg($"Song index before open: {songPlaylistIndexBeforeOpen}");
            _logger.Msg($"Current plist song idx {PlaylistManagementController.GetInstance.CurrentPlaylistSongIndex}");

            // Hide center panel
            var centerView = SongSelectionView.GetView();

            // Create panel if needed
            if (playlistPanel == null)
            {
                // Add our playlist menu
                var panel = ScrollablePanel.Create("playlist_panel", () => OnMenuClose(centerView), _logger);
                panel.Panel.transform.parent = centerView.SelectionSongPanel.transform;

                // Add header
                panel.AddHeader("playlists_header", "Playlists");

                playlistPanel = panel;
            }

            // Add items
            _logger.Msg($"Adding items");
            playlistPanel.ClearItems();
            foreach (var playlist in playlists)
            {
                var panelItem = new PlaylistPanelItem(playlist, currentSong, _logger);
                playlistPanel.AddItem(panelItem);
            }

            // Show
            _logger.Msg($"Showing");
            playlistPanel.SetVisibility(true);
            centerView.SetVisibility(false);
        }

        private Il2CppSynth.Retro.Game_Track_Retro GetSelectedTrack()
        {
            return Il2CppSynth.SongSelection.SongSelectionManager.GetInstance?.SelectedGameTrack;
        }

        private void OnMenuClose(SongSelectionView centerView)
        {
            // Try to open center view again
            _logger.Msg("Menu close, showing center view again");
            centerView.SetVisibility(true);

            // Now that we're visible again, refresh the playlist view's visuals if needed
            _logger.Msg("Refreshing playlist view");
            RefreshCurrentPlaylistView();

            // Select at the current index, if any
            // Different checks for fixed playlists/views
            var currentPlist = PlaylistManagementController.GetInstance?.CurrentSelectedPlaylist;
            _logger.Msg($"Current plist {currentPlist.Name}");
            _logger.Msg($"Current plist idx {PlaylistManagementController.GetInstance.CurrentPlaylistIndex}");
            _logger.Msg($"Current plist song idx {PlaylistManagementController.GetInstance.CurrentPlaylistSongIndex}");
            if (currentPlist?.FixedPlaylist ?? true)
            {
                // No easy way to check bounds, so just try and hope
                _logger.Msg("Fixed playlist, selecting song idx " + songPlaylistIndexBeforeOpen);
                if (songPlaylistIndexBeforeOpen >= 0)
                {
                    try
                    {
                        SongSelectionManager.GetInstance?.OnSongItemClicked(songPlaylistIndexBeforeOpen);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Failed to select song after menu close", ex);
                    }
                }
            }
            else
            {
                // User playlist

                // Select current song item
                var currPlaylistSongCount = currentPlist?.Songs.Count ?? 0;
                if (songPlaylistIndexBeforeOpen > currPlaylistSongCount - 1)
                {
                    songPlaylistIndexBeforeOpen = currPlaylistSongCount - 1;
                    _logger.Msg("Index too big, changed to " + songPlaylistIndexBeforeOpen);
                }

                if (songPlaylistIndexBeforeOpen < 0)
                {
                    _logger.Msg("Index out of range, not clicking song");
                }
                else
                {
                    _logger.Msg("Custom playlist, clicking song at index " + songPlaylistIndexBeforeOpen);
                    SongSelectionManager.GetInstance?.OnSongItemClicked(songPlaylistIndexBeforeOpen);
                }
            }

            // Now that the extra click has happened, make sure the center view is still visible
            _logger.Msg("Second check for center view visible");
            centerView.SetVisibility(true);

            // Resume audio
            // TODO fix
            _logger.Msg($"Selected song: {SongSelectionManager.GetInstance?.SelectedGameTrack?.name}");
            SongSelectionManager.GetInstance?.PlayPreviewAudio(true);
        }

        public static void RefreshCurrentPlaylistView()
        {
            var controller = PlaylistManagementController.GetInstance;

            _logger.Msg("Current playlist idx: " + controller.CurrentPlaylistIndex);
            _logger.Msg("Current selected playlist: " + controller.CurrentSelectedPlaylist?.Name);
            _logger.Msg("Current selection type: " + controller.CurrentSongSelectionType);
            // Refresh currently selected playlist view
            if (controller.CurrentPlaylistIndex >= 0)
            {
                if (controller.CurrentSelectedPlaylist.ShowFavorites)
                {
                    // Favorites playlist logic
                    _logger.Msg("ShowFavorites");
                    // 0 is all songs, 1 is favorites
                    controller.Interface__OnPlaylistScrollItemClick(1);
                }
                else if (controller.CurrentSelectedPlaylist.ShowAllSongs)
                {
                    _logger.Msg("ShowAllSongs");
                    controller.Interface__OnPlaylistScrollItemClick(0);
                }
                else if (controller.CurrentSelectedPlaylist.ShowAllExperiences)
                {
                    _logger.Msg("ShowAllExperiences");
                    controller.Interface__ShowExperiencesShelf();
                }
                else
                {
                    // Not the special playlists, so just click it like normal
                    var index = PlaylistPanelItem.FindMatchingPlaylistIndex(_logger, controller, controller.CurrentSelectedPlaylist);
                    _logger.Msg($"ItemClick playlist index {index}");
                    controller.Interface__OnPlaylistScrollItemClick(index);
                }
            }
            else
            {
            }

            // This turns on song preview audio. Turn that off until we fully exit
            //SongSelectionManager.GetInstance.StopPreviewAudio();
        }
    }
}
