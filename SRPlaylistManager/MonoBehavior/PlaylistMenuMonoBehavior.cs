using SRModCore;
using SRPlaylistManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            // PlaylistScrollItem
        }
    }
}
