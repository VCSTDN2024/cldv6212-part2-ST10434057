using Microsoft.AspNetCore.Mvc;
using ABC_Retail_StorageApp.Models;
using ABC_Retail_StorageApp.Services;
using System.Threading.Tasks;

namespace ABC_Retail_StorageApp.Controllers
{
    public class TableController : Controller
    {
        private readonly StorageService _storageService;

        public TableController(StorageService storageService)
        {
            _storageService = storageService;
        }

        public async Task<IActionResult> Index()
        {
            var entities = await _storageService.GetTableEntitiesAsync();
            return View(entities);
        }

        [HttpPost]
        [Route("Table/Add")]
        public async Task<IActionResult> Add(TableEntityModel model)
        {
            if (ModelState.IsValid)
            {
                await _storageService.AddEntityToTableAsync(model);
                return RedirectToAction("Index");
            }
            return View("Index", await _storageService.GetTableEntitiesAsync());
        }
    }
}
