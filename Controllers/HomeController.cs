using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using news_project_mvc.Data;
using news_project_mvc.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace news_project_mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, int pageNumber = 1)
        {
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                return RedirectToAction(nameof(Search), new { searchString = searchString, pageNumber = pageNumber });
            }

            _logger.LogInformation("Fetching published articles for the home page. Page: {PageNumber}", pageNumber);

            int pageSize = 6;
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            try
            {
                var query = _context.Articles
                                    .Include(a => a.Category)
                                    .Include(a => a.Author)
                                    .Where(a => a.IsPublished && a.PublishedDate <= DateTime.UtcNow)
                                    .OrderByDescending(a => a.PublishedDate)
                                    .AsQueryable();

                int totalItems = await query.CountAsync();
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                if (pageNumber > totalPages && totalPages > 0)
                {
                    pageNumber = totalPages;
                }
                
                var articlesForPage = await query
                                            .Skip((pageNumber - 1) * pageSize)
                                            .Take(pageSize)
                                            .ToListAsync();

                ViewData["PageNumber"] = pageNumber;
                ViewData["TotalPages"] = totalPages;
                ViewData["HasPreviousPage"] = pageNumber > 1 && totalPages > 0;
                ViewData["HasNextPage"] = pageNumber < totalPages;

                return View(articlesForPage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching articles for home page. Page: {PageNumber}", pageNumber);
                return View(Array.Empty<Article>());
            }
        }        public async Task<IActionResult> Search(string searchString, int pageNumber = 1)
        {
            try
            {
                _logger.LogInformation("Performing search for: '{SearchString}', Page: {PageNumber}", searchString, pageNumber);

                // Lưu trữ từ khóa tìm kiếm gốc để hiển thị trong view
                ViewData["CurrentSearch"] = searchString;

                // Kiểm tra searchString có hợp lệ không
                if (string.IsNullOrWhiteSpace(searchString))
                {
                    return RedirectToAction(nameof(Index));
                }

                // Xử lý searchString để tìm kiếm
                string effectiveSearchString = searchString.Trim();
                
                int pageSize = 6;
                if (pageNumber < 1) 
                {
                    pageNumber = 1;
                }
                
                // Chuẩn bị query
                var query = _context.Articles
                                    .Include(a => a.Category)
                                    .Include(a => a.Author)
                                    .Where(a => a.IsPublished && a.PublishedDate <= DateTime.UtcNow)
                                    .AsQueryable();

                // Tìm kiếm theo các trường (xử lý null trước khi gọi Contains)
                query = query.Where(a =>
                    (a.Title != null && a.Title.Contains(effectiveSearchString)) ||
                    (a.Summary != null && a.Summary.Contains(effectiveSearchString)) ||
                    (a.Content != null && a.Content.Contains(effectiveSearchString)) ||
                    (a.Category != null && a.Category.Name != null && a.Category.Name.Contains(effectiveSearchString))
                );

                query = query.OrderByDescending(a => a.PublishedDate);

                // Đếm tổng số kết quả và tính số trang
                int totalItems = await query.CountAsync();
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                // Điều chỉnh số trang nếu cần
                if (pageNumber > totalPages && totalPages > 0)
                {
                    pageNumber = totalPages;
                }

                // Lấy kết quả cho trang hiện tại
                var searchResults = await query
                                        .Skip((pageNumber - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToListAsync();

                // Đặt thông tin phân trang vào ViewData
                ViewData["PageNumber"] = pageNumber;
                ViewData["TotalPages"] = totalPages;
                ViewData["HasPreviousPage"] = pageNumber > 1;
                ViewData["HasNextPage"] = pageNumber < totalPages;

                _logger.LogInformation("Search complete. Found {ResultCount} results", totalItems);
                
                return View("Index", searchResults);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for articles. Search: '{SearchString}', Page: {PageNumber}", searchString, pageNumber);
                ViewData["CurrentSearch"] = searchString; // Giữ từ khóa tìm kiếm
                return View("Index", Array.Empty<Article>());
            }
        }

        // GET: /Home/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Details action called with null ID.");
                return NotFound();
            }

            _logger.LogInformation("Fetching details for article ID {ArticleId}.", id);
            try
            {
                var article = await _context.Articles
                                            .Include(a => a.Category)
                                            .Include(a => a.Author)
                                            .Where(a => a.IsPublished && a.PublishedDate <= DateTime.UtcNow)
                                            .FirstOrDefaultAsync(m => m.ArticleId == id);

                if (article == null)
                {
                    _logger.LogWarning("Published article with ID {ArticleId} not found.", id);
                    return NotFound();
                }

                article.ViewCount++;
                _context.Update(article);
                await _context.SaveChangesAsync();

                return View(article);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching article details for ID {ArticleId}.", id);
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
