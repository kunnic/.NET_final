// JavaScript for article editor
function setupArticleEditor() {
    // Lấy textarea nội dung
    const contentTextarea = document.getElementById('content-textarea');
    if (!contentTextarea) return;
    
    // Thêm class cho textarea
    contentTextarea.classList.add('content-editor');
    
    // Lấy container cho trình soạn thảo
    const editorContainer = document.getElementById('editor-container');
    
    // Tạo thanh công cụ
    const toolbar = document.createElement('div');
    toolbar.className = 'editor-toolbar btn-toolbar';
    toolbar.setAttribute('role', 'toolbar');
    toolbar.innerHTML = `
        <div class="btn-group btn-group-sm mr-2 mb-1" role="group">
            <button type="button" class="btn btn-outline-secondary" data-tag="b" title="In đậm"><strong>B</strong></button>
            <button type="button" class="btn btn-outline-secondary" data-tag="i" title="In nghiêng"><em>I</em></button>
            <button type="button" class="btn btn-outline-secondary" data-tag="u" title="Gạch dưới"><u>U</u></button>
        </div>
        <div class="btn-group btn-group-sm mr-2 mb-1" role="group">
            <button type="button" class="btn btn-outline-secondary" data-tag="h2" title="Tiêu đề lớn">H2</button>
            <button type="button" class="btn btn-outline-secondary" data-tag="h3" title="Tiêu đề vừa">H3</button>
            <button type="button" class="btn btn-outline-secondary" data-tag="h4" title="Tiêu đề nhỏ">H4</button>
            <button type="button" class="btn btn-outline-secondary" data-tag="p" title="Đoạn văn">P</button>
        </div>
        <div class="btn-group btn-group-sm mr-2 mb-1" role="group">
            <button type="button" class="btn btn-outline-secondary" data-tag="ul" title="Danh sách không thứ tự">UL</button>
            <button type="button" class="btn btn-outline-secondary" data-tag="ol" title="Danh sách có thứ tự">OL</button>
            <button type="button" class="btn btn-outline-secondary" data-tag="li" title="Mục danh sách">LI</button>
        </div>
        <div class="btn-group btn-group-sm mr-2 mb-1" role="group">
            <button type="button" class="btn btn-outline-secondary" data-tag="table" title="Bảng">Bảng</button>
            <button type="button" class="btn btn-outline-secondary" data-tag="tr" title="Hàng bảng">TR</button>
            <button type="button" class="btn btn-outline-secondary" data-tag="td" title="Ô bảng">TD</button>
        </div>
        <div class="btn-group btn-group-sm mb-1" role="group">
            <button type="button" class="btn btn-outline-secondary" data-tag="a" title="Liên kết">Link</button>
            <button type="button" class="btn btn-outline-secondary" data-tag="img" title="Hình ảnh">Ảnh</button>
            <button type="button" class="btn btn-outline-secondary" data-tag="br" title="Ngắt dòng">BR</button>
        </div>
    `;
    editorContainer.appendChild(toolbar);
    editorContainer.appendChild(contentTextarea);
      // Tạo container xem trước
    const previewContainer = document.createElement('div');
    previewContainer.className = 'preview-container mt-3';
    previewContainer.style.display = 'none';
    previewContainer.innerHTML = '<h6>Xem trước:</h6><div id="content-preview" class="content-preview"></div>';
    editorContainer.appendChild(previewContainer);
    
    // Thêm nút xem trước
    const previewBtn = document.createElement('button');
    previewBtn.type = 'button';
    previewBtn.className = 'btn btn-sm btn-outline-info mt-2';
    previewBtn.innerHTML = '<i class="bi bi-eye"></i> Xem trước';
    previewBtn.onclick = function() {
        const previewDiv = document.getElementById('content-preview');
        previewDiv.innerHTML = contentTextarea.value;
        previewContainer.style.display = previewContainer.style.display === 'none' ? 'block' : 'none';
        previewBtn.innerHTML = previewContainer.style.display === 'none' ? 
            '<i class="bi bi-eye"></i> Xem trước' : 
            '<i class="bi bi-eye-slash"></i> Ẩn xem trước';
    };
    editorContainer.appendChild(previewBtn);
    
    // Xử lý sự kiện click cho các nút định dạng
    const formatButtons = toolbar.querySelectorAll('button');
    formatButtons.forEach(button => {
        button.addEventListener('click', function() {
            const tag = this.getAttribute('data-tag');
            
            // Xử lý đặc biệt cho các thẻ khác nhau
            switch(tag) {
                case 'a':
                    const url = prompt('Nhập URL:', 'https://');
                    if (url) {
                        insertText(`<a href="${url}" target="_blank">`, '</a>');
                    }
                    break;
                case 'img':
                    const imgUrl = prompt('Nhập URL hình ảnh:', 'https://');
                    if (imgUrl) {
                        const altText = prompt('Nhập mô tả hình ảnh:', '');
                        insertText(`<img src="${imgUrl}" alt="${altText || ''}" style="max-width:100%;" />`, '');
                    }
                    break;
                case 'br':
                    insertText('<br>', '');
                    break;
                case 'table':
                    const rows = prompt('Số hàng:', '3');
                    const cols = prompt('Số cột:', '3');
                    if (rows && cols) {
                        let tableHtml = '<table class="table table-bordered">\n<thead>\n<tr>\n';
                        
                        // Header row
                        for (let i = 0; i < parseInt(cols); i++) {
                            tableHtml += `<th>Cột ${i+1}</th>\n`;
                        }
                        tableHtml += '</tr>\n</thead>\n<tbody>\n';
                        
                        // Body rows
                        for (let i = 0; i < parseInt(rows); i++) {
                            tableHtml += '<tr>\n';
                            for (let j = 0; j < parseInt(cols); j++) {
                                tableHtml += `<td>Nội dung</td>\n`;
                            }
                            tableHtml += '</tr>\n';
                        }
                        
                        tableHtml += '</tbody>\n</table>';
                        insertText(tableHtml, '');
                    }
                    break;
                default:
                    const startTag = `<${tag}>`;
                    const endTag = `</${tag}>`;
                    insertText(startTag, endTag);
            }
        });
    });
    
    // Hàm chèn thẻ HTML vào vị trí con trỏ hoặc bọc quanh văn bản đã chọn
    function insertText(startTag, endTag) {
        const textarea = contentTextarea;
        const start = textarea.selectionStart;
        const end = textarea.selectionEnd;
        const selectedText = textarea.value.substring(start, end);
        const beforeText = textarea.value.substring(0, start);
        const afterText = textarea.value.substring(end);
        
        textarea.value = beforeText + startTag + selectedText + endTag + afterText;
        textarea.focus();
        
        // Đặt con trỏ vào vị trí thích hợp
        if (endTag === '') {
            // Nếu không có end tag (như trong trường hợp img hoặc br), đặt con trỏ sau thẻ
            textarea.selectionStart = start + startTag.length;
        } else if (selectedText === '') {
            // Nếu không có văn bản được chọn, đặt con trỏ giữa các thẻ
            textarea.selectionStart = start + startTag.length;
        } else {
            // Nếu có văn bản được chọn, đặt con trỏ sau toàn bộ văn bản mới
            textarea.selectionStart = start + startTag.length + selectedText.length + endTag.length;
        }
        textarea.selectionEnd = textarea.selectionStart;
    }
}

// Khởi tạo trình soạn thảo khi trang đã tải xong
document.addEventListener('DOMContentLoaded', function() {
    setupArticleEditor();
});
