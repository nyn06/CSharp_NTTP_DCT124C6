/* 
* CHƯƠNG TRÌNH TÍNH TOÁN BIỂU THỨC 
* Tác giả: Huỳnh Thị Mỹ Tiên 
* Ngày viết: 08/09/2026 
* 
* Phát biểu đề bài:
* Nhập vào hai số nguyên dương n, k với k <= n.
* Tính tổ hợp C(n,k) theo công thức:
* C(n,k) = n! / (k! * (n-k)!)
* 
* Ý tưởng:
* - Xây dựng hàm GiaiThua(int n) để tính n!.
* - Nhập hai số k và n từ bàn phím.
* - Kiểm tra n, k là số nguyên dương và k <= n.
* - Nếu dữ liệu hợp lệ, sử dụng hàm GiaiThua() để tính C(n,k).
* - Nếu dữ liệu không hợp lệ, thông báo "Du lieu khong hop le!".
* 
* Mã giả:
* Bắt đầu
*     Nhập k
*     Nhập n
*     Nếu n > 0 và k > 0 và k <= n thì
*         C = GiaiThua(n) / (GiaiThua(k) * GiaiThua(n-k))
*         Xuất C
*     Ngược lại
*         Xuất "Du lieu khong hop le!"
*     Kết thúc nếu
* Kết thúc
*/

using System;

namespace NNLT.Buoi08
{
    class Cau01
    {
        //Tính giai thừa 
        public static int GiaiThua(int n)
        {
            int gt = 1;

            for (int i = 1; i <= n; i++)
            {
                gt = gt * i;
            }
            return gt;

            
        }

        //Tính tổ hợp
        public static void TinhToHop()
        {
            int k, n;

            Console.Write("Moi ban nhap k: ");
            k = int.Parse(Console.ReadLine()!);
            Console.Write("Moi ban nhap n: ");
            n = int.Parse(Console.ReadLine()!);

            
            if (n > 0 && k > 0 && k <= n)
            {
                int C = GiaiThua(n) / (GiaiThua(k) * GiaiThua(n - k));
                Console.WriteLine("C(" + n + ", " + k + ") = " + C);

            }
            else
            {
                 Console.WriteLine("Du lieu khong hop le!");
            }

        }

    }
}

/*
TEST CASE 1:
Input:
k = 5
n = 10

Output:
C(10, 5) = 252


TEST CASE 2:
Input:
k = 3
n = 7

Output:
C(7, 3) = 35


TEST CASE 3:
Input:
k = 2
n = 5

Output:
C(5, 2) = 10


TEST CASE 4:
Input:
k = 4
n = 4

Output:
C(4, 4) = 1


TEST CASE 5:
Input:
k = 1
n = 10

Output:
C(10, 1) = 10
*/