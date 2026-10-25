/*
* CHƯƠNG TRÌNH TÍNH TIỀN NƯỚC
* Tác giả : Nguyễn Thiên Phú
* MSSV: 3124411232
* Ngày viết: 10/09/2026
*
* Phát biểu đề bài:
* Nhập vào chỉ số nước tháng trước, chỉ số nước trong tháng và số người đăng ký sử dụng nước.
* Tính số m3 nước tiêu thụ và số tiền nước phải trả theo định mức:
* - 4 m3 đầu tiên/người: 4400 đồng/m3
* - 2 m3 tiếp theo/người: 8300 đồng/m3
* - Các m3 còn lại: 10500 đồng/m3
* Tiền nước phải trả bao gồm VAT 5% và phí bảo vệ môi trường 10%.
*
* Ý tưởng:
* - Tính số m3 nước tiêu thụ = chỉ số mới - chỉ số cũ.
* - Tính định mức 4 m3/người và 6 m3/người.
* - Nếu số m3 tiêu thụ nằm trong định mức đầu thì tính theo giá 4400 đồng/m3.
* - Nếu vượt định mức đầu nhưng không vượt định mức tiếp theo thì phần vượt tính theo giá 8300 đồng/m3.
* - Nếu vượt cả hai định mức thì phần còn lại tính theo giá 10500 đồng/m3.
* - Sau đó cộng thêm VAT 5% và phí bảo vệ môi trường 10%.
* - Xuất số tiền phải trả.
*
* Mã giả:
* - Nhập chỉ số cũ, chỉ số mới và số người.
* - Nếu chỉ số mới < chỉ số cũ hoặc số người <= 0: thông báo dữ liệu không hợp lệ và kết thúc.
* - m3 = chỉ số mới - chỉ số cũ.
* - Định mức 4 m3 = số người * 4.
* - Định mức 6 m3 = số người * 6.
* - Nếu m3 <= định mức 4 m3: tiền nước = m3 * 4400.
* - Ngược lại nếu m3 <= định mức 6 m3: tiền nước = định mức 4 m3 * 4400
* + phần vượt * 8300.
* - Ngược lại: tiền nước = định mức 4 m3 * 4400
* + 2 m3/người * 8300
* + phần còn lại * 10500.
* - tiền phải trả = tiền nước * 1.15.
* - Xuất kết quả.
*/

using System;

namespace Buoi04
{
    class Bai02
    {
        // Hằng số của chương trình
        const int M3_DAU = 4400;
        const int M3_KE = 8300;
        const int M3_TREN = 10500;
        const double VAT = 0.05;
        const double PHI_MOI_TRUONG = 0.10;

        public static void ChayBai02()
        {
            // Khai báo biến
            int chiSoCu, chiSoMoi, m3, soNguoi;
            double soTien;

            // Nhập dữ liệu
            Console.Write("Moi ban nhap chi so tieu thu nuoc thang truoc (m^3): ");
            chiSoCu = int.Parse(Console.ReadLine());

            Console.Write("Moi ban nhap chi so tieu thu nuoc trong thang (m^3): ");
            chiSoMoi = int.Parse(Console.ReadLine());

            Console.Write("Moi ban nhap so nguoi tieu thu nuoc: ");
            soNguoi = int.Parse(Console.ReadLine());

            // Xử lý
            if (chiSoMoi < chiSoCu || soNguoi <= 0)
            {
                Console.WriteLine("Du lieu khong hop le.");
                return;
            }

            m3 = chiSoMoi - chiSoCu;

            // Tính tiền nước theo định mức của số người đăng ký
            int dinhMuc4m3 = soNguoi * 4;
            int dinhMuc6m3 = soNguoi * 6;

            if (m3 <= dinhMuc4m3)
                soTien = m3 * M3_DAU;
            else if (m3 <= dinhMuc6m3)
                soTien = dinhMuc4m3 * M3_DAU
                       + (m3 - dinhMuc4m3) * M3_KE;
            else
                soTien = dinhMuc4m3 * M3_DAU
                       + soNguoi * 2 * M3_KE
                       + (m3 - dinhMuc6m3) * M3_TREN;

            // Tính thuế VAT và phí bảo vệ môi trường
            soTien = (1 + VAT + PHI_MOI_TRUONG) * soTien;

            // Xuất kết quả
            Console.WriteLine(
                "So tien phai tra cho {0} m^3 tieu thu trong thang la {1:#,#} Đ.",
                m3, soTien);
        }
    }
}

/*
- TEST CASE 1:
Input:
Chỉ số cũ: 100
Chỉ số mới: 105
Số người: 1

Output:
So tien phai tra cho 5 m^3 tieu thu trong thang la 29,785 Đ.

- TEST CASE 2:
Input:
Chỉ số cũ: 100
Chỉ số mới: 110
Số người: 2

Output:
So tien phai tra cho 10 m^3 tieu thu trong thang la 59,570 Đ.

- TEST CASE 3:
Input:
Chỉ số cũ: 200
Chỉ số mới: 220
Số người: 2

Output:
So tien phai tra cho 20 m^3 tieu thu trong thang la 175,260 Đ.

- TEST CASE 4:
Input:
Chỉ số cũ: 100
Chỉ số mới: 90
Số người: 2

Output:
Du lieu khong hop le.

- TEST CASE 5:
Input:
Chỉ số cũ: 100
Chỉ số mới: 110
Số người: 0

Output:
Du lieu khong hop le.
*/