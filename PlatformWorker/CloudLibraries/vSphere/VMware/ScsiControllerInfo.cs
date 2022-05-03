namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware
{
    public class ScsiControllerInfo
    {
        public string Type;
        public int BusNumber;
        public int CtrlKey;

        public ScsiControllerInfo(string type, int ctrlKey, int busNumber)
        {
            this.Type = type;
            this.CtrlKey = ctrlKey;
            this.BusNumber = busNumber;
        }
    }
}
