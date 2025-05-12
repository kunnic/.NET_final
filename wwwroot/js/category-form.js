/**
 * category-form.js - Xử lý form tạo và sửa danh mục
 * 
 * Script này xử lý client-side validation cho form danh mục,
 * đặc biệt là vấn đề với trường slug không bắt buộc.
 */

document.addEventListener('DOMContentLoaded', function() {
    console.log('Category form script loaded');
    
    // Nếu đang ở trang tạo hoặc sửa danh mục
    if (document.querySelector('form[action*="Categories/Create"]') || 
        document.querySelector('form[action*="Categories/Edit"]')) {
        
        // Lấy form
        const form = document.querySelector('form');
        
        // Lắng nghe sự kiện submit
        form.addEventListener('submit', function(event) {
            // Không cần validate slug (sẽ được xử lý ở server)
            const slugInput = document.querySelector('input[name="Slug"]');
            if (slugInput) {
                // Đánh dấu trường là valid dù có giá trị hay không
                if (slugInput.classList.contains('input-validation-error')) {
                    slugInput.classList.remove('input-validation-error');
                }
                
                // Xóa thông báo lỗi nếu có
                const slugErrorSpan = slugInput.parentElement.querySelector('.text-danger');
                if (slugErrorSpan) {
                    slugErrorSpan.textContent = '';
                }
            }
        });
        
        // Cũng có thể ghi đè jQuery validation
        if (window.jQuery && jQuery.validator) {
            console.log('Overriding jQuery validation for slug');
            jQuery.validator.setDefaults({
                ignore: "input[name='Slug']" // Bỏ qua validation cho trường slug
            });
        }
    }
});
