﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SRPlaylistManager.Models
{
    internal interface PanelItem
    {
        GameObject Setup(GameObject item);
    }
}
