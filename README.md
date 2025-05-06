# 📰 Dự Án Trang Web Tin Tức ASP.NET Core

## 📖 Giới thiệu

Đây là một dự án **Trang Web Tin Tức** được phát triển bằng ASP.NET Core nhằm cung cấp nền tảng đọc tin tức trực tuyến cho người dùng. Trang web hỗ trợ các chuyên mục như: **Tổng hợp**, **Giải trí**, **Thể thao**, v.v.

**Chức năng chính:**

- 🗞️ Hiển thị danh sách các bài viết mới nhất hoặc nổi bật trên trang chủ.
- 📖 Đọc nội dung chi tiết bài viết.
- 🔍 Tìm kiếm bài viết theo từ khóa.
- 🔐 (Nếu có) Đăng nhập tài khoản người dùng.
- 🛠️ Khu vực quản trị để quản lý bài viết, danh mục và người dùng.

---

## ⚙️ Công nghệ sử dụng

| Thành phần                | Công nghệ                          |
|---------------------------|------------------------------------|
| Back-end                  | ASP.NET Core MVC (.NET 8)          |
| Front-end                 | CSS (Giao diện đơn giản)           |
| Cơ sở dữ liệu             | SQL Server                         |
| Xác thực & Phân quyền     | JWT (JSON Web Tokens)              |
| Ghi log                   | NLog                               |

---

## ✨ Tính năng chính

### 👤 Dành cho người dùng

- Xem danh sách bài viết mới nhất hoặc nổi bật.
- Đọc nội dung bài viết chi tiết.
- Tìm kiếm bài viết theo từ khóa.
- (Tùy chọn) Đăng nhập để trải nghiệm cá nhân hóa.

### 🔐 Dành cho quản trị viên

- Quản lý bài viết: Thêm / Sửa / Xóa.
- Quản lý danh mục tin tức.
- Quản lý tài khoản quản trị, phân quyền truy cập.
- Bảo mật và xác thực bằng JWT.

---

## 🗃️ Cơ sở dữ liệu

Hệ thống sử dụng **SQL Server** với các bảng chính:

### 1. `Category` – Danh mục bài viết

| Cột            | Kiểu dữ liệu       | Mô tả                   |
|----------------|--------------------|--------------------------|
| CategoryId     | int (PK)           | Mã danh mục              |
| Name           | nvarchar(150)      | Tên danh mục             |
| Slug           | nvarchar(150)      | Định danh URL            |
| Description    | nvarchar(150)      | Mô tả                    |
| CreatedAt      | datetime           | Ngày tạo                 |

### 2. `Article` – Bài viết

| Cột            | Kiểu dữ liệu       | Mô tả                     |
|----------------|--------------------|----------------------------|
| ArticleId      | int (PK)           | Mã bài viết                |
| Title          | nvarchar(250)      | Tiêu đề                    |
| Slug           | varchar(250)       | Định danh URL (duy nhất)  |
| Summary        | nvarchar(1000)     | Tóm tắt                    |
| Content        | nvarchar(max)      | Nội dung đầy đủ            |
| ImageUrl       | varchar(500)       | Ảnh đại diện               |
| PublishedDate  | datetime2          | Ngày xuất bản              |
| IsPublished    | bit                | Trạng thái bài viết        |
| ViewCount      | int                | Lượt xem                   |
| AuthorId       | nvarchar(450)      | FK đến bảng `AspNetUsers` |
| CategoryId     | int                | FK đến bảng `Category`    |
| CreatedAt      | datetime2          | Ngày tạo                   |
| UpdatedAt      | datetime2          | Ngày cập nhật cuối         |

### 3. `Setting` – Cấu hình hệ thống

| Cột            | Kiểu dữ liệu       | Mô tả                    |
|----------------|--------------------|---------------------------|
| SettingKey     | varchar(150) (PK)  | Khóa cấu hình             |
| SettingValue   | nvarchar(max)      | Giá trị cấu hình          |

### 4. ASP.NET Core Identity

Sử dụng các bảng mặc định như:

- `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, v.v.

---
