using SRModCore;
using SRPlaylistManager.Models;
using SRPlaylistManager.Services;
using Synth.SongSelection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Util.Data;

namespace SRPlaylistManager.MonoBehavior
{
    class PlaylistMenuMonoBehavior
    {
        private SRLogger logger;
        private PlaylistService playlistService;
        private ScrollablePanel playlistPanel;

        public PlaylistMenuMonoBehavior(SRLogger logger, PlaylistService playlistService)
        {
            this.logger = logger;
            this.playlistService = playlistService;
        }

        public void OpenMenu()
        {
            var playlists = playlistService.GetPlaylists();

            // Get currently selected song
            var currentSong = GetSelectedTrack();
            logger.Msg($"Current song: '{currentSong.name}'");

            // Hide center panel
            var centerView = SongSelectionView.GetView();
            centerView.SetVisibility(false);

            // Create panel if needed
            if (playlistPanel== null)
            {
                // Add our playlist menu
                var panel = ScrollablePanel.Create("playlist_panel", () => centerView.SetVisibility(true));
                panel.Panel.transform.parent = centerView.SelectionSongPanel.transform;

                // Add header
                panel.AddHeader("playlists_header", "Playlists");

                playlistPanel = panel;
            }

            // Add items
            playlistPanel.ClearItems();
            foreach (var playlist in playlists)
            {
                var panelItem = new PlaylistPanelItem(playlist, currentSong, logger);
                playlistPanel.AddItem(panelItem);
            }

            // Show
            playlistPanel.SetVisibility(true);
        }

        private Synth.Retro.Game_Track_Retro GetSelectedTrack()
        {
            return Synth.SongSelection.SongSelectionManager.GetInstance?.SelectedGameTrack;
        }
    }
}
