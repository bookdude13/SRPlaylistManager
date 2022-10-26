using SRModCore;
using SRPlaylistManager.Services;
using Synth.SongSelection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace SRPlaylistManager.UI
{
    public class PlaylistMenu
    {
        private readonly SRLogger logger;
        private readonly PlaylistService playlistService;

        private GameObject menuGO = null;

        public bool IsOpen { get; private set; }

        public PlaylistMenu(SRLogger logger, PlaylistService playlistService)
        {
            this.logger = logger;
            this.playlistService = playlistService;
        }

        public void Open()
        {
            if (IsOpen)
            {
                logger.Msg("Already open, skipping");
                return;
            }
            IsOpen = true;

            string currentSongHash = SongSelectionManager.GetInstance?.SelectedGameTrack?.LeaderboardHash;
            if (currentSongHash == null)
            {
                logger.Error("Selected song hash is null; cannot open up playlist menu");
                IsOpen = false;
                return;
            }

            var rootGO = GameObject.Find("PlayListManagement/VisibleWrap/Main Background Image (1)");
            var menuItems = GetMenuItems(currentSongHash);
            menuGO = Create(rootGO, menuItems);
            
            // PlaylistScrollItem
        }

        public void Close()
        {
            IsOpen = false;
        }

        private GameObject Create(GameObject toClone, List<PlaylistDisplayItem> menuItems)
        {
            logger.Msg(toClone == null ? "Null toClone!" : "Creating menu");

            var menu = GameObject.Instantiate(toClone, toClone.transform.parent);
            menu.name = "srplaylistmanager_menu";
            menu.SetActive(true);
            menu.transform.parent.gameObject.SetActive(true);

            var transform = menu.GetComponent<Transform>();
            transform.parent.gameObject.SetActive(true);
            transform.localPosition = new Vector3(-1f, 0.0f, -1f);

            Image bgImg = menu.GetComponent<Image>();
            bgImg.color = Color.red;

            return menu;
        }

        private List<PlaylistDisplayItem> GetMenuItems(string currentSongHash)
        {
            var playlists = playlistService.GetPlaylists();
            var displayItems = playlists.Select(plist => new PlaylistDisplayItem(plist, currentSongHash)).ToList();
            return displayItems;
        }
    }
}
