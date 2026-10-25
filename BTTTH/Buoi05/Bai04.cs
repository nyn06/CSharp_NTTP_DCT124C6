/*
* CHƯƠNG TRÌNH TÌM SỐ NGUYÊN DƯƠNG N NHỎ NHẤT
* Tác giả : Nguyễn Thiên Phú
* MSSV: 3124411232
* Ngày viết: 14/09/2026
*
* Phát biểu đề bài: Nhập vào số thực a. Hãy tìm số nguyên dương n bé nhất thỏa: 1 + 1/2 + 1/3 + ... + 1/n > a
* Ý tưởng:
* - Nhập a.
* - Khởi tạo S = 0, n = 0.
* - Tăng n lên 1 và cộng 1/n vào S.
* - Lặp lại cho đến khi S > a.
* - Xuất n.
*
* Mã giả:
* - Nhập a.
* - S = 0, n = 0.
* - Trong khi S <= a: n = n + 1.  S = S + 1/n.
* - Xuất n.
*/

using System;

namespace Buoi05
{
    class Bai04
    {
        public static void ChayBai04()
        {
            // Khai bao bien
            double a;
            double S = 0;
            int n = 0;

            // Nhap du lieu
            Console.Write("Moi ban nhap so a: ");
            a = double.Parse(Console.ReadLine());

            // Xu ly
            while (S <= a)
            {
                n++;
                S = S + 1.0 / n;
            }

            // Xuat ket qua
            Console.WriteLine("So nguyen duong n be nhat = {0}", n);
            Console.WriteLine("Tong S = {0}", S);
        }
    }
}

/*
- TEST CASE 1:
Input: 0.5
Output: So nguyen duong n be nhat = 1

- TEST CASE 2:
Input: 1
Output: So nguyen duong n be nhat = 2

- TEST CASE 3:
Input: 2
Output: So nguyen duong n be nhat = 4

- TEST CASE 4:
Input: 3
Output: So nguyen duong n be nhat = 11
*/