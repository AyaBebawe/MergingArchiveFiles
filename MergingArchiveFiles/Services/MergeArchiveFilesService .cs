using Microsoft.AspNetCore.Mvc;
using static MergingArchiveFiles.Services.MergeArchiveFilesService;
using System.IO.Compression;

namespace MergingArchiveFiles.Services
{
    public class MergeArchiveFilesService : IMergeArchiveFilesService
    {
       
            private readonly string[] AllowedExtensions = { ".zip", ".7z" };
            private readonly ILogger<MergeArchiveFilesService> _logger;

            public MergeArchiveFilesService(ILogger<MergeArchiveFilesService> logger)
            {
                _logger = logger;
            }

            public async Task<IActionResult> MergeZipFiles(List<IFormFile> zipFiles)
            {
                try
                {
                
                    if (!ValidateZipFiles(zipFiles, out var validationErrorMessage))
                    {
                        _logger.LogError(validationErrorMessage +" Input: {@zipFiles}", zipFiles);
                        return new BadRequestObjectResult(validationErrorMessage);
                    }
                
                    using (var memoryStream = new MemoryStream())
                    {
                        await MergeFilesIntoZipArchive(zipFiles, memoryStream);
                        return CreateFileResult(memoryStream);
                    }
                }
                catch (Exception ex)
                {
                  LogAndHandleException(ex, zipFiles);
                  throw new Exception("This should not have happened. We are already working on a solution.");
                }
            }

            private bool ValidateZipFiles(List<IFormFile> files, out string errorMessage)
            {
                if (files == null || files.Count == 0)
                {
                    errorMessage = "No files were uploaded.";
                    return false;
                }
            if ( files.Count == 1)
            {
                errorMessage = "At Least 2 Files Should be uploaded";
                return false;
            }

            if (!files.All(file => AllowedExtensions.Contains(Path.GetExtension(file.FileName))))
                {
                    errorMessage = "Invalid file type. Only ZIP files are allowed.";
                    return false;
                }

                errorMessage = null;
                return true;
            }

            private async Task MergeFilesIntoZipArchive(List<IFormFile> sourceFiles, MemoryStream targetStream)
            {
                using (var archive = new ZipArchive(targetStream, ZipArchiveMode.Create, leaveOpen: true))
                {
                    foreach (var sourceFile in sourceFiles)
                    {
                        using (var sourceStream = sourceFile.OpenReadStream())
                        {
                            try
                            {
                                using (var sourceArchive = new ZipArchive(sourceStream, ZipArchiveMode.Read))
                                {
                                    await CopyEntriesToNewArchive(sourceArchive, archive);
                                }
                            }
                            catch (InvalidDataException)
                            {
                                throw new InvalidDataException("Invalid ZIP file format.");
                            }
                        }
                    }
                }
            }

            private async Task CopyEntriesToNewArchive(ZipArchive sourceArchive, ZipArchive targetArchive)
            {
                foreach (var entry in sourceArchive.Entries)
                {
                    var newEntry = targetArchive.CreateEntry(entry.FullName, CompressionLevel.Optimal);

                    using (var entryStream = entry.Open())
                    using (var newEntryStream = newEntry.Open())
                    {
                        await entryStream.CopyToAsync(newEntryStream);
                    }
                }
            }

            private IActionResult CreateFileResult(MemoryStream memoryStream)
            {
                memoryStream.Seek(0, SeekOrigin.Begin);
                var zipBytes = memoryStream.ToArray();
                return new FileContentResult(zipBytes, "application/zip")
                {
                    FileDownloadName = "merged.zip"
                };
            }

            private void LogAndHandleException(Exception ex, List<IFormFile> zipFiles)
            {
                _logger.LogError(ex, ex.Message.ToString() + " Input: {@zipFiles}", zipFiles);
            }
        }
    
}
