using Microsoft.AspNetCore.Mvc;
using ABC_Retail_StorageApp.Services;
using System.Threading.Tasks;

namespace ABC_Retail_StorageApp.Controllers
{
    public class QueueController : Controller
    {
        private readonly StorageService _storageService;

        public QueueController(StorageService storageService)
        {
            _storageService = storageService;
        }

        public async Task<IActionResult> Index()
        {
            var messages = await _storageService.ListQueueMessagesAsync();
            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> AddMessage(string text)
        {
            await _storageService.AddMessageToQueueAsync(text);
            return RedirectToAction("Index");
        }
    }
}
