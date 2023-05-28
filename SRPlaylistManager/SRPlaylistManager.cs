using MelonLoader;
using SRModCore;
using SRPlaylistManager.MonoBehavior;
using SRPlaylistManager.Services;
using UnityEngine;

namespace SRPlaylistManager
{
    public class SRPlaylistManager : MelonMod
    {
        public static SRPlaylistManager Instance { get; private set; }

        private SRLogger logger;
        private PlaylistService playlistService;
        private PlaylistMenuMonoBehavior menuMonoBehavior = null;

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();

            logger = new MelonLoggerWrapper(LoggerInstance);
            playlistService = new PlaylistService(logger);
            Instance = this;
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            base.OnSceneWasInitialized(buildIndex, sceneName);

            SRScene scene = new SRScene(sceneName);

            if (scene.SceneType == SRScene.SRSceneType.MAIN_MENU)
            {
                var gameObjectName = "srplaylistmanager_menu";
                var playlistGO = GameObject.Find(gameObjectName);
                if (playlistGO == null)
                {
                    logger.Msg("Playlist GO not found; creating...");
                    playlistGO = new GameObject("srplaylistmanager_menu");
                    menuMonoBehavior = playlistGO.AddComponent<PlaylistMenuMonoBehavior>();
                    menuMonoBehavior.Init(logger, playlistService);
                }
                else
                {
                    logger.Msg("Playlist GO found");
                }
            }
            /*else
            {
                logger.Msg("Moving to different scene, destrying playlist monobehavior");
                menuMonoBehavior = null;
            }*/
        }

        public void OnTogglePlaylistButton()
        {
            logger.Msg("Toggled playlist button");

            if (menuMonoBehavior == null)
            {
                logger.Error("Toggled playlist with no monobehavior defined!");
                return;
            }

            menuMonoBehavior?.OpenMenu();
        }
    }
}
