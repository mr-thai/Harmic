# Harmic E-Commerce Platform

Harmic là một ứng dụng thương mại điện tử (E-Commerce Web Application) được phát triển trên nền tảng **ASP.NET Core 9.0 MVC**. Ứng dụng cung cấp các tính năng quản lý sản phẩm, giỏ hàng, thanh toán trực tuyến, cũng như tích hợp trí tuệ nhân tạo (AI) để nâng cao trải nghiệm người dùng.

## 🚀 Tính năng nổi bật (Features)

*   **Mô hình MVC & Razor Pages:** Kiến trúc rõ ràng, sử dụng Razor Runtime Compilation để phát triển giao diện linh hoạt.
*   **Cơ sở dữ liệu:** Sử dụng Entity Framework Core 9.0 kết nối với Microsoft SQL Server (LocalDB cho môi trường phát triển).
*   **Thanh toán VNPAY:** Tích hợp cổng thanh toán trực tuyến VNPAY thông qua thư viện `VNPAY.NET`.
*   **Tích hợp AI (Gemini):** Ứng dụng Google Gemini AI thông qua `Google.GenAI` và `Mscc.GenerativeAI` để tạo ra các tính năng thông minh (ví dụ: Chat Retrieval Service).
*   **Quản lý tệp tin (File Management):** Tích hợp `elFinder.NetCore` cho phép dễ dàng quản lý hình ảnh và tệp tin trong hệ thống (đặc biệt trong trang Admin).
*   **Phân trang (Pagination):** Xử lý phân trang dữ liệu mượt mà với `X.PagedList.Mvc.Core`.
*   **Quản trị viên (Admin Area):** Phân chia không gian riêng (`Areas/Admin`) để quản trị hệ thống.
*   **Tiện ích khác:**
    *   Tự động tạo Slug thân thiện với SEO (`SlugGenerator`).
    *   Xử lý hình ảnh hiệu quả (`SixLabors.ImageSharp`).

## 🛠 Yêu cầu hệ thống (Prerequisites)

Để chạy dự án này trên môi trường local, máy tính của bạn cần cài đặt:
*   [.NET 9.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
*   [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) hoặc SQL Server Express (LocalDB).
*   Công cụ quản lý SQL Server như SSMS (SQL Server Management Studio) hoặc Azure Data Studio.
*   IDE khuyên dùng: Visual Studio 2022 hoặc JetBrains Rider hoặc VS Code.

## ⚙️ Cài đặt & Chạy dự án (Installation & Setup)

1. **Clone repository về máy:**
   ```bash
   git clone <đường-dẫn-repo-của-bạn>
   cd Harmic/Harmic
   ```

2. **Cấu hình chuỗi kết nối Database & API Keys:**
   Mở file `appsettings.json` (hoặc `appsettings.Development.json`) và kiểm tra các cấu hình:
   *   **Database:** Kiểm tra `ConnectionStrings:HarmicContext`. Mặc định đang sử dụng LocalDB `(localdb)\MSSQLLocalDB`.
   *   **VNPAY:** Điền thông tin Sandbox VNPAY của bạn tại `Vnpay:TmnCode` và `Vnpay:HashSecret`.
   *   **Gemini AI:** Thay thế API Key tại `Gemini:ApiKey` bằng Key hợp lệ của bạn.

3. **Cập nhật Database (Entity Framework Migrations):**
   Mở Terminal/Command Prompt hoặc Package Manager Console, chạy lệnh sau để tạo database và áp dụng migrations:
   ```bash
   dotnet ef database update
   ```

4. **Chạy ứng dụng:**
   Sử dụng lệnh sau để chạy ứng dụng:
   ```bash
   dotnet run
   ```
   Ứng dụng sẽ mặc định khởi chạy và lắng nghe tại: `http://localhost:<port>` hoặc `https://localhost:<port>`. (Xem chi tiết cổng trong file `Properties/launchSettings.json`).

## 📂 Cấu trúc dự án cơ bản

*   `Areas/Admin/`: Không gian riêng cho hệ thống quản trị nội dung (CMS).
*   `Controllers/`: Các Controller xử lý logic chính của người dùng.
*   `Models/`: Chứa các entity database và ViewModel.
*   `Services/`: Nơi chứa các service logic nghiệp vụ như `CartService`, `GeminiService`, `ChatRetrievalService`.
*   `Views/`: Chứa các file giao diện Razor `.cshtml`.
*   `wwwroot/`: Thư mục chứa tài nguyên tĩnh như CSS, JS, hình ảnh, file upload.

## 💳 Tích hợp VNPAY (Môi trường Sandbox)

Dự án hiện đang cấu hình sử dụng môi trường Sandbox của VNPAY. 
URL Callback mặc định khi thanh toán xong là: `http://localhost:5220/VNplay/Callback`. (Hãy đảm bảo port của ứng dụng khi chạy local khớp với callback này, hoặc chỉnh sửa lại trong `appsettings.json`).

## 📄 Giấy phép (License)

Dự án này là mã nguồn mở (Open Source).
