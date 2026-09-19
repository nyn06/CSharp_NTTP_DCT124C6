/*
* CHƯƠNG TRÌNH TÍNH TỔNG CÁC PHẦN TỬ CỰC TRỊ
* Tác giả: Huỳnh Thị Mỹ Tiên
* Ngày viết: 13/09/2026
*
* Phát biểu đề bài:
* Cho dãy số thực có n phần tử (n <= 500).
* Hãy tính tổng các phần tử cực trị có trong dãy số.
* Biết một phần tử được gọi là cực trị khi nó lớn hơn
* hoặc nhỏ hơn các phần tử xung quanh nó.
*
* Ý tưởng:
* - Nhập số lượng phần tử n của mảng.
* - Kiểm tra n phải lớn hơn 0 và không vượt quá 500.
* - Khai báo mảng số thực có n phần tử.
* - Nhập các phần tử của mảng.
* - Xuất các phần tử trong dãy.
* - Khai báo biến tong để lưu tổng các phần tử cực trị.
* - Duyệt các phần tử từ vị trí 1 đến n - 2.
* - Không xét phần tử đầu và phần tử cuối vì không có đủ
*   hai phần tử xung quanh.
* - Nếu phần tử hiện tại lớn hơn cả hai phần tử xung quanh
*   hoặc nhỏ hơn cả hai phần tử xung quanh thì đó là cực trị.
* - Cộng phần tử cực trị vào biến tong.
* - Xuất tổng các phần tử cực trị.
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
*     Xuất các phần tử trong dãy
*
*     tong = 0
*
*     Cho i chạy từ 1 đến n - 2
*         Nếu a[i] > a[i - 1] và a[i] > a[i + 1]
*         hoặc a[i] < a[i - 1] và a[i] < a[i + 1] thì
*             tong = tong + a[i]
*         Kết thúc nếu
*     Kết thúc vòng lặp
*
*     Xuất tong
* Kết thúc
*/

using System;

namespace NNLT.Buoi10
{
    class Cau01
    {
        public static void TrungBinhCongCucTri()
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

            double[] a = new double[n];

            for (int i = 0; i < n; i++)
            {
                Console.Write("Phan tu " + i + ": ");
                a[i] = double.Parse(Console.ReadLine()!);

            }

            //Xuất các phần tử trong mảng
            Console.Write("Day so co " + n + " phan tu: ");

            for (int i = 0; i < n; i++)
            {
                Console.Write(a[i] + " ");
            }

            Console.WriteLine();

            double tong = 0;

            for (int i = 1; i < n - 1; i++)
            {
                if (a[i] > a[i - 1] && a[i] > a[i + 1] ||
                   (a[i] < a[i - 1] && a[i] < a[i + 1]))
                {
                    tong = tong + a[i];
                }
            }

            Console.WriteLine("Tong cac phan tu cuc tri co trong day so: " + tong);

        }
    }
}

/*
TEST CASE 1:
Input:
n = 7
a = -2 5 -3 -4 -6 2 1

Output:
Day so co 7 phan tu: -2 5 -3 -4 -6 2 1
Tong cac phan tu cuc tri co trong day so: 1


TEST CASE 2:
Input:
n = 5
a = 1 5 2 8 3

Output:
Day so co 5 phan tu: 1 5 2 8 3
Tong cac phan tu cuc tri co trong day so: 15


TEST CASE 3:
Input:
n = 6
a = 2 1 4 3 6 5

Output:
Day so co 6 phan tu: 2 1 4 3 6 5
Tong cac phan tu cuc tri co trong day so: 14


TEST CASE 4:
Input:
n = 5
a = 1 2 3 4 5

Output:
Day so co 5 phan tu: 1 2 3 4 5
Tong cac phan tu cuc tri co trong day so: 0


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
