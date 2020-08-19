using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Nokia.Storage.FileStorage;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Nokia.Storage.IntergrationTest
{
    [TestFixture]
    public class FileControllerStorageIntegrationTest
    {
        private HttpClient _storageClient;
        private WebApplicationFactory<Startup> _apiFactory;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _apiFactory = new WebApplicationFactory<Startup>();
            _storageClient = _apiFactory.CreateClient();
        }

        [Test]
        public async Task FileController_ImportNewFile()
        {
            // setup
            var testFile = TestFile.CreateNewFile();
            var formContent = TestHelpers.GetMultipartFormDataContent(testFile);

            // act
            var postRes = await _storageClient.PostAsync("/api/file", formContent);
            var newCreatedFile = await postRes.DeserializeBody<StoredFileInfo>();

            // assert
            Assert.AreEqual(testFile.FileName, newCreatedFile.FileName);
            Assert.AreEqual(testFile.Length, newCreatedFile.Length);
        }

        [Test]
        public async Task FileController_ReadFile()
        {
            // setup
            var testFile = TestFile.CreateNewFile();
            var formContent = TestHelpers.GetMultipartFormDataContent(testFile);

            var postRes = await _storageClient.PostAsync("/api/file", formContent);
            var newCreatedFile = await postRes.DeserializeBody<StoredFileInfo>();


            // act
            var getResponse = await _storageClient.GetAsync($"/api/file?fileId={newCreatedFile.FileId}");
            var downloadedFileContent = await getResponse.Content.ReadAsStringAsync();

            // assert
            Assert.AreEqual(testFile.FileName, newCreatedFile.FileName);
            Assert.AreEqual(testFile.Length, newCreatedFile.Length);

            Assert.AreEqual(downloadedFileContent, testFile.Content);

        }

        [Test]
        public async Task FileController_ListFiles()
        {
            
            // setup
            for(var i=0; i<10; i++)
            {
                var testFile = TestFile.CreateNewFile();
                var formContent = TestHelpers.GetMultipartFormDataContent(testFile);

                var postRes = await _storageClient.PostAsync("/api/file", formContent);
                var newCreatedFile = await postRes.DeserializeBody<StoredFileInfo>();
            }


            // act
            var getResponse = await _storageClient.GetAsync($"/api/file/list");
            var fileList = await getResponse.DeserializeBody<PaginatedFileInfo>();

            // assert
            Assert.AreEqual(10, fileList.Files.Count());
            Assert.AreEqual(1, fileList.PageNumber);
            Assert.AreEqual(10, fileList.PageSize);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            if (_storageClient != null)
            {
                _storageClient.Dispose();
            }
            if (_apiFactory != null)
            {
                _apiFactory.Dispose();
            }
        }
    }
}