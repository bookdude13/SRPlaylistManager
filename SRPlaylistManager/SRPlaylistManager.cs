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
        private MainMenuPlaylistMenuMonoBehavior mainMenuMonoBehavior = null;
        private MultiplayerPlaylistMenuMonoBehavior multiplayerMonoBehavior = null;

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();

            logger = new MelonLoggerWrapper(LoggerInstance);
            playlistService = new PlaylistService(logger);
            Instance = this;
        }
        
        private void EnsureMainMenuSetup(GameObject zWrap)
        {
            var gameObjectName = "srplaylistmanager_mainmenu";
            var mainMenuPlaylistGO = zWrap.transform.Find(gameObjectName)?.gameObject;
            if (mainMenuPlaylistGO == null)
            {
                logger.Msg("Playlist GO not found in main menu; creating...");
                mainMenuPlaylistGO = new GameObject(gameObjectName);
                mainMenuPlaylistGO.transform.SetParent(zWrap.transform, false);
                mainMenuMonoBehavior = mainMenuPlaylistGO.AddComponent<MainMenuPlaylistMenuMonoBehavior>();
                mainMenuMonoBehavior.Init(logger, playlistService);
            }
            else
            {
                logger.Msg("Playlist main menu GO found");
            }
        }

        private void EnsureMultiplayerSetup(GameObject zWrap)
        {
            var gameObjectName = "srplaylistmanager_multiplayer";
            var multiplayerPlaylistGO = zWrap.transform.Find(gameObjectName)?.gameObject;
            if (multiplayerPlaylistGO == null)
            {
                logger.Msg("Playlist GO not found for multiplayer; creating...");
                multiplayerPlaylistGO = new GameObject(gameObjectName);
                multiplayerPlaylistGO.transform.SetParent(zWrap.transform, false);
                multiplayerMonoBehavior = multiplayerPlaylistGO.AddComponent<MultiplayerPlaylistMenuMonoBehavior>();
                multiplayerMonoBehavior.Init(logger, playlistService);
            }
            else
            {
                logger.Msg("Playlist multiplayer GO found");
            }
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            base.OnSceneWasInitialized(buildIndex, sceneName);

            SRScene scene = new SRScene(sceneName);

            if (scene.SceneType == SRScene.SRSceneType.MAIN_MENU)
            {
                var zWrap = GameObject.Find("Main Stage Prefab/Z-Wrap");
                EnsureMainMenuSetup(zWrap);
                EnsureMultiplayerSetup(zWrap);
                DisableAddSongsButtonFromPlaylistView(zWrap);
            }
            /*else
            {
                logger.Msg("Moving to different scene, destrying playlist monobehavior");
                menuMonoBehavior = null;
            }*/
        }

        public void OnToggleMainMenuPlaylistButton()
        {
            logger.Msg("Toggled playlist button for main menu");

            if (mainMenuMonoBehavior == null)
            {
                logger.Error("Toggled playlist with no monobehavior defined!");
                return;
            }

            mainMenuMonoBehavior?.OpenMenu();
        }

        public void OnToggleMultiplayerPlaylistButton()
        {
            logger.Msg("Toggled playlist button for multiplayer");

            if (multiplayerMonoBehavior == null)
            {
                logger.Error("Toggled playlist with no monobehavior defined!");
                return;
            }

            multiplayerMonoBehavior?.OpenMenu();
        }

        public void Log(string message)
        {
            logger.Msg(message);
        }

        private void DisableAddSongsButtonFromPlaylistView(GameObject zWrap)
        {
            var playlistManagemet = zWrap.transform.Find("SongSelection/SelectionSongPanel/PlayListManagement");
            var plistButtons = playlistManagemet.Find("VisibleWrap/Middle/Playlist Edition Control/Action Buttons");
            var btnAddSongs = plistButtons.Find("AddSongs");
            var btnChangeCover = plistButtons.Find("ChangeCover");
            var btnDelete = plistButtons.Find("Delete Playlist");

            // Disable the "Add Songs" button when a playlist is selected,
            // since it makes the logic more complex and just opens the main view
            // which can add songs to any playlist anyways.
            btnAddSongs.gameObject.SetActive(false);

            // Offset the other buttons for a more natural look.
            // Originally AddSongs -3.95, ChangeCover 0, Delete Playlist 3.95
            btnChangeCover.localPosition = new Vector3(-3.2f, 0, 0);
            btnDelete.localPosition = new Vector3(3.2f, 0, 0);
        }
    }
}
