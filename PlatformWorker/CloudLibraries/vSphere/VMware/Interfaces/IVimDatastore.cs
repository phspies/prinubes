using PlatformWorker.VMware.Contracts;

namespace PlatformWorker.VMware.Interfaces
{
    public interface IVimDatastore : IVimManagedItem
    {
        string BracketedName { get; }
        long Capacity { get; }
        DatastoreProperties DsProperties { get; set; }
        long FreeSpace { get; }

        Task CreateDirectoryAsync(string folder);
        Task DeleteFileAsync(string filePath, VimClientlContext ctx);
        Task<bool> DirectoryExistAsync(string dir, VimClientlContext ctx);
        void DownloadFile(string remotePath, string localPath, string fileName, VimClientlContext ctx);
        Task<bool> FileExistAsync(string fullName, VimClientlContext ctx);
        Task<Dictionary<string, VimDatastoreItem[]>> FindDatastoreItemsAsync(VimClientlContext ctx);
        Task<VimDatastoreItem[]> FindDatastoreItemsInFolderAsync(string folderName, VimClientlContext ctx);
        Task<Dictionary<string, List<VmdkFileInfo>>> GetAllFoldersAndFilesInfoAsync(VimClientlContext ctx);
        DatastoreProperties GetCommonProperties();
        Task GetCommonPropertiesAsync(Dictionary<string, object> properties);
        Task<IVimDatacenter> GetDatacenterAndPropertiesAsync();
        Task<IVimHost> GetFirstOrDefaultHostAsync();
        Task<IVimHost[]> GetHostsAsync();
        string GetName();
        string GetPath();
        Task<Dictionary<string, bool>> GetVirtualDisksTypesAsync(string folderName, VimClientlContext ctx);
        Task<Dictionary<string, VmdkFileInfo>> GetVmdksFileInfoAsync(string fullName, VimClientlContext ctx);
        Task<string[]> GetVmdksFullNameAsync(string folderName, VimClientlContext ctx);
        Task<bool> IsFolderOnRootExistAsync(string folderName, VimClientlContext ctx);
        Task<bool> IsReadOnlyAsync(IVimHost vimHost);
        Task MoveFilesByFullNameAsync(string source, string target, string targetFolderName, bool force, VimClientlContext ctx);
        Task MoveFilesByNameAsync(string srcFolder, string srcFile, string tgtFolder, bool force, VimClientlContext ctx);
        void UploadFile(string remotePath, string localPath, string fileName);
    }

}
