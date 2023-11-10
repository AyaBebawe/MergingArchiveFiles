using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;

namespace MergingArchiveFiles.Services
{
    public class MergeArchiveFilesService : IMergeArchiveFilesService
    {

           // Allowed file extensions for validation
            private readonly string[] AllowedExtensions = { ".zip", ".7z" };
           
           // Logger for logging messages
            private readonly ILogger<MergeArchiveFilesService> _logger;

           // Constructor for initializing the service with a logger
            public MergeArchiveFilesService(ILogger<MergeArchiveFilesService> logger)
            {
                _logger = logger;
            }

            // Method to merge uploaded ZIP files 
            public async Task<IActionResult> MergeZipFiles(List<IFormFile> zipFiles)
            {
                try
                {
                  
                    // Validate uploaded ZIP files
                    if (!ValidateZipFiles(zipFiles, out var validationErrorMessage))
                    {
                        _logger.LogError(validationErrorMessage +" Input: {@zipFiles}", zipFiles);
                        return new BadRequestObjectResult(validationErrorMessage);
                    }
                   // Create a memory stream to store the merged ZIP file
                    using (var memoryStream = new MemoryStream())
                    {   
                       // Merge the files into a new ZIP archive
                        await MergeFilesIntoZipArchive(zipFiles, memoryStream);
                       // Create and return a downloable zip file result
                        return CreateFileResult(memoryStream);
                    }
                }
                catch (Exception ex)
                {
                  //Log Errors in log File
                  LogException(ex, zipFiles);
                  throw new Exception("This should not have happened. We are already working on a solution.");
                }
            }

            // Method to validate uploaded ZIP files
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
           // Method to merge source files into a target ZIP archive
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

                               // Read source ZIP archive and copy entries to the target archive
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


           // Method to copy entries from a source ZIP archive to a target ZIP archive
            private async Task CopyEntriesToNewArchive(ZipArchive sourceArchive, ZipArchive targetArchive)
            {
                foreach (var entry in sourceArchive.Entries)
                {
                    var newEntry = targetArchive.CreateEntry(entry.FullName, CompressionLevel.Optimal);

                    using (var entryStream = entry.Open())
                    using (var newEntryStream = newEntry.Open())
                    {  
                        // Copy entry content to the new archive
                        await entryStream.CopyToAsync(newEntryStream);
                    }
                }
            }

           // Method to create a file result from a memory stream
            private IActionResult CreateFileResult(MemoryStream memoryStream)
            {
                memoryStream.Seek(0, SeekOrigin.Begin);
                var zipBytes = memoryStream.ToArray();
                return new FileContentResult(zipBytes, "application/zip")
                {
                    FileDownloadName = "merged.zip"
                };
            }

           // Method to log exceptions
            private void LogException(Exception ex, List<IFormFile> zipFiles)
            {
                _logger.LogError(ex, ex.Message.ToString() + " Input: {@zipFiles}", zipFiles);
            }
        }
    
}
