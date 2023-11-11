using MergingArchiveFiles.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO.Compression;

namespace MergingArchiveFilesTests
{
    [TestClass]
    public class MergeArchivingFilesServicesTests
    {
        [TestMethod]
        public async Task MergeZipFiles_WithValidFiles_ReturnsFileResult()
        {
            // Mocking List of IformFiles
            var loggerMock = new Mock<ILogger<MergeArchiveFilesService>>();
            var mergeArchiveService = new MergeArchiveFilesService(loggerMock.Object);
            var zipFiles = new List<IFormFile>
        {
            MockIFormFile("file1.zip"),
            MockIFormFile("file2.zip")
        };

            
            var result = await mergeArchiveService.MergeZipFiles(zipFiles);

            // Assert
            Assert.IsInstanceOfType(result, typeof(FileContentResult));
            var fileResult = (FileContentResult)result;
            Assert.AreEqual("application/zip", fileResult.ContentType);
            Assert.AreEqual("merged.zip", fileResult.FileDownloadName);
        }


        [TestMethod]
        public async Task MergeZipFiles_WithInvalidFiles_ReturnsBadRequest()
        {
            
            var loggerMock = new Mock<ILogger<MergeArchiveFilesService>>();
            var mergeArchiveService = new MergeArchiveFilesService(loggerMock.Object);
            var zipFiles = new List<IFormFile>
        {
            
            MockIFormFile("file1.txt"),
            MockIFormFile("file2.zip")
        };

            
            var result = await mergeArchiveService.MergeZipFiles(zipFiles);

            
            Assert.IsInstanceOfType(result,typeof(BadRequestObjectResult));
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.IsInstanceOfType(badRequestResult.Value, typeof(string));
            Assert.AreEqual("Invalid file type. Only ZIP files are allowed.", badRequestResult.Value);
        }

        [TestMethod]
        public async Task MergeZipFiles_WithInvalidNumberOfFiles_ReturnsBadRequest()
        {
            
            var loggerMock = new Mock<ILogger<MergeArchiveFilesService>>();
            var mergeArchiveService = new MergeArchiveFilesService(loggerMock.Object);
            var zipFiles = new List<IFormFile>
        {
            
            MockIFormFile("file1.zip"),
   
        };

            
            var result = await mergeArchiveService.MergeZipFiles(zipFiles);

            
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.IsInstanceOfType(badRequestResult.Value, typeof(string));
            Assert.AreEqual("At Least 2 Files Should be uploaded", badRequestResult.Value);
        }

        [TestMethod]
        public async Task MergeZipFiles_WithNoUploadedFiles_ReturnsBadRequest()
        {
           
            var loggerMock = new Mock<ILogger<MergeArchiveFilesService>>();
            var mergeArchiveService = new MergeArchiveFilesService(loggerMock.Object);
            var zipFiles = new List<IFormFile>();
        

            
            var result = await mergeArchiveService.MergeZipFiles(zipFiles);

            
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.IsInstanceOfType(badRequestResult.Value, typeof(string));
            Assert.AreEqual("No files were uploaded.", badRequestResult.Value);
        }

        private static IFormFile MockIFormFile(string fileName)
        {
            var fileMock = new Mock<IFormFile>();
            var stream = new MemoryStream();

            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                var entry = archive.CreateEntry("sample.txt");
                using (var entryStream = entry.Open())
                using (var writer = new StreamWriter(entryStream))
                {
                    writer.Write("Mock file content");
                }
            }

            stream.Position = 0;

            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(stream.Length);
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

            return fileMock.Object;
        }

    }





       
}