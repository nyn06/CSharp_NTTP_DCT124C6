/*
* CHƯƠNG TRÌNH LIỆT KÊ SỐ HOÀN HẢO
* Tác giả : TRẦN THẢO NGUYÊN
* Ngày viết: 18/9/2026
*
* Phát biểu đề bài:
* Nhập vào số nguyên dương n.
* Hãy liệt kê tất cả các số hoàn hảo trong đoạn [1, n].
*
* Ý tưởng:
* - Nhập số nguyên dương n.
* - Duyệt các số i từ n giảm xuống 1.
* - Với mỗi số i, tính tổng các ước nhỏ hơn i.
* - Nếu tổng các ước bằng i thì i là số hoàn hảo.
* - Lưu các số hoàn hảo tìm được vào chuỗi.
* - Cuối cùng in ra các số hoàn hảo trong đoạn [1, n].
*
* Mã giả:
* Nhập n
* Trong khi n <= 0:
*     Nhập lại n
*
* chucai = ""
*
* For i = n giảm đến 1:
*     tong = 0
*
*     For j = 1 đến i - 1:
*         Nếu i chia hết cho j:
*             tong = tong + j
*
*     Nếu tong = i:
*         Thêm i vào danh sách số hoàn hảo
*
* In các số hoàn hảo trong đoạn [1, n]
*
* TEST CASE:
*
* Test case 1:
* Input:
* n = 10
*
* Expected output:
* Cac so hoan hao trong doan [1, 10] la: 6
*
* Test case 2:
* Input:
* n = 30
*
* Expected output:
* Cac so hoan hao trong doan [1, 30] la: 28 6
*
* Test case 3:
* Input:
* n = 100
*
* Expected output:
* Cac so hoan hao trong doan [1, 100] la: 28 6
*
* Test case 4:
* Input:
* n = 5
*
* Expected output:
* Cac so hoan hao trong doan [1, 5] la:
*/
using System;
namespace Buoi6Prj
{
    public class Bai5
    {
        public static void Lietkesohoanhao()
        {
            Console.Write("Nhap so n: ");
            int n = int.Parse(Console.ReadLine()!);
            while (n <= 0)
            {
                Console.Write("n phai lon hon 0. Vui long nhap lai n: ");
                n = int.Parse(Console.ReadLine()!);
            }
            int tam =n;
            string chucai ="";
            for (int i = tam; i >= 1; i--)
            {
                int tong = 0;
                for (int j = 1; j < i; j++)
                {
                    if (i % j == 0)
                    {
                        tong += j;
                    }
                }
                if (tong == i)
                {
                    if (chucai == "")
                    {
                        chucai =i.ToString();
                    }
                    else
                    {
                        chucai += " "+i.ToString();
                    }
                }
            }
            Console.WriteLine($"Cac so hoan hao trong doan [1, {tam}] la: {chucai}");
        }
    }
}