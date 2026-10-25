/* 
* CHƯƠNG TRÌNH TÍNH TOÁN BIỂU THỨC 
* Tác giả : Nguyễn Thiên Phú
* MSSV: 3124411232 
* Ngày viết: 09/09/2026 
* 
* Phát biểu đề bài: Hãy tính giá trị của hàm số f1(x) và f2(x):
* Ý tưởng:
* - Nhập vào số thực x.
* - Dựa vào giá trị của x để tính f1(x).
* - Nếu x <= 0 thì f1(x) = 0.
* - Nếu 0 <= x <= 1 thì f1(x) = x.
* - Nếu x > 1 thì f1(x) = x^4.
* - Nếu x <= 2 thì f2(x) = x^2 + 4x + 5.
* - Nếu x > 2 thì f2(x) = 1 / (x^2 + 4x + 5).
* - Xuất kết quả f1(x) và f2(x).
* 
* Mã giả:
* - Nhập x.
* - Nếu x <= 0 thì f1 = 0.
* - Ngược lại nếu x <= 1 thì f1 = x.
* - Ngược lại thì f1 = x^4.
* - Nếu x <= 2 thì f2 = x^2 + 4x + 5.
* - Ngược lại thì f2 = 1 / (x^2 + 4x + 5).
* - Xuất f1 và f2.
*/

using System;

namespace Buoi03
{
    class Bai02
    {
        public static void ChayBai02()
        {
            // Khai bao bien
            double x, f1, f2;

            // Nhap du lieu
            Console.Write("Moi ban nhap so thuc x: ");
            x = double.Parse(Console.ReadLine());

            // Xu ly f1(x)
            if (x <= 0)
            {
                f1 = 0;
            }
            else if ( x >= 0 && x <= 1)
            {
                f1 = x;
            }
            else 
            {
                f1 = x * x * x * x;
            }

            // Xu ly f2(x)
            if (x <= 2)
            {
                f2 = x * x + 4 * x + 5;
            }
            else
            {
                f2 = 1 / (x * x + 4 * x + 5);
            }

            // Xuat ket qua
            Console.WriteLine("f1({0}) = {1}.", x, f1);
            Console.WriteLine("f2({0}) = {1}.", x, f2);
        }
    }
}

/*
- TEST CASE 1:
Input:  x = -2
Output: f1(-2) = 4.
        f2(-2) = 1.

- TEST CASE 2:
Input:  x = -1
Output: f1(-1) = 1.
        f2(-1) = 2.

- TEST CASE 3:
Input:  x = 0
Output: f1(0) = 0.
        f2(0) = 5.

- TEST CASE 4:
Input:  x = 0.5
Output: f1(0.5) = 0.5.
        f2(0.5) = 7.25.

- TEST CASE 5:
Input:  x = 1
Output: f1(1) = 1.
        f2(1) = 10.

- TEST CASE 6:
Input:  x = 1.2
Output: f1(1.2) = 2.0736.
        f2(1.2) = 11.24.
*/