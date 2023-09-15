using SRModCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppUtil.Controller;
using Il2CppUtil.Data;

namespace SRPlaylistManager.Services
{
    class PlaylistService
    {
        private SRLogger logger;

        public PlaylistService(SRLogger logger)
        {
            this.logger = logger;
        }

        public List<PlaylistItem> GetPlaylists(bool includeAllSongs=false)
        {
            var playlists = new List<PlaylistItem>();

            var playlistItems = PlaylistManagementController.GetInstance.UserPlaylistList.playlists;
            foreach (var playlistItem in playlistItems)
            {
                playlists.Add(playlistItem);
            }
            logger.Msg($"{playlists.Count} playlists found");

            if (!includeAllSongs)
            {
                logger.Msg("Filtering out 'All Songs' playlist");
                playlists = playlists.Where(plist => !(plist.FixedPlaylist && plist.Name == "All Songs")).ToList();
            }

            return playlists;
        }
    }
}
