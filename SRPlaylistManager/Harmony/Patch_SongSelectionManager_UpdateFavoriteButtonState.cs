using HarmonyLib;
using Il2CppSynth.SongSelection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppTMPro;
using UnityEngine;
using Il2CppUtil.Controller;

namespace SRPlaylistManager.Harmony
{
    /*[HarmonyPatch(typeof(SongSelectionManager), "UpdateFavoriteButtonState")]
    public class Patch_SongSelectionManager_UpdateFavoriteButtonState
    {
        public static bool Prefix(TMP_Text ___favoriteBtnLabel)
        {
            // Don't follow the normal text changes
            // TODO better text and do translation
            ___favoriteBtnLabel.SetText("Playlist", true);

            return false;
        }
    }*/
}
