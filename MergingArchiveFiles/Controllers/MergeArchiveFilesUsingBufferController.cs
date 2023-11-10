
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
    public class MergeArchiveFilesUsingBufferController : ControllerBase
    {
        private const int BufferSize = 8192; 

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
                                                await ProcessEntryInChunks(entryStream, newEntryStream);
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

                    return File(zipBytes, "application/zip", "merged.zip");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");

                return BadRequest("An error occurred while merging ZIP files.");
            }
        }

        private static async Task ProcessEntryInChunks(Stream sourceStream, Stream targetStream)
        {
            var buffer = new byte[BufferSize];
            int bytesRead;

            while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await targetStream.WriteAsync(buffer, 0, bytesRead);
            }
        }
    }
}
