using SRModCore;
using SRPlaylistManager.Models;
using SRPlaylistManager.Services;
using Il2CppSynth.SongSelection;
using System;
using UnityEngine;
using Il2CppUtil.Controller;
using MelonLoader;

namespace SRPlaylistManager.MonoBehavior
{
    [RegisterTypeInIl2Cpp]
    public class MultiplayerPlaylistMenuMonoBehavior : AbstractPlaylistMenuMonoBehavior
    {
        public MultiplayerPlaylistMenuMonoBehavior(IntPtr ptr) : base(ptr) { }

        protected override GameObject GetToggledView()
        {
            // Find good parent so the panel can be seen
            var center = GameObject.Find("Main Stage Prefab/Z-Wrap/Multiplayer/RoomPanel/Scale Wrap/MultiplayerRoomPanel");
            _logger.Msg("Toggled: " + center);
            return center;
        }

        protected override void OnMenuClose(GameObject toHide)
        {
            _logger.Msg("Menu close, showing hidden view again");
            toHide.SetActive(true);

            // Resume audio
            _logger.Msg($"Selected song: {SongSelectionManager.GetInstance?.SelectedGameTrack?.name}");
            SongSelectionManager.GetInstance?.PlayPreviewAudio(true);
        }
    }
}
