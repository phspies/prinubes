﻿using System;

namespace PlatformWorker.VMware
{
    [Serializable]
    public struct ScsiInfo
    {
        public int CtrlKey;
        public int Unit;

        public ScsiInfo(int key, int unit)
        {
            this.CtrlKey = key;
            this.Unit = unit;
        }
    }
}
