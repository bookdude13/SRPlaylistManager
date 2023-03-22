using SRModCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util.Controller;
using Util.Data;

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

            playlists.AddRange(PlaylistManagementController.GetInstance.UserPlaylistList.playlists);
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
