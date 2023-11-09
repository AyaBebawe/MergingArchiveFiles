using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace MergingArchiveFiles.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MergeArchiveFilesController : ControllerBase
    {
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> MergeZipFiles([FromForm] List<IFormFile> zipFiles)
        {
            try
            {
                if (zipFiles == null || zipFiles.Count == 0)
                {
                    return BadRequest("No files were uploaded.");
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
                                    return BadRequest("Invalid ZIP file format.");
                                }
                            }
                        }
                    }

                    memoryStream.Seek(0, SeekOrigin.Begin);
                    var zipBytes = memoryStream.ToArray();

                    // Return the merged ZIP archive as a downloadable file
                    return File(zipBytes, "application/zip", "merged.zip");
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging and troubleshooting
                Console.WriteLine($"Error: {ex.Message}");

                // Return an error response to the client
                return BadRequest("An error occurred while merging ZIP files.");
            }
        }
    }
}
