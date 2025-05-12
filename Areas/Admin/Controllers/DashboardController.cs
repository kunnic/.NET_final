using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using news_project_mvc.Data;
using System.Linq;
using System.Threading.Tasks;

namespace news_project_mvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get dashboard statistics
            ViewBag.ArticleCount = await _context.Articles.CountAsync();
            ViewBag.CategoryCount = await _context.Categories.CountAsync();
            ViewBag.TotalViews = await _context.Articles.SumAsync(a => a.ViewCount);
            
            // Get recent published articles
            ViewBag.RecentArticles = await _context.Articles
                .Include(a => a.Category)
                .Include(a => a.Author)
                .OrderByDescending(a => a.PublishedDate)
                .Take(5)
                .ToListAsync();

            return View();
        }
        
        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return RedirectToAction(nameof(Index));
            }

            ViewData["SearchTerm"] = searchTerm;

            var searchResults = await _context.Articles
                .Include(a => a.Category)
                .Include(a => a.Author)
                .Where(a => 
                    (a.Title != null && a.Title.Contains(searchTerm)) || 
                    (a.Content != null && a.Content.Contains(searchTerm)) ||
                    (a.Summary != null && a.Summary.Contains(searchTerm)) ||
                    (a.Category != null && a.Category.Name != null && a.Category.Name.Contains(searchTerm)))
                .OrderByDescending(a => a.PublishedDate)
                .ToListAsync();

            return View(searchResults);
        }
        
        public IActionResult UITest()
        {
            // This action is used to test the UI styling for the admin area
            return View();
        }
    }
}