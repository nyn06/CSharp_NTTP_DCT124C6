/*
* CHƯƠNG TRÌNH TÍNH TỔNG PHÂN SỐ (SAI SỐ)
* Tác giả : Nguyễn Thiên Phú
* MSSV: 3124411232
* Ngày viết: 14/09/2026
*
* Phát biểu đề bài:
* Nhập vào số thực a và độ sai số epsilon (<= 0.1). Tính tổng: S = 1/a + 1/(a+1) + ... + 1/(a+n) cho đến khi 1/(a+n) < epsilon.
* Ý tưởng:
* - Nhập a và epsilon.
* - Khởi tạo S = 0, n = 0.
* - Tính từng phân số 1/(a+n) và cộng vào S.
* - Khi phân số < epsilon thì dừng.
* - Xuất kết quả.
*
* Mã giả:
* - Nhập a, epsilon.
* - S = 0, n = 0.
* - Trong khi 1/(a+n) >= epsilon: S = S + 1/(a+n). n = n + 1.
* - Xuất S.
*/

using System;

namespace Buoi05
{
    class Bai03
    {
        public static void ChayBai03()
        {
            // Khai bao bien
            double a;
            double epsilon;
            double S = 0;
            int n = 0;

            // Nhap du lieu
            Console.Write("Moi ban nhap so a: ");
            a = double.Parse(Console.ReadLine());

            Console.Write("Moi ban nhap sai so epsilon (<=0.1): ");
            epsilon = double.Parse(Console.ReadLine());

            // Xu ly
            while (1.0 / (a + n) >= epsilon)
            {
                S = S + 1.0 / (a + n);
                n++;
            }

            // Xuat ket qua
            Console.WriteLine("Gia tri cua bieu thuc S(a = {0}, epsilon = {1}) = {2}", a, epsilon, S);
        }
    }
}

/*
- TEST CASE 1:
Input: a = 2
epsilon = 0.1
Output: Gia tri cua bieu thuc S(a = 2, epsilon = 0.1) = 1.928968253968254

- TEST CASE 2:
Input: a = 5
epsilon = 0.1
Output: Gia tri cua bieu thuc S(a = 5, epsilon = 0.1) =  0.8456349206349206

- TEST CASE 3:
Input: a = 10
epsilon = 0.05
Output: Gia tri cua bieu thuc S(a = 10, epsilon = 0.05) = 0.768771403175428
*/