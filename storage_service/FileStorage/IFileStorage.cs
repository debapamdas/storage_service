using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Nokia.Storage.FileStorage
{
    public interface IFileStorage
    {
        Task<(Stream, StoredFileInfo)> GetAsync(string fileId);
        Task<StoredFileInfo> StoreAsync(IFormFile file);
        Task<IEnumerable<StoredFileInfo>> ListAsync();
    }
}
