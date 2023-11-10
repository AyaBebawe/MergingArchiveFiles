using MergingArchiveFiles.Services;
using Microsoft.AspNetCore.Mvc;


namespace MergingArchiveFiles.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MergeArchiveFilesController : ControllerBase
    {
        private readonly IMergeArchiveFilesService _mergeArchiveService;


        // Constructor to inject the merge archive service
        public MergeArchiveFilesController(IMergeArchiveFilesService mergeArchiveService)
        {
            _mergeArchiveService = mergeArchiveService;
        }
        // Action method for handling HTTP POST requests with multipart form data
        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(3L * 1024 * 1024 * 1024)] // Limits the request size to 3 GB
        public async Task<IActionResult> MergeZipFiles([FromForm] List<IFormFile> zipFiles)
        {
            try
            {
                // Call the service to merge ZIP files
                return await _mergeArchiveService.MergeZipFiles(zipFiles);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "This should not have happened. We are already working on a solution.");
            }
        }
    }
}
