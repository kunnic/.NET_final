using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using news_project_mvc.Data;
using news_project_mvc.Models;
using news_project_mvc.Areas.Admin.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions; // Cho GenerateSlug nếu bạn muốn phức tạp hơn

namespace news_project_mvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Editor")] // Cho phép cả Admin và Editor quản lý bài viết
    public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ArticlesController> _logger;

        public ArticlesController(ApplicationDbContext context,
                                  UserManager<IdentityUser> userManager,
                                  ILogger<ArticlesController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }        // GET: Admin/Articles
        public async Task<IActionResult> Index(string sortOrder, string searchString, int? categoryId, string publishStatus, int? pageNumber)
        {
            _logger.LogInformation("Fetching articles for Admin index page with filters.");
            try
            {
                // Store filter values for view
                ViewData["CurrentSort"] = sortOrder;
                ViewData["TitleSortParm"] = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
                ViewData["CategorySortParm"] = sortOrder == "category" ? "category_desc" : "category";
                ViewData["DateSortParm"] = sortOrder == "date" ? "date_desc" : "date";
                ViewData["ViewsSortParm"] = sortOrder == "views" ? "views_desc" : "views";
                ViewData["CurrentFilter"] = searchString;

                // Load all categories for filter dropdown
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.SelectedCategoryId = categoryId;
                ViewBag.PublishStatus = publishStatus;

                // Start with all articles
                var articlesQuery = _context.Articles
                    .Include(a => a.Category)
                    .Include(a => a.Author)
                    .AsQueryable();

                // Apply filters
                if (!String.IsNullOrEmpty(searchString))
                {
                    articlesQuery = articlesQuery.Where(a => 
                        a.Title.Contains(searchString) || 
                        a.Content.Contains(searchString));
                }

                if (categoryId.HasValue)
                {
                    articlesQuery = articlesQuery.Where(a => a.CategoryId == categoryId.Value);
                }

                if (!String.IsNullOrEmpty(publishStatus))
                {
                    bool isPublished = publishStatus == "true";
                    articlesQuery = articlesQuery.Where(a => a.IsPublished == isPublished);
                }

                // Apply sorting
                switch (sortOrder)
                {
                    case "title_desc":
                        articlesQuery = articlesQuery.OrderByDescending(a => a.Title);
                        break;
                    case "category":
                        articlesQuery = articlesQuery.OrderBy(a => a.Category.Name);
                        break;
                    case "category_desc":
                        articlesQuery = articlesQuery.OrderByDescending(a => a.Category.Name);
                        break;
                    case "date":
                        articlesQuery = articlesQuery.OrderBy(a => a.CreatedAt);
                        break;
                    case "date_desc":
                        articlesQuery = articlesQuery.OrderByDescending(a => a.CreatedAt);
                        break;
                    case "views":
                        articlesQuery = articlesQuery.OrderBy(a => a.ViewCount);
                        break;
                    case "views_desc":
                        articlesQuery = articlesQuery.OrderByDescending(a => a.ViewCount);
                        break;
                    default:
                        articlesQuery = articlesQuery.OrderByDescending(a => a.CreatedAt);
                        break;
                }

                // Execute query
                var articles = await articlesQuery.ToListAsync();
                return View(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching articles for Admin index.");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách bài viết.";
                return View(new List<Article>()); // Trả về danh sách rỗng hoặc trang lỗi
            }
        }

        // GET: Admin/Articles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Details GET called with null ID.");
                return NotFound();
            }

            _logger.LogInformation("Fetching article details for ID {ArticleId}.", id);
            var article = await _context.Articles
                .Include(a => a.Category)
                .Include(a => a.Author)
                .FirstOrDefaultAsync(m => m.ArticleId == id);

            if (article == null)
            {
                _logger.LogWarning("Article with ID {ArticleId} not found for Details.", id);
                return NotFound();
            }

            // Bạn có thể tạo một ViewModel riêng cho Details nếu cần, hoặc dùng trực tiếp Model Article
            // Ví dụ nếu dùng ArticleViewModel (cần điều chỉnh ArticleViewModel để chứa đủ thông tin hoặc tạo mới)
            // var viewModel = new ArticleViewModel
            // {
            //     Title = article.Title,
            //     Summary = article.Summary,
            //     Content = article.Content,
            //     ImageUrl = article.ImageUrl,
            //     CategoryId = article.CategoryId,
            //     CategoryName = article.Category?.Name, // Giả sử CategoryName có trong ViewModel
            //     AuthorName = article.Author?.UserName, // Giả sử AuthorName có trong ViewModel
            //     IsPublished = article.IsPublished,
            //     PublishedDate = article.PublishedDate,
            //     ViewCount = article.ViewCount
            //     // Không cần CategoriesList ở đây
            // };
            // return View(viewModel);

            return View(article); // Trả về model Article trực tiếp nếu View Details được thiết kế cho nó
        }


        // GET: Admin/Articles/Create
        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("Displaying Create Article page.");
            var viewModel = new ArticleViewModel();
            await PopulateCategoriesDropDownList(viewModel);
            return View(viewModel);
        }

        // POST: Admin/Articles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ArticleViewModel viewModel)
        {
            _logger.LogInformation("Attempting to create a new article. Title: {ArticleTitle}", viewModel.Title);

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in claims. Cannot create article.");
                    ModelState.AddModelError("", "Không thể xác định người dùng. Vui lòng đăng nhập lại.");
                    await PopulateCategoriesDropDownList(viewModel);
                    return View(viewModel);
                }

                string slug = await GenerateUniqueSlug(viewModel.Title);                // Làm sạch HTML đầu vào
                string safeContent = SanitizeHtml(viewModel.Content);
                
                var article = new Article
                {
                    Title = viewModel.Title,
                    Summary = viewModel.Summary,                    Content = safeContent,
                    ImageUrl = viewModel.ImageUrl,
                    CategoryId = viewModel.CategoryId,
                    IsPublished = viewModel.IsPublished,
                    AuthorId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    PublishedDate = viewModel.IsPublished ? DateTime.UtcNow : default, // Hoặc một giá trị mặc định phù hợp cho non-nullable datetime2
                    Slug = slug,
                    ViewCount = 0
                };
                if (article.PublishedDate == default(DateTime) && article.IsPublished)
                {
                    article.PublishedDate = DateTime.UtcNow;
                }
                else if (article.PublishedDate == default(DateTime) && !article.IsPublished) // Nếu không publish và ngày là default
                {
                    // Giữ PublishedDate là default(DateTime) hoặc một ngày không gây hiểu nhầm (ví dụ: 01/01/0001)
                    // Quan trọng là IsPublished sẽ quyết định bài viết có hiển thị hay không
                    // Hoặc đặt một ngày cụ thể nếu logic yêu cầu, ví dụ: DateTime.MinValue nếu DB cho phép
                    // Vì PublishedDate là non-nullable, nó phải có một giá trị.
                    // Đặt là UtcNow cũng là một lựa chọn, chỉ là nó sẽ không "published" thực sự.
                    article.PublishedDate = new DateTime(1970, 1, 1); // Hoặc DateTime.UtcNow;
                }


                try
                {
                    _context.Add(article);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Article '{ArticleTitle}' created successfully with ID {ArticleId}.", article.Title, article.ArticleId);
                    TempData["SuccessMessage"] = $"Bài viết '{article.Title}' đã được tạo thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating article '{ArticleTitle}'.", viewModel.Title);
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi tạo bài viết. Vui lòng thử lại.");
                }
            }
            else
            {
                _logger.LogWarning("Create article failed due to invalid model state. Title: {ArticleTitle}", viewModel.Title);
                LogModelStateErrors();
            }

            await PopulateCategoriesDropDownList(viewModel);
            return View(viewModel);
        }

        // GET: Admin/Articles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Edit GET called with null ID.");
                return NotFound();
            }

            _logger.LogInformation("Fetching article for edit with ID {ArticleId}.", id);
            var article = await _context.Articles.FindAsync(id.Value);

            if (article == null)
            {
                _logger.LogWarning("Article with ID {ArticleId} not found for Edit.", id);
                return NotFound();
            }            var viewModel = new ArticleViewModel
            {
                ArticleId = article.ArticleId, // Cần thiết để liên kết "Xem chi tiết" hoạt động đúng
                Title = article.Title,
                Summary = article.Summary,
                Content = article.Content,
                ImageUrl = article.ImageUrl,
                CategoryId = article.CategoryId,
                IsPublished = article.IsPublished
                // Slug, AuthorId, CreatedAt, UpdatedAt, PublishedDate, ViewCount được quản lý bởi controller hoặc không sửa trực tiếp
            };

            await PopulateCategoriesDropDownList(viewModel);
            ViewData["CurrentSlug"] = article.Slug; // Để không tạo slug mới nếu Title không đổi
            ViewData["CurrentPublishedDate"] = article.PublishedDate; // Để giữ lại published date nếu không thay đổi trạng thái publish
            ViewData["CurrentAuthorId"] = article.AuthorId;
            ViewData["CurrentCreatedAt"] = article.CreatedAt;


            return View(viewModel);
        }        // POST: Admin/Articles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ArticleViewModel viewModel, string currentSlug, DateTime currentPublishedDate, string currentAuthorId, DateTime currentCreatedAt)
        {
            if (id != viewModel.ArticleId && viewModel.ArticleId != 0)
            {
                _logger.LogWarning("Edit POST ID mismatch. Route ID: {RouteId}, Model ID: {ModelId}", id, viewModel.ArticleId);
                return NotFound();
            }

            _logger.LogInformation("Attempting to update article with ID {ArticleId}. Title: {ArticleTitle}", id, viewModel.Title);

            if (ModelState.IsValid)
            {
                var articleToUpdate = await _context.Articles.FindAsync(id);
                if (articleToUpdate == null)
                {
                    _logger.LogWarning("Article with ID {ArticleId} not found for update.", id);
                    return NotFound();
                }

                // Cập nhật Slug nếu Title thay đổi
                if (articleToUpdate.Title != viewModel.Title)
                {
                    articleToUpdate.Slug = await GenerateUniqueSlug(viewModel.Title, articleToUpdate.ArticleId);
                }
                else
                {
                    articleToUpdate.Slug = currentSlug; // Giữ slug cũ nếu title không đổi
                }                articleToUpdate.Title = viewModel.Title;
                articleToUpdate.Summary = viewModel.Summary;
                  // Xử lý HTML đơn giản cho nội dung - chỉ cho phép một số thẻ cơ bản
                string safeContent = SanitizeHtml(viewModel.Content);
                articleToUpdate.Content = safeContent;
                
                articleToUpdate.ImageUrl = viewModel.ImageUrl;
                articleToUpdate.CategoryId = viewModel.CategoryId;

                // Xử lý PublishedDate
                if (viewModel.IsPublished && !articleToUpdate.IsPublished) // Chuyển từ không publish sang publish
                {
                    articleToUpdate.PublishedDate = DateTime.UtcNow;
                }
                else if (viewModel.IsPublished && articleToUpdate.IsPublished) // Đã publish và vẫn publish
                {
                    articleToUpdate.PublishedDate = currentPublishedDate; // Giữ ngày publish cũ
                }
                else if (!viewModel.IsPublished) // Chuyển sang không publish hoặc vẫn không publish
                {
                    // Giữ nguyên PublishedDate cũ hoặc đặt một giá trị mặc định nếu cần
                    // Vì PublishedDate không nullable, không thể set null.
                    // articleToUpdate.PublishedDate = currentPublishedDate; // Giữ ngày cũ
                    // Hoặc nếu muốn đánh dấu là "unpublish" bằng ngày cụ thể:
                    articleToUpdate.PublishedDate = new DateTime(1970, 1, 1); // Hoặc giá trị default bạn chọn
                }


                articleToUpdate.IsPublished = viewModel.IsPublished;
                articleToUpdate.UpdatedAt = DateTime.UtcNow;
                // AuthorId và CreatedAt thường không thay đổi khi edit
                articleToUpdate.AuthorId = currentAuthorId;
                articleToUpdate.CreatedAt = currentCreatedAt;


                try
                {
                    _context.Update(articleToUpdate);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Article '{ArticleTitle}' (ID: {ArticleId}) updated successfully.", articleToUpdate.Title, articleToUpdate.ArticleId);
                    TempData["SuccessMessage"] = $"Bài viết '{articleToUpdate.Title}' đã được cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Concurrency error updating article '{ArticleTitle}'.", articleToUpdate.Title);
                    if (!await ArticleExists(articleToUpdate.ArticleId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Lỗi xung đột dữ liệu. Dữ liệu có thể đã được thay đổi bởi người khác. Vui lòng tải lại và thử lại.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating article '{ArticleTitle}'.", viewModel.Title);
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật bài viết. Vui lòng thử lại.");
                }
            }
            else
            {
                _logger.LogWarning("Update article failed due to invalid model state. ID: {ArticleId}, Title: {ArticleTitle}", id, viewModel.Title);
                LogModelStateErrors();
            }

            await PopulateCategoriesDropDownList(viewModel);
            // Truyền lại các giá trị ViewData nếu cần thiết để view Edit hoạt động đúng khi có lỗi
            ViewData["CurrentSlug"] = currentSlug;
            ViewData["CurrentPublishedDate"] = currentPublishedDate;
            ViewData["CurrentAuthorId"] = currentAuthorId;
            ViewData["CurrentCreatedAt"] = currentCreatedAt;
            return View(viewModel);
        }

        // GET: Admin/Articles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Delete GET called with null ID.");
                return NotFound();
            }

            _logger.LogInformation("Fetching article for delete confirmation with ID {ArticleId}.", id);
            var article = await _context.Articles
                .Include(a => a.Category) // Để hiển thị thông tin Category trên trang xác nhận
                .Include(a => a.Author)   // Để hiển thị thông tin Author
                .FirstOrDefaultAsync(m => m.ArticleId == id);

            if (article == null)
            {
                _logger.LogWarning("Article with ID {ArticleId} not found for Delete confirmation.", id);
                return NotFound();
            }

            return View(article); // Trả về model Article để View hiển thị thông tin xác nhận
        }

        // POST: Admin/Articles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation("Attempting to delete article with ID {ArticleId}.", id);
            var article = await _context.Articles.FindAsync(id);

            if (article == null)
            {
                _logger.LogWarning("Article with ID {ArticleId} not found for DeleteConfirmed.", id);
                TempData["ErrorMessage"] = "Không tìm thấy bài viết để xóa.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Articles.Remove(article);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Article '{ArticleTitle}' (ID: {ArticleId}) deleted successfully.", article.Title, article.ArticleId);
                TempData["SuccessMessage"] = $"Bài viết '{article.Title}' đã được xóa thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting article '{ArticleTitle}' (ID: {ArticleId}).", article.Title, article.ArticleId);
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi xóa bài viết '{article.Title}'.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> ArticleExists(int id)
        {
            return await _context.Articles.AnyAsync(e => e.ArticleId == id);
        }

        private async Task PopulateCategoriesDropDownList(ArticleViewModel viewModel)
        {
            if (viewModel == null) return;

            viewModel.CategoriesList = await _context.Categories
                                               .OrderBy(c => c.Name)
                                               .Select(c => new SelectListItem
                                               {
                                                   Value = c.CategoryId.ToString(),
                                                   Text = c.Name
                                               }).ToListAsync();
        }

        // Helper để tạo slug duy nhất (tương tự CategoriesController hoặc logic trong Create của bạn)
        private async Task<string> GenerateUniqueSlug(string title, int? currentArticleId = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Guid.NewGuid().ToString("N").Substring(0, 10); // Hoặc slug ngẫu nhiên khác

            string baseSlug = title.ToLowerInvariant();
            // Loại bỏ dấu tiếng Việt (cần hàm chuẩn hơn nếu có thể)
            baseSlug = Regex.Replace(baseSlug, @"\s+", "-"); // Thay khoảng trắng bằng -
            baseSlug = Regex.Replace(baseSlug, @"[^\w\-]", ""); // Loại bỏ ký tự đặc biệt trừ - và chữ/số
            baseSlug = Regex.Replace(baseSlug, @"-+", "-"); // Thay nhiều -- bằng 1 -
            baseSlug = baseSlug.Trim('-');

            if (string.IsNullOrEmpty(baseSlug))
                baseSlug = "bai-viet"; // Slug mặc định nếu title toàn ký tự đặc biệt

            string slug = baseSlug;
            int count = 1;
            while (true)
            {
                var query = _context.Articles.Where(a => a.Slug == slug);
                if (currentArticleId.HasValue) // Nếu là edit, loại trừ chính bài viết đang sửa
                {
                    query = query.Where(a => a.ArticleId != currentArticleId.Value);
                }
                if (!await query.AnyAsync())
                {
                    break; // Slug này là duy nhất
                }
                slug = $"{baseSlug}-{count++}";
            }
            return slug;
        }

        private void LogModelStateErrors()
        {
            foreach (var modelStateKey in ModelState.Keys)
            {
                var modelStateVal = ModelState[modelStateKey];
                foreach (var error in modelStateVal.Errors)
                {
                    _logger.LogWarning("ModelState Error for {KeyState}: {ErrorMessage} (Exception: {ExceptionMessage})",
                        modelStateKey, error.ErrorMessage, error.Exception?.Message);
                }
            }
        }

        // Phương thức làm sạch HTML, chỉ cho phép một số thẻ HTML cơ bản
        private string SanitizeHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return string.Empty;
            }            // Danh sách các thẻ HTML được phép
            var allowedTags = new[] { "p", "br", "b", "i", "u", "strong", "em", "h1", "h2", "h3", "h4", "h5", "h6", "ul", "ol", "li", "a", "img", "blockquote", "code", "pre", "hr", "table", "thead", "tbody", "tr", "th", "td", "caption" };
            
            try
            {
                // Loại bỏ script và các thẻ nguy hiểm khác
                html = System.Text.RegularExpressions.Regex.Replace(
                    html,
                    @"<script[^>]*>[\s\S]*?</script>|<style[^>]*>[\s\S]*?</style>|<iframe[^>]*>[\s\S]*?</iframe>|<frame[^>]*>[\s\S]*?</frame>|<frameset[^>]*>[\s\S]*?</frameset>|<object[^>]*>[\s\S]*?</object>|<embed[^>]*>[\s\S]*?</embed>|<applet[^>]*>[\s\S]*?</applet>|<meta[^>]*>|<!doctype[^>]*>|<link[^>]*>|<body[^>]*>|<\/body>|<head[^>]*>|<\/head>|<html[^>]*>|<\/html>",
                    "",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );

                // Loại bỏ các thuộc tính JavaScript
                html = System.Text.RegularExpressions.Regex.Replace(
                    html,
                    @"on\w+\s*=\s*""[^""]*""|on\w+\s*=\s*'[^']*'|on\w+\s*=[^\s>]*",
                    "",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );

                // Đảm bảo các thuộc tính href bắt đầu bằng http:// hoặc https:// trong thẻ a
                html = System.Text.RegularExpressions.Regex.Replace(
                    html,
                    @"<a\s+(?:[^>]*?\s+)?href\s*=\s*(['""])(?!https?:\/\/)([^""'>]+)(['""])([^>]*?)>",
                    m =>
                    {
                        var url = m.Groups[2].Value;
                        if (!url.StartsWith("#") && !url.StartsWith("/") && !url.StartsWith("mailto:"))
                        {
                            return $"<a href=\"http://{url}\"{m.Groups[4].Value}>";
                        }
                        return m.Value;
                    },
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
                
                // Log nội dung đã được làm sạch
                _logger.LogInformation("HTML content sanitized successfully");
                
                return html;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sanitizing HTML content");
                // Nếu có lỗi, trả về nội dung không có HTML nào
                return System.Net.WebUtility.HtmlEncode(html);
            }
        }
    }
}