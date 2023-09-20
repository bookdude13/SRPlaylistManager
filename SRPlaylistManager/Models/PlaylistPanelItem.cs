﻿using SRModCore;
using Il2Cpp;
using Il2CppSynth.Retro;
using Il2CppSynth.SongSelection;
using Il2CppSynth.Utils;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Il2CppUtil.Controller;
using Il2CppUtil.Data;
using System.Collections.Generic;

namespace SRPlaylistManager.Models
{
    public class PlaylistPanelItem : PanelItem
    {
        private PlaylistItem PlaylistItem;
        private Il2Cpp.SynthUIButton ItemButton;
        private VRTKButtonHelper ItemButtonHelper;
        private Il2CppTMPro.TextMeshProUGUI Text;
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
                trackDuration = (float)track.DurationInSeconds,
                addedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            return currentSong;
        }

        public GameObject Setup(GameObject item)
        {
            // Logger.Msg("Setting up playlist item " + PlaylistItem.Name);
            item.name = "playlist_item_" + PlaylistItem.Name;

            // Stop text from being changed from localization running
            item.GetComponentInChildren<Il2CppSynth.Utils.LocalizationHelper>().enabled = false;

            // Remove unused area if found
            item.transform.Find("Background/Value Area").gameObject.SetActive(false);

            // Update text
            Text = item.GetComponentInChildren<Il2CppTMPro.TextMeshProUGUI>();
            Text.SetText(PlaylistItem.Name);

            // Set up as button toggle
            try
            {
                //ItemEvents = item.GetComponent<VRTK_InteractableObject_UnityEvents>();
                ItemButton = item.GetComponent<SynthUIButton>();
                ItemButtonHelper = item.AddComponent<VRTKButtonHelper>();

                ItemButton.enabled = true;

                // Directly removing persistent listeners doesn't work
                // See https://forum.unity.com/threads/documentation-unityevent-removealllisteners-only-removes-non-persistent-listeners.341796/
                // But...it looks like the persistent listener just calls the synth button.
                // Wiping out the WhenClicked callback gets rid of old behavior
                // and lets us add our own callbacks without any hassle
                ItemButton.WhenClicked = new UnityEvent();
                ItemButton.WhenClicked.AddListener(new Action(() => Toggle()));
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
            Text.fontStyle = Il2CppTMPro.FontStyles.Bold;
            Text.color = Color.white;
            Text.SetText(text);
        }

        private void SetAppearanceNotInPlaylist()
        {
            Text.fontStyle = Il2CppTMPro.FontStyles.Normal;
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
        private void Toggle()
        {
            // Favorites are handled separately
            if (PlaylistItem.FixedPlaylist && PlaylistItem.ShowFavorites)
            {
                ToggleFavorite();
            }
            else
            {
                // See if the currently selected song is in us or not
                int existingSongIndex = GetExistingSongIndex(SelectedSong);
                if (existingSongIndex < 0)
                {
                    AddToPlaylist(SelectedSong);
                }
                else
                {
                    RemoveFromPlaylist(SelectedSong, existingSongIndex);
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
        private void RemoveFromPlaylist(PlaylistSong songToRemove, int currentSongIdx)
        {
            var songs = GetSongsNotMatching(songToRemove);
            var numRemoved = PlaylistItem.Songs.Count - songs.Count;
            Logger.Msg($"Removing {numRemoved} instances of '{songToRemove.name}' from playlist '{PlaylistItem.Name}'");

            // Update our reference to the song list
            PlaylistItem.Songs = songs;

            // New playlist item created; to make equality check fail later??
            var newSongIdx = Math.Min(songs.Count - 1, currentSongIdx);
            UpdateController(PlaylistItem, newSongIdx);

            //PlaylistMenuMonoBehavior.OnMenuClosed += () => RefreshSelectedSong(existingIndex - 1);
        }

        private void UpdateController(PlaylistItem newItem, int newSongIdx)
        {
            var controller = PlaylistManagementController.GetInstance;
            if (controller == null)
            {
                Logger.Error("Null playlist controller, not updating");
                return;
            }

            // Update controller item
            // Creation date used as unique id
            int playlistIdx = FindMatchingPlaylistIndex(Logger, controller, newItem);
            if (playlistIdx < 0)
            {
                Logger.Error($"Playlist '{newItem.Name}' not found in controller");
                return;
            }
            Logger.Msg($"Updating playlist at index {playlistIdx}, name {controller.UserPlaylistList.playlists[playlistIdx].Name}");
            controller.UserPlaylistList.playlists[playlistIdx] = newItem;

            var oldPlaylistIdx = controller.CurrentPlaylistIndex;
            var oldCasspi = controller.CurrentAddingSongsSelectedPlaylistIndex;
            var oldSongIdx = controller.CurrentPlaylistSongIndex;
            var oldPlaylist = controller.CurrentSelectedPlaylist;
            var oldSongIdx2 = oldPlaylist.CurrentIndex;
            Logger.Msg($"Old indices. P: {oldPlaylistIdx}, S: {oldSongIdx} S2: {oldSongIdx2}");
            Logger.Msg($"New indices. P: {playlistIdx}, S: {newSongIdx}");
            Logger.Msg($"Old P: {oldPlaylist.Name}, {oldPlaylist.FixedPlaylist}");
            Logger.Msg($"CSST {controller.CurrentSongSelectionType}");

            // Adding the song updates the song preview music. Turn that off until we actually leave
            SongSelectionManager.GetInstance.StopPreviewAudio();
        }

        public static int FindMatchingPlaylistIndex(SRLogger logger, PlaylistManagementController controller, PlaylistItem searched)
        {
            var matchingIndices = new List<int>();
            for (int i = 0; i < controller.UserPlaylistList.playlists.Count; i++)
            {
                if (controller.UserPlaylistList.playlists[i].CreationDate == searched.CreationDate)
                {
                    matchingIndices.Add(i);
                }
            }

            if (!matchingIndices.Any())
            {
                logger.Msg("No playlist found");
                return -1;
            }

            if (matchingIndices.Count == 1)
            {
                return matchingIndices[0];
            }

            // More than one match. Try to go by name then
            foreach (int i in matchingIndices)
            {
                if (controller.UserPlaylistList.playlists[i].Name == searched.Name)
                {
                    logger.Msg($"Name match on '{searched.Name}', using index {i}");
                    return i;
                }
            }

            // No name match; just go with the first one I guess
            logger.Msg("No matching playlist name, going with first option");
            return matchingIndices[0];
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
            for (int i = 0; i < PlaylistItem.Songs.Count; i++)
            {
                var s = PlaylistItem.Songs[i];

                // Based on hash primarily
                if (!string.IsNullOrEmpty(s.hash))
                {
                    if (s.hash.Equals(song.hash))
                    {
                        return i;
                    }
                }
                // Fall back on name and author match
                else if (SongsMatchNameAuthor(s, song))
                {
                    return i;
                }
            }

            // Not found
            return -1;
        }

        private bool SongsMatchNameAuthor(PlaylistSong a, PlaylistSong b)
        {
            return a.name.ToLower().Equals(b.name.ToLower()) && a.author.ToLower().Equals(b.author.ToLower());
        }

        private Il2CppSystem.Collections.Generic.List<PlaylistSong> GetSongsNotMatching(PlaylistSong original)
        {
            // Remove duplicates that have different hashes but matching name+author
            // This catches drafts with changing hashes / out of date
            var deduplicated = new Il2CppSystem.Collections.Generic.List<PlaylistSong>();
            foreach (PlaylistSong song in PlaylistItem.Songs)
            {
                if (!SongsMatchNameAuthor(song, original))
                {
                    deduplicated.Add(song);
                }
            }
            return deduplicated;
        }

        private void AddToPlaylist(PlaylistSong songToAdd)
        {
            var songs = GetSongsNotMatching(songToAdd);
            var numDuplicates = PlaylistItem.Songs.Count - songs.Count;

            Logger.Msg($"Adding song '{songToAdd.name}' to end of playlist '{PlaylistItem.Name}'. {numDuplicates} duplicates removed.");
            songs.Add(songToAdd);

            // Update our reference to the song list
            PlaylistItem.Songs = songs;

            // New playlist item created; to make equality check fail later??
            var newSongIdx = songs.Count - 1;
            UpdateController(PlaylistItem, newSongIdx);
        }
    }
}
