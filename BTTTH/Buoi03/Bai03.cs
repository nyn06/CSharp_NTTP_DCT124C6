/* 
* CHƯƠNG TRÌNH TÍNH TOÁN BIỂU THỨC 
* Tác giả : Nguyễn Thiên Phú 
* MSSV: 3124411232
* Ngày viết: 09/09/2026 
* 
* Phát biểu đề bài: Hãy tìm nghiệm của phương trình bậc 2: ax2 + bx + c = 0 
* Ý tưởng: 
* - Tính delta = b^2 - 4ac.
* - Nếu delta < 0 thì phương trình vô nghiệm.
* - Nếu delta = 0 thì phương trình có nghiệm kép.
* - Nếu delta > 0 thì phương trình có 2 nghiệm phân biệt.
* 
* Mã giả: 
* - Nhập a, b, c.
* - Nếu a = 0 thì giải phương trình bậc nhất.
* - Ngược lại tính delta = b^2 - 4ac.
* - Nếu delta < 0 thì phương trình vô nghiệm.
* - Nếu delta = 0 thì tính nghiệm kép.
* - Nếu delta > 0 thì tính x1, x2.
* - Xuất kết quả.
*/

using System;

namespace Buoi03
{
    class Bai03
    {
        public static void ChayBai03()
        {
            // Khai bao bien
            double a, b, c;
            double delta;
            double x1, x2;

            // Nhap du lieu
            Console.Write("Moi ban nhap so a: ");
            a = double.Parse(Console.ReadLine());
            Console.Write("Moi ban nhap so b: ");
            b = double.Parse(Console.ReadLine());
            Console.Write("Moi ban nhap so c: ");
            c = double.Parse(Console.ReadLine());

            // Xu ly 
            if (a == 0) 
            { 
                if (b == 0) 
                { 
                    if (c == 0) 
                    { 
                        Console.WriteLine("Phuong trinh co vo so nghiem."); 
                    } 
                    else 
                    { 
                        Console.WriteLine("Phuong trinh vo nghiem."); 
                    } 
                }
                else
                { 
                    x1 = -c / b; 
                    Console.WriteLine("Phuong trinh co 1 nghiem x = {0}.", x1); 
                } 
            } 
                else 
                { 
                    delta = b * b - 4 * a * c; 
                if (delta < 0) 
                { 
                    Console.WriteLine("Phuong trinh vo nghiem."); 
                } 
                    
                else if (delta == 0) 
                { 
                    x1 = -b / (2 * a); 
                    Console.WriteLine("Phuong trinh co 1 nghiem kep x = {0}.", x1); 
                } 
                else 
                { 
                    x1 = (-b + Math.Sqrt(delta)) / (2 * a); 
                    x2 = (-b - Math.Sqrt(delta)) / (2 * a); 
                    Console.WriteLine("Phuong trinh bac 2 {0}x^2 + {1}x + {2} = 0 co:", a, b, c); 
                    Console.WriteLine("2 nghiem, x1 = {0}, x2 = {1}.", x1, x2); 
                } 
            } 
         } 
     } 
}

/* 
- TEST CASE 1:
+ Input:
a = 1
b = 5
c = 6
+ Expected: Phuong trinh co 2 nghiem, x1 = -2, x2 = -3

- TEST CASE 2:
+ Input:
a = 1
b = 2
c = 1
+ Expected: Phuong trinh co 1 nghiem kep x = -1

- TEST CASE 3: 
+ Input:
a = 1
b = 2
c = 5
+ Expected: Phuong trinh vo nghiem

- TEST CASE 4: 
+ Input:
a = 0
b = 2
c = -6
+ Expected: Phuong trinh co 1 nghiem x = 3

- TEST CASE 5: 
+ Input:
a = 0
b = 0
c = 0
+ Expected: Phuong trinh co vo so nghiem
*/