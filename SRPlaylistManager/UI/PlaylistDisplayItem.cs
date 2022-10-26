using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util.Data;

namespace SRPlaylistManager.UI
{
    internal class PlaylistDisplayItem
    {
        public PlaylistItem Playlist { get; private set; }
        public string SelectedHash { get; private set; }

        public PlaylistDisplayItem(PlaylistItem playlist, string selectedHash)
        {
            Playlist = playlist;
            SelectedHash = selectedHash;
        }
    }
}
