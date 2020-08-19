using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Nokia.Storage.FileStorage
{
    public interface IFileStorage
    {
        Task<(Stream, StoredFileInfo)> GetAsync(string fileId, CancellationToken cancellationToken = default);
        Task<StoredFileInfo> StoreAsync(IFormFile file, CancellationToken cancellationToken = default);
        Task<PaginatedFileInfo> ListAsync(int pageSize, int pageNumber, CancellationToken cancellationToken = default);
    }
}
