namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware
{
    [Serializable]
    public class VmdkFileInfo
    {
        private string _name;
        private string _fullName;
        private string _dataFileName;
        private ulong _size;

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public string FullName
        {
            get
            {
                return this._fullName;
            }
        }

        public string DataFileName
        {
            get
            {
                return this._dataFileName;
            }
        }

        public ulong Size
        {
            get
            {
                return this._size;
            }
        }

        public VmdkFileInfo(string name, string path, ulong size)
        {
            if (!path.EndsWith("/") && !path.EndsWith("\\"))
                path += "/";
            this._fullName = path + name;
            if (name.EndsWith("-flat.vmdk", StringComparison.InvariantCultureIgnoreCase))
            {
                this._dataFileName = name;
                this._name = name.Substring(0, name.Length - 10) + ".vmdk";
            }
            else
            {
                this._name = name;
                this._dataFileName = name.Substring(0, name.Length - 5) + "-flat.vmdk";
            }
            this._size = size;
        }
    }
}
