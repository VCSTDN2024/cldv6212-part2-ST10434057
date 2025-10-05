using Microsoft.AspNetCore.Mvc;
using ABC_Retail_StorageApp.Models;
using ABC_Retail_StorageApp.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ABC_Retail_StorageApp.Controllers
{
    public class BlobController : Controller
    {
        private readonly StorageService _storageService;

        public BlobController(StorageService storageService)
        {
            _storageService = storageService;
        }

        // Display list of blobs
        public async Task<IActionResult> Index()
        {
            var blobs = await _storageService.GetBlobsAsync();

            var model = new List<BlobModel>();
            foreach (var (name, url) in blobs)
            {
                model.Add(new BlobModel
                {
                    BlobName = name,
                    BlobUrl = url
                });
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UploadBlob(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                await _storageService.UploadBlobAsync(file);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBlob(string blobName)
        {
            if (!string.IsNullOrEmpty(blobName))
            {
                await _storageService.DeleteBlobAsync(blobName);
            }

            return RedirectToAction("Index");
        }
    }
}
