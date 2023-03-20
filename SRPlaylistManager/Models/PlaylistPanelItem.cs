using SRModCore;
using Synth.Item;
using Synth.SongSelection;
using Synth.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Util.Controller;
using Util.Data;
using VRTK.UnityEventHelper;

namespace SRPlaylistManager.Models
{
    internal class PlaylistPanelItem : PanelItem
    {
        /*public string Name { get; private set; }
        public string Text { get; private set; }
        public Color BGColor { get; private set; }*/

        private PlaylistItem PlaylistItem;
        private VRTK_InteractableObject_UnityEvents ItemEvents;
        private VRTKButtonHelper ItemButtonHelper;
        private PlaylistSong SelectedSong;
        private SRLogger Logger;

        public PlaylistPanelItem(PlaylistItem playlistItem, PlaylistSong selectedSong, SRLogger logger)
        {
            PlaylistItem = playlistItem;
            SelectedSong = selectedSong;
            Logger = logger;
        }

        public GameObject Setup(GameObject item)
        {
            // Logger.Msg("Setting up playlist item " + PlaylistItem.Name);
            item.name = "playlist_item_" + PlaylistItem.Name;

            // Stop text from being changed from localization running
            item.GetComponentInChildren<Synth.Utils.LocalizationHelper>().enabled = false;

            // Remove unused area?
            item.transform.Find("Value Area").gameObject.SetActive(false);

            // Update text
            item.GetComponentInChildren<TMPro.TextMeshProUGUI>().SetText(PlaylistItem.Name);

            // Set up as button toggle
            try
            {
                ItemEvents = item.GetComponent<VRTK_InteractableObject_UnityEvents>();
                ItemButtonHelper = item.AddComponent<VRTKButtonHelper>();

                ItemEvents.enabled = true;

                ItemEvents.OnUse.RemoveAllListeners();
                var num = ItemEvents.OnUse.GetPersistentEventCount();
                for (var i = 0; i < num; i++)
                {
                    var nm = ItemEvents.OnUse.GetPersistentMethodName(i);
                    ItemEvents.OnUse.SetPersistentListenerState(i, UnityEventCallState.Off);
                }

                ItemEvents.OnUse.AddListener(Toggle);
                ItemButtonHelper.SetActive();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            // TODO update appearance if in list, and refresh after toggle

            // Show by default
            item.SetActive(true);

            return item;
        }

        private void Toggle(object sender, VRTK.InteractableObjectEventArgs e)
        {
            // Favorites are handled separately
            if (PlaylistItem.FixedPlaylist && PlaylistItem.ShowFavorites)
            {
                ToggleFavorite();
            }

            // See if the currently selected song is in us or not
            int existingIndex = GetExistingSongIndex(SelectedSong);
            if (existingIndex < 0)
            {
                AddToPlaylist(SelectedSong, existingIndex);
            }
            else
            {
                RemoveFromPlaylist(SelectedSong, existingIndex);
            }
        }

        private void ToggleFavorite()
        {
            var songSelectMgr = SongSelectionManager.GetInstance;

            // This should be the same as SelectedSong, just a different format
            var selectedTrack = songSelectMgr.SelectedGameTrack;

            // Toggle
            if (selectedTrack.OnFavorites)
            {
                Logger.Msg($"Removing '{selectedTrack.TrackName}' from Favorites");
                Game_InfoProvider.RemoveFromFavoritesList(selectedTrack.TrackName, selectedTrack.Author, selectedTrack.LeaderboardHash);
                selectedTrack.OnFavorites = false;
            }
            else
            {
                Logger.Msg($"Adding '{selectedTrack.TrackName}' to Favorites");
                Game_InfoProvider.AddToFavoritesList(selectedTrack.TrackName, selectedTrack.Author, selectedTrack.LeaderboardHash);
                selectedTrack.OnFavorites = true;
            }
            
            // Make sure the singleton is updated
            songSelectMgr.SelectedGameTrack = selectedTrack;
        }

        /// <summary>
        /// Removes the song from the given index in the current playlist.
        /// TODO remove all instances of the hash match?
        /// </summary>
        /// <param name="songToRemove"></param>
        /// <param name="existingIndex"></param>
        private void RemoveFromPlaylist(PlaylistSong songToRemove, int existingIndex)
        {
            List<PlaylistSong> songs = PlaylistItem.Songs;

            if (existingIndex < 0)
            {
                Logger.Error($"Song '{songToRemove.name}' not found in playlist '{PlaylistItem.Name}'; cannot remove");
                return;
            }

            Logger.Msg($"Found at difficulty '{songs[existingIndex].difficulty}'. Removing from playlist '{PlaylistItem.Name}'"); 
            songs.RemoveAt(existingIndex);
            PlaylistItem.Songs = songs;

            UpdateController();
        }

        private void UpdateController()
        {
            // Update controller
            var controller = PlaylistManagementController.GetInstance;
            var playlistIdx = controller.UserPlaylistList.playlists.FindIndex(p => p.Name == PlaylistItem.Name && p.CreationDate == PlaylistItem.CreationDate);
            if (playlistIdx < 0)
            {
                Logger.Error($"Playlist '{PlaylistItem.Name}' not found in controller");
                return;
            }
            controller.UserPlaylistList.playlists[playlistIdx] = PlaylistItem;
        }

        /// <summary>
        /// Gets the index of the given song in this playlist.
        /// Tries to match hash, falls back on name+author if no hash set
        /// Returns -1 if not found
        /// </summary>
        /// <param name="song">Song to look for</param>
        /// <returns></returns>
        private int GetExistingSongIndex(PlaylistSong song)
        {
            return PlaylistItem.Songs.FindIndex(delegate (PlaylistSong x)
            {
                // Based on hash primarily
                if (!string.IsNullOrEmpty(x.hash))
                {
                    return x.hash.Equals(song.hash);
                }

                // Fall back on name and author match
                return x.name.ToLower().Equals(song.name.ToLower()) && x.author.ToLower().Equals(song.author.ToLower());
            });
        }

        private void AddToPlaylist(PlaylistSong songToAdd, int existingIndex)
        {
            List<PlaylistSong> songs = PlaylistItem.Songs;

            // Add or replace
            if (existingIndex >= 0)
            {
                Logger.Msg($"Replacing song '{songToAdd.name}' to playlist '{PlaylistItem.Name}' at index {existingIndex}");
                songs[existingIndex] = songToAdd;
            }
            else
            {
                Logger.Msg($"Adding song '{songToAdd.name}' to end of playlist '{PlaylistItem.Name}'");
                songs.Add(songToAdd);
            }

            // Update list
            PlaylistItem.Songs = songs;

            UpdateController();
        }
    }
}
