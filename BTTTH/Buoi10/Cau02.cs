/*
* CHƯƠNG TRÌNH TÍNH TOÁN BIỂU THỨC
* Tác giả: Huỳnh Thị Mỹ Tiên
* Ngày viết: 15/09/2026
*
* Phát biểu đề bài:
* Cho dãy số thực có n phần tử (n <= 500).
* Hãy xác định số lượng các phần tử kề nhau mà cả hai đều trái dấu.
*
* Ý tưởng:
* - Nhập số lượng phần tử n của mảng.
* - Kiểm tra n phải lớn hơn 0 và không vượt quá 500.
* - Khai báo mảng số thực có n phần tử.
* - Nhập các phần tử của mảng.
* - Xuất các phần tử trong mảng.
* - Duyệt từng cặp phần tử kề nhau trong mảng.
* - Nếu một phần tử âm và phần tử kế tiếp dương,
*   hoặc một phần tử dương và phần tử kế tiếp âm thì tăng biến đếm.
* - Xuất số lượng các cặp phần tử kề nhau trái dấu.
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
*
*     Cho i chạy từ 0 đến n - 1
*         Nhập a[i]
*     Kết thúc vòng lặp
*
*     dem = 0
*
*     Cho i chạy từ 0 đến n - 2
*         Nếu a[i] < 0 và a[i + 1] > 0
*         hoặc a[i] > 0 và a[i + 1] < 0 thì
*             dem = dem + 1
*         Kết thúc nếu
*     Kết thúc vòng lặp
*
*     Xuất dem
* Kết thúc
*/

using System;

namespace NNLT.Buoi10
{
    class Cau02
    {
        //Đếm các phần tử kề nhau trái dấu
        public static void KeNhauTraiDau()
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

            //Nhập các phần tử của mảng
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

            int dem = 0; 

            for (int i = 0; i < n - 1; i++)
            {
                //Kiểm tra hai phần tử kề nhau có trái dấu không
                if ((a[i] < 0 && a[i + 1] > 0) ||
                    (a[i] > 0 && a[i + 1] < 0))
                {
                    dem++;
                }
            }

            Console.WriteLine("So luong cac phan tu ke nhau ma trai dau: " + dem);

        }
    }
}

/*
TEST CASE 1:
Input:
n = 7
a = -2 5 -3 -4 -6 2 1

Output:
So luong cac phan tu ke nhau ma trai dau: 3


TEST CASE 2:
Input:
n = 5
a = 1 -2 3 -4 5

Output:
So luong cac phan tu ke nhau ma trai dau: 4


TEST CASE 3:
Input:
n = 5
a = 1 2 3 4 5

Output:
So luong cac phan tu ke nhau ma trai dau: 0


TEST CASE 4:
Input:
n = 5
a = -1 -2 0 3 4

Output:
So luong cac phan tu ke nhau ma trai dau: 0
*/
