using Microsoft.AspNetCore.Mvc;
using ABC_Retail_StorageApp.Models;
using ABC_Retail_StorageApp.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABC_Retail_StorageApp.Controllers
{
    public class FileShareController : Controller
    {
        private readonly StorageService _storageService;

        public FileShareController(StorageService storageService)
        {
            _storageService = storageService;
        }

        public async Task<IActionResult> Index()
        {
            // Get list of filenames from Azure
            var fileNames = await _storageService.ListFileShareFilesAsync();

            // Convert List<string> → List<FileShareModel>
            var fileList = new List<FileShareModel>();
            foreach (var name in fileNames)
            {
                fileList.Add(new FileShareModel { FileName = name });
            }

            return View(fileList);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                await _storageService.UploadFileToFileShareAsync(file);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFile(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                await _storageService.DeleteFileFromFileShareAsync(fileName);
            }

            return RedirectToAction("Index");
        }
    }
}