using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SRPlaylistManager.Models
{
    internal class SongSelectionView
    {
        public GameObject SelectionSongPanel { get; private set; }
        private GameObject CenterPanel;
        private GameObject PlayListManagement;

        public SongSelectionView(GameObject selectionSongPanel, GameObject centerPanel, GameObject playlistManagement)
        {
            SelectionSongPanel = selectionSongPanel;
            CenterPanel = centerPanel;
            PlayListManagement= playlistManagement;
        }

        public static SongSelectionView GetView()
        {
            var songSelectPanel = GameObject.Find("Z-Wrap/SongSelection/SelectionSongPanel");
            if (songSelectPanel == null)
            {
                Console.Error.WriteLine("Failed to find song select panel");
                return null;
            }

            var centerPanel = songSelectPanel.transform.Find("CentralPanel");
            if (centerPanel == null)
            {
                Console.Error.WriteLine("Failed to find center panel");
                return null;
            }

            var playlistButtons = songSelectPanel.transform.Find("PlayListManagement");
            if (playlistButtons == null)
            {
                Console.Error.WriteLine("Failed to find playlist buttons");
                return null;
            }

            return new SongSelectionView(songSelectPanel, centerPanel.gameObject, playlistButtons.gameObject);
        }

        public void SetVisibility(bool visible)
        {
            CenterPanel.gameObject.SetActive(visible);
            PlayListManagement.gameObject.SetActive(visible);
        }
    }
}
