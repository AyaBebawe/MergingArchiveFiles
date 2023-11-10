using Microsoft.AspNetCore.Mvc;

namespace MergingArchiveFiles.Services
{
    // Service Interface
    public interface IMergeArchiveFilesService
    {
        Task<IActionResult> MergeZipFiles(List<IFormFile> zipFiles);
    }
}
