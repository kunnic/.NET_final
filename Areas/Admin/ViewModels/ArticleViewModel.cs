using Microsoft.AspNetCore.Mvc.Rendering; // Cần cho SelectList
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace news_project_mvc.Areas.Admin.ViewModels
{
    public class ArticleViewModel
    {
        public int ArticleId { get; set; } // Sẽ cần cho Edit

        [Required(ErrorMessage = "Tiêu đề không được để trống.")]
        [StringLength(250, ErrorMessage = "Tiêu đề không được vượt quá 250 ký tự.")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        [StringLength(1000, ErrorMessage = "Tóm tắt không được vượt quá 1000 ký tự.")]
        [Display(Name = "Tóm tắt")]
        public string? Summary { get; set; } // Cho phép null

        [Required(ErrorMessage = "Nội dung không được để trống.")]
        [Display(Name = "Nội dung")]
        public string Content { get; set; }

        [Display(Name = "URL Hình ảnh đại diện")]
        [StringLength(500, ErrorMessage = "URL hình ảnh không được vượt quá 500 ký tự.")]
        [Url(ErrorMessage = "URL hình ảnh không hợp lệ.")] // Kiểm tra định dạng URL
        public string? ImageUrl { get; set; } // Cho phép null

        [Required(ErrorMessage = "Vui lòng chọn một danh mục.")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        [Display(Name = "Xuất bản?")]
        public bool IsPublished { get; set; } = false; // Mặc định là chưa xuất bản

        // Thuộc tính này dùng để đổ danh sách Category vào dropdownlist trên View
        public IEnumerable<SelectListItem>? CategoriesList { get; set; }

        // Các thuộc tính chỉ đọc, sẽ được hiển thị ở View (nếu cần) nhưng không phải input
        [Display(Name = "Ngày xuất bản")]
        public DateTime? PublishedDate { get; set; }

        [Display(Name = "Tác giả")]
        public string? AuthorName { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedAt { get; set; }
    }
}