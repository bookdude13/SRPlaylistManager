using Il2CppSynth.SongSelection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace SRPlaylistManager.Harmony
{
    [HarmonyPatch(typeof(SongSelectionManager), nameof(SongSelectionManager.OpenMultiplayerRoomMenu))]
    public class Patch_SongSelectionManager_OpenMultiplayerRoomMenu
    {
        public static void Postfix(Il2CppSynth.Versus.Room room)
        {
            SRPlaylistManager.Instance?.OnOpenMultiplayerRoomMenu(room);
        }
    }
}
