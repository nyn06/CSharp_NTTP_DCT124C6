/*
* CHƯƠNG TRÌNH TÍNH TOÁN BIỂU THỨC
* Tác giả: Huỳnh Thị Mỹ Tiên
* Ngày viết: 12/09/2026
*
* Phát biểu đề bài:
* Cho dãy số thực có n phần tử (n <= 500).
* Hãy đếm dãy số có bao nhiêu số dương, số âm và số không.
*
* Ý tưởng:
* - Nhập số lượng phần tử n của mảng.
* - Kiểm tra n phải lớn hơn 0 và không vượt quá 500.
* - Khai báo mảng số thực có n phần tử.
* - Nhập các phần tử của mảng.
* - Khai báo ba biến đếm số dương, số âm và số không.
* - Duyệt từng phần tử trong mảng.
* - Nếu phần tử lớn hơn 0 thì tăng biến đếm số dương.
* - Nếu phần tử nhỏ hơn 0 thì tăng biến đếm số âm.
* - Nếu phần tử bằng 0 thì tăng biến đếm số không.
* - Xuất kết quả đếm được.
*
* Mã giả:
* Bắt đầu
*     Nhập n
*     Nếu n <= 0 hoặc n > 500 thì
*         Xuất "Du lieu khong hop le!"
*         Kết thúc
*     Kết thúc nếu
*
*     Khai báo mảng a có n phần tử
*     Cho i chạy từ 0 đến n - 1
*         Nhập a[i]
*     Kết thúc vòng lặp
*
*     demDuong = 0
*     demAm = 0
*     demKhong = 0
*
*     Cho i chạy từ 0 đến n - 1
*         Nếu a[i] > 0 thì
*             demDuong = demDuong + 1
*         Ngược lại nếu a[i] < 0 thì
*             demAm = demAm + 1
*         Ngược lại
*             demKhong = demKhong + 1
*         Kết thúc nếu
*     Kết thúc vòng lặp
*
*     Xuất demAm, demKhong, demDuong
* Kết thúc
*/

using System;

namespace NNLT.Buoi09
{
    class Cau03
    {
        //Đếm số âm, số dương và số không
        public static void DemDau()
        {
            int n;
            
            Console.Write("Moi ban nhap so luong phan tu: ");
            n = int.Parse(Console.ReadLine()!);

            if (n <= 0 || n > 500)
            {
                Console.WriteLine("Du lieu khong hop le!");
                return;
            }

            double[] a = new double[n];
            
            int demDuong = 0;
            int demAm = 0;
            int demKhong = 0;

            for (int i = 0; i < n; i++)
            {
                //Nhập các phần tử của mảng
                Console.Write("Phan tu " + i + ": ");
                a[i] = double.Parse(Console.ReadLine()!);   
            }

            //Đếm số âm, số dương và số không
            for (int i = 0; i < n; i++)
            {
                if (a[i] > 0)
                {
                    demDuong++;
                }

                else if (a[i] < 0)
                {
                    demAm++;
                }

                else
                {
                    demKhong++;
                }
            }

            //Xuất kết quả
            Console.Write("Day so co " + demAm + " so am, " + demKhong + " so khong va " + demDuong + " so duong.");

        }
    }
}

/*
TEST CASE 1:
Input:
n = 5
a = -2 5 0 -3 -4

Output:
Day so co 3 so am, 1 so khong va 1 so duong.


TEST CASE 2:
Input:
n = 4
a = 1 2 3 4

Output:
Day so co 0 so am, 0 so khong va 4 so duong.


TEST CASE 3:
Input:
n = 6
a = -5 10 -2 0 7 -8

Output:
Day so co 3 so am, 1 so khong va 2 so duong.


TEST CASE 4:
Input:
n = 0

Output:
Du lieu khong hop le!

TEST CASE 5:
Input:
n = 501

Output:
Du lieu khong hop le!
*/

