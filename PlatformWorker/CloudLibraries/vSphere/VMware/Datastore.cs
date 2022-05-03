using Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware.Contracts;
using Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware.Interfaces;
using Prinubes.vCenterSDK;
using System.Text.RegularExpressions;

namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware
{

    internal class Datastore : VCManagedItem, IVimDatastore, IVimManagedItem
    {
        //private static Tracer tracer = Tracer.GetTracer(typeof (Datastore));
        public static string[] VCProperties = new string[7] { "summary.name", "host", "summary.capacity", "summary.freeSpace", "summary.uncommitted", "summary.type", "info" };
        private Regex snapshotDiskPattern;
        private DatastoreProperties _dsProperties;
        private IVimService _vimService;

        public DatastoreProperties DsProperties
        {
            get
            {
                return this._dsProperties;
            }
            set
            {
                this._dsProperties = value;
            }
        }

        public long Capacity
        {
            get
            {
                if (this._dsProperties.Capacity <= 0L)
                    this.GetCommonPropertiesAsync(this.GetPropertiesAsync(Datastore.VCProperties).Result);
                return this._dsProperties.Capacity;
            }
        }

        public long FreeSpace
        {
            get
            {
                if (this._dsProperties.FreeSpace <= 0L)
                    this.GetCommonPropertiesAsync(this.GetPropertiesAsync(Datastore.VCProperties).Result);
                return this._dsProperties.FreeSpace;
            }
        }

        public string BracketedName
        {
            get
            {
                return "[" + this.Name + "]";
            }
        }

        public Datastore(IVimService vimService, ManagedObjectReference managedObject)
          : base(vimService, managedObject)
        {
            this._vimService = vimService;
        }

        public DatastoreProperties GetCommonProperties()
        {
            if (string.IsNullOrEmpty(this._dsProperties.Name))
                this.GetCommonPropertiesAsync(this.GetPropertiesAsync(Datastore.VCProperties).Result).Wait();
            return this._dsProperties;
        }

        public async Task GetCommonPropertiesAsync(Dictionary<string, object> properties)
        {
            for (int index = 0; index < 3; ++index)
            {

                if (properties.Count != Datastore.VCProperties.Length)
                {
                    IEnumerable<string> strings = VCProperties.Where<string>(p => !properties.ContainsKey(p));
                    if (strings != null)
                    {
                        if (strings.Count<string>() == 1 && strings.Contains<string>("summary.uncommitted"))
                            break;
                    }
                }
                else
                    break;
                Thread.Sleep(1000);
                properties = await this.GetPropertiesAsync(Datastore.VCProperties);
            }
            if (properties.ContainsKey("summary.name"))
                this._dsProperties.Name = (string)properties["summary.name"];
            if (properties.ContainsKey("host"))
            {
                DatastoreHostMount[] property = (DatastoreHostMount[])properties["host"];
                if (property.Length != 0)
                    this._dsProperties.Url = property[0].mountInfo.path;
            }
            if (properties.ContainsKey("summary.capacity"))
                this._dsProperties.Capacity = (long)properties["summary.capacity"];
            if (properties.ContainsKey("summary.freeSpace"))
                this._dsProperties.FreeSpace = (long)properties["summary.freeSpace"];
            long num = 0;
            if (properties.ContainsKey("summary.uncommitted"))
                num = (long)properties["summary.uncommitted"];
            this._dsProperties.ProvisionedSpace = this._dsProperties.Capacity - this._dsProperties.FreeSpace + num;
            if (properties.ContainsKey("summary.type"))
                this._dsProperties.Type = (string)properties["summary.type"];
            if (properties.ContainsKey("info"))
            {
                DatastoreInfo property1 = properties["info"] as DatastoreInfo;
                if (property1 != null)
                    this._dsProperties.MaxFileSize = property1.maxFileSize;
                VmfsDatastoreInfo property2 = properties["info"] as VmfsDatastoreInfo;
                if (property2 != null && property2.vmfs != null)
                    this._dsProperties.Version = property2.vmfs.version;
            }
            this._dsProperties.RemoteId = Datastore.GetRemoteId(this._dsProperties.Url);
            this.Name = this._dsProperties.Name;
        }

        public string GetPath()
        {
            if (string.IsNullOrEmpty(this._dsProperties.Url))
                this.GetCommonProperties();
            return this._dsProperties.Url;
        }

        public override string GetName()
        {
            if (string.IsNullOrEmpty(this.Name))
                this.GetCommonProperties();
            return this.Name;
        }

        public async Task<IVimHost[]> GetHostsAsync()
        {
            Dictionary<string, object> properties = await GetPropertiesAsync(new string[1] { "host" });
            List<IVimHost> vimHostList = new List<IVimHost>();
            if (properties.ContainsKey("host"))
            {
                foreach (DatastoreHostMount datastoreHostMount in (DatastoreHostMount[])properties["host"])
                {
                    try
                    {
                        IVimHost vimHost = new Host(this.VcService, datastoreHostMount.key);
                        vimHost.GetCommonProperties();
                        vimHostList.Add(vimHost);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            return vimHostList.ToArray();
        }

        public async Task<IVimHost> GetFirstOrDefaultHostAsync()
        {
            Dictionary<string, object> properties = await GetPropertiesAsync(new string[1] { "host" });
            if (properties.ContainsKey("host"))
            {
                foreach (DatastoreHostMount datastoreHostMount in (DatastoreHostMount[])properties["host"])
                {
                    try
                    {
                        Host host = new Host(this.VcService, datastoreHostMount.key);
                        host.GetCommonProperties();
                        return host;
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            throw new Exception("No ESX host found.");
        }

        public static string GetRemoteId(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;
            string str;
            if (url.StartsWith("sanfs"))
                str = url.Split(':')[2].TrimEnd('/');
            else if (url.StartsWith("netfs"))
                str = url.Split('/')[3];
            else
                str = url.Split('/')[3];
            return str;
        }

        public async Task<Dictionary<string, VmdkFileInfo>> GetVmdksFileInfoAsync(string fullName, VimClientlContext ctx)
        {
            fullName = fullName.Replace("\\", "/");
            string str1 = fullName.Substring(0, fullName.LastIndexOf("/"));
            string fileName = Path.GetFileName(fullName);
            Dictionary<string, VmdkFileInfo> dictionary = new Dictionary<string, VmdkFileInfo>(StringComparer.InvariantCultureIgnoreCase);
            try
            {
                string str2 = VCService.GetVolumeName(this.Name) + str1;
                HostDatastoreBrowserSearchResults[] browserSearchResults1 = await GetBrowserSearchResultsAsync(str2, ctx);
                if (browserSearchResults1 != null)
                {
                    if (browserSearchResults1.Length != 0)
                    {
                        foreach (HostDatastoreBrowserSearchResults browserSearchResults2 in browserSearchResults1)
                        {
                            string strA = browserSearchResults2.folderPath.TrimEnd('/');
                            if (browserSearchResults2.file != null && string.Compare(strA, str2, true) == 0)
                            {
                                foreach (Prinubes.vCenterSDK.FileInfo fileInfo in browserSearchResults2.file)
                                {
                                    if (!dictionary.ContainsKey(fileInfo.path))
                                    {
                                        dictionary.Add(fileInfo.path, new VmdkFileInfo(fileInfo.path, browserSearchResults2.folderPath, (ulong)fileInfo.fileSize));
                                        if (dictionary[fileInfo.path].Name != fileName)
                                            dictionary.Remove(fileInfo.path);
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return dictionary;
        }

        public async Task<Dictionary<string, VimDatastoreItem[]>> FindDatastoreItemsAsync(VimClientlContext ctx)
        {
            if (this.snapshotDiskPattern == null)
                this.snapshotDiskPattern = new Regex("w*-[0-9][0-9][0-9][0-9][0-9][0-9]w*");
            Dictionary<string, VimDatastoreItem[]> dictionary = new Dictionary<string, VimDatastoreItem[]>(StringComparer.CurrentCultureIgnoreCase);
            HostDatastoreBrowserSearchResults[] datastoreSearchResults = await GetDatastoreSearchResultsAsync(ctx);
            int length = VCService.GetVolumeName(this.Name).Length;
            if (datastoreSearchResults != null && datastoreSearchResults.Length != 0)
            {
                foreach (HostDatastoreBrowserSearchResults browserSearchResults in datastoreSearchResults)
                {
                    string str = browserSearchResults.folderPath.TrimEnd('/');
                    if (length > str.Length)
                        length = str.Length;
                    string key = str.Remove(0, length);
                    List<VimDatastoreItem> vimDatastoreItemList = new List<VimDatastoreItem>();
                    if (browserSearchResults.file != null)
                    {
                        foreach (Prinubes.vCenterSDK.FileInfo fileInfo in browserSearchResults.file)
                        {
                            VimDatastoreItem vimDatastoreItem = new VimDatastoreItem() { Name = fileInfo.path, Size = fileInfo.fileSize };
                            if (fileInfo is FolderFileInfo)
                                vimDatastoreItem.Type = "FOLDER";
                            else if (fileInfo is VmDiskFileInfo && !this.snapshotDiskPattern.IsMatch(fileInfo.path))
                                vimDatastoreItem.Type = "VMDK";
                            else
                                continue;
                            vimDatastoreItemList.Add(vimDatastoreItem);
                        }
                    }
                    dictionary.Add(key, vimDatastoreItemList.ToArray());
                }
            }
            return dictionary;
        }

        public async Task<VimDatastoreItem[]> FindDatastoreItemsInFolderAsync(string folderName, VimClientlContext ctx)
        {
            if (this.snapshotDiskPattern == null)
                this.snapshotDiskPattern = new Regex("w*-[0-9][0-9][0-9][0-9][0-9][0-9]w*");
            List<VimDatastoreItem> vimDatastoreItemList = new List<VimDatastoreItem>();
            HostDatastoreBrowserSearchResults datastoreSearchResults = await GetDatastoreSearchResultsAsync(VCService.GetVolumeName(Name) + folderName, ctx);
            if (datastoreSearchResults != null && datastoreSearchResults.file != null && datastoreSearchResults.file.Length != 0)
            {
                foreach (Prinubes.vCenterSDK.FileInfo fileInfo in datastoreSearchResults.file)
                {
                    VimDatastoreItem vimDatastoreItem = new VimDatastoreItem() { Name = fileInfo.path, Size = fileInfo.fileSize };
                    if (fileInfo is FolderFileInfo)
                        vimDatastoreItem.Type = "FOLDER";
                    else if (fileInfo is VmDiskFileInfo && !this.snapshotDiskPattern.IsMatch(fileInfo.path))
                        vimDatastoreItem.Type = "VMDK";
                    else
                        continue;
                    vimDatastoreItemList.Add(vimDatastoreItem);
                }
            }
            return vimDatastoreItemList.ToArray();
        }

        public async Task<Dictionary<string, List<VmdkFileInfo>>> GetAllFoldersAndFilesInfoAsync(VimClientlContext ctx)
        {
            Dictionary<string, List<VmdkFileInfo>> dictionary = new Dictionary<string, List<VmdkFileInfo>>(StringComparer.InvariantCultureIgnoreCase);
            string volumeName = VimUtils.GetVolumeName(this.Name);
            HostDatastoreBrowserSearchResults[] browserSearchResults1 = await GetBrowserSearchResultsAsync(volumeName, ctx);
            if (browserSearchResults1 != null && browserSearchResults1.Length != 0)
            {
                foreach (HostDatastoreBrowserSearchResults browserSearchResults2 in browserSearchResults1)
                {
                    string str = browserSearchResults2.folderPath.TrimEnd('/');
                    int length = volumeName.Length;
                    if (length > str.Length)
                        length = str.Length;
                    string key = str.Remove(0, length);
                    List<VmdkFileInfo> vmdkFileInfoList = new List<VmdkFileInfo>();
                    if (browserSearchResults2.file != null)
                    {
                        foreach (Prinubes.vCenterSDK.FileInfo fileInfo in browserSearchResults2.file)
                        {
                            string pattern = "w*-[0-9][0-9][0-9][0-9][0-9][0-9]w*";
                            if (!Regex.Match(fileInfo.path, pattern, RegexOptions.IgnoreCase).Success)
                                vmdkFileInfoList.Add(new VmdkFileInfo(fileInfo.path, browserSearchResults2.folderPath, (ulong)fileInfo.fileSize));
                        }
                    }
                    if (!dictionary.ContainsKey(key))
                        dictionary.Add(key, vmdkFileInfoList);
                }
            }
            return dictionary;
        }

        public async Task<string[]> GetVmdksFullNameAsync(string folderName, VimClientlContext ctx)
        {
            List<string> stringList = new List<string>();
            string str1 = VimUtils.GetVolumeName(this.Name) + folderName;
            HostDatastoreBrowserSearchResults[] browserSearchResults1 = await GetBrowserSearchResultsAsync(str1, ctx);
            if (browserSearchResults1 != null && browserSearchResults1.Length != 0)
            {
                foreach (HostDatastoreBrowserSearchResults browserSearchResults2 in browserSearchResults1)
                {
                    string strA = browserSearchResults2.folderPath.TrimEnd('/');
                    if (browserSearchResults2.file != null && string.Compare(strA, str1, true) == 0)
                    {
                        foreach (Prinubes.vCenterSDK.FileInfo fileInfo in browserSearchResults2.file)
                        {
                            string pattern = "w*-[0-9][0-9][0-9][0-9][0-9][0-9]w*";
                            if (!Regex.Match(fileInfo.path, pattern, RegexOptions.IgnoreCase).Success)
                            {
                                string str2 = browserSearchResults2.folderPath + fileInfo.path;
                                if (!stringList.Contains(str2))
                                    stringList.Add(browserSearchResults2.folderPath + fileInfo.path);
                            }
                        }
                        break;
                    }
                }
            }
            return stringList.ToArray();
        }

        private async Task<HostDatastoreBrowserSearchResults[]> GetBrowserSearchResultsAsync(string datastorePath, VimClientlContext ctx)
        {
            HostDatastoreBrowserSearchResults[] browserSearchResultsArray = null;
            ManagedObjectReference[] managedObjects = await this.GetManagedObjectsAsync(new string[1] { "browser" });
            HostDatastoreBrowserSearchSpec searchSpec = new HostDatastoreBrowserSearchSpec();
            searchSpec.matchPattern = new string[1] { "*.vmdk" };
            searchSpec.searchCaseInsensitive = true;
            searchSpec.searchCaseInsensitiveSpecified = true;
            searchSpec.sortFoldersFirst = true;
            searchSpec.sortFoldersFirstSpecified = true;
            searchSpec.searchCaseInsensitiveSpecified = true;
            searchSpec.details = new FileQueryFlags();
            searchSpec.details.fileSize = true;
            searchSpec.details.fileOwner = false;
            searchSpec.details.fileOwnerSpecified = true;
            VCTask task = new VCTask(VcService, await VcService.Service.SearchDatastoreSubFolders_TaskAsync(managedObjects[0], datastorePath, searchSpec));
            string op = "Browse Datastore";
            VimClientlContext rstate = ctx;
            task.WaitForResultAsync(op, rstate);
            string[] properties1 = new string[1] { "info.result" };
            Dictionary<string, object> properties2 = await task.GetPropertiesAsync(properties1);
            if (properties2.ContainsKey("info.result"))
                browserSearchResultsArray = (HostDatastoreBrowserSearchResults[])properties2["info.result"];
            return browserSearchResultsArray;
        }

        private async Task<HostDatastoreBrowserSearchResults> GetDatastoreSearchResultsAsync(string datastorePath, VimClientlContext ctx)
        {
            HostDatastoreBrowserSearchResults browserSearchResults = null;
            ManagedObjectReference[] managedObjects = await GetManagedObjectsAsync(new string[1] { "browser" });
            HostDatastoreBrowserSearchSpec searchSpec = new HostDatastoreBrowserSearchSpec();
            VmDiskFileQuery vmDiskFileQuery = new VmDiskFileQuery();
            vmDiskFileQuery.details = new VmDiskFileQueryFlags();
            vmDiskFileQuery.details.capacityKb = true;
            searchSpec.details = new FileQueryFlags();
            searchSpec.details.fileSize = true;
            searchSpec.details.fileOwner = false;
            searchSpec.details.fileOwner = false;
            searchSpec.details.fileType = true;
            searchSpec.details.fileOwnerSpecified = true;
            searchSpec.query = new FileQuery[2]
            {
         new FolderFileQuery(),
         vmDiskFileQuery
            };
            VCTask task = new VCTask(this.VcService, await VcService.Service.SearchDatastore_TaskAsync(managedObjects[0], datastorePath, searchSpec));
            string op = "Search Datastore";
            VimClientlContext rstate = ctx;
            task.WaitForResultAsync(op, rstate);
            string[] properties1 = new string[1] { "info.result" };
            Dictionary<string, object> properties2 = await task.GetPropertiesAsync(properties1);
            if (properties2.ContainsKey("info.result"))
                browserSearchResults = (HostDatastoreBrowserSearchResults)properties2["info.result"];
            return browserSearchResults;
        }

        private async Task<HostDatastoreBrowserSearchResults[]> GetDatastoreSearchResultsAsync(VimClientlContext ctx)
        {
            string volumeName = VCService.GetVolumeName(this.Name);
            HostDatastoreBrowserSearchResults[] browserSearchResultsArray = null;
            ManagedObjectReference[] managedObjects = await GetManagedObjectsAsync(new string[1] { "browser" });
            HostDatastoreBrowserSearchSpec searchSpec = new HostDatastoreBrowserSearchSpec();
            searchSpec.matchPattern = new string[1] { "*.vmdk" };
            searchSpec.searchCaseInsensitive = true;
            searchSpec.searchCaseInsensitiveSpecified = true;
            searchSpec.sortFoldersFirst = true;
            searchSpec.sortFoldersFirstSpecified = true;
            searchSpec.searchCaseInsensitiveSpecified = true;
            searchSpec.details = new FileQueryFlags();
            searchSpec.details.fileSize = true;
            searchSpec.details.fileOwner = false;
            searchSpec.details.fileType = true;
            searchSpec.details.fileOwnerSpecified = true;
            searchSpec.query = new FileQuery[3]
            {
         new FolderFileQuery(),
         new VmDiskFileQuery(),
        new FileQuery()
            };
            VCTask task = new VCTask(VcService, await VcService.Service.SearchDatastoreSubFolders_TaskAsync(managedObjects[0], volumeName, searchSpec));
            string op = "Search Datastore";
            VimClientlContext rstate = ctx;
            task.WaitForResultAsync(op, rstate);
            string[] properties1 = new string[1] { "info.result" };
            Dictionary<string, object> properties2 = await task.GetPropertiesAsync(properties1);
            if (properties2.ContainsKey("info.result"))
                browserSearchResultsArray = (HostDatastoreBrowserSearchResults[])properties2["info.result"];
            return browserSearchResultsArray;
        }

        public async Task MoveFilesByFullNameAsync(string source, string target, string targetFolderName, bool force, VimClientlContext ctx)
        {
            if (!await this.IsFolderOnRootExistAsync(targetFolderName, ctx))
                await this.CreateDirectoryAsync(targetFolderName);
            await new VCTask(this.VcService, await VcService.Service.MoveDatastoreFile_TaskAsync(this._vimService.FileManager, source, (await this.GetDatacenterAndPropertiesAsync()).ManagedObject, target, null, force)).WaitForResultAsync("MoveFile", ctx);
        }

        public async Task MoveFilesByNameAsync(string srcFolder, string srcFile, string tgtFolder, bool force, VimClientlContext ctx)
        {
            await new VCTask(VcService, await VcService.Service.MoveDatastoreFile_TaskAsync(this._vimService.FileManager, this.GetVolumeName(this.Name) + srcFolder + "/" + srcFile, (await GetDatacenterAndPropertiesAsync()).ManagedObject, this.GetVolumeName(this.Name) + tgtFolder + "/" + srcFile, null, force)).WaitForResultAsync("MoveFile", ctx);
        }

        public async Task<Dictionary<string, bool>> GetVirtualDisksTypesAsync(string folderName, VimClientlContext ctx)
        {
            Dictionary<string, bool> dictionary = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);
            try
            {
                string datastorePath = VCService.GetVolumeName(this.Name) + folderName;
                HostDatastoreBrowserSearchResults[] browserSearchResultsArray = null;
                ManagedObjectReference[] managedObjects = await this.GetManagedObjectsAsync(new string[1] { "browser" });
                HostDatastoreBrowserSearchSpec searchSpec = new HostDatastoreBrowserSearchSpec();
                searchSpec.matchPattern = new string[1] { "*.vmdk" };
                searchSpec.searchCaseInsensitive = true;
                searchSpec.searchCaseInsensitiveSpecified = true;
                searchSpec.sortFoldersFirst = true;
                searchSpec.sortFoldersFirstSpecified = true;
                searchSpec.searchCaseInsensitiveSpecified = true;
                searchSpec.details = new FileQueryFlags();
                searchSpec.details.fileOwner = false;
                searchSpec.details.fileOwnerSpecified = true;
                searchSpec.details.fileSize = true;
                searchSpec.details.modification = true;
                searchSpec.details.fileType = true;
                VmDiskFileQuery vmDiskFileQuery = new VmDiskFileQuery();
                vmDiskFileQuery.details = new VmDiskFileQueryFlags();
                vmDiskFileQuery.details.thin = true;
                vmDiskFileQuery.details.thinSpecified = true;
                FileQuery fileQuery = new FileQuery();
                searchSpec.query = new FileQuery[1]
                {
           vmDiskFileQuery
                };
                VCTask task = new VCTask(this._vimService, await _vimService.Service.SearchDatastoreSubFolders_TaskAsync(managedObjects[0], datastorePath, searchSpec));
                string op = "Browse Datastore";
                VimClientlContext rstate = ctx;
                await task.WaitForResultAsync(op, rstate);
                string[] properties1 = new string[1] { "info.result" };
                Dictionary<string, object> properties2 = await task.GetPropertiesAsync(properties1);
                if (properties2.ContainsKey("info.result"))
                    browserSearchResultsArray = (HostDatastoreBrowserSearchResults[])properties2["info.result"];
                if (browserSearchResultsArray != null)
                {
                    if (browserSearchResultsArray.Length != 0)
                    {
                        foreach (HostDatastoreBrowserSearchResults browserSearchResults in browserSearchResultsArray)
                        {
                            List<VmdkFileInfo> vmdkFileInfoList = new List<VmdkFileInfo>();
                            if (browserSearchResults.file != null)
                            {
                                foreach (Prinubes.vCenterSDK.FileInfo fileInfo in browserSearchResults.file)
                                {
                                    if (fileInfo is VmDiskFileInfo && !dictionary.ContainsKey(fileInfo.path))
                                        dictionary.Add(fileInfo.path, ((VmDiskFileInfo)fileInfo).thin);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return dictionary;
        }

        public async Task CreateDirectoryAsync(string folder)
        {
            await VcService.Service.MakeDirectoryAsync(this.VcService.FileManager, VCService.GetVolumeName(this.Name) + folder, (await this.GetDatacenterAndPropertiesAsync()).ManagedObject, true);
        }

        public async Task DeleteFileAsync(string filePath, VimClientlContext ctx)
        {
            filePath = filePath.Replace("\\", "/");
            await VcService.Service.DeleteFileAsync((await GetManagedObjectsAsync(new string[1] { "browser" }))[0], filePath);
        }

        public async Task<IVimDatacenter> GetDatacenterAndPropertiesAsync()
        {
            return await (await GetFirstOrDefaultHostAsync()).GetDatacenterAndPropertiesAsync();
        }

        public async Task<bool> IsFolderOnRootExistAsync(string folderName, VimClientlContext ctx)
        {
            VCTask task = new VCTask(VcService, await VcService.Service.SearchDatastoreSubFolders_TaskAsync((await GetManagedObjectsAsync(new string[1] { "browser" }))[0], VimUtils.GetVolumeName(this.Name), new HostDatastoreBrowserSearchSpec() { matchPattern = new string[1] { folderName }, searchCaseInsensitive = true, searchCaseInsensitiveSpecified = true }));
            string op = "Browse Datastore";
            VimClientlContext rstate = ctx;
            await task.WaitForResultAsync(op, rstate);
            string[] properties1 = new string[1] { "info.result" };
            Dictionary<string, object> properties2 = await task.GetPropertiesAsync(properties1);
            if (properties2.ContainsKey("info.result"))
            {
                HostDatastoreBrowserSearchResults[] browserSearchResultsArray = (HostDatastoreBrowserSearchResults[])properties2["info.result"];
                if (browserSearchResultsArray != null && browserSearchResultsArray.Length != 0)
                {
                    string strB = VimUtils.GetVolumeName(this.Name).Trim();
                    foreach (HostDatastoreBrowserSearchResults browserSearchResults in browserSearchResultsArray)
                    {
                        if (browserSearchResults.file != null && browserSearchResults.file.Length != 0 && string.Compare(browserSearchResults.folderPath, strB) == 0)
                            return true;
                    }
                }
            }
            return false;
        }

        public async Task<bool> DirectoryExistAsync(string dir, VimClientlContext ctx)
        {
            dir = dir.Replace("\\", "/");
            HostDatastoreBrowserSearchResults[] browserSearchResultsArray = null;
            VCTask task = new VCTask(VcService, await VcService.Service.SearchDatastoreSubFolders_TaskAsync((await GetManagedObjectsAsync(new string[1] { "browser" }))[0], this.GetVolumeName(this.Name), new HostDatastoreBrowserSearchSpec() { matchPattern = new string[1] { dir }, searchCaseInsensitive = true, searchCaseInsensitiveSpecified = true, sortFoldersFirst = true, sortFoldersFirstSpecified = true }));
            string op = "Browse Datastore";
            VimClientlContext rstate = ctx;
            await task.WaitForResultAsync(op, rstate);
            string[] properties1 = new string[1] { "info.result" };
            Dictionary<string, object> properties2 = await task.GetPropertiesAsync(properties1);
            if (properties2.ContainsKey("info.result"))
                browserSearchResultsArray = (HostDatastoreBrowserSearchResults[])properties2["info.result"];
            return browserSearchResultsArray != null && browserSearchResultsArray.Length != 0 && browserSearchResultsArray[0].file != null;
        }

        public async Task<bool> FileExistAsync(string fullName, VimClientlContext ctx)
        {
            fullName = fullName.Replace("\\", "/");
            string str1 = fullName.Substring(0, fullName.LastIndexOf("/"));
            string str2 = fullName.Substring(fullName.LastIndexOf("/") + 1);
            HostDatastoreBrowserSearchResults[] browserSearchResultsArray = null;
            VCTask task = new VCTask(VcService, await VcService.Service.SearchDatastoreSubFolders_TaskAsync((await GetManagedObjectsAsync(new string[1] { "browser" }))[0], this.GetVolumeName(this.Name) + str1.Trim("/".ToCharArray()), new HostDatastoreBrowserSearchSpec() { matchPattern = new string[1] { str2 }, searchCaseInsensitive = true, searchCaseInsensitiveSpecified = true, sortFoldersFirst = true, sortFoldersFirstSpecified = true }));
            string op = "Browse Datastore";
            VimClientlContext rstate = ctx;
            await task.WaitForResultAsync(op, rstate);
            string[] properties1 = new string[1] { "info.result" };
            Dictionary<string, object> properties2 = await task.GetPropertiesAsync(properties1);
            if (properties2.ContainsKey("info.result"))
                browserSearchResultsArray = (HostDatastoreBrowserSearchResults[])properties2["info.result"];
            return browserSearchResultsArray != null && browserSearchResultsArray.Length != 0 && browserSearchResultsArray[0].file != null;
        }

        public void UploadFile(string remotePath, string localPath, string fileName)
        {
            //string url = this.VcService.Service.Url;
            //string str1 = url.Substring(0, url.LastIndexOf("sdk") - 1);
            //remotePath = remotePath.Replace("\\", "/");
            //remotePath = remotePath.Trim("/".ToCharArray());
            //string str2 = "/" + remotePath + "/" + fileName;
            //string path = Path.Combine(localPath, fileName);
            //if (!System.IO.File.Exists(path))
            //    throw new FileNotFoundException(string.Format("The file {0} not found.", (object)path));
            //string str3 = (str1 + "/folder" + str2 + "?dcPath=" + this.GetDatacenterAndProperties().Name + "&dsName=" + this.Name).Replace("\\ ", "%20");
            //WebClient webClient = new WebClient();
            //string str4 = this.VcService.Service.CookieContainer.GetCookies(new Uri(this.VcService.Service.Url))[0].ToString();
            //webClient.Headers.Add(HttpRequestHeader.Cookie, str4);
            //string address = str3;
            //string method = "PUT";
            //string fileName1 = path;
            //webClient.UploadFile(address, method, fileName1);
        }

        public void DownloadFile(string remotePath, string localPath, string fileName, VimClientlContext ctx)
        {
            //string url = this.VcService..Service..Url;
            //string str1 = url.Substring(0, url.LastIndexOf("sdk") - 1);
            //remotePath = remotePath.Replace("\\", "/");
            //remotePath = remotePath.Trim("/".ToCharArray());
            //string fullName = "/" + remotePath + "/" + fileName;
            //if (!this.FileExist(fullName, ctx))
            //    throw new FileNotFoundException(string.Format("The file {0} not found on datastore {1}.", (object)fullName, (object)this.Name));
            //string str2 = Path.Combine(localPath, fileName);
            //string str3 = (str1 + "/folder" + fullName + "?dcPath=" + this.GetDatacenterAndProperties().Name + "&dsName=" + this.Name).Replace("\\ ", "%20");
            //WebClient webClient = new WebClient();
            //string str4 = this.VcService.Service.CookieContainer.GetCookies(new Uri(this.VcService.Service.Url))[0].ToString();
            //webClient.Headers.Add(HttpRequestHeader.Cookie, str4);
            //string address = str3;
            //string fileName1 = str2;
            //webClient.DownloadFile(address, fileName1);
        }

        public async Task<bool> IsReadOnlyAsync(IVimHost vimHost)
        {
            bool flag = false;
            Dictionary<string, object> properties = await GetPropertiesAsync(new string[1] { "host" });
            if (properties.ContainsKey("host"))
            {
                foreach (DatastoreHostMount datastoreHostMount in (DatastoreHostMount[])properties["host"])
                {
                    if (datastoreHostMount.key.Value == vimHost.ManagedObject.Value && string.Compare(datastoreHostMount.mountInfo.accessMode, "readOnly", true) == 0)
                    {
                        flag = true;
                        break;
                    }
                }
            }
            return flag;
        }
    }
}
