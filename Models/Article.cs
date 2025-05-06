using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace news_project_mvc.Models
{
    public class Article
    {
        [Key]
        public int ArticleId { get; set; }

        [Required(ErrorMessage = "Tiêu đề là bắt buộc.")]
        [StringLength(250, ErrorMessage = "Tiêu đề không được vượt quá 250 ký tự.")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slug là bắt buộc.")]
        [StringLength(250, ErrorMessage = "Slug không được vượt quá 250 ký tự.")]
        [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug chỉ được chứa ký tự thường, số và dấu gạch ngang nối.")]
        public string Slug { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Tóm tắt không được vượt quá 1000 ký tự.")]
        [Display(Name = "Tóm tắt")]
        public string? Summary { get; set; }

        [Required(ErrorMessage = "Nội dung là bắt buộc.")]
        [DataType(DataType.Html)]
        [Display(Name = "Nội dung")]
        public string Content { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "URL ảnh không được vượt quá 500 ký tự.")]
        [Display(Name = "Ảnh đại diện (URL)")]
        [DataType(DataType.ImageUrl)]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Ngày xuất bản là bắt buộc.")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày xuất bản")]
        public DateTime PublishedDate { get; set; }

        [Required]
        [Display(Name = "Đã xuất bản?")]
        public bool IsPublished { get; set; } = false;

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượt xem phải là số không âm.")]
        [Display(Name = "Lượt xem")]
        public int ViewCount { get; set; } = 0;

        [Required(ErrorMessage = "Tác giả là bắt buộc.")]
        [Display(Name = "Tác giả")]
        public string AuthorId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Danh mục là bắt buộc.")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("CategoryId")]
        [Display(Name = "Danh mục")]
        public virtual Category? Category { get; set; }

        [ForeignKey("AuthorId")]
        [Display(Name = "Tác giả")]
        public virtual IdentityUser? Author { get; set; }
    }
}
