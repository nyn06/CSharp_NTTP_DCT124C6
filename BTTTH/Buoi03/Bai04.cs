/*  
* CHƯƠNG TRÌNH TÍNH TOÁN BIỂU THỨC  
* Tác giả : Nguyễn Thiên Phú  
* MSSV 3124411232
* Ngày viết: 09/09/2026  
*  
* Phát biểu đề bài: Cho biết tên gọi tiếng Anh của tháng vừa nhập. 
* Ý tưởng:  
* - Nhập vào số tháng n.
* - Dùng switch để kiểm tra số tháng.
* - Nếu n từ 1 đến 12 thì xuất tên tiếng Anh tương ứng.
* - Nếu n không thuộc khoảng từ 1 đến 12 thì thông báo không xác định.
*  
* Mã giả:
* - Nhập n.
* - Nếu n = 1 thì xuất January.
* - Nếu n = 2 thì xuất February.
* - Nếu n = 3 thì xuất March.
* - Nếu n = 4 thì xuất April.
* - Nếu n = 5 thì xuất May.
* - Nếu n = 6 thì xuất June.
* - Nếu n = 7 thì xuất July.
* - Nếu n = 8 thì xuất August.
* - Nếu n = 9 thì xuất September.
* - Nếu n = 10 thì xuất October.
* - Nếu n = 11 thì xuất November.
* - Nếu n = 12 thì xuất December.
* - Ngược lại xuất Not define.
*/

using System;

namespace Buoi03
{
    class Bai04
    {
        public static void ChayBai04()
        {
            // Khai báo biến
            int n;

            // Nhập dữ liệu
            Console.Write("Moi ban nhap vao thang: ");
            n = int.Parse(Console.ReadLine());

            // Xử lý
            // Xuất kết quả
            Console.Write("Tieng anh cua thang {0} la: ", n);
            switch (n)
            {
                case 1:
                Console.WriteLine("January.");
                break;
                case 2:
                Console.WriteLine("February.");
                break;
                case 3:
                Console.WriteLine("March.");
                break;
                case 4:
                Console.WriteLine("April.");
                break;
                case 5:
                Console.WriteLine("May.");
                break;
                case 6:
                Console.WriteLine("June.");
                break;
                case 7:
                Console.WriteLine("July.");
                break;
                case 8:
                Console.WriteLine("August.");
                break;
                case 9:
                Console.WriteLine("September.");
                break;
                case 10:
                Console.WriteLine("October.");
                break;
                case 11:
                Console.WriteLine("November.");
                break;
                case 12:
                Console.WriteLine("December.");
                break;
                default:
                Console.WriteLine("Not define.");
                break;
            }
        }
    }
}

/*
- TEST CASE 1:
Input: 1
Output: Tieng anh cua thang 1 la: January.

- TEST CASE 2:
Input: 12
Output: Tieng anh cua thang 12 la: December.

- TEST CASE 3: 
Input: 0
Output: Tieng anh cua thang 0 la: Not define.

- TEST CASE 4: 
Input: 13
Output: Tieng anh cua thang 13 la: Not define.

- TEST CASE 5: 
Input: -1
Output: Tieng anh cua thang -1 la: Not define.
*/