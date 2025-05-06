using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace news_project_mvc.Models
{
    public class Setting
    {
        [Key]
        [Required]
        [StringLength(100)]
        [Display(Name = "Khóa cấu hình")]
        public string SettingKey { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá trị cấu hình là bắt buộc.")]
        [Display(Name = "Giá trị cấu hình")]
        public string SettingValue { get; set; } = string.Empty;
    }
}

