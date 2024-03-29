﻿using SRModCore;
using SRPlaylistManager.MonoBehavior;
using Synth.Data;
using Synth.Item;
using Synth.Lod;
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
                    AddToPlaylist(SelectedSong);
                }
                else
                {
                    RemoveFromPlaylist(SelectedSong);
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
        /// </summary>
        /// <param name="songToRemove"></param>
        private void RemoveFromPlaylist(PlaylistSong songToRemove)
        {
            List<PlaylistSong> songs = GetSongsNotMatching(songToRemove);
            var numRemoved = PlaylistItem.Songs.Count - songs.Count;
            Logger.Msg($"Removing {numRemoved} instances of '{songToRemove.name}' from playlist '{PlaylistItem.Name}'");

            // Update our reference to the song list
            PlaylistItem.Songs = songs;

            // New playlist item created; to make equality check fail later??
            UpdateController(CloneItemWithNewSongs(songs));

            //PlaylistMenuMonoBehavior.OnMenuClosed += () => RefreshSelectedSong(existingIndex - 1);
        }

        private void UpdateController(PlaylistItem newItem)
        {
            var controller = PlaylistManagementController.GetInstance;
            if (controller == null)
            {
                Logger.Error("Null playlist controller, not updating");
                return;
            }

            // Update controller item
            // Creation date used as unique id
            var playlistIdx = controller.UserPlaylistList.playlists.FindIndex(p => p.CreationDate == newItem.CreationDate);
            if (playlistIdx < 0)
            {
                Logger.Error($"Playlist '{newItem.Name}' not found in controller");
                return;
            }
            controller.UserPlaylistList.playlists[playlistIdx] = newItem;
        }

        private PlaylistItem CloneItemWithNewSongs(List<PlaylistSong> songs)
        {
            return new PlaylistItem
            {
                Songs = songs,
                Name = PlaylistItem.Name,
                Description = PlaylistItem.Description,
                CurrentIndex = PlaylistItem.CurrentIndex,
                SelectedIconIndex = PlaylistItem.SelectedIconIndex,
                SelectedTexture = PlaylistItem.SelectedTexture,
                GradientColor1 = PlaylistItem.GradientColor1,
                GradientColor2 = PlaylistItem.GradientColor2,
                TextColor = PlaylistItem.TextColor,
                TextureColor = PlaylistItem.TextureColor,
                CreationDate = PlaylistItem.CreationDate
            };
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
                return SongsMatchNameAuthor(x, song);
            });
        }

        private bool SongsMatchNameAuthor(PlaylistSong a, PlaylistSong b)
        {
            return a.name.ToLower().Equals(b.name.ToLower()) && a.author.ToLower().Equals(b.author.ToLower());
        }

        private List<PlaylistSong> GetSongsNotMatching(PlaylistSong original)
        {
            // Remove duplicates that have different hashes but matching name+author
            // This catches drafts with changing hashes / out of date
            return PlaylistItem.Songs.Where(song => !SongsMatchNameAuthor(song, original)).ToList();
        }

        private void AddToPlaylist(PlaylistSong songToAdd)
        {
            List<PlaylistSong> songs = GetSongsNotMatching(songToAdd);
            var numDuplicates = PlaylistItem.Songs.Count - songs.Count;

            Logger.Msg($"Adding song '{songToAdd.name}' to end of playlist '{PlaylistItem.Name}'. {numDuplicates} duplicates removed.");
            songs.Add(songToAdd);

            // Update our reference to the song list
            PlaylistItem.Songs = songs;

            // New playlist item created; to make equality check fail later??
            UpdateController(CloneItemWithNewSongs(songs));
        }
    }
}
