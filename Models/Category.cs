using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace news_project_mvc.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc.")]
        [StringLength(150, ErrorMessage = "Tên danh mục không được vượt quá 150 ký tự.")]        [Display(Name = "Tên danh mục")]
        public string Name { get; set; } = string.Empty;        
        
        [StringLength(150, ErrorMessage = "Slug không được vượt quá 150 ký tự.")]
        [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug chỉ được chứa ký tự thường, số và dấu gạch ngang nối.", MatchTimeoutInMilliseconds = 1000)]
        [Display(Name = "Slug (Đường dẫn)")]
        public string Slug { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Article>? Articles { get; set; }
    }
}
