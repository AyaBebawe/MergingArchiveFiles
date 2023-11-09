﻿using Microsoft.AspNetCore.Mvc;

using System.IO.Compression;


namespace MergingArchiveFiles.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MergeArchiveFilesController : ControllerBase
    {
        private readonly string[] AllowedExtensions = { ".zip",".7z" };
        private readonly ILogger<MergeArchiveFilesController> _logger; // Inject the ILogger
        // Inject the ILogger in the controller's constructor
        public MergeArchiveFilesController(ILogger<MergeArchiveFilesController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(3L * 1024 * 1024 * 1024)] // 3 GB
        public async Task<IActionResult> MergeZipFiles([FromForm] List<IFormFile> zipFiles)
        {
            try
            {
                if (zipFiles == null || zipFiles.Count == 0)
                {
                    _logger.LogError("No files were uploaded.");
                    return BadRequest("No files were uploaded.");
                }

                // Validate file types
                if (!ValidateFileTypes(zipFiles))
                {
                    _logger.LogError("Invalid file type. Only ZIP files are allowed.Input: {@zipFiles}", zipFiles);

                    return StatusCode(StatusCodes.Status415UnsupportedMediaType,"Invalid file type. Only ZIP files are allowed.");
                }

                using (var memoryStream = new MemoryStream())
                {
                    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
                    {
                        foreach (var zipFileModel in zipFiles)
                        {
                            using (var stream = zipFileModel.OpenReadStream())
                            {
                                try
                                {
                                    using (var sourceArchive = new ZipArchive(stream, ZipArchiveMode.Read))
                                    {
                                        foreach (var entry in sourceArchive.Entries)
                                        {
                                            var newEntry = archive.CreateEntry(entry.FullName, CompressionLevel.Optimal);

                                            using (var entryStream = entry.Open())
                                            using (var newEntryStream = newEntry.Open())
                                            {
                                                await entryStream.CopyToAsync(newEntryStream);
                                            }
                                        }
                                    }
                                }
                                catch (InvalidDataException)
                                {
                                    _logger.LogError("Invalid ZIP file format.Input: {@zipFiles}", zipFiles);

                                    return StatusCode(StatusCodes.Status415UnsupportedMediaType,"Invalid ZIP file format.");
                                }
                            }
                        }
                    }

                    // Store the merged ZIP file securely, for example, in a secure storage service.

                    memoryStream.Seek(0, SeekOrigin.Begin);
                    var zipBytes = memoryStream.ToArray();

                    // Return the merged ZIP archive as a downloadable file
                    return File(zipBytes, "application/zip", "merged.zip");
                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, ex.Message.ToString() + "Input: {@zipFiles}", zipFiles);
                // Return an error response to the client
                return StatusCode(StatusCodes.Status500InternalServerError, "This should not have happened. We are already working on a solution.");
            }
        }

        private bool ValidateFileTypes(List<IFormFile> files)
        {
            return files.All(file => AllowedExtensions.Contains(Path.GetExtension(file.FileName)));
        }
    }
}
