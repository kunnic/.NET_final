using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Cần cho ToListAsync, AnyAsync
using Microsoft.Extensions.Logging;
using news_project_mvc.Data;
using news_project_mvc.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions; // Cần cho Regex trong GenerateSlug
using System.Threading.Tasks;

namespace news_project_mvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Editor")] // Hoặc chỉ "Admin" nếu bạn muốn
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ApplicationDbContext context, ILogger<CategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Fetching all categories for Admin index page.");
            var categories = await _context.Categories
                                           .OrderByDescending(c => c.CreatedAt)
                                           .ToListAsync();
            return View(categories);
        }

        // GET: Admin/Categories/Create
        public IActionResult Create()
        {
            _logger.LogInformation("Displaying Create Category page.");
            return View(new Category()); // Truyền một Category rỗng để form có model
        }

        // POST: Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Slug,Description")] Category category) // Bind các thuộc tính bạn muốn nhận từ form
        {
            _logger.LogInformation("Attempting to create a new category. Initial Name: {CategoryName}, Initial Slug: {CategorySlug}", category.Name, category.Slug);

            // 1. Tự động tạo Slug nếu người dùng không nhập và Name có giá trị
            if (string.IsNullOrEmpty(category.Slug) && !string.IsNullOrEmpty(category.Name))
            {
                _logger.LogInformation("Slug is empty from form, generating from Name: {CategoryName}", category.Name);
                category.Slug = GenerateSlug(category.Name);
                _logger.LogInformation("Generated Slug: {CategorySlug}", category.Slug);

                if (ModelState.ContainsKey(nameof(Category.Slug)) && !string.IsNullOrEmpty(category.Slug))
                {
                    // Xóa tất cả lỗi hiện tại của trường Slug trong ModelState
                    // vì chúng ta đã cung cấp giá trị mới và sẽ re-validate ngay sau đây.
                    ModelState[nameof(Category.Slug)].Errors.Clear();
                    // Đánh dấu lại rằng trường này không còn lỗi (nếu không có lỗi nào khác được thêm sau đó)
                    ModelState[nameof(Category.Slug)].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid; // Hoặc Unvalidated để nó được kiểm tra lại
                    _logger.LogInformation("Cleared existing ModelState errors for Slug as it was auto-generated.");
                }
            }

            // 2. Re-validate model SAU KHI đã có thể gán giá trị cho Slug
            // Điều này cho phép [Required] trên Slug được kiểm tra với giá trị mới (nếu có)
            // Và các validation khác như RegularExpression trên Slug cũng được kiểm tra
            bool isModelStillValid = TryValidateModel(category);


            if (isModelStillValid) // Sử dụng kết quả của TryValidateModel
            {
                // 3. Kiểm tra tính duy nhất của Slug (sau khi đã có slug cuối cùng và model cơ bản hợp lệ)
                bool slugExists = await _context.Categories.AnyAsync(c => c.Slug == category.Slug);
                if (slugExists)
                {
                    int count = 1;
                    string originalSlug = category.Slug;
                    // Vòng lặp này có thể gây nhiều truy vấn DB nếu có nhiều slug trùng,
                    // cân nhắc giới hạn số lần thử hoặc có chiến lược tạo slug duy nhất tốt hơn.
                    while (await _context.Categories.AnyAsync(c => c.Slug == category.Slug))
                    {
                        category.Slug = $"{originalSlug}-{count++}";
                    }
                    _logger.LogInformation("Slug was duplicated, adjusted to: {CategorySlug}", category.Slug);
                    // Sau khi điều chỉnh slug, có thể bạn muốn thông báo cho người dùng rằng slug đã được thay đổi.
                    // Hoặc nếu bạn muốn người dùng tự sửa, thì nên báo lỗi ModelState ở đây.
                    // Hiện tại, chúng ta tự động sửa và tiếp tục.
                }

                category.CreatedAt = DateTime.UtcNow;
                try
                {
                    _context.Add(category);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Category '{CategoryName}' created successfully with ID {CategoryId}.", category.Name, category.CategoryId);
                    TempData["SuccessMessage"] = $"Danh mục '{category.Name}' đã được tạo thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex) // Bắt lỗi cụ thể từ DB
                {
                    _logger.LogError(ex, "Error creating category '{CategoryName}' due to database update issue.", category.Name);
                    // Lỗi này thường xảy ra nếu có ràng buộc DB khác bị vi phạm mà EF không bắt được trước
                    // (ví dụ: trigger, hoặc UNIQUE constraint khác ngoài Slug đã check).
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi lưu vào cơ sở dữ liệu. Vui lòng thử lại.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error creating category '{CategoryName}'.", category.Name);
                    ModelState.AddModelError("", "Đã xảy ra lỗi không mong muốn khi tạo danh mục. Vui lòng thử lại.");
                }
            }
            else // ModelState không hợp lệ sau TryValidateModel
            {
                _logger.LogWarning("Create category failed due to invalid model state (after TryValidateModel). Name: {CategoryName}", category.Name);
                LogModelStateErrors();
            }

            // Luôn trả về View nếu có lỗi để người dùng sửa
            _logger.LogWarning("Returning Create view with model due to errors. Name: {CategoryName}", category.Name);
            return View(category);
        }


        // GET: Admin/Categories/Edit/5
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

        // POST: Admin/Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,Name,Slug,Description,CreatedAt")] Category category)
        {
            if (id != category.CategoryId)
            {
                _logger.LogWarning("Edit POST ID mismatch. Route ID: {RouteId}, Model ID: {ModelId}", id, category.CategoryId);
                return NotFound();
            }

            _logger.LogInformation("Attempting to update category. ID: {CategoryId}, Name: {CategoryName}, Slug: {CategorySlug}",
                category.CategoryId, category.Name, category.Slug);

            // Tự động tạo Slug nếu rỗng và Name có giá trị (tương tự Create)
            if (string.IsNullOrEmpty(category.Slug) && !string.IsNullOrEmpty(category.Name))
            {
                _logger.LogInformation("Slug is empty from form, generating from Name: {CategoryName}", category.Name);
                category.Slug = GenerateSlug(category.Name);
                _logger.LogInformation("Generated Slug for update: {CategorySlug}", category.Slug);
            }

            bool isModelStillValid = TryValidateModel(category);

            if (isModelStillValid)
            {
                // Kiểm tra tính duy nhất của Slug (phải loại trừ chính category đang sửa)
                bool slugExists = await _context.Categories.AnyAsync(c => c.Slug == category.Slug && c.CategoryId != category.CategoryId);
                if (slugExists)
                {
                    // Thay vì tự động sửa, ở đây chúng ta nên báo lỗi cho người dùng vì họ đang edit
                    ModelState.AddModelError("Slug", "Slug này đã được sử dụng bởi một danh mục khác. Vui lòng chọn Slug khác.");
                    _logger.LogWarning("Slug '{CategorySlug}' for Category ID {CategoryId} is already in use by another category.", category.Slug, category.CategoryId);
                    return View(category); // Trả về view với lỗi
                }

                try
                {
                    // CreatedAt không nên thay đổi khi Edit, nên lấy giá trị gốc từ DB nếu cần
                    // Hoặc đảm bảo nó không bị bind nếu không muốn thay đổi
                    // var originalCategory = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.CategoryId == id);
                    // if (originalCategory != null) category.CreatedAt = originalCategory.CreatedAt;

                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Category '{CategoryName}' (ID: {CategoryId}) updated successfully.", category.Name, category.CategoryId);
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
            }
            else
            {
                _logger.LogWarning("Update category failed due to invalid model state. ID: {CategoryId}, Name: {CategoryName}", category.CategoryId, category.Name);
                LogModelStateErrors();
            }

            _logger.LogWarning("Returning Edit view with model due to errors. ID: {CategoryId}, Name: {CategoryName}", category.CategoryId, category.Name);
            return View(category);
        }

        // GET: Admin/Categories/Delete/5
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

        [HttpPost, ActionName("Delete")] // Nếu bạn muốn URL là /Admin/Categories/Delete/5 cho POST
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) // Tham số tên là 'id'
        {
            _logger.LogInformation("Attempting to delete category with ID {CategoryId}.", id);
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                bool hasArticles = await _context.Articles.AnyAsync(a => a.CategoryId == id);
                if (hasArticles)
                {
                    _logger.LogWarning("Attempted to delete category '{CategoryName}' (ID: {CategoryId}) which still has articles.", category.Name, id);
                    TempData["ErrorMessage"] = $"Không thể xóa danh mục '{category.Name}' vì vẫn còn bài viết thuộc danh mục này.";
                    return RedirectToAction(nameof(Delete), new { id = id }); // Quay lại trang xác nhận Delete với thông báo
                }

                try
                {
                    _context.Categories.Remove(category);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Category '{CategoryName}' (ID: {CategoryId}) deleted successfully.", category.Name, id);
                    TempData["SuccessMessage"] = $"Danh mục '{category.Name}' đã được xóa thành công!";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting category '{CategoryName}' (ID: {CategoryId}).", category.Name, id);
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

        // Hàm helper để tạo slug (bạn nên đặt ở một class tiện ích dùng chung)
        private string GenerateSlug(string phrase)
        {
            if (string.IsNullOrWhiteSpace(phrase))
                return Guid.NewGuid().ToString("N").Substring(0, 10); // Slug ngẫu nhiên nếu tên rỗng

            string str = phrase.ToLowerInvariant().Trim();

            // Loại bỏ dấu tiếng Việt (cần một giải pháp tốt hơn cho việc này)
            // Ví dụ đơn giản:
            str = Regex.Replace(str, @"[áàảãạăắằẳẵặâấầẩẫậ]", "a");
            str = Regex.Replace(str, @"[éèẻẽẹêếềểễệ]", "e");
            str = Regex.Replace(str, @"[íìỉĩị]", "i");
            str = Regex.Replace(str, @"[óòỏõọôốồổỗộơớờởỡợ]", "o");
            str = Regex.Replace(str, @"[úùủũụưứừửữự]", "u");
            str = Regex.Replace(str, @"[ýỳỷỹỵ]", "y");
            str = Regex.Replace(str, @"[đ]", "d");

            // Loại bỏ các ký tự đặc biệt không mong muốn
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // Thay thế nhiều khoảng trắng bằng một khoảng trắng
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // Giới hạn độ dài của slug (ví dụ: 100 ký tự)
            str = str.Substring(0, str.Length <= 100 ? str.Length : 100).Trim();
            // Thay thế khoảng trắng bằng dấu gạch nối
            str = Regex.Replace(str, @"\s", "-");
            // Loại bỏ các dấu gạch nối thừa ở đầu hoặc cuối (nếu có)
            str = str.Trim('-');
            // Đảm bảo không có nhiều dấu gạch nối liền nhau
            str = Regex.Replace(str, @"-+", "-");

            return string.IsNullOrEmpty(str) ? Guid.NewGuid().ToString("N").Substring(0, 10) : str; // Fallback nếu slug rỗng sau khi xử lý
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