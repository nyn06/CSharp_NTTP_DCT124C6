/*
* CHƯƠNG TRÌNH TÍNH TOÁN BIỂU THỨC
* Tác giả: Huỳnh Thị Mỹ Tiên
* Ngày viết: 10/09/2026
*
* Phát biểu đề bài:
* Nhập vào số nguyên dương n.
* Hãy đếm số lần xuất hiện của từng chữ số trong số n.
*
* Ý tưởng:
* - Nhập số nguyên dương n từ bàn phím.
* - Viết hàm DemChuSo() để đếm số lần xuất hiện của một chữ số.
* - Dùng phép chia lấy dư cho 10 để lấy chữ số cuối.
* - Nếu chữ số lấy được bằng chữ số cần tìm thì tăng biến đếm.
* - Chia n cho 10 để bỏ chữ số cuối.
* - Dùng vòng lặp cho chữ số chạy từ 0 đến 9.
* - Gọi hàm DemChuSo() để đếm số lần xuất hiện của từng chữ số.
* - Chỉ in những chữ số có số lần xuất hiện lớn hơn 0.
*
* Mã giả:
* Bắt đầu
*     Nhập n
*     Nếu n <= 0 thì
*         Xuất "Du lieu khong hop le!"
*         Kết thúc
*     Kết thúc nếu
*
*     Cho i chạy từ 0 đến 9
*         dem = DemChuSo(n, i)
*         Nếu dem > 0 thì
*             Xuất số i và số lần xuất hiện
*         Kết thúc nếu
*     Kết thúc vòng lặp
* Kết thúc
*/

using System;

namespace NNLT.Buoi08
{
    class Cau04
    {
        //Đếm số lần xuất hiện của một chữ số
        public static int DemChuSo(int n, int cs)
        {
            int dem = 0;

            while (n > 0)
            {
                //Lấy chữ số cuối
                int ChuSo = n % 10;

                //Kiểm tra chữ số 
                if (ChuSo == cs)
                {
                    dem++;
                }

                //Bỏ chữ số cuối
                n = n / 10;
            }
            return dem;
        }

        //Liệt kê chữ số 
        public static void LKChuSo()
        {
            Console.Write("Moi ban nhap so nguyen n: ");
            int n = int.Parse(Console.ReadLine()!);

             //Kiểm tra dữ liệu
            if (n <= 0)
            {
                Console.WriteLine("Du lieu khong hop le!");
                return;
            }

            //Duyệt các chữ số từ 0 đến 9 
            for (int i = 0; i <= 9; i++)
            {
                    int dem = DemChuSo(n, i);

                    if (dem > 0)
                    {
                        Console.WriteLine("Chu so " + i + " xuat hien " + dem + " lan.");
                    }
            }
        }
    }
}

/*
TEST CASE 1:
Input:
n = 13324

Output:
Chu so 1 xuat hien 1 lan.
Chu so 2 xuat hien 1 lan.
Chu so 3 xuat hien 2 lan.
Chu so 4 xuat hien 1 lan.


TEST CASE 2:
Input:
n = 123456789

Output:
Chu so 1 xuat hien 1 lan.
Chu so 2 xuat hien 1 lan.
Chu so 3 xuat hien 1 lan.
Chu so 4 xuat hien 1 lan.
Chu so 5 xuat hien 1 lan.
Chu so 6 xuat hien 1 lan.
Chu so 7 xuat hien 1 lan.
Chu so 8 xuat hien 1 lan.
Chu so 9 xuat hien 1 lan.


TEST CASE 3:
Input:
n = 112233

Output:
Chu so 1 xuat hien 2 lan.
Chu so 2 xuat hien 2 lan.
Chu so 3 xuat hien 2 lan.


TEST CASE 4:
Input:
n = 10001

Output:
Chu so 0 xuat hien 3 lan.
Chu so 1 xuat hien 2 lan.


TEST CASE 5:
Input:
n = 0

Output:
Du lieu khong hop le!


TEST CASE 6:
Input:
n = -123

Output:
Du lieu khong hop le!
*/