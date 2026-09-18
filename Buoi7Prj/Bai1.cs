/*
* CHƯƠNG TRÌNH IN HÌNH TAM GIÁC
* Tác giả : TRẦN THẢO NGUYÊN
* Ngày viết: 18/9/2026
*
* Phát biểu đề bài:
* Nhập vào chiều cao h.
* Hãy in ra một hình tam giác cân có chiều cao h.
* Tam giác được in bằng ký tự '*', bên trong rỗng
* và có đáy được in đầy đủ.
*
* Ý tưởng:
* - Nhập chiều cao h.
* - Duyệt từng dòng từ 1 đến h.
* - Với mỗi dòng, trước tiên in khoảng trắng để
*   tạo hình tam giác cân.
* - Sau đó in các vị trí của dấu '*'.
* - Chỉ in '*' ở hai cạnh bên của tam giác.
* - Nếu đang ở dòng cuối cùng thì in toàn bộ '*'
*   để tạo đáy tam giác.
*
* Mã giả:
* Nhập h
*
* For i = 1 đến h:
*     In h - i khoảng trắng
*
*     For j = 1 đến 2*i - 1:
*         Nếu j = 1 hoặc j = 2*i - 1 hoặc i = h:
*             In '*'
*         Ngược lại:
*             In khoảng trắng
*
*     Xuống dòng
*/
using System;

namespace Buoi7Prj
{
    public class Bai1
    {
        public static void InTGvaV()
        {
            Console.Write("Moi ban nhap chieu cao h: ");
            int h = int.Parse(Console.ReadLine()!);

            InHinh(h);
            Console.WriteLine();
        }

        // Hàm con dùng để in một hình tam giác
        public static void InHinh(int h)
        {
            Console.WriteLine("h=" + h);

            for (int i = 1; i <= h; i++)
            {
                // In khoảng trắng
                for (int j = 1; j <= h - i; j++)
                {
                    Console.Write("  ");
                }

                // In dấu *
                for (int j = 1; j <= 2 * i - 1; j++)
                {
                    if (j == 1 || j == 2 * i - 1 || i == h)
                    {
                        Console.Write("* ");
                    }
                    else
                    {
                        Console.Write("  ");
                    }
                }

                Console.WriteLine();
            }
        }
    }
}