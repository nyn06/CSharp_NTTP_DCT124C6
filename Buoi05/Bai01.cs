/*
* CHƯƠNG TRÌNH TÍNH GIAI THỪA
* Tác giả : Nguyễn Thiên Phú
* MSSV: 3124411232
* Ngày viết: 14/09/2026
*
* Phát biểu đề bài: Nhập vào số nguyên dương n (1 <= n <= 12). Tính n! = 1.2.3.4...n
*
* Ý tưởng:
* - Nhập n.
* - Cho biến giaiThua = 1.
* - Dùng vòng lặp từ 1 đến n để tính giai thừa.
* - Xuất kết quả.
*
* Mã giả:
* - Nhập n.
* - giaiThua = 1.
* - Cho i chạy từ 1 đến n: giaiThua = giaiThua * i.
* - Xuất giaiThua.
*/

using System;

namespace Buoi05
{
    class Bai01
    {
        public static void ChayBai01()
        {
            // Khai bao bien
            int n;
            int giaiThua = 1;

            // Nhap du lieu
            Console.Write("Moi ban nhap so n: ");
            n = int.Parse(Console.ReadLine());

            // Xu ly
            for (int i = 1; i <= n; i++)
            {
                giaiThua = giaiThua * i;
            }

            // Xuat ket qua
            Console.Write("{0}! = ", n);

            for (int i = 1; i <= n; i++)
            {
                Console.Write(i);

                if (i < n)
                {
                    Console.Write(".");
                }
            }

            Console.WriteLine(" = {0}", giaiThua);
        }
    }
}

/*
- TEST CASE 1:
Input: 1
Output: 1! = 1 = 1

- TEST CASE 2:
Input: 3
Output: 3! = 1.2.3 = 6

- TEST CASE 3:
Input: 5
Output: 5! = 1.2.3.4.5 = 120

- TEST CASE 4:
Input: 8
Output: 8! = 1.2.3.4.5.6.7.8 = 40320

- TEST CASE 5:
Input: 12
Output: 12! = 1.2.3.4.5.6.7.8.9.10.11.12 = 479001600
*/