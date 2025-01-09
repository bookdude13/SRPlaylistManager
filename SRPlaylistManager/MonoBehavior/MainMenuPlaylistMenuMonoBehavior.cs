using SRModCore;
using SRPlaylistManager.Models;
using SRPlaylistManager.Services;
using Il2CppSynth.SongSelection;
using System;
using UnityEngine;
using Il2CppUtil.Controller;
using MelonLoader;
using Il2Cppcom.Kluge.XR.Utils;

namespace SRPlaylistManager.MonoBehavior
{
    [RegisterTypeInIl2Cpp]
    public class MainMenuPlaylistMenuMonoBehavior : AbstractPlaylistMenuMonoBehavior
    {
        public MainMenuPlaylistMenuMonoBehavior(IntPtr ptr) : base(ptr) { }

        protected override GameObject GetToggledView()
        {
            return SongSelectionView.GetView().SelectionSongPanel;
        }

        protected override void OnMenuClose(GameObject toHide)
        {
            // Try to open center view again
            _logger.Msg("Menu close, showing center view again");
            toHide.SetActive(true);

            // Now that we're visible again, refresh the playlist view's visuals if needed
            _logger.Msg("Refreshing playlist view");
            bool isFixedRefresh = RefreshCurrentPlaylistView();

            // Select at the current index, if any
            // Different checks for fixed playlists/views
            var currentPlist = PlaylistManagementController.GetInstance?.CurrentSelectedPlaylist;
            _logger.Msg($"Current plist {currentPlist.Name}, fixed reset? {isFixedRefresh}");
            _logger.Msg($"Current plist idx {PlaylistManagementController.GetInstance.CurrentPlaylistIndex}");
            _logger.Msg($"Current plist song idx {PlaylistManagementController.GetInstance.CurrentPlaylistSongIndex}");
            _logger.Msg($"Current plist current song idx {PlaylistManagementController.GetInstance.CurrentSelectedPlaylist.CurrentIndex}");
            if (isFixedRefresh)
            {
                // No easy way to check bounds, so just try and hope
                _logger.Msg("Fixed playlist, selecting song idx " + songPlaylistIndexBeforeOpen);
                if (songPlaylistIndexBeforeOpen >= 0)
                {
                    try
                    {
                        SongSelectionManager.GetInstance?.OnSongItemClicked(songPlaylistIndexBeforeOpen);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Failed to select song after menu close", ex);
                    }
                }
            }
            else
            {
                // User playlist

                // Select current song item
                var currPlaylistSongCount = currentPlist?.Songs.Count ?? 0;

                if (songPlaylistIndexBeforeOpen > currPlaylistSongCount - 1)
                {
                    songPlaylistIndexBeforeOpen = currPlaylistSongCount - 1;
                    _logger.Msg("Index too big, changed to " + songPlaylistIndexBeforeOpen);
                }

                if (songPlaylistIndexBeforeOpen < 0)
                {
                    _logger.Msg("Index out of range, not clicking song");
                }
                else
                {
                    _logger.Msg($"Non-fixed playlist, clicking song at index {songPlaylistIndexBeforeOpen}, count is {currPlaylistSongCount}");
                    foreach (var song in currentPlist.Songs)
                    {
                        _logger.Msg($"  Playlist song {song.name} {song.author} {song.hash} {song.trackDuration} {song.addedTime}");
                    }

                    SongSelectionManager.GetInstance?.OnSongItemClicked(songPlaylistIndexBeforeOpen);
                }
            }

            // Now that the extra click has happened, make sure the center view is still visible
            _logger.Msg("Second check for center view visible");
            toHide.SetActive(true);

            // Resume audio
            // Sometimes throws error when song in current playlist is removed in other playlists.
            // Not sure why, so leaving it as-is for now
            _logger.Msg($"Selected song: {SongSelectionManager.GetInstance?.SelectedGameTrack?.TrackName}");
            try
            {
                SongSelectionManager.GetInstance?.PlayPreviewAudio(true);
            }
            catch (Exception)
            {
                _logger.Error("Failed to re-enable preview audio (occasionally happens)");
            }
        }
    }
}
