# Myrtle Incremental (Fnuy Incremental Game)

Một tựa game clicker/incremental trên nền tảng Windows Forms (.NET), tập trung vào cơ chế tiến trình số học quy mô lớn, nhiều tầng Reset (Prestige, Ascension, Transcend) và hệ thống thử thách (Challenges).

---

## Tính năng nổi bật

### 1. Hệ thống Số học & Nâng cấp (Progression & Upgrades)

* **Hỗ trợ số học cực lớn**: Sử dụng thư viện `BreakInfinity` để xử lý các giá trị vượt ngưỡng số thực chuẩn (`double`), hiển thị định dạng chuẩn (K, M, B, T, Qa,... cho đến các ký hiệu quy mô lớn).
* **Cơ chế Soft Cap & Hard Cap**: Giới hạn tốc độ gia tăng khi chạm mốc điểm nhất định để thử thách khả năng tối ưu hóa chiến lược tích luỹ của người chơi.
* **Generator tự động**: Mua máy phát điểm để duy trì điểm thụ động mỗi giây (Points Per Second - PPS).
* **Hỗ trợ tiện ích**: Mở khoá chức năng chuột phải để mua tối đa nâng cấp (*Buy Max*) và tự động mua nâng cấp (*Auto Buy*).

### 2. Các tầng Reset (Prestige Layers)

* **Prestige**: Tầng reset cơ bản đầu tiên giúp quy đổi điểm lấy hệ số nhân nâng cấp và mở khóa Generator.
* **Ascension**: Tầng reset bậc 2, cung cấp Ascension Points giúp nhân bội số tăng điểm, giảm thời gian hồi chiêu nút bấm và kích hoạt cơ chế tự động click.
* **Transcend**: Reset toàn diện để chuyển đổi tiến trình thành tài nguyên cao cấp (**Milk**).

### 3. Hệ thống Thử thách (Ascension Challenges)

Chế độ thử thách đặt người chơi vào các điều kiện bất lợi nhằm mở khoá những bùa lợi vĩnh viễn:

* **Thử thách 1**: Giảm mạnh tỷ lệ nhận điểm $\rightarrow$ Phần thưởng: Bội số tăng điểm vĩnh viễn x3.
* **Thử thách 2**: Giảm một nửa hiệu quả của Prestige $\rightarrow$ Phần thưởng: Tăng hiệu suất Prestige thêm 10%.
* **Thử thách 3**: Tăng cooldown click và chu kỳ tick generator lên 10 giây $\rightarrow$ Phần thưởng: Giảm 0.1s cooldown và tick rate.
* **Thử thách 4**: Kết hợp toàn bộ hạn chế của cả 3 thử thách trước $\rightarrow$ Phần thưởng: Tăng ngưỡng Soft Cap.

### 4. Tiện ích & Hỗ trợ ngoại tuyến (QoL & Offline System)

* **Offline Progress**: Tính toán tiến trình và thu thập điểm thụ động dựa trên thời gian offline (kiểm tra đối chiếu với máy chủ).
* **Daily Rewards & Premium Shop**: Điểm danh nhận Milk mỗi ngày theo chuỗi đăng nhập (Streak). Dùng Milk nâng cấp hiệu suất offline, giảm nhẹ soft cap hoặc tăng điểm nền.
* **Savegame mã hóa**: Dữ liệu lưu trữ an toàn bằng DPAPI (`ProtectedData`) cục bộ, có cơ chế tự động tạo bản sao lưu (backup) khi phát hiện xung đột dữ liệu.
* **Hệ thống âm thanh**: Tích hợp engine audio đa kênh thông qua NAudio, hỗ trợ phát đồng thời các hiệu ứng âm thanh tương tác.

---

## Công nghệ sử dụng

* **Ngôn ngữ**: C# (.NET / Windows Forms)
* **Thư viện số học**: [BreakInfinity.cs](https://github.com/Razenpok/BreakInfinity.cs)
* **Xử lý âm thanh**: [NAudio](https://github.com/naudio/NAudio)
* **Định dạng dữ liệu**: Newtonsoft.Json

---

## Cài đặt & Chạy dự án

### Yêu cầu hệ thống

* Hệ điều hành: Windows 10/11
* .NET SDK (phiên bản tương thích với dự án)
* IDE: Visual Studio 2022 trở lên (với workload *.NET Desktop Development*)

### Build từ mã nguồn

1. Clone mã nguồn về máy:
```bash
git clone https://github.com/<your-username>/<your-repo-name>.git
cd <your-repo-name>

```


2. Mở file giải pháp `WinFormsApp1.sln` bằng Visual Studio.
3. Restore các NuGet package:
```bash
dotnet restore

```


4. Build và chạy:
* Nhấn **F5** hoặc chọn **Debug / Release > Start Debugging** trong Visual Studio.
* Hoặc chạy qua dòng lệnh:



```bash
dotnet run --project WinFormsApp1.csproj

```

---

## Vị trí lưu dữ liệu

Dữ liệu màn chơi (`savegame.json`) và nhật ký lỗi (`crashlog.txt`) được lưu trữ tại:

```text
%LocalAppData%\FnuyIncrementalGame\

```

---

## Giấy phép (License)

Dự án được phân phối dưới giấy phép [MIT](https://www.google.com/search?q=LICENSE). Tự do phân nhánh, phát triển và tái sử dụng.
