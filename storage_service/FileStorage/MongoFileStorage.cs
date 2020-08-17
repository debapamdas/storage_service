using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nokia.Storage.FileStorage
{
    public class MongoFileStorage : IFileStorage
    {
        public MongoFileStorage(string connectionString)
        {

        }
        public Task<(Stream, StoredFileInfo)> GetAsync(string fileId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<StoredFileInfo>> ListAsync()
        {
            throw new NotImplementedException();
        }

        public Task<StoredFileInfo> StoreAsync(IFormFile file)
        {
            throw new NotImplementedException();
        }
    }
}
