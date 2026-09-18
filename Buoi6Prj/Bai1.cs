/*
* CHƯƠNG TRÌNH IN CHỮ I VÀ U
* Tác giả : TRẦN THẢO NGUYÊN
* Ngày viết: 18/9/2026
*
* Phát biểu đề bài:
* Nhập vào chiều cao h. Hãy in ra hai ký tự I và U
* có chiều cao h bằng các ký tự '*'.
*
* Ý tưởng:
* - Nhập chiều cao h từ bàn phím.
* - Dùng vòng lặp để duyệt từng dòng từ 0 đến h - 1.
* - Với mỗi dòng, gọi hàm InChuI() để in chữ I.
* - In khoảng cách giữa chữ I và chữ U.
* - Gọi hàm InChuU() để in chữ U.
* - Chữ I: dòng đầu và dòng cuối in toàn bộ dấu '*',
*   các dòng giữa chỉ in '*' ở vị trí chính giữa.
* - Chữ U: các dòng đầu in '*' ở hai bên,
*   dòng cuối in toàn bộ dấu '*'.
*
* Mã giả:
* Nhập h
* In "Chu I va U ung voi h = h la:"
*
* For i = 0 đến h - 1:
*     In dòng thứ i của chữ I
*     In khoảng cách
*     In dòng thứ i của chữ U
*     Xuống dòng
*
* InChuI(i, h):
*     For j = 0 đến h - 1:
*         Nếu i = 0 hoặc i = h - 1:
*             In '*'
*         Ngược lại nếu j = h / 2:
*             In '*'
*         Ngược lại:
*             In khoảng trắng
*
* InChuU(i, h):
*     For j = 0 đến h - 1:
*         Nếu i = h - 1:
*             In '*'
*         Ngược lại nếu j = 0 hoặc j = h - 1:
*             In '*'
*         Ngược lại:
*             In khoảng trắng
*/

/* 
* TEST CASE
*
* Test case 1:
* Input: 3
* Expected output:
* * * *     *     * *
*   *       *   *
* * * *     * * * *
*
* Test case 2:
* Input: 5
* Expected output:
* * * * *     *       * *
*     *       *       * *
*     *       *       * *
*     *       *       * *
* * * * *     * * * * *
*
* Test case 3:
* Input: 7
* Expected output:
* * * * * * *     *           * *
*       *         *           * *
*       *         *           * *
*       *         *           * *
*       *         *           * *
*       *         *           * *
* * * * * * *     * * * * * * *
*/
using System;

namespace Buoi6Prj
{
    public class Bai1
    {
        public static void InChu()
        {
            Console.Write("Moi ban nhap chieu cao h: ");
            int h = int.Parse(Console.ReadLine()!);

            Console.WriteLine("Chu I va U ung voi h = " + h + " la:");

            for (int i = 0; i < h; i++)
            {
                InChuI(i, h);
                // Khoang cach giua chu I va U
                Console.Write("   ");
                InChuU(i, h);
                Console.WriteLine();
            }
        }

        // Ham con in chu I
        private static void InChuI(int i, int h)
        {
            for (int j = 0; j < h; j++)
            {
                if (i == 0 || i == h - 1)
                {
                    Console.Write("* ");
                }
                else if (j == h / 2)
                {
                    Console.Write("* ");
                }
                else
                {
                    Console.Write("  ");
                }
            }
        }

        // Ham con in chu U
        private static void InChuU(int i, int h)
        {
            for (int j = 0; j < h; j++)
            {
                if (i == h - 1)
                {
                    Console.Write("* ");
                }
                else if (j == 0 || j == h - 1)
                {
                    Console.Write("* ");
                }
                else
                {
                    Console.Write("  ");
                }
            }
        }
    }
}