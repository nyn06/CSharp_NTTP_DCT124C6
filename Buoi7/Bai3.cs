/*
* CHƯƠNG TRÌNH TÍNH SỐ PI
* Tác giả : TRẦN THẢO NGUYÊN
* Ngày viết: 18/9/2026
*
* Phát biểu đề bài:
* Nhập vào sai số epsilon > 0.
* Tính giá trị gần đúng của số Pi với độ chính xác
* theo sai số epsilon.
* Sau đó tính sai số thực tế giữa giá trị Pi tính được
* và giá trị Pi của hệ thống.
*
* Ý tưởng:
* - Nhập sai số epsilon và kiểm tra epsilon > 0.
* - Sử dụng công thức:
*   Pi = 4 - 4/3 + 4/5 - 4/7 + ...
* - Dùng biến n để xác định số hạng thứ n.
* - Nếu n là số chẵn thì cộng số hạng vào tổng.
* - Nếu n là số lẻ thì trừ số hạng khỏi tổng.
* - Tiếp tục tính cho đến khi số hạng nhỏ hơn epsilon.
* - In giá trị Pi tính được.
* - Dùng Math.PI để tính sai số thực tế.
*
* Mã giả:
* Nhập epsilon
*
* Trong khi epsilon <= 0:
*     Nhập lại epsilon
*
* tong = 0
* n = 0
*
* Lặp:
*     soHang = 4 / (2*n + 1)
*
*     Nếu n là số chẵn:
*         tong = tong + soHang
*     Ngược lại:
*         tong = tong - soHang
*
*     Nếu soHang < epsilon:
*         Thoát vòng lặp
*
*     n = n + 1
*
* In tong
* saiSo = |Pi thực tế - tong|
* In saiSo
*
* TEST CASE:
*
* Test case 1:
* Input:
* epsilon = 0.1
*
* Expected output:
* So pi tinh duoc den chinh xac 0.1 la: 3.041839...
* Do sai so thuc te la: ...
*
* Test case 2:
* Input:
* epsilon = 0.01
*
* Expected output:
* So pi tinh duoc den chinh xac 0.01 la: 3.136592...
* Do sai so thuc te la: ...
*
* Test case 3:
* Input:
* epsilon = 0.001
*
* Expected output:
* So pi tinh duoc den chinh xac 0.001 la: 3.140592...
* Do sai so thuc te la: ...
*
* Test case 4:
* Input:
* epsilon = -0.1
*
* Expected:
* Chương trình yêu cầu nhập lại epsilon vì epsilon phải lớn hơn 0.
*/
using System;

namespace Buoi7Prj
{
    public class Bai3
    {
        public static void TinhPi()
        {
            Console.Write("Nhap sai so epsilon: ");
            double epsilon = double.Parse(Console.ReadLine()!);

            while (epsilon <= 0)
            {
                Console.Write("Epsilon phai lon hon 0. Vui long nhap lai: ");
                epsilon = double.Parse(Console.ReadLine()!);
            }

            double tong = 0;
            int n = 0;

            while (true)
            {
                double soHang = 4.0 / (2 * n + 1);

                if (n % 2 == 0)
                {
                    tong += soHang;
                }
                else
                {
                    tong -= soHang;
                }

                if (soHang < epsilon)
                {
                    break;
                }

                n++;
            }

            Console.WriteLine($"So pi tinh duoc den chinh xac {epsilon} la: {tong}");

            double saiSo = Math.Abs(Math.PI - tong);

            Console.WriteLine($"Do sai so thuc te la: {saiSo}");
        }
    }
}