using MelonLoader;
using SRModCore;
using SRPlaylistManager.MonoBehavior;
using SRPlaylistManager.Services;
using Synth.SongSelection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MelonLoader.MelonLogger;

namespace SRPlaylistManager
{
    public class SRPlaylistManager : MelonMod
    {
        public static SRPlaylistManager Instance { get; private set; }

        private SRLogger logger;
        private PlaylistService playlistService;
        private PlaylistMenuMonoBehavior menuMonoBehavior = null;

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();

            logger = new MelonLoggerWrapper(LoggerInstance);
            playlistService = new PlaylistService(logger);
            Instance = this;
        }

        public void OnTogglePlaylistButton()
        {
            logger.Msg("Toggled playlist button");

            if (menuMonoBehavior == null)
            {
                menuMonoBehavior = new PlaylistMenuMonoBehavior(logger, playlistService);
            }

            menuMonoBehavior?.OpenMenu();
        }
    }
}
