using Il2Cpp;
using Il2Cppcom.Kluge.XR.Utils;
using Il2CppSynth.Multiplayer;
using Il2CppSynth.SongSelection;
using Il2CppUtil.Controller;
using MelonLoader;
using SRModCore;
using SRPlaylistManager.Models;
using SRPlaylistManager.MonoBehavior;
using SRPlaylistManager.Services;
using System.Collections;
using UnityEngine;
using static MelonLoader.MelonLogger;

namespace SRPlaylistManager
{
    public class SRPlaylistManager : MelonMod
    {
        // If set, adds a ton more logs to debug issues
        public static bool VERBOSE_LOGS = true;

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
                LogVerbose("Playlist GO not found in main menu; creating...");
                mainMenuPlaylistGO = new GameObject(gameObjectName);
                mainMenuPlaylistGO.transform.SetParent(zWrap.transform, false);
                mainMenuMonoBehavior = mainMenuPlaylistGO.AddComponent<MainMenuPlaylistMenuMonoBehavior>();
                mainMenuMonoBehavior.Init(logger, playlistService);
            }
            else
            {
                LogVerbose("Playlist main menu GO found");
            }
        }

        private void EnsureMultiplayerSetup(GameObject zWrap)
        {
            var gameObjectName = "srplaylistmanager_multiplayer";
            var multiplayerPlaylistGO = zWrap.transform.Find(gameObjectName)?.gameObject;
            if (multiplayerPlaylistGO == null)
            {
                LogVerbose("Playlist GO not found for multiplayer; creating...");
                multiplayerPlaylistGO = new GameObject(gameObjectName);
                multiplayerPlaylistGO.transform.SetParent(zWrap.transform, false);
                multiplayerMonoBehavior = multiplayerPlaylistGO.AddComponent<MultiplayerPlaylistMenuMonoBehavior>();
                multiplayerMonoBehavior.Init(logger, playlistService);
            }
            else
            {
                LogVerbose("Playlist multiplayer GO found");

                // Refresh UI, mostly to make sure the favorites/playlist button is active
                OnMultiplayerTrackFill();
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


                // Need to force update so the text is correct if initially starting on one of the playlists.
                //HookTextRefreshForInitialLoad("Main Stage Prefab/Z-Wrap/Home/wrap/Home - Play solo [Button]");
                //HookTextRefreshForInitialLoad("Main Stage Prefab/Z-Wrap/Home/wrap/Home - Party mode [Button]");
            }
            /*else
            {
                logger.Msg("Moving to different scene, destrying playlist monobehavior");
                menuMonoBehavior = null;
            }*/
        }

        public void OnToggleMainMenuPlaylistButton()
        {
            LogVerbose("Toggled playlist button for main menu");

            if (mainMenuMonoBehavior == null)
            {
                logger.Error("Toggled playlist with no monobehavior defined!");
                return;
            }


            mainMenuMonoBehavior?.OpenMenu();
        }

        public void OnToggleMultiplayerPlaylistButton()
        {
            LogVerbose("Toggled playlist button for multiplayer");

            if (multiplayerMonoBehavior == null)
            {
                logger.Error("Toggled playlist with no monobehavior defined!");
                return;
            }

            multiplayerMonoBehavior?.OpenMenu();
        }

        /// <summary>
        /// Checks if the currently selected song is in favorites and/or any other playlist.
        /// Returns false for both if the current song can't be determined (or there is none selected)
        /// </summary>
        /// <param name="isInFavorites"></param>
        /// <param name="isInPlaylist"></param>
        public void GetCurrentSongPlaylistState(out bool isInFavorites,  out bool isInPlaylist)
        {
            isInFavorites = false;
            isInPlaylist = false;

            if (SongSelectionManager.GetInstance == null)
                return;

            var selectedTrack = SongSelectionManager.GetInstance.SelectedGameTrack;
            if (selectedTrack == null)
                return;

            // Easy enough :)
            // Note - playlist.Songs is null for the Favorites playlist, so this being here helps simplify logic a bit
            isInFavorites = selectedTrack.OnFavorites;

            var playlists = playlistService.GetPlaylists();
            foreach (var playlist in playlists)
            {
                if (playlist == null || playlist.Songs == null)
                    continue;

                foreach (var song in playlist.Songs)
                {
                    if (song == null)
                        continue;

                    if (PlaylistPanelItem.SongMatches(selectedTrack.LeaderboardHash, selectedTrack.TrackName, selectedTrack.Author, song))
                    {
                        // If found in any playlist, we have all the info we need and can exit early
                        isInPlaylist = true;
                        return;
                    }
                }
            }
        }

        public void OnMultiplayerTrackFill()
        {
            MelonCoroutines.Start(RefreshMultiplayerDelayed());
        }

        public void OnOpenMultiplayerRoomMenu(Il2CppSynth.Versus.Room room)
        {
            var zWrap = GameObject.Find("Main Stage Prefab/Z-Wrap");
            EnsureMultiplayerSetup(zWrap);

            // Refresh MP state when returning to menu, since this is also needed when returning from a song
            // if we are the client after the host returns (since we miss the host's song update then)
            OnMultiplayerTrackFill();
        }

        private IEnumerator RefreshMultiplayerDelayed()
        {
            // Wait for track to be set
            while (SongSelectionManager.GetInstance == null || SongSelectionManager.GetInstance.SelectedGameTrack == null)
            {
                yield return null;
            }

            PlaylistManagementController.GetInstance?.TryDisplayCorrectRemoveFavoriteButton();

            while (SongSelectionManager.GetInstance == null || SongSelectionManager.GetInstance.favoriteBtn == null || SongSelectionManager.GetInstance.favoriteBtn.synthUIButton == null)
            {
                // I'm not sure why, but when returning from a multiplayer run the synthUIButton gets nulled out...but it's on the same GO so I just set it again here so things don't break and properly refresh.
                if (SongSelectionManager.GetInstance?.favoriteBtn != null && SongSelectionManager.GetInstance.favoriteBtn.synthUIButton == null)
                {
                    SongSelectionManager.GetInstance.favoriteBtn.synthUIButton = SongSelectionManager.GetInstance.favoriteBtn.GetComponent<SynthUIButton>();
                }

                yield return null;
            }

            SongSelectionManager.GetInstance?.UpdateFavoriteButtonState();

            // Make sure the favorite button is enabled!
            // When returning from a multiplayer run _after_ the host, this is turned off :/
            // Fix it here. In the future this may be unnecessary
            var mpPanel = multiplayerMonoBehavior.GetMultiplayerRoomPanel();
            var bottomPanel = mpPanel.transform.Find("MainPanel/BottomPanel");
            var favWrap = bottomPanel.transform.Find("Song Info Wrap/Favorite Wrap");
            logger.Msg("Ensuring favorite button is shown!");
            favWrap.SetActive(true);
        }

        public void Log(string message)
        {
            logger.Msg(message);
        }

        public void LogVerbose(string message)
        {
            if (VERBOSE_LOGS)
            {
                Log(message);
            }
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
            // Rearrange so the Change Cover button is in the more natural corner.
            // Delete mostly stays where it is
            btnChangeCover.localPosition += new Vector3(0f, -2.4f, 0f);
            btnDelete.localPosition += new Vector3(0f, 0f, 0f);
        }

        private void HookTextRefreshForInitialLoad(string buttonPath)
        {
            // On the initial game load the selected song in the playlist doesn't go through the normal flow,
            // so updates to the "remove favorites" button don't happen until a song is selected.
            // This adds additional checks when navigating from the main menu to avoid this case
            var button = GameObject.Find(buttonPath);
            if (button == null)
            {
                logger.Error($"Button not found at path '{buttonPath}'!");
                return;
            }

            var synthButton = button.GetComponentInChildren<SynthUIButton>();
            if (synthButton == null)
            {
                logger.Error($"Synth button not found under button at path '{buttonPath}'");
                return;
            }

            logger.Msg("Hooking into button");
            synthButton.add_OnButtonSelected(new System.Action(() => {
                Log("OnButtonSelected called");
                PlaylistManagementController.GetInstance?.TryDisplayCorrectRemoveFavoriteButton();
                SongSelectionManager.GetInstance?.UpdateFavoriteButtonState();
            }));

            synthButton.add_OnButtonSelectedAndPass(new System.Action<SynthUIButton>((passedButton) => {
                Log("OnButtonSelected called with passed button " + passedButton);
                PlaylistManagementController.GetInstance?.TryDisplayCorrectRemoveFavoriteButton();
                SongSelectionManager.GetInstance?.UpdateFavoriteButtonState();
            }));
        }
    }
}
