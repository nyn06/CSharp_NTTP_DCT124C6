/*
* CHƯƠNG TRÌNH TÌM NGÀY TRƯỚC
* Tác giả : TRẦN THẢO NGUYÊN
* Ngày viết: 18/9/2026
*
* Phát biểu đề bài:
* Nhập vào ngày, tháng, năm.
* Kiểm tra ngày tháng năm có hợp lệ hay không.
* Nếu hợp lệ, hãy tìm và in ra ngày ngay trước đó.
*
* Ý tưởng:
* - Nhập ngày, tháng, năm.
* - Kiểm tra ngày tháng năm có hợp lệ.
* - Nếu không hợp lệ thì yêu cầu nhập lại.
* - Nếu ngày > 1 thì giảm ngày đi 1.
* - Nếu ngày = 1 thì chuyển sang ngày cuối cùng
*   của tháng trước.
* - Nếu đang ở tháng 1 thì tháng trước là tháng 12
*   và giảm năm đi 1.
* - Xác định số ngày của tháng trước,
*   trong đó tháng 2 phải xét năm nhuận.
*
* Mã giả:
* Nhập ngày, tháng, năm
*
* Trong khi ngày tháng năm không hợp lệ:
*     Thông báo không hợp lệ
*     Nhập lại ngày, tháng, năm
*
* Lưu ngày, tháng, năm ban đầu
*
* Nếu ngày > 1:
*     ngày = ngày - 1
* Ngược lại:
*     tháng = tháng - 1
*
*     Nếu tháng = 0:
*         tháng = 12
*         năm = năm - 1
*
*     ngày = số ngày của tháng đó
*
* In ngày trước đó
*
* Hàm SoNgayTrongThang(tháng, năm):
*     Nếu tháng = 2:
*         Nếu năm nhuận:
*             Trả về 29
*         Ngược lại:
*             Trả về 28
*
*     Nếu tháng là 4, 6, 9 hoặc 11:
*         Trả về 30
*
*     Ngược lại:
*         Trả về 31
*
* Hàm LaNamNhuan(năm):
*     Nếu năm chia hết cho 400:
*         Trả về true
*     Nếu năm chia hết cho 100:
*         Trả về false
*     Trả về năm chia hết cho 4
*
* TEST CASE:
*
* Test case 1:
* Input:
* ngay = 1
* thang = 2
* nam = 2015
*
* Expected output:
* Ngay truoc ngay 1/2/2015 la ngay 31/1/2015.
*
* Test case 2:
* Input:
* ngay = 15
* thang = 5
* nam = 2020
*
* Expected output:
* Ngay truoc ngay 15/5/2020 la ngay 14/5/2020.
*
* Test case 3:
* Input:
* ngay = 1
* thang = 3
* nam = 2020
*
* Expected output:
* Ngay truoc ngay 1/3/2020 la ngay 29/2/2020.
*
* Test case 4:
* Input:
* ngay = 1
* thang = 1
* nam = 2025
*
* Expected output:
* Ngay truoc ngay 1/1/2025 la ngay 31/12/2024.
*
* Test case 5:
* Input:
* ngay = 31
* thang = 2
* nam = 2025
*
* Expected:
* Ngay thang nam khong hop le.
* Chương trình yêu cầu nhập lại.
*/
using System;

namespace Buoi7Prj
{
    public class Bai4
    {
        public static void NgayTruoc()
        {
            Console.Write("Nhap ngay: ");
            int ngay = int.Parse(Console.ReadLine()!);

            Console.Write("Nhap thang: ");
            int thang = int.Parse(Console.ReadLine()!);

            Console.Write("Nhap nam: ");
            int nam = int.Parse(Console.ReadLine()!);

            // Kiểm tra ngày tháng năm hợp lệ
            while (!LaNgayHopLe(ngay, thang, nam))
            {
                Console.WriteLine("Ngay thang nam khong hop le. Vui long nhap lai.");

                Console.Write("Nhap ngay: ");
                ngay = int.Parse(Console.ReadLine()!);

                Console.Write("Nhap thang: ");
                thang = int.Parse(Console.ReadLine()!);

                Console.Write("Nhap nam: ");
                nam = int.Parse(Console.ReadLine()!);
            }

            int ngayBanDau = ngay;
            int thangBanDau = thang;
            int namBanDau = nam;

            if (ngay > 1)
            {
                ngay--;
            }
            else
            {
                // Nếu là ngày 1 thì lùi về tháng trước
                thang--;

                if (thang == 0)
                {
                    thang = 12;
                    nam--;
                }

                ngay = SoNgayTrongThang(thang, nam);
            }

            Console.WriteLine(
                $"Ngay truoc ngay {ngayBanDau}/{thangBanDau}/{namBanDau} la ngay {ngay}/{thang}/{nam}."
            );
        }

        // Hàm cho biết tháng có bao nhiêu ngày
        public static int SoNgayTrongThang(int thang, int nam)
        {
            if (thang == 2)
            {
                if (LaNamNhuan(nam))
                    return 29;
                else
                    return 28;
            }

            if (thang == 4 || thang == 6 || thang == 9 || thang == 11)
                return 30;

            return 31;
        }

        // Hàm kiểm tra năm nhuận
        public static bool LaNamNhuan(int nam)
        {
            if (nam % 400 == 0)
                return true;

            if (nam % 100 == 0)
                return false;

            return nam % 4 == 0;
        }

        // Hàm kiểm tra ngày tháng năm có hợp lệ không
        public static bool LaNgayHopLe(int ngay, int thang, int nam)
        {
            if (nam <= 0 || thang < 1 || thang > 12)
                return false;

            int soNgay = SoNgayTrongThang(thang, nam);

            if (ngay < 1 || ngay > soNgay)
                return false;

            return true;
        }
    }
}