/*
* CHƯƠNG TRÌNH KIỂM TRA CÁC CHỮ SỐ CỦA SỐ CÓ 4 CHỮ SỐ
* Tác giả : Nguyễn Thiên Phú
* MSSV: 3124411232
* Ngày viết: 10/09/2026
*
* Phát biểu đề bài:
* Cho số tự nhiên n có 4 chữ số. Hãy xác định:
* (a) Cách biểu diễn n có đối xứng không?
* (b) n có 3 chữ số giống nhau không?
* (c) n có 4 chữ số khác nhau không?
* Lưu ý: sắp xếp các chữ số.
*
* Ý tưởng:
* - Tách số n thành 4 chữ số a, b, c, d.
* - Kiểm tra số đối xứng bằng cách so sánh chữ số đầu với chữ số cuối
*   và chữ số thứ hai với chữ số thứ ba.
* - Sắp xếp 4 chữ số theo thứ tự tăng dần.
* - Nếu sau khi sắp xếp có 3 chữ số liên tiếp giống nhau
*   thì n có 3 chữ số giống nhau.
* - Nếu cả 4 chữ số sau khi sắp xếp đều khác nhau
*   thì n có 4 chữ số khác nhau.
*
* Mã giả:
* - Nhập n.
* - Tách n thành a, b, c, d.
* - Nếu a = d và b = c:
*   n là số đối xứng.
* - Ngược lại:
*   n không đối xứng.
* - Sắp xếp a, b, c, d tăng dần.
* - Nếu a = c hoặc b = d:
*   n có 3 chữ số giống nhau.
* - Ngược lại:
*   n không có 3 chữ số giống nhau.
* - Nếu a != b và b != c và c != d:
*   n có 4 chữ số khác nhau.
* - Ngược lại:
*   n không có 4 chữ số khác nhau.
*/

using System;

namespace Buoi04
{
    class Bai05
    {
        public static void ChayBai05()
        {
            // Khai báo biến
            int n;
            int a, b, c, d, tmp;

            // Nhập dữ liệu
            Console.Write("Moi ban nhap so n: ");
            n = int.Parse(Console.ReadLine());

            // Xử lý
            // Tách 4 chữ số của n
            a = n / 1000;
            b = n / 100 % 10;
            c = n / 10 % 10;
            d = n % 10;

            // Kiểm tra số đối xứng
            if (a == d && b == c)
            {
                Console.WriteLine("{0} la so doi xung.", n);
            }
            else
            {
                Console.WriteLine("{0} khong la so doi xung.", n);
            }

            // Sắp xếp 4 chữ số a, b, c, d tăng dần
            if (a > b)
            {
                tmp = a;
                a = b;
                b = tmp;
            }

            if (a > c)
            {
                tmp = a;
                a = c;
                c = tmp;
            }

            if (a > d)
            {
                tmp = a;
                a = d;
                d = tmp;
            }

            if (b > c)
            {
                tmp = b;
                b = c;
                c = tmp;
            }

            if (b > d)
            {
                tmp = b;
                b = d;
                d = tmp;
            }

            if (c > d)
            {
                tmp = c;
                c = d;
                d = tmp;
            }

            // Kiểm tra có 3 chữ số giống nhau
            if (a == c || b == d)
            {
                Console.WriteLine("{0} co 3 chu so giong nhau.", n);
            }
            else
            {
                Console.WriteLine("{0} khong co 3 chu so giong nhau.", n);
            }

            // Kiểm tra có 4 chữ số khác nhau
            if (a != b && b != c && c != d)
            {
                Console.WriteLine("{0} co 4 chu so khac nhau.", n);
            }
            else
            {
                Console.WriteLine("{0} khong co 4 chu so khac nhau.", n);
            }
        }
    }
}

/*
- TEST CASE 1:
Input: 1221
Output:
1221 la so doi xung.
1221 khong co 3 chu so giong nhau.
1221 khong co 4 chu so khac nhau.

- TEST CASE 2:
Input: 1222
Output:
1222 khong la so doi xung.
1222 co 3 chu so giong nhau.
1222 khong co 4 chu so khac nhau.

- TEST CASE 3:
Input: 1234
Output:
1234 khong la so doi xung.
1234 khong co 3 chu so giong nhau.
1234 co 4 chu so khac nhau.

- TEST CASE 4:
Input: 1111
Output:
1111 la so doi xung.
1111 co 3 chu so giong nhau.
1111 khong co 4 chu so khac nhau.

- TEST CASE 5:
Input: 1122
Output:
1122 khong la so doi xung.
1122 khong co 3 chu so giong nhau.
1122 khong co 4 chu so khac nhau.

- TEST CASE 6:
Input: 1331
Output: 
1331 la so doi xung.
1331 khong co 3 chu so giong nhau.
1331 khong co 4 chu so khac nhau.
*/