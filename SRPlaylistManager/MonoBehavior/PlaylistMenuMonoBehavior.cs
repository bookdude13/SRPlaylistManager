using SRModCore;
using SRPlaylistManager.Models;
using SRPlaylistManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SRPlaylistManager.MonoBehavior
{
    class PlaylistMenuMonoBehavior
    {
        private SRLogger logger;
        private PlaylistService playlistService;

        public PlaylistMenuMonoBehavior(SRLogger logger, PlaylistService playlistService)
        {
            this.logger = logger;
            this.playlistService = playlistService;
        }

        public void OpenMenu()
        {
            var playlists = playlistService.GetPlaylists();

            // Hide center panel
            var centerView = SongSelectionView.GetView();
            centerView.SetVisibility(false);

            // Add our playlist menu
            var panel = ScrollablePanel.Create("playlist_panel");
            panel.Panel.transform.parent = centerView.SelectionSongPanel.transform;
            panel.SetVisibility(true);

            // Add header
            panel.AddHeader("playlists_header", "Playlists");

            // Add items
            foreach (var playlist in playlists)
            {
                panel.AddItem("playlist_item_" + playlist.Name, playlist.Name);
            }
        }
    }
}
