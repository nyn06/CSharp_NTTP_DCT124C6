/*
* CHƯƠNG TRÌNH TÍNH TOÁN BIỂU THỨC
* Tác giả: Huỳnh Thị Mỹ Tiên
* Ngày viết: 12/09/2026
*
* Phát biểu đề bài:
* Cho dãy số nguyên có n phần tử (n <= 500).
* Hãy liệt kê các phần tử chẵn trong mảng theo thứ tự từ phải sang trái.
*
* Ý tưởng:
* - Nhập số lượng phần tử n của mảng.
* - Kiểm tra n phải lớn hơn 0 và không vượt quá 500.
* - Khai báo mảng có n phần tử.
* - Nhập các phần tử của mảng.
* - Duyệt mảng từ phần tử cuối về phần tử đầu.
* - Kiểm tra từng phần tử có phải là số chẵn hay không.
* - Nếu là số chẵn thì xuất phần tử đó.
* - Sử dụng biến timThay để kiểm tra mảng có phần tử chẵn hay không.
* - Nếu không có phần tử chẵn thì thông báo "Khong co gia tri chan trong mang.".
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
*     timThay = false
*
*     Cho i chạy từ n - 1 giảm đến 0
*         Nếu a[i] chia hết cho 2 thì
*             Nếu timThay == false thì
*                 Xuất "Cac phan tu chan trong mang: "
*             Kết thúc nếu
*             Xuất a[i]
*             timThay = true
*         Kết thúc nếu
*     Kết thúc vòng lặp
*
*     Nếu timThay == false thì
*         Xuất "Khong co gia tri chan trong mang."
*     Kết thúc nếu
* Kết thúc
*/

using System;

namespace NNLT.Buoi09
{
    class Cau01
    {
        //Liệt kê số chẵn
        public static void LKSoChan()
        {
            int n;
            
            Console.Write("Moi ban nhap so luong phan tu: ");
            n = int.Parse(Console.ReadLine()!);

            //Kiểm tra số lượng phần tử
            if (n <= 0 || n > 500)
            {
                Console.WriteLine("Du lieu khong hop le!");
                return;
            }

            int[] a = new int[n];
            
            //Nhập các phần tử của mảng
            for (int i = 0; i < n; i++)
            {
                Console.Write("Phan tu " + i + ": ");
                a[i] = int.Parse(Console.ReadLine()!);
            }

            bool timThay = false;

            for (int i = n - 1; i >= 0; i--)
            {
                //Kiểm tra a[i] có chẵn không
                if (a[i] % 2 == 0)
                {
                    if (!timThay)
                    {
                        Console.Write("Cac phan tu chan trong mang: ");
                    }

                    Console.Write(a[i] + " ");
                    timThay = true;
                }
            }

            //Kiểm tra có số chẵn hay không
            if (!timThay)
            {
                Console.WriteLine("Khong co gia tri chan trong mang.");
            }
        }
    }
}

/*
TEST CASE 1:
Input:
n = 4
a = 2 3 5 6

Output:
Cac phan tu chan trong mang: 6 2


TEST CASE 2:
Input:
n = 5
a = 1 2 4 7 8

Output:
Cac phan tu chan trong mang: 8 4 2


TEST CASE 3:
Input:
n = 6
a = 10 3 8 5 12 7

Output:
Cac phan tu chan trong mang: 12 8 10


TEST CASE 4:
Input:
n = 4
a = 1 3 5 7

Output:
Khong co gia tri chan trong mang.


TEST CASE 5:
Input:
n = 0

Output:
Du lieu khong hop le!


TEST CASE 6:
Input:
n = 501

Output:
Du lieu khong hop le!
*/