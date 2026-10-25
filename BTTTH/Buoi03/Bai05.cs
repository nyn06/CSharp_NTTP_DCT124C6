/*
* CHƯƠNG TRÌNH TÍNH TOÁN BIỂU THỨC
* Tác giả : Nguyễn Thiên Phú
* MSSV 3124411232
* Ngày viết: 09/09/2026
*
* Phát biểu đề bài: Cho biết tháng trong năm vừa nhập có bao nhiêu ngày.
*
* Ý tưởng:
* - Nhập tháng và năm.
* - Nếu tháng là 1, 3, 5, 7, 8, 10, 12 thì có 31 ngày.
* - Nếu tháng là 4, 6, 9, 11 thì có 30 ngày.
* - Nếu tháng là 2 thì kiểm tra năm nhuận.
* - Năm nhuận có 29 ngày, năm không nhuận có 28 ngày.
*
* Mã giả:
* - Nhập tháng, năm.
* - Nếu tháng thuộc 1, 3, 5, 7, 8, 10, 12 thì số ngày = 31.
* - Nếu tháng thuộc 4, 6, 9, 11 thì số ngày = 30.
* - Nếu tháng = 2:
* + Nếu năm nhuận thì số ngày = 29.
* + Ngược lại số ngày = 28.
* - Xuất số ngày của tháng.
*/

using System;

namespace Buoi03
{
    class Bai05
    {
        public static void ChayBai05()
        {
            // Khai bao bien
            int thang, nam, songay;

            // Nhap du lieu
            Console.Write("Moi ban nhap thang: ");
            thang = int.Parse(Console.ReadLine());

            Console.Write("Moi ban nhap nam: ");
            nam = int.Parse(Console.ReadLine());

            if (thang <= 0 || nam <= 0)
            {
                Console.WriteLine("Thang hoac nam khong hop le.");
                return;
            }
            // Xu ly
            else if (thang == 1 || thang == 3 || thang == 5 || thang == 7 || thang == 8 || thang == 10 || thang == 12)
            {
                songay = 31;
            }
            else if (thang == 4 || thang == 6 || thang == 9 || thang == 11)
            {
                songay = 30;
            }
            else if (thang == 2)
            {
                if ((nam % 400 == 0) || (nam % 4 == 0 && nam % 100 != 0))
                {
                    songay = 29;
                }
                else
                {
                    songay = 28;
                }
            }
            else
            {
                songay = 0;
            }

            // Xuat ket qua
            if (songay != 0)
            {
                Console.WriteLine("Thang {0} trong nam {1} co {2} ngay.", thang, nam, songay);
            }
            else
            {
                Console.WriteLine("Thang khong hop le.");
            }
        }
    }
}

/*
- TEST CASE 1:
Input: thang: 1 
       nam: 2015
Output: Thang 1 trong nam 2015 co 31 ngay.

- TEST CASE 2:
Input: thang: 2 
       nam: 2015
Output: Thang 2 trong nam 2015 co 28 ngay.

- TEST CASE 3:
Input: thang: 2 
       nam: 2000
Output: Thang 2 trong nam 2000 co 29 ngay.

- TEST CASE 4:
Input: thang: 12 
       nam: 2026
Output: Thang 12 trong nam 2026 co 31 ngay.

- TEST CASE 5:
Input: thang: 13 
       nam: 2026
Output: Thang khong hop le.
*/