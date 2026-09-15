/*
* CHƯƠNG TRÌNH HIỂN THỊ THỰC ĐƠN
* Tác giả : Nguyễn Thiên Phú
* MSSV: 3124411232
* Ngày viết: 14/09/2026
*
* Phát biểu đề bài: Viết chương trình hiển thị thực đơn gồm:
* 1. Tính diện tích tam giác
* 2. Tính diện tích hình chữ nhật
* 3. Tính diện tích hình tròn
* 4. Thoát
*
* Sau khi người dùng chọn chức năng, hiển thị kết quả chức năng.
* Chờ người dùng nhấn phím bất kỳ và xóa màn hình để hiển thị lại thực đơn.
* Chương trình kết thúc khi người dùng chọn Thoát.
*
* Ý tưởng:
* - Dùng vòng lặp do...while để hiển thị thực đơn nhiều lần.
* - Dùng switch để xử lý chức năng người dùng chọn.
* - Nếu chọn 4 thì thoát chương trình.
* - Sau khi thực hiện chức năng, chờ nhấn phím và xóa màn hình.
*/

using System;

namespace Buoi05
{
    class Bai05
    {
        public static void ChayBai05()
        {
            // Khai bao bien
            int chon;

            // Xu ly
            do
            {
                Console.WriteLine("THUC DON");
                Console.WriteLine("1. Tinh dien tich tam giac");
                Console.WriteLine("2. Tinh dien tich hinh chu nhat");
                Console.WriteLine("3. Tinh dien tich hinh tron");
                Console.WriteLine("4. Thoat");
                Console.WriteLine("------------------------------------------");

                Console.Write("Moi ban chon chuc nang [1, 2, 3, 4]: ");
                chon = int.Parse(Console.ReadLine());

                switch (chon)
                {
                    case 1:
                        Console.WriteLine();
                        Console.WriteLine("Ban moi vua chon chuc nang tinh dien tich tam giac.");
                        break;

                    case 2:
                        Console.WriteLine();
                        Console.WriteLine("Ban moi vua chon chuc nang tinh dien tich hinh chu nhat.");
                        break;

                    case 3:
                        Console.WriteLine();
                        Console.WriteLine("Ban moi vua chon chuc nang tinh dien tich hinh tron.");
                        break;

                    case 4:
                        Console.WriteLine();
                        Console.WriteLine("Cam on ban da su dung chuong trinh!");
                        break;

                    default:
                        Console.WriteLine();
                        Console.WriteLine("Chuc nang khong hop le!");
                        break;
                }

                if (chon != 4)
                {
                    Console.WriteLine();
                    Console.WriteLine("Ban nhan phim bat ky de tiep tuc ...");
                    Console.ReadKey();
                    Console.Clear();
                }

            } while (chon != 4);
        }
    }
}

/*
- TEST CASE 1:
Input: 1
Output: Ban moi vua chon chuc nang tinh dien tich tam giac.

- TEST CASE 2:
Input: 2
Output: Ban moi vua chon chuc nang tinh dien tich hinh chu nhat.


- TEST CASE 3:
Input: 3
Output: Ban moi vua chon chuc nang tinh dien tich hinh tron.


- TEST CASE 4:
Input: 4
Output: Cam on ban da su dung chuong trinh!


- TEST CASE 5:
Input: 5
Output: Chuc nang khong hop le!


- TEST CASE 6:
Input: 0
Output: Chuc nang khong hop le!
*/