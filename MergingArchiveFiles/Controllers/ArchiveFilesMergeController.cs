using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

[Route("api/archive")]
[ApiController]
public class MergingArchiveFilesController : ControllerBase
{
    [HttpPost("merge")]
    public async Task<IActionResult> MergeArchivesAsync([FromForm] List<IFormFile> files)
    {
        try
        {
            if (files == null || files.Count < 2)
            {
                return BadRequest("You need at least two ZIP files to merge.");
            }

            using (var mergedStream = new MemoryStream())
            using (var zipStream = new ZipOutputStream(mergedStream))
            {
                foreach (var file in files)
                {
                    using (var fileStream = file.OpenReadStream())
                    using (var subZipStream = new ZipInputStream(fileStream))
                    {
                        ZipEntry entry;
                        while ((entry = subZipStream.GetNextEntry()) != null)
                        {
                            if (!entry.IsDirectory)
                            {
                                var entryName = entry.Name;

                                // Specify UTF-8 encoding when decoding entry names
                                var entryBytes = Encoding.UTF8.GetBytes(entryName);
                                entryName = Encoding.UTF8.GetString(entryBytes);

                                var newEntry = new ZipEntry(entryName);
                                zipStream.PutNextEntry(newEntry);

                                await subZipStream.CopyToAsync(zipStream);
                            }
                        }
                    }
                }

                zipStream.Finish();

                mergedStream.Seek(0, SeekOrigin.Begin);

                return File(mergedStream, "application/zip", "MergedArchive.zip");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}
