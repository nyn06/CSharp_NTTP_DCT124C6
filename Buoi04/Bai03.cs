/*
* CHƯƠNG TRÌNH TÌM NGÀY SAU
* Tác giả : Nguyễn Thiên Phú
* MSSV: 3124411232
* Ngày viết: 10/09/2026
*
* Phát biểu đề bài: Nhập vào ngày, tháng, năm. Hỏi ngày sau đó là ngày nào?
*
* Ý tưởng:
* - Nhập vào ngày, tháng, năm.
* - Xác định số ngày của tháng hiện tại.
* - Nếu ngày hiện tại chưa phải ngày cuối tháng thì tăng ngày lên 1.
* - Nếu ngày hiện tại là ngày cuối tháng thì chuyển sang ngày 1 của tháng tiếp theo.
* - Nếu đang ở tháng 12 và là ngày cuối tháng thì chuyển sang ngày 1 tháng 1 của năm tiếp theo.
*
* Mã giả:
* - Nhập d, m, y.
* - Xác định số ngày của tháng m.
* - Nếu d < số ngày của tháng: d1 = d + 1, m1 = m, y1 = y.
* - Ngược lại: d1 = 1.
* - Nếu m = 12: m1 = 1, y1 = y + 1.
* - Ngược lại: m1 = m + 1, y1 = y.
* - Xuất ngày sau.
*/

using System;

namespace Buoi04
{
    class Bai03
    {
        public static void ChayBai03()
        {
            // Khai báo biến
            int d, m, y;
            int d1, m1, y1, soNgay = 0;
            bool laNhuan;

            // Nhập dữ liệu
            Console.Write("Moi ban nhap ngay: ");
            d = int.Parse(Console.ReadLine());

            Console.Write("Moi ban nhap thang: ");
            m = int.Parse(Console.ReadLine());

            Console.Write("Moi ban nhap nam: ");
            y = int.Parse(Console.ReadLine());

            // Xử lý
            // Tính số ngày của tháng m / y
            switch (m)
            {
                case 1: case 3: case 5: case 7: case 8: case 10: case 12:
                    soNgay = 31;
                    break;

                case 2:
                    laNhuan = (y % 400 == 0) || (y % 4 == 0 && y % 100 != 0);

                    if (laNhuan == true)
                        soNgay = 29;
                    else
                        soNgay = 28;
                    break;

                case 4: case 6: case 9: case 11:
                    soNgay = 30;
                    break;
            }

            // Tính ngày sau
            if (d < soNgay)
            {
                d1 = d + 1;
                m1 = m;
                y1 = y;
            }
            else
            {
                d1 = 1;

                if (m == 12)
                {
                    m1 = 1;
                    y1 = y + 1;
                }
                else
                {
                    m1 = m + 1;
                    y1 = y;
                }
            }

            // Xuất kết quả
            Console.WriteLine(
                "Ngay sau ngay {0}/{1}/{2} la ngay {3}/{4}/{5}.", d, m, y, d1, m1, y1);
        }
    }
}

/*
- TEST CASE 1:
Input: 31 1 2015
Output: Ngay sau ngay 31/1/2015 la ngay 1/2/2015.

- TEST CASE 2:
Input: 28 2 2015
Output: Ngay sau ngay 28/2/2015 la ngay 1/3/2015.

- TEST CASE 3:
Input: 28 2 2016
Output: Ngay sau ngay 28/2/2016 la ngay 29/2/2016.

- TEST CASE 4:
Input: 29 2 2016
Output: Ngay sau ngay 29/2/2016 la ngay 1/3/2016.

- TEST CASE 5:
Input: 31 12 2015
Output: Ngay sau ngay 31/12/2025 la ngay 1/1/2026.

- TEST CASE 6:
Input: 15 6 2020
Output: Ngay sau ngay 15/6/2020 la ngay 16/6/2020.
*/