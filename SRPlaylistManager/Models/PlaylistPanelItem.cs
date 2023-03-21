using SRModCore;
using Synth.Item;
using Synth.Retro;
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
using static ChartLoaderTest;

namespace SRPlaylistManager.Models
{
    internal class PlaylistPanelItem : PanelItem
    {
        private PlaylistItem PlaylistItem;
        private VRTK_InteractableObject_UnityEvents ItemEvents;
        private VRTKButtonHelper ItemButtonHelper;
        private TMPro.TextMeshProUGUI Text;
        private SRLogger Logger;

        private Game_Track_Retro SelectedTrack;
        private PlaylistSong SelectedSong;

        public PlaylistPanelItem(PlaylistItem playlistItem, Game_Track_Retro selectedTrack, SRLogger logger)
        {
            PlaylistItem = playlistItem;
            SelectedTrack = selectedTrack;
            Logger = logger;

            SelectedSong = CreatePlaylistSongFromTrack(selectedTrack);
        }
        
        private PlaylistSong CreatePlaylistSongFromTrack(Game_Track_Retro track)
        {
            // PlaylistManagementController
            // Interface__OnSongSelectedForPlaylist

            PlaylistSong currentSong = new PlaylistSong
            {
                name = track.TrackName,
                author = track.Author,
                beatmapper = (track.IsCustomSong ? track.Beatmapper : string.Empty),
                difficulty = (int)Game_InfoProvider.s_instance.CurrentDifficulty,
                hash = track.LeaderboardHash,
                trackDuration = (float)track.DurationOnSeconds,
                addedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            return currentSong;
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
            Text = item.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            Text.SetText(PlaylistItem.Name);

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

            UpdateAppearance();

            // Show by default
            item.SetActive(true);

            return item;
        }

        private void UpdateAppearance()
        {
            if (PlaylistItem.FixedPlaylist && PlaylistItem.ShowFavorites)
            {
                if (SelectedTrack.OnFavorites)
                {
                    // Difficulty is hard to get for favorites.
                    // Just add stars for now
                    SetAppearanceInPlaylist($"\u2606 | {PlaylistItem.Name}");
                }
                else
                {
                    SetAppearanceNotInPlaylist();
                }
            }
            else
            {
                int existingIndex = GetExistingSongIndex(SelectedSong);
                if (existingIndex >= 0)
                {
                    // Show difficulty in playlist as well
                    int difficulty = PlaylistItem.Songs[existingIndex].difficulty;
                    SetAppearanceInPlaylist($"({GetDifficultyAsName(difficulty)}) | {PlaylistItem.Name}");
                }
                else
                {
                    SetAppearanceNotInPlaylist();
                }
            }
        }

        private void SetAppearanceInPlaylist(string text)
        {
            Text.fontStyle = TMPro.FontStyles.Bold;
            Text.color = Color.white;
            Text.SetText(text);
        }

        private void SetAppearanceNotInPlaylist()
        {
            Text.fontStyle = TMPro.FontStyles.Normal;
            Text.color = Color.gray;

            Text.SetText($"{PlaylistItem.Name}");
        }

        /// <summary>
        /// Converts difficulty index to human readable name.
        /// Uses custom difficulty name if present.
        /// </summary>
        /// <returns></returns>
        private string GetDifficultyAsName(int index)
        {
            switch (index) {
                case 0:
                    return "Easy";
                case 1:
                    return "Normal";
                case 2:
                    return "Hard";
                case 3:
                    return "Expert";
                case 4:
                    return "Master";
                case 5:
                    return SelectedTrack.CustomDiffName;
                default:
                    return "UNKNOWN";
            }
        }

        /// <summary>
        /// Button callback for toggling insert/remove of song to/from playlist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Toggle(object sender, VRTK.InteractableObjectEventArgs e)
        {
            // Favorites are handled separately
            if (PlaylistItem.FixedPlaylist && PlaylistItem.ShowFavorites)
            {
                ToggleFavorite();
            }
            else
            {
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

            UpdateAppearance();
        }

        private void ToggleFavorite()
        {
            // Toggle
            if (SelectedTrack.OnFavorites)
            {
                Logger.Msg($"Removing '{SelectedTrack.TrackName}' from Favorites");
                Game_InfoProvider.RemoveFromFavoritesList(SelectedTrack.TrackName, SelectedTrack.Author, SelectedTrack.LeaderboardHash);
                SelectedTrack.OnFavorites = false;
            }
            else
            {
                Logger.Msg($"Adding '{SelectedTrack.TrackName}' to Favorites");
                Game_InfoProvider.AddToFavoritesList(SelectedTrack.TrackName, SelectedTrack.Author, SelectedTrack.LeaderboardHash);
                SelectedTrack.OnFavorites = true;
            }

            // Make sure the singleton is updated
            SongSelectionManager.GetInstance.SelectedGameTrack = SelectedTrack;
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
