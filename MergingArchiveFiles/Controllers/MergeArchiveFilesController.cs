using MergingArchiveFiles.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace MergingArchiveFiles.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MergeArchiveFilesController : ControllerBase
    {
        private readonly IMergeArchiveFilesService _mergeArchiveService;

        public MergeArchiveFilesController(IMergeArchiveFilesService mergeArchiveService)
        {
            _mergeArchiveService = mergeArchiveService;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(3L * 1024 * 1024 * 1024)] // 3 GB
        public async Task<IActionResult> MergeZipFiles([FromForm] List<IFormFile> zipFiles)
        {
            try
            {
              return  await _mergeArchiveService.MergeZipFiles(zipFiles);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "This should not have happened. We are already working on a solution.");
            }
        }
    }
}
