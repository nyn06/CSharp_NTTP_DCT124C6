/*
* CHƯƠNG TRÌNH KIỂM TRA VÀ PHÂN LOẠI TAM GIÁC
* Tác giả : Nguyễn Thiên Phú
* MSSV: 3124411232
* Ngày viết: 10/09/2026
*
* Phát biểu đề bài:
* Nhập vào ba số thực a, b, c.
* Kiểm tra ba số đó có tạo thành một tam giác không.
* Nếu có, cho biết đó là tam giác thường, cân, vuông, vuông cân hay đều.
*
* Ý tưởng:
* - Sắp xếp ba cạnh a, b, c theo thứ tự tăng dần.
* - Kiểm tra nếu a + b > c và các cạnh đều lớn hơn 0
*   thì ba cạnh tạo thành một tam giác.
* - Nếu không thì không tạo thành tam giác.
* - Nếu ba cạnh bằng nhau thì là tam giác đều.
* - Nếu có hai cạnh bằng nhau và thỏa mãn định lý Pythagore
*   thì là tam giác vuông cân.
* - Nếu thỏa mãn định lý Pythagore thì là tam giác vuông.
* - Nếu có hai cạnh bằng nhau thì là tam giác cân.
* - Các trường hợp còn lại là tam giác thường.
*
* Mã giả:
* - Nhập a, b, c.
* - Sắp xếp a <= b <= c.
* - Nếu a <= 0 hoặc a + b <= c:
*   thông báo không tạo thành tam giác.
* - Ngược lại:
*   thông báo tạo thành tam giác.
* - Nếu a = b và b = c:
*   tam giác đều.
* - Ngược lại nếu a = b và a^2 + b^2 = c^2:
*   tam giác vuông cân.
* - Ngược lại nếu a^2 + b^2 = c^2:
*   tam giác vuông.
* - Ngược lại nếu a = b hoặc b = c:
*   tam giác cân.
* - Ngược lại:
*   tam giác thường.
*/

using System;

namespace Buoi04
{
    class Bai04
    {
        public static void ChayBai04()
        {
            // Khai báo biến
            double a, b, c, tmp;
            bool laTamGiac;

            // Nhập dữ liệu
            Console.Write("Moi ban nhap ba so thuc a, b, c: ");
            string[] t = Console.ReadLine().Split(' ');

            a = double.Parse(t[0]);
            b = double.Parse(t[1]);
            c = double.Parse(t[2]);

            // Xử lý
            // Sắp xếp a, b, c sao cho a <= b <= c
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

            if (b > c)
            {
                tmp = b;
                b = c;
                c = tmp;
            }

            // Kiểm tra tam giác
            laTamGiac = (a > 0 && a + b > c);

            // Xuất kết quả
            Console.Write("Ba so ({0}, {1}, {2}) ", a, b, c);

            if (laTamGiac == true)
            {
                Console.WriteLine("tao thanh duoc tam giac.");

                // Tam giác đều
                if (a == b && b == c)
                {
                    Console.WriteLine("Tam giac tao thanh la tam giac deu.");
                }
                // Tam giác vuông cân
                else if (a == b && a * a + b * b == c * c)
                {
                    Console.WriteLine("Tam giac tao thanh la tam giac vuong can.");
                }
                // Tam giác vuông
                else if (a * a + b * b == c * c)
                {
                    Console.WriteLine("Tam giac tao thanh la tam giac vuong.");
                }
                // Tam giác cân
                else if (a == b || b == c)
                {
                    Console.WriteLine("Tam giac tao thanh la tam giac can.");
                }
                // Tam giác thường
                else
                {
                    Console.WriteLine("Tam giac tao thanh la tam giac thuong.");
                }
            }
            else
            {
                Console.WriteLine("khong tao thanh duoc tam giac.");
            }
        }
    }
}

/*
- TEST CASE 1:
Input:
5 3 4

Output:
Ba so (3, 4, 5) tao thanh duoc tam giac.
Tam giac tao thanh la tam giac vuong.

- TEST CASE 2:
Input:
5 5 5

Output:
Ba so (5, 5, 5) tao thanh duoc tam giac.
Tam giac tao thanh la tam giac deu.

- TEST CASE 3:
Input:
5 5 7

Output:
Ba so (5, 5, 7) tao thanh duoc tam giac.
Tam giac tao thanh la tam giac can.

- TEST CASE 4:
Input:
1 1 1.414213562

Output:
Ba so (1, 1, 1.414213562) tao thanh duoc tam giac.
Tam giac tao thanh la tam giac vuong can.

- TEST CASE 5:
Input:
2 3 4

Output:
Ba so (2, 3, 4) tao thanh duoc tam giac.
Tam giac tao thanh la tam giac thuong.

- TEST CASE 6:
Input:
1 2 3

Output:
Ba so (1, 2, 3) khong tao thanh duoc tam giac.

- TEST CASE 7:
Input:
-1 2 3

Output:
Ba so (-1, 2, 3) khong tao thanh duoc tam giac.
*/