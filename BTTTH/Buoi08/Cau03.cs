/*
* CHƯƠNG TRÌNH TÍNH TOÁN BIỂU THỨC
* Tác giả: Huỳnh Thị Mỹ Tiên
* Ngày viết: 10/09/2026
*
* Phát biểu đề bài:
* Nhập vào tử số và mẫu số của một phân số.
* Hãy rút gọn phân số đã nhập.
*
* Ý tưởng:
* - Nhập tử số và mẫu số từ bàn phím.
* - Kiểm tra mẫu số có bằng 0 hay không.
* - Nếu mẫu số bằng 0 thì thông báo dữ liệu không hợp lệ.
* - Tìm ước chung lớn nhất (UCLN) của tử số và mẫu số.
* - Chia tử số và mẫu số cho UCLN để rút gọn phân số.
* - Xuất phân số ban đầu và phân số sau khi rút gọn.
*
* Mã giả:
* Bắt đầu
*     Nhập tử số
*     Nhập mẫu số
*     Nếu mẫu số == 0 thì
*         Xuất "Du lieu khong hop le!"
*         Kết thúc
*     Kết thúc nếu
*
*     Tìm UCLN của tử số và mẫu số
*     Tử số = tử số / UCLN
*     Mẫu số = mẫu số / UCLN
*     Xuất phân số sau khi rút gọn
* Kết thúc
*/

using System;

namespace NNLT.Buoi08
{
    class Cau03
    {
        //Tìm ước chung lớn nhất
        public static int UCLN(int a, int b)
        {
            int ucln = 1;

            for (int i = 1; i <= a && i <= b; i ++ )
            {
                if (a % i == 0 && b % i == 0)
                {
                    ucln = i;
                }
            }

            return ucln;
        }

        //Rút gọn phân số
        public static void RutGonPhanSo()
        {
            Console.Write("Moi ban nhap tu so: ");
            int tu = int.Parse(Console.ReadLine()!);
            Console.Write("Moi ban nhap mau so: ");
            int mau = int.Parse(Console.ReadLine()!);

            //Kiểm tra mẫu số 
            if (mau == 0)
            {
               Console.Write("Du lieu khong hop le!");
                return;
            }

            Console.Write("Phan so " + tu + "/" + mau);

            int ucln = UCLN(tu, mau);

            tu = tu / ucln;

            mau = mau / ucln;

            Console.Write(" duoc rut gon thanh " + tu + "/" + mau);

        }
    }

}

/*
TEST CASE 1:
Input:
tu = 12
mau = 18

Output:
Phan so 12/18 duoc rut gon thanh 2/3


TEST CASE 2:
Input:
tu = 20
mau = 30

Output:
Phan so 20/30 duoc rut gon thanh 2/3


TEST CASE 3:
Input:
tu = 7
mau = 5

Output:
Phan so 7/5 duoc rut gon thanh 7/5


TEST CASE 4:
Input:
tu = 12
mau = 0

Output:
Du lieu khong hop le!


TEST CASE 5:
Input:
tu = 15
mau = 25

Output:
Phan so 15/25 duoc rut gon thanh 3/5
*/