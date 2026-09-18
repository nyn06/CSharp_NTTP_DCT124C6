/*
* CHƯƠNG TRÌNH KIỂM TRA SỐ HOÀN HẢO
* Tác giả : TRẦN THẢO NGUYÊN
* Ngày viết: 18/9/2026
*
* Phát biểu đề bài:
* Nhập vào số nguyên dương n.
* Kiểm tra xem n có phải là số hoàn hảo hay không.
* Nếu là số hoàn hảo, in ra tổng các ước
* nhỏ hơn n bằng n.
*
* Ý tưởng:
* - Nhập số nguyên dương n.
* - Duyệt các số từ 1 đến n - 1 để tìm các ước của n.
* - Cộng các ước tìm được vào biến tổng.
* - Nếu tổng các ước bằng n thì n là số hoàn hảo.
* - Sử dụng hàm LaSoHoanHao() để thực hiện việc kiểm tra
*   và trả về true hoặc false.
* - Đồng thời tạo chuỗi chứa các ước của n để in ra.
*
* Mã giả:
* Hàm LaSoHoanHao(n):
*     tong = 0
*
*     For i = 1 đến n - 1:
*         Nếu n chia hết cho i:
*             tong = tong + i
*
*     Trả về tong = n
*
* Hàm KiemTra():
*     Nhập n
*
*     Trong khi n <= 0:
*         Nhập lại n
*
*     chucai = ""
*
*     For i = 1 đến n - 1:
*         Nếu n chia hết cho i:
*             Thêm i vào chuỗi chucai
*
*     Nếu LaSoHoanHao(n) = true:
*         In n và các ước
*         Thông báo n là số hoàn hảo
*     Ngược lại:
*         Thông báo n không là số hoàn hảo
*
* TEST CASE:
*
* Test case 1:
* Input:
* n = 6
*
* Expected output:
* 6 = 1 + 2 + 3 la so hoan hao.
*
* Test case 2:
* Input:
* n = 28
*
* Expected output:
* 28 = 1 + 2 + 4 + 7 + 14 la so hoan hao.
*
* Test case 3:
* Input:
* n = 10
*
* Expected output:
* 10 khong la so hoan hao.
*
* Test case 4:
* Input:
* n = 1
*
* Expected output:
* 1 khong la so hoan hao.
*/
using System;

namespace Buoi7Prj
{
    public class Bai2
    {
        // Hàm kiểm tra số hoàn hảo
        public static bool LaSoHoanHao(int n)
        {
            int tong = 0;

            for (int i = 1; i < n; i++)
            {
                if (n % i == 0)
                {
                    tong += i;
                }
            }

            return tong == n;
        }

        public static void KiemTra()
        {
            Console.Write("Nhap n: ");
            int n = int.Parse(Console.ReadLine()!);

            while (n <= 0)
            {
                Console.Write("n phai lon hon 0. Vui long nhap lai n: ");
                n = int.Parse(Console.ReadLine()!);
            }

            string chucai = "";

            for (int i = 1; i < n; i++)
            {
                if (n % i == 0)
                {
                    if (chucai == "")
                    {
                        chucai = i.ToString();
                    }
                    else
                    {
                        chucai += " + " + i.ToString();
                    }
                }
            }

            if (LaSoHoanHao(n))
            {
                Console.WriteLine($"{n} = {chucai} la so hoan hao.");
            }
            else
            {
                Console.WriteLine($"{n} khong la so hoan hao.");
            }
        }
    }
}