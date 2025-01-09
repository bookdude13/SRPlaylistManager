using SRModCore;
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
using Il2CppSynth.Data;

namespace SRPlaylistManager.Models
{
    public class PlaylistPanelItem : PanelItem
    {
        private PlaylistItem PlaylistItem;
        private Il2Cpp.SynthUIButton ItemButton;
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

        private void LogVerbose(string message)
        {
            if (SRPlaylistManager.VERBOSE_LOGS)
            {
                Logger.Msg(message);
            }
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

            LogVerbose("Setting up item " + item.name);

            // Stop text from being changed from localization running
            var l8ns = item.GetComponentsInChildren<Il2CppSynth.Utils.LocalizationHelper>();
            if (l8ns != null)
            {
                foreach (var l8n in l8ns)
                {
                    l8n.enabled = false;
                }
            }

            // Remove unused area if found
            var valueArea = item.transform.Find("Value Area");
            valueArea?.gameObject.SetActive(false);

            // Find main label area
            var itemBackgroundArea = item.transform.Find("Setting Item Background");

            // Update text
            Text = itemBackgroundArea?.GetComponentInChildren<Il2CppTMPro.TextMeshProUGUI>();
            Text?.SetText(PlaylistItem.Name);

            // Set up as button toggle
            try
            {
                ItemButton = item.GetComponent<SynthUIButton>();

                ItemButton.enabled = true;
                ItemButton.showTooltip = false;

                // TODO there's a background color change that happens on hover. Can't figure out why atm.

                // Directly removing persistent listeners doesn't work
                // See https://forum.unity.com/threads/documentation-unityevent-removealllisteners-only-removes-non-persistent-listeners.341796/
                // But...it looks like the persistent listener just calls the synth button.
                // Wiping out the WhenClicked callback gets rid of old behavior
                // and lets us add our own callbacks without any hassle
                ItemButton.WhenClicked = new UnityEvent();
                ItemButton.WhenClicked.AddListener(new Action(() => Toggle()));

                ItemButton.gameObject.SetActive(true);
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
            if (PlaylistItem == null)
            {
                Logger.Error("Null playlist item!");
                return;
            }

            if (PlaylistItem.FixedPlaylist && PlaylistItem.ShowFavorites)
            {
                if (SelectedTrack == null)
                {
                    Logger.Error("Selected track is null!");
                }

                if (SelectedTrack != null && SelectedTrack.OnFavorites)
                {
                    // It's difficult to get the current difficulty that was added to Favorites, so just don't include that info for now.
                    // A unicode star used to be shown, but that no longer renders.
                    SetAppearanceInPlaylist($"{PlaylistItem.Name}");
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
            if (Text == null)
            {
                Logger.Error("Text is null! Can't set in playlist, " + text);
                return;
            }

            Text.fontStyle = Il2CppTMPro.FontStyles.Bold;
            Text.color = Color.white;
            Text.SetText(text);
        }

        private void SetAppearanceNotInPlaylist()
        {
            if (Text == null)
            {
                Logger.Error("Text is null! Can't set not in playlist, " + PlaylistItem.Name);
                return;
            }

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
                LogVerbose("Toggling playlist as favorite");
                ToggleFavorite();
            }
            else
            {
                // See if the currently selected song is in us or not
                int existingSongIndex = GetExistingSongIndex(SelectedSong);
                if (existingSongIndex < 0)
                {
                    LogVerbose("Adding to playlist, existing song index " + existingSongIndex);
                    AddToPlaylist(SelectedSong);

                    SongSelectionManager.GetInstance.SelectedGameTrack = SelectedTrack;
                }
                else
                {
                    LogVerbose("Removing from playlist, existing song index " + existingSongIndex);
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
                LogVerbose($"Removing '{SelectedTrack.TrackName}' from Favorites");
                Game_InfoProvider.RemoveFromFavoritesList(SelectedTrack.TrackName, SelectedTrack.Author, SelectedTrack.LeaderboardHash);
                SelectedTrack.OnFavorites = false;
            }
            else
            {
                LogVerbose($"Adding '{SelectedTrack.TrackName}' to Favorites");
                Game_InfoProvider.AddToFavoritesList(SelectedTrack.TrackName, SelectedTrack.Author, SelectedTrack.LeaderboardHash);
                SelectedTrack.OnFavorites = true;
            }

            // Make sure the singleton is updated
            SongSelectionManager.GetInstance.SelectedGameTrack = SelectedTrack;
        }

        private void SetPlaylistItemSongs(PlaylistItem playlistItem, Il2CppSystem.Collections.Generic.List<PlaylistSong> songs)
        {
            playlistItem.Songs.Clear();
            foreach (var song in songs)
            {
                playlistItem.Songs.Add(song);
            }
        }

        /// <summary>
        /// Removes the song from the given index in the current playlist.
        /// </summary>
        /// <param name="songToRemove"></param>
        private void RemoveFromPlaylist(PlaylistSong songToRemove, int currentSongIdx)
        {
            var songs = GetSongsNotMatching(GetSongsThatExist(), songToRemove);
            var numRemoved = PlaylistItem.Songs.Count - songs.Count;
            LogVerbose($"Removing {numRemoved} instances of '{songToRemove.name}' from playlist '{PlaylistItem.Name}'");

            // Update our reference to the song list
            SetPlaylistItemSongs(PlaylistItem, songs);

            var newSongIdx = Math.Min(songs.Count - 1, currentSongIdx);
            UpdateController(PlaylistItem, newSongIdx);

            LogVerbose($"After removal, new song index is {newSongIdx} (count {songs.Count}, current idx {currentSongIdx}");

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
            //var oldCasspi = controller.CurrentAddingSongsSelectedPlaylistIndex;
            var oldSongIdx = controller.CurrentPlaylistSongIndex;
            var oldPlaylist = controller.CurrentSelectedPlaylist;
            var oldSongIdx2 = oldPlaylist.CurrentIndex;
            LogVerbose($"Old indices. P: {oldPlaylistIdx}, S: {oldSongIdx} S2: {oldSongIdx2}");
            LogVerbose($"New indices. P: {playlistIdx}, S: {newSongIdx}");
            LogVerbose($"Old P: {oldPlaylist.Name}, {oldPlaylist.FixedPlaylist}");
            LogVerbose($"CSST {controller.CurrentSongSelectionType}");

            //// If we altered the currently selected playlist, make sure it's updated too!
            //if (controller.CurrentSelectedPlaylist != null &&
            //    controller.CurrentSelectedPlaylist.CreationDate == newItem.CreationDate &&
            //    controller.CurrentSelectedPlaylist.Name == newItem.Name)
            //{
            //    LogVerbose("Altered current selected playlist, making sure index matches");
            //    if (newSongIdx > newItem.Songs.Count - 1)
            //    {
            //        Logger.Error("New song index will be out of bounds!");
            //    }
            //    //controller.CurrentSelectedPlaylist.Songs = newItem.Songs;
            //    //SetPlaylistItemSongs(controller.CurrentSelectedPlaylist, newItem.Songs);
            //}

            //controller.Interface__OnSongSelectedForPlaylist();
            //controller.Interface__OnPlaylistScrollItemClick(oldSongIdx);
            //controller.Interface__TryAddSongsToSelectredPlaylist;

            // Adding the song updates the song preview music. Turn that off until we actually leave
            try
            {
                SongSelectionManager.GetInstance.StopPreviewAudio();
            }
            catch (Exception e)
            {
                Logger.Error("Failed to stop preview audio", e);
            }
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
            if (song == null)
            {
                return -1;
            }

            for (int i = 0; i < PlaylistItem.Songs.Count; i++)
            {
                var s = PlaylistItem.Songs[i];

                if (s == null)
                    continue;

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

        private Il2CppSystem.Collections.Generic.List<PlaylistSong> GetSongsThatExist()
        {
            //return PlaylistItem.Songs;

            if (SongsProvider.GetInstance == null)
            {
                Logger.Error("Null songs provider");
                return PlaylistItem.Songs;
            }

            // Remove songs that don't exist (i.e. deleted customs that were in a playlist)
            var existing = new Il2CppSystem.Collections.Generic.List<PlaylistSong>();
            foreach (PlaylistSong song in PlaylistItem.Songs)
            {
                var foundSong = SongsProvider.GetInstance.GetSongByLeaderboardHash(song.hash);
                if (foundSong == null)
                {
                    Logger.Msg($"Falling back on title/author for song {song.name} {song.author}");
                    foundSong = SongsProvider.GetInstance.GetSongByNameAndAutor(song.name, song.author);
                }
                if (foundSong != null)
                {
                    //Logger.Msg($"  Found song {foundSong.Name} {foundSong.FilePath} NoP?{foundSong.IsNotPlayable}, init {foundSong.IsInit}, adm {foundSong.IsAdminOnly}, tbyb {foundSong.IsTBYB}, prod {foundSong.ProductionMode}");
                    existing.Add(song);
                }
                else
                {
                    Logger.Error($"Song {song.name} {song.author} {song.hash} not found! Removing from playlist");
                }
            }
            return existing;
        }

        private Il2CppSystem.Collections.Generic.List<PlaylistSong> GetSongsNotMatching(Il2CppSystem.Collections.Generic.List<PlaylistSong> songs, PlaylistSong original)
        {
            // Remove duplicates that have different hashes but matching name+author
            // This catches drafts with changing hashes / out of date
            var deduplicated = new Il2CppSystem.Collections.Generic.List<PlaylistSong>();
            foreach (PlaylistSong song in songs)
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
            var songs = GetSongsNotMatching(GetSongsThatExist(), songToAdd);
            var numDuplicates = PlaylistItem.Songs.Count - songs.Count;

            LogVerbose($"Adding song '{songToAdd.name}' to end of playlist '{PlaylistItem.Name}'. {numDuplicates} duplicates removed.");
            songs.Add(songToAdd);

            // Update our reference to the song list
            SetPlaylistItemSongs(PlaylistItem, songs);

            // New playlist item created; to make equality check fail later??
            var newSongIdx = songs.Count - 1;
            UpdateController(PlaylistItem, newSongIdx);
        }
    }
}
