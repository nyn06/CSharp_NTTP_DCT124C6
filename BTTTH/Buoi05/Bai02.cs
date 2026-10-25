/*
* CHƯƠNG TRÌNH TÍNH GIÁ TRỊ BIỂU THỨC
* Tác giả : Nguyễn Thiên Phú
* MSSV: 3124411232
* Ngày viết: 14/09/2026
*
* Phát biểu đề bài:
* Nhập vào số nguyên dương n. Hãy tính giá trị của các biểu thức:
*
* S1 = 1/1 + 1/2 + ... + 1/n
*
* S2 = 1/n - 1/(n-1) + 1/(n-2) - 1/(n-3) + ... + (-1)^(n+1) * 1/1
*
* Ý tưởng:
* - Nhập n.
* - Dùng vòng lặp để tính S1.
* - Dùng vòng lặp từ n về 1 để tính S2.
* - S2: số hạng đầu tiên cộng, sau đó lần lượt cộng và trừ xen kẽ.
*
* Mã giả:
* - Nhập n.
* - S1 = 0, S2 = 0.
* - Cho i chạy từ 1 đến n:
*   S1 = S1 + 1/i.
* - Cho i chạy từ n về 1:
*   Nếu là số hạng thứ 1, 3, 5,... thì cộng 1/i.
*   Ngược lại thì trừ 1/i.
* - Xuất S1 và S2.
*/

using System;

namespace Buoi05
{
    class Bai02
    {
        public static void ChayBai02()
        {
            // Khai bao bien
            int n;
            double S1 = 0;
            double S2 = 0;

            // Nhap du lieu
            Console.Write("Moi ban nhap so nguyen n: ");
            n = int.Parse(Console.ReadLine());

            // Xu ly S1
            for (int i = 1; i <= n; i++)
            {
                S1 = S1 + 1.0 / i;
            }

            // Xu ly S2
            for (int i = n; i >= 1; i--)
            {
                if ((n - i) % 2 == 0)
                {
                    S2 = S2 + 1.0 / i;
                }
                else
                {
                    S2 = S2 - 1.0 / i;
                }
            }

            // Xuat ket qua
            Console.WriteLine("S1 (n={0}) = {1}", n, S1);
            Console.WriteLine("S2 (n={0}) = {1}", n, S2);
        }
    }
}

/*
- TEST CASE 1:
Input: 1
Output:
S1 (n=1) = 1
S2 (n=1) = 1

- TEST CASE 2:
Input: 2
Output:
S1 (n=2) = 1.5
S2 (n=2) = -0.5

- TEST CASE 3:
Input: 3
Output:
S1 (n=3) = 1.8333333333333333
S2 (n=3) = 0.8333333333333333

- TEST CASE 4:
Input: 5
Output:
S1 (n=5) = 2.283333333333333
S2 (n=5) = 0.7833333333333333
*/
