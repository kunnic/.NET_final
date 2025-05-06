# Dự án Trang Web Tin Tức ASP.NET Core

## Giới thiệu

Dự án này là một trang web tin tức được xây dựng nhằm cung cấp cho người dùng một nền tảng để đọc các loại tin tức khác nhau như tin tổng hợp, giải trí, thể thao. [cite: 1, 2] Người dùng có thể dễ dàng truy cập, đọc tin, xem các bài viết mới nhất và sử dụng chức năng tìm kiếm để lọc nội dung. [cite: 3] Hệ thống cũng bao gồm một khu vực quản trị để quản lý bài viết và các danh mục tin tức. [cite: 4]

## Công nghệ sử dụng

Dự án được phát triển bằng các công nghệ sau:

* **Back-end:** .NET 8, ASP.NET Core MVC
* **Front-end:** CSS (Thiết kế đơn giản, không sử dụng các công nghệ front-end phức tạp) [cite: 12]
* **Database:** SQL Server [cite: 5]
* **Authentication & Authorization:** JWT (JSON Web Tokens) [cite: 18]
* **Logging:** NLog

## Tính năng chính

### Cho người dùng:

* Xem danh sách các bài viết mới nhất hoặc bài viết nổi bật trên trang chủ. [cite: 9]
* Đọc nội dung chi tiết của bài viết. [cite: 10]
* Tìm kiếm bài viết theo từ khóa. [cite: 3]
* Đăng nhập tài khoản (nếu có chức năng cho người dùng cuối). [cite: 10]

### Cho quản trị viên:

* Quản lý bài viết: Thêm, xóa, sửa bài viết. [cite: 7]
* Quản lý danh mục tin tức. [cite: 4]
* Quản lý tài khoản người dùng quản trị và phân quyền. [cite: 6, 18]
* Các chức năng liên quan đến tài khoản quản trị. [cite: 11]

## Cơ sở dữ liệu

Hệ thống sử dụng SQL Server để lưu trữ dữ liệu. [cite: 5]

### Sơ đồ quan hệ (ERD)

*(Trong tài liệu không có sơ đồ ERD trực quan, phần này mô tả các bảng chính)*

### Các bảng chính:

1.  **Category (Danh mục)** [cite: 13]
    * `CategoryId` (PK, int, IDENTITY): Mã danh mục
    * `Name` (nvarchar(150), NOT NULL): Tên danh mục
    * `Slug` (nvarchar(150), NOT NULL): Chuỗi định danh cho URL
    * `Description` (nvarchar(150)): Mô tả cho danh mục
    * `CreatedAt` (datetime, NOT NULL): Ngày tạo danh mục

2.  **Article (Bài viết)** [cite: 14, 15]
    * `ArticleId` (PK, int, IDENTITY): Mã bài viết
    * `Title` (nvarchar(250), NOT NULL): Tiêu đề bài viết
    * `Slug` (varchar(250), NOT NULL, UNIQUE): Chuỗi định danh cho URL
    * `Summary` (nvarchar(1000)): Tóm tắt/Mô tả ngắn
    * `Content` (nvarchar(max), NOT NULL): Nội dung đầy đủ bài viết
    * `ImageUrl` (varchar(500)): Đường dẫn ảnh đại diện
    * `PublishedDate` (datetime2, NOT NULL): Ngày xuất bản
    * `IsPublished` (bit, NOT NULL, DEFAULT(0)): Trạng thái (0: Nháp, 1: Xuất bản)
    * `ViewCount` (int, NOT NULL, DEFAULT(0)): Số lượt xem
    * `AuthorId` (nvarchar(450), NOT NULL, FK -> AspNetUsers(Id)): Mã tác giả
    * `CategoryId` (int, NOT NULL, FK -> Categories(CategoryId)): Mã danh mục
    * `CreatedAt` (datetime2, NOT NULL, DEFAULT(GETDATE())): Ngày tạo
    * `UpdatedAt` (datetime2): Ngày cập nhật cuối

3.  **Setting (Cấu hình)** [cite: 16, 17]
    * `SettingKey` (PK, varchar(150), NOT NULL): Khóa cấu hình
    * `SettingValue` (nvarchar(max), NOT NULL): Giá trị cấu hình

4.  **Quản lý người dùng (ASP.NET Core Identity)** [cite: 18, 19]
    * Hệ thống sử dụng các bảng mặc định của ASP.NET Core Identity như `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles` để quản lý tài khoản quản trị viên và vai trò.

## Phạm vi và giới hạn của dự án

* Thiết kế cơ sở dữ liệu. [cite: 8]
* Thiết kế trang chủ hiển thị danh sách bài viết/bài viết nổi bật. [cite: 9]
* Hiện thực chức năng xem bài viết. [cite: 10]
* Hiện thực chức năng đăng nhập và các biện pháp bảo vệ trang web. [cite: 10]
* Hiện thực các chức năng liên quan đến tài khoản. [cite: 11]
* Thiết kế điều hướng cơ bản. [cite: 11]
* Thiết kế giao diện đơn giản, tập trung vào chức năng back-end. [cite: 12]

## Nghiên cứu VQA (Visual Question Answering)

*(Phần này được đề cập trong tài liệu như một cơ sở lý thuyết và thực nghiệm riêng, có thể là một module/nghiên cứu liên quan hoặc một dự án khác. Nếu đây là một phần của repository, bạn có thể mở rộng thêm)*

Tài liệu có bao gồm một phần nghiên cứu về bài toán Trả lời câu hỏi trực quan (VQA), sử dụng các mô hình như ResNet34 và LSTM. [cite: 20, 42, 43] Thực nghiệm được tiến hành trên bộ dữ liệu hình ảnh động vật, với mục tiêu xây dựng mô hình có khả năng trả lời câu hỏi liên quan đến nội dung ảnh. [cite: 37, 38]

## Cài đặt và Chạy dự án

*(Phần này cần được bổ sung thông tin cụ thể về cách cài đặt môi trường, restore database, và chạy dự án)*

1.  **Yêu cầu:**
    * .NET 8 SDK
    * SQL Server
    * (Các công cụ khác nếu có)
2.  **Các bước cài đặt:**
    * Clone repository: `git clone <URL_REPO>`
    * Mở dự án bằng Visual Studio hoặc `dotnet CLI`.
    * Cấu hình chuỗi kết nối database trong `appsettings.json`.
    * Chạy Entity Framework Migrations (nếu có): `dotnet ef database update`
    * Chạy dự án: `dotnet run` hoặc nhấn F5 trong Visual Studio.

## Đóng góp

*(Hướng dẫn cách đóng góp vào dự án - nếu có)*

## Giấy phép

*(Thông tin giấy phép của dự án - ví dụ: MIT, Apache 2.0, ...)*
