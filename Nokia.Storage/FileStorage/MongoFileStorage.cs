using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Nokia.Storage.FileStorage
{
    public class MongoFileStorage : IFileStorage
    {
        private readonly IMongoClient _dbClient;
        private const string bucket_name = "tar_fs";
        public MongoFileStorage(string connectionString)
        {
            _dbClient = new MongoClient(connectionString);
        }

        public MongoFileStorage(IMongoClient client)
        {
            _dbClient = client;
        }

        public async Task<(Stream, StoredFileInfo)> GetAsync(string fileId, CancellationToken cancellationToken = default)
        {
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            IGridFSBucket bucket = getBucket(_dbClient);
            FilterDefinition<GridFSFileInfo> filter = Builders<GridFSFileInfo>.Filter.Eq(p => p.Filename, fileId);

            using var cursor = await bucket.FindAsync(filter, null, cancellationToken);
            var file = await cursor.FirstOrDefaultAsync(cancellationToken);

            if(file ==null)
            {
                return (null, null);
            }
            var stream = await bucket.OpenDownloadStreamAsync(file.Id,null, cancellationToken);
            return (stream, BsonSerializer.Deserialize<StoredFileInfo>(file.Metadata));
        }

        public async Task<PaginatedFileInfo> ListAsync(int pageSize, int pageNumber, CancellationToken cancellationToken = default)
        {
            if(pageSize< 0)
            {
                throw new ArgumentException(nameof(pageSize));
            }
            if(pageNumber < 0 )
            {
                throw new ArgumentException(nameof(pageNumber));
            }
            var tarFileBucketCollection = GetFileBucketCollection(_dbClient, bucket_name);
            // no need of parallel calls(EstimatedDocumentCountAsync is of O(1) complexity)
            var totalRecords = await tarFileBucketCollection.EstimatedDocumentCountAsync();

            if(totalRecords > 0)
            {
                var filter = Builders<GridFSFileInfo>.Filter.Empty;
                var list = await tarFileBucketCollection.Find(filter).Skip((pageNumber-1)* pageSize).Limit(pageSize).ToListAsync(cancellationToken);

                return new PaginatedFileInfo
                {
                    Files = list.Select(file => BsonSerializer.Deserialize<StoredFileInfo>(file.Metadata)),
                    TotalRecords = totalRecords,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }

            return new PaginatedFileInfo()
            {
                Files = null,
                TotalRecords = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            
        }

        public async Task<StoredFileInfo> StoreAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            _ = file ?? throw new ArgumentNullException(nameof(file));

            var fileId = Guid.NewGuid().ToString();
            IGridFSBucket bucket = getBucket(_dbClient);

            StoredFileInfo fileInfo = await uploadFile(fileId, file, bucket, cancellationToken);

            return fileInfo;
        }


        private IGridFSBucket getBucket(IMongoClient client)
        {
            var database = client.GetDatabase("file_storage");
            var options = new GridFSBucketOptions
            {
                BucketName = bucket_name,
                ChunkSizeBytes = 1048576,
            };
            return new GridFSBucket(database, options);
        }

        private async Task<StoredFileInfo> uploadFile(string fileId, IFormFile file, IGridFSBucket bucket, CancellationToken cancellationToken = default)
        {
            var fileInfo = new StoredFileInfo
            {
                FileId = fileId,
                FileName = file.FileName,
                ContentType = file.ContentType,
                Length = file.Length
            };

            using Stream fileStream = file.OpenReadStream();
            var uploadOptions = new GridFSUploadOptions
            {
                Metadata = BsonDocument.Parse(JsonSerializer.Serialize(fileInfo)),
            };
            // assigning filename in gridfs to fileid(for filtering by FileName while downloading file)
            _= await bucket.UploadFromStreamAsync(fileId, fileStream, uploadOptions, cancellationToken);

            return fileInfo;
        }

        private IMongoCollection<GridFSFileInfo> GetFileBucketCollection(IMongoClient client, string bucketName)
        {
            var db = client.GetDatabase("file_storage");
            return db.GetCollection<GridFSFileInfo>($"{bucketName}.files");
        }
    }
}
