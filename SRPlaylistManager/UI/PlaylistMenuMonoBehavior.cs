using SRModCore;
using SRPlaylistManager.Services;
using Synth.SongSelection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SRPlaylistManager.UI
{
    class PlaylistMenuMonoBehavior
    {
        private SRLogger logger;
        private PlaylistService playlistService;
        private PlaylistMenu playlistMenu;

        public PlaylistMenuMonoBehavior(SRLogger logger, PlaylistService playlistService)
        {
            this.logger = logger;
            this.playlistService = playlistService;
            this.playlistMenu = new PlaylistMenu(logger, playlistService);
        }

        public void Toggle()
        {
            if (playlistMenu.IsOpen)
            {
                playlistMenu.Close();
            }
            else
            {
                playlistMenu.Open();
            }
        }
    }
}
