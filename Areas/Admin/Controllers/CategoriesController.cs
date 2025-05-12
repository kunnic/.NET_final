using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using news_project_mvc.Data;
using news_project_mvc.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace news_project_mvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Editor")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ApplicationDbContext context, ILogger<CategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Fetching all categories for Admin index page.");
            var categories = await _context.Categories
                                           .OrderByDescending(c => c.CreatedAt)
                                           .ToListAsync();
            return View(categories);
        }

        public IActionResult Create()
        {
            _logger.LogInformation("Displaying Create Category page.");
            return View(new Category());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Slug,Description")] Category category)
        {
            _logger.LogInformation("Attempting to create a new category. Initial Name: {CategoryName}", category.Name ?? "null");

            ModelState.Remove("Slug");

            if (string.IsNullOrEmpty(category.Name))
            {
                ModelState.AddModelError("Name", "Tên danh mục là bắt buộc.");
                return View(category);
            }

            if (string.IsNullOrWhiteSpace(category.Slug))
            {
                _logger.LogInformation("Tự động tạo slug từ tên: {CategoryName}", category.Name);
                category.Slug = GenerateSlug(category.Name);
                _logger.LogInformation("Đã tạo slug: {CategorySlug}", category.Slug);
            }
            else
            {
                var regex = new Regex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$");
                if (!regex.IsMatch(category.Slug))
                {
                    ModelState.AddModelError("Slug", "Slug chỉ được chứa ký tự thường, số và dấu gạch ngang nối.");
                    return View(category);
                }
            }

            bool slugExists = await _context.Categories.AnyAsync(c => c.Slug == category.Slug);
            if (slugExists)
            {
                int count = 1;
                string originalSlug = category.Slug;

                while (await _context.Categories.AnyAsync(c => c.Slug == category.Slug))
                {
                    category.Slug = $"{originalSlug}-{count++}";
                }
                _logger.LogInformation("Slug bị trùng, đã điều chỉnh thành: {CategorySlug}", category.Slug);
            }

            category.CreatedAt = DateTime.UtcNow;

            try
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Category '{CategoryName}' created successfully with ID {CategoryId}.",
                    category.Name, category.CategoryId);

                TempData["SuccessMessage"] = $"Danh mục '{category.Name}' đã được tạo thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error creating category '{CategoryName}' due to database update issue.", category.Name);
                ModelState.AddModelError("", "Đã xảy ra lỗi khi lưu vào cơ sở dữ liệu. Vui lòng thử lại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating category '{CategoryName}'.", category.Name);
                ModelState.AddModelError("", "Đã xảy ra lỗi không mong muốn khi tạo danh mục. Vui lòng thử lại.");
            }

            _logger.LogWarning("Returning Create view with model due to errors. Name: {CategoryName}", category.Name);
            return View(category);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Edit GET called with null ID.");
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found for Edit.", id);
                return NotFound();
            }

            _logger.LogInformation("Displaying Edit page for Category ID {CategoryId}, Name: {CategoryName}", id, category.Name);
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,Name,Slug,Description,CreatedAt")] Category category)
        {
            if (id != category.CategoryId)
            {
                _logger.LogWarning("Edit POST ID mismatch. Route ID: {RouteId}, Model ID: {ModelId}", id, category.CategoryId);
                return NotFound();
            }

            _logger.LogInformation("Attempting to update category. ID: {CategoryId}, Name: {CategoryName}",
                category.CategoryId, category.Name);

            ModelState.Remove("Slug");

            if (string.IsNullOrEmpty(category.Name))
            {
                ModelState.AddModelError("Name", "Tên danh mục là bắt buộc.");
                return View(category);
            }

            if (string.IsNullOrEmpty(category.Slug))
            {
                _logger.LogInformation("Tự động tạo slug từ tên: {CategoryName}", category.Name);
                category.Slug = GenerateSlug(category.Name);
                _logger.LogInformation("Đã tạo slug: {CategorySlug}", category.Slug);
            }
            else
            {
                var regex = new Regex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$");
                if (!regex.IsMatch(category.Slug))
                {
                    ModelState.AddModelError("Slug", "Slug chỉ được chứa ký tự thường, số và dấu gạch ngang nối.");
                    return View(category);
                }
            }

            bool slugExists = await _context.Categories.AnyAsync(c => c.Slug == category.Slug && c.CategoryId != category.CategoryId);
            if (slugExists)
            {
                ModelState.AddModelError("Slug", "Slug này đã được sử dụng bởi một danh mục khác. Vui lòng chọn Slug khác.");
                _logger.LogWarning("Slug '{CategorySlug}' for Category ID {CategoryId} is already in use by another category.",
                    category.Slug, category.CategoryId);
                return View(category);
            }

            try
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Category '{CategoryName}' (ID: {CategoryId}) updated successfully.",
                    category.Name, category.CategoryId);

                TempData["SuccessMessage"] = $"Danh mục '{category.Name}' đã được cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating category '{CategoryName}'.", category.Name);
                if (!CategoryExists(category.CategoryId))
                {
                    return NotFound();
                }
                else
                {
                    ModelState.AddModelError("", "Lỗi xung đột dữ liệu. Dữ liệu có thể đã được thay đổi bởi người khác. Vui lòng tải lại trang và thử lại.");
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating category '{CategoryName}'.", category.Name);
                ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật cơ sở dữ liệu. Vui lòng thử lại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating category '{CategoryName}'.", category.Name);
                ModelState.AddModelError("", "Đã xảy ra lỗi không mong muốn khi cập nhật danh mục.");
            }

            _logger.LogWarning("Returning Edit view with model due to errors. ID: {CategoryId}, Name: {CategoryName}",
                category.CategoryId, category.Name);
            return View(category);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Delete GET called with null ID.");
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found for Delete.", id);
                return NotFound();
            }

            _logger.LogInformation("Displaying Delete confirmation for Category ID {CategoryId}, Name: {CategoryName}", id, category.Name);
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation("Attempting to delete category with ID {CategoryId}.", id);
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                bool hasArticles = await _context.Articles.AnyAsync(a => a.CategoryId == id);
                if (hasArticles)
                {
                    _logger.LogWarning("Attempted to delete category '{CategoryName}' (ID: {CategoryId}) which still has articles.",
                        category.Name, id);
                    TempData["ErrorMessage"] = $"Không thể xóa danh mục '{category.Name}' vì vẫn còn bài viết thuộc danh mục này.";
                    return RedirectToAction(nameof(Delete), new { id = id });
                }

                try
                {
                    _context.Categories.Remove(category);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Category '{CategoryName}' (ID: {CategoryId}) deleted successfully.",
                        category.Name, id);
                    TempData["SuccessMessage"] = $"Danh mục '{category.Name}' đã được xóa thành công!";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting category '{CategoryName}' (ID: {CategoryId}).",
                        category.Name, id);
                    TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi xóa danh mục '{category.Name}'.";
                }
            }
            else
            {
                _logger.LogWarning("Category with ID {CategoryId} not found for DeleteConfirmed.", id);
                TempData["ErrorMessage"] = "Không tìm thấy danh mục để xóa.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }

        private string GenerateSlug(string phrase)
        {
            if (string.IsNullOrWhiteSpace(phrase))
                return Guid.NewGuid().ToString("N").Substring(0, 10);

            string str = phrase.ToLowerInvariant().Trim();
            str = Regex.Replace(str, @"[áàảãạăắằẳẵặâấầẩẫậ]", "a");
            str = Regex.Replace(str, @"[éèẻẽẹêếềểễệ]", "e");
            str = Regex.Replace(str, @"[íìỉĩị]", "i");
            str = Regex.Replace(str, @"[óòỏõọôốồổỗộơớờởỡợ]", "o");
            str = Regex.Replace(str, @"[úùủũụưứừửữự]", "u");
            str = Regex.Replace(str, @"[ýỳỷỹỵ]", "y");
            str = Regex.Replace(str, @"[đ]", "d");
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = str.Substring(0, str.Length <= 100 ? str.Length : 100).Trim();
            str = Regex.Replace(str, @"\s", "-");
            str = str.Trim('-');
            str = Regex.Replace(str, @"-+", "-");

            return string.IsNullOrEmpty(str) ?
                Guid.NewGuid().ToString("N").Substring(0, 10) : str;
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
    }
}
