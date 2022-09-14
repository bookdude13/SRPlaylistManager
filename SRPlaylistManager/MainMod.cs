using MelonLoader;
using SRModCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MelonLoader.MelonLogger;

namespace SRPlaylistManager
{
    public class MainMod : MelonMod
    {
        private SRLogger logger;

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();

            logger = new MelonLoggerWrapper(LoggerInstance);
        }
    }
}
