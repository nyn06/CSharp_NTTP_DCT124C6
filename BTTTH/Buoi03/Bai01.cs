/*
* CHƯƠNG TRÌNH TÍNH TOÁN BIỂU THỨC
* Tác giả : Nguyễn Thiên Phú
* MSSV: 3124411232
* Ngày viết: 08/09/2026
*
* Phát biểu đề bài: Tìm giá trị lớn nhất và nhỏ nhất của 5 số a, b, c, d, e.
* Ý tưởng:
* - Khởi tạo vmax = a và vmin = a.
* - Lần lượt so sánh b, c, d, e với vmax để tìm giá trị lớn nhất.
* - Lần lượt so sánh b, c, d, e với vmin để tìm giá trị nhỏ nhất.
* - Xuất giá trị lớn nhất và nhỏ nhất.
*
* Mã giả:
* - Nhập a, b, c, d, e.
* - vmax = a.
* - Nếu b, c, d, e lớn hơn vmax thì cập nhật vmax.
* - vmin = a.
* - Nếu b, c, d, e nhỏ hơn vmin thì cập nhật vmin.
* - Xuất vmax và vmin.
*/

using System;
 
namespace Buoi03
{
    class Bai01
    {
        public static void ChayBai01()
        {
            // Khai bao bien
            int a, b, c, d, e;
            int vmax;
            int vmin;

            // Nhap du lieu
            Console.Write("Moi ban nhap so a: ");
            a = int.Parse(Console.ReadLine());
            Console.Write("Moi ban nhap so b: ");
            b = int.Parse(Console.ReadLine());
            Console.Write("Moi ban nhap so c: ");
            c = int.Parse(Console.ReadLine());
            Console.Write("Moi ban nhap so d: ");
            d = int.Parse(Console.ReadLine());
            Console.Write("Moi ban nhap so e: ");
            e = int.Parse(Console.ReadLine());

            vmax = a;
            if (b > vmax)
            {
                vmax = b;
            }
            if (c > vmax)
            {
                vmax = c;
            }
            if (d > vmax)
            {
                vmax = d;
            }
            if (e > vmax)
            {
                vmax = e;
            }
            // Xuất kết quả
            Console.WriteLine("Max({0}, {1}, {2}, {3}, {4})= {5}.", a, b, c, d, e, vmax);

            vmin = a;
            if (b < vmin)
            {
                vmin = b;
            }
             if (c < vmin)
            {
                vmin = c;
            }
            if (d < vmin)
            {
                vmin = d;
            }
            if (e < vmin)
            {
                vmin = e;
            }
            // Xuất kết quả
            Console.WriteLine("Min({0}, {1}, {2}, {3}, {4})= {5}.", a, b, c, d, e, vmin);
        }
    }
}

/*
- TEST CASE 1:
+ Input:
1
2
3
4
5
+ Expected:
Max(1, 2, 3, 4, 5)= 5.
Min(1, 2, 3, 4, 5)= 1.

- TEST CASE 2: 
+ Input:
-1
-2
-3
-4
-5
+ Expected:
Max(-1, -2, -3, -4, -5)= -1.
Min(-1, -2, -3, -4, -5)= -5.

- TEST CASE 3: 
+ Input:
0
5
-2
8
3
+ Expected:
Max(0, 5, -2, 8, 3)= 8.
Min(0, 5, -2, 8, 3)= -2.

- TEST CASE 4:
+ Input:
5
5
5
5
5
+ Expected:
Max(5, 5, 5, 5, 5)= 5.
Min(5, 5, 5, 5, 5)= 5.
*/
