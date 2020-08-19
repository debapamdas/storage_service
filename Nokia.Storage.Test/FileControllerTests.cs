using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Nokia.Storage.Controllers;
using Nokia.Storage.FileStorage;
using NUnit.Framework;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Nokia.Storage.Test
{
    [TestFixture]
    public class FileControllerTests
    {

        [Test]
        public async Task FileController_ImportFile_Success()
        {
            // setup

            var mockFileInfo = new StoredFileInfo() { FileId ="id", ContentType = "application/x-tar", FileName = "name",Length = 2048};

            Mock<IFileStorage> mockStorage = new Mock<IFileStorage>();
            mockStorage.Setup(s => s.StoreAsync(It.IsAny<IFormFile>(), default))
                .ReturnsAsync(mockFileInfo);

            Mock<ILogger<FileController>> mockLogger = new Mock<ILogger<FileController>>();

            Mock<IFormFile> mockFile = new Mock<IFormFile>();
            mockFile.Setup(s => s.ContentType).Returns(mockFileInfo.ContentType);

            // act
            var controller = new FileController(mockStorage.Object, mockLogger.Object);
            var res = await controller.ImportFile(mockFile.Object, default) as ObjectResult;
            var returnedFileInfo = res.Value as StoredFileInfo;

            // assert
            Assert.AreEqual((int)HttpStatusCode.Created, res.StatusCode);

            mockStorage.Verify(m => m.StoreAsync(It.IsAny<IFormFile>(), default), Times.Once());

            Assert.AreEqual(mockFileInfo.FileName, returnedFileInfo.FileName);
        }

        [Test]
        public async Task FileController_ImportFile_Failue_ForUnsupportedContentType()
        {
            // setup

            var mockFileInfo = new StoredFileInfo() { FileId = "id", ContentType = "application/text", FileName = "name", Length = 2048 };

            Mock<IFileStorage> mockStorage = new Mock<IFileStorage>();
            mockStorage.Setup(s => s.StoreAsync(It.IsAny<IFormFile>(), default))
                .ReturnsAsync(mockFileInfo);

            Mock<ILogger<FileController>> mockLogger = new Mock<ILogger<FileController>>();

            Mock<IFormFile> mockFile = new Mock<IFormFile>();
            mockFile.Setup(s => s.ContentType).Returns(mockFileInfo.ContentType);

            // act
            var controller = new FileController(mockStorage.Object, mockLogger.Object);
            var res = await controller.ImportFile(mockFile.Object, default) as ObjectResult;

            // assert
            Assert.AreEqual((int)HttpStatusCode.BadRequest, res.StatusCode);

            mockStorage.Verify(m => m.StoreAsync(It.IsAny<IFormFile>(), default), Times.Never);
        }

        [Test]
        public async Task FileController_ImportFile_Failure_StorageException()
        {
            // setup

            var mockFileInfo = new StoredFileInfo() { FileId = "id", ContentType = "application/x-tar", FileName = "name", Length = 2048 };

            Mock<IFileStorage> mockStorage = new Mock<IFileStorage>();
            mockStorage.Setup(s => s.StoreAsync(It.IsAny<IFormFile>(), default))
                .Throws<Exception>();

            Mock<ILogger<FileController>> mockLogger = new Mock<ILogger<FileController>>();

            Mock<IFormFile> mockFile = new Mock<IFormFile>();
            mockFile.Setup(s => s.ContentType).Returns(mockFileInfo.ContentType);

            // act
            var controller = new FileController(mockStorage.Object, mockLogger.Object);
            var res = await controller.ImportFile(mockFile.Object, default) as ObjectResult;

            // assert
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, res.StatusCode);

            mockStorage.Verify(m => m.StoreAsync(It.IsAny<IFormFile>(), default), Times.Once);
        }

        [Test]
        public async Task FileController_ListFile_Sucess_ForNoDefaultPageSize()
        {
            // setup
            var DEFAULT_PAGE_SIZE = 10;

            var mockPaginatedFiles = new PaginatedFileInfo() {
                Files = Enumerable.Range(0, DEFAULT_PAGE_SIZE).Select(n => new StoredFileInfo()),
                PageNumber = 1,
                PageSize = DEFAULT_PAGE_SIZE,
                TotalRecords = DEFAULT_PAGE_SIZE
            }; 
            Mock<IFileStorage> mockStorage = new Mock<IFileStorage>();
            mockStorage.Setup(s => s.ListAsync(DEFAULT_PAGE_SIZE, 1, default))
                .ReturnsAsync(mockPaginatedFiles);

            Mock<ILogger<FileController>> mockLogger = new Mock<ILogger<FileController>>();

            // act
            var controller = new FileController(mockStorage.Object, mockLogger.Object);
            var res = await controller.ListFiles(0, 1, default) as ObjectResult;
            var paginatedFiles = res.Value as PaginatedFileInfo;

            // assert
            Assert.AreEqual((int)HttpStatusCode.OK, res.StatusCode);
            Assert.AreEqual(DEFAULT_PAGE_SIZE, paginatedFiles.TotalRecords);
            Assert.AreEqual(DEFAULT_PAGE_SIZE, paginatedFiles.Files.Count());

            mockStorage.Verify(m => m.ListAsync(DEFAULT_PAGE_SIZE, 1, default), Times.Once);
        }

        [Test]
        public async Task FileController_ListFile_Sucess_ForNoDefaultPageNumber()
        {
            // setup
            var DEFAULT_PAGCE_NUMBER = 1;

            var paginatedFileInfo = new PaginatedFileInfo()
            {
                Files = Enumerable.Range(10, 0).Select(n => new StoredFileInfo()),
                PageNumber = DEFAULT_PAGCE_NUMBER,
                PageSize = 10,
                TotalRecords = 10
            };
            Mock<IFileStorage> mockStorage = new Mock<IFileStorage>();
            mockStorage.Setup(s => s.ListAsync(0, 1, default))
                .ReturnsAsync(paginatedFileInfo);

            Mock<ILogger<FileController>> mockLogger = new Mock<ILogger<FileController>>();

            // act
            var controller = new FileController(mockStorage.Object, mockLogger.Object);
            var res = await controller.ListFiles(10, 0, default) as ObjectResult;

            // assert
            Assert.AreEqual((int)HttpStatusCode.OK, res.StatusCode);

            mockStorage.Verify(m => m.ListAsync(10, 1, default), Times.Once);
        }

        [Test]
        public async Task FileController_ListFile_Failure_StorageException()
        {
            // setup
            Mock<IFileStorage> mockStorage = new Mock<IFileStorage>();
            mockStorage.Setup(s => s.ListAsync(It.IsAny<int>(), It.IsAny<int>(), default))
                .Throws<Exception>();

            Mock<ILogger<FileController>> mockLogger = new Mock<ILogger<FileController>>();

            // act
            var controller = new FileController(mockStorage.Object, mockLogger.Object);
            var res = await controller.ListFiles(It.IsAny<int>(), It.IsAny<int>(), default) as ObjectResult;

            // assert
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, res.StatusCode);

            mockStorage.Verify(m => m.ListAsync(It.IsAny<int>(), It.IsAny<int>(), default), Times.Once);
        }

        [Test]
        public async Task FileController_PullFile_Success_ForValidFileId()
        {
            // setup

            var mockStream = new Mock<Stream>();
            var mockFileInfo = new StoredFileInfo() { FileId = "id", ContentType = "application/x-tar", FileName = "name", Length = 2048 };

            Mock<IFileStorage> mockStorage = new Mock<IFileStorage>();
            mockStorage.Setup(s => s.GetAsync(It.IsAny<string>(), default)).ReturnsAsync((mockStream.Object,mockFileInfo));

            Mock<ILogger<FileController>> mockLogger = new Mock<ILogger<FileController>>();

            // act
            var controller = new FileController(mockStorage.Object, mockLogger.Object);
            var res = await controller.PullFile("id", default) as FileStreamResult;

            // assert
            Assert.AreEqual("application/x-tar", res.ContentType);
            Assert.AreEqual(mockStream.Object, res.FileStream);
            Assert.AreEqual(res.FileDownloadName, mockFileInfo.FileName);

            mockStorage.Verify(m => m.GetAsync("id", default), Times.Once);
        }

        [Test]
        public async Task FileController_PullFile_Failure_ForInValidFileId()
        {
            // setup

            Mock<IFileStorage> mockStorage = new Mock<IFileStorage>();
            mockStorage.Setup(s => s.GetAsync(It.IsAny<string>(), default)).ReturnsAsync((null, null));

            Mock<ILogger<FileController>> mockLogger = new Mock<ILogger<FileController>>();

            // act
            var controller = new FileController(mockStorage.Object, mockLogger.Object);
            var res = await controller.PullFile("id", default) as ObjectResult;

            // assert
            Assert.AreEqual((int)HttpStatusCode.NotFound, res.StatusCode);

            mockStorage.Verify(m => m.GetAsync("id", default), Times.Once);
        }

        [Test]
        public async Task FileController_PullFile_Failure_ForStorageException()
        {
            // setup

            Mock<IFileStorage> mockStorage = new Mock<IFileStorage>();
            mockStorage.Setup(s => s.GetAsync(It.IsAny<string>(), default)).Throws<Exception>();

            Mock<ILogger<FileController>> mockLogger = new Mock<ILogger<FileController>>();

            // act
            var controller = new FileController(mockStorage.Object, mockLogger.Object);
            var res = await controller.PullFile("id", default) as ObjectResult;

            // assert
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, res.StatusCode);

            mockStorage.Verify(m => m.GetAsync("id", default), Times.Once);
        }
    }
}