using Microsoft.AspNetCore.Mvc;

namespace news_project_mvc.Controllers
{
    public class CategoriesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult getCategories()
        {
            return Ok();
        }
    }
}
