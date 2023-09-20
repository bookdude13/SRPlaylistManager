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

        private ScrollablePanel CreatePlaylistPanel(GameObject toHide)
        {
            // Add our playlist menu
            var panel = ScrollablePanel.Create("playlist_panel", () => OnMenuClose(toHide), _logger);

            // Use hidden view's parent as our own, since we basically replace it
            panel.Panel.transform.parent = toHide.transform.parent;

            // Add header
            panel.AddHeader("playlists_header", "Playlists");

            return panel;
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
            _logger.Msg($"Current song: '{currentSong.SongDataName}'");

            // Stop any songs that are playing
            SongSelectionManager.GetInstance?.StopPreviewAudio();

            songPlaylistIndexBeforeOpen = currentSong.SearchIndex;
            _logger.Msg($"Song index before open: {songPlaylistIndexBeforeOpen}");
            _logger.Msg($"Current plist song idx {PlaylistManagementController.GetInstance.CurrentPlaylistSongIndex}");

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
            }

            // Add items
            _logger.Msg($"Adding items");
            playlistPanel.ClearItems();
            foreach (var playlist in playlists)
            {
                var panelItem = new PlaylistPanelItem(playlist, currentSong, _logger);
                playlistPanel.AddItem(panelItem);
            }

            // Show menu, hide center view
            _logger.Msg($"Showing");
            playlistPanel.SetVisibility(true);
            viewToHide.gameObject.SetActive(false);
        }

        private Il2CppSynth.Retro.Game_Track_Retro GetSelectedTrack()
        {
            return Il2CppSynth.SongSelection.SongSelectionManager.GetInstance?.SelectedGameTrack;
        }

        public void RefreshCurrentPlaylistView()
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
