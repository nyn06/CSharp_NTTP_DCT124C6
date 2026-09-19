/*
* CHƯƠNG TRÌNH KIỂM TRA SỐ HOÀN HẢO
* Tác giả : TRẦN THẢO NGUYÊN
* Ngày viết: 18/9/2026
*
* Phát biểu đề bài:
* Nhập vào số nguyên dương n.
* Kiểm tra xem n có phải là số hoàn hảo hay không.
* Nếu là số hoàn hảo, in ra các ước của n và tổng của chúng.
*
* Ý tưởng:
* - Nhập số nguyên dương n.
* - Duyệt các số i từ 1 đến n - 1.
* - Nếu i là ước của n thì cộng i vào tổng.
* - Đồng thời lưu các ước của n để in ra màn hình.
* - Sau khi duyệt xong, nếu tổng các ước bằng n
*   thì n là số hoàn hảo.
* - Ngược lại, n không phải là số hoàn hảo.
*
* Mã giả:
* Nhập n
* Trong khi n <= 0:
*     Nhập lại n
*
* tong = 0
* chucai = ""
*
* For i = 1 đến n - 1:
*     Nếu n chia hết cho i:
*         tong = tong + i
*         Thêm i vào danh sách ước
*
* Nếu tong = n:
*     In n là số hoàn hảo
*     In các ước và tổng
* Ngược lại:
*     In n không phải số hoàn hảo
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

namespace Buoi6Prj
{
    public class Bai4
    {
        public static void Ktrasohoanhao()
        {
            Console.Write("Moi ban nhap n: ");
            int n = int.Parse(Console.ReadLine()!);

            while (n <= 0)
            {
                Console.Write("n phai lon hon 0. Vui long nhap lai n: ");
                n = int.Parse(Console.ReadLine()!);
            }

            int tong = 0;
            string chucai = "";

            for (int i = 1; i < n; i++)
            {
                if (n % i == 0)
                {
                    tong += i;

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

            if (tong == n)
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