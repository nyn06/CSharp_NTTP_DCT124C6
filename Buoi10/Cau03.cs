/*
* CHƯƠNG TRÌNH TÍNH TOÁN BIỂU THỨC
* Tác giả: Huỳnh Thị Mỹ Tiên
* Ngày viết: 17/09/2026
*
* Phát biểu đề bài:
* Cho dãy số thực có n phần tử (n <= 500).
* Hãy xác định dãy có tính chất các phần tử kề nhau
* là trái dấu nhau hay không.
*
* Ý tưởng:
* - Nhập số lượng phần tử n của mảng.
* - Kiểm tra n phải lớn hơn 0 và không vượt quá 500.
* - Khai báo mảng số thực có n phần tử.
* - Nhập các phần tử của mảng.
* - Xuất các phần tử trong mảng.
* - Ban đầu giả sử dãy là dãy đan dấu.
* - Duyệt từng cặp phần tử kề nhau trong mảng.
* - Nếu tích của hai phần tử kề nhau lớn hơn hoặc bằng 0
*   thì hai phần tử không trái dấu, do đó dãy không đan dấu.
* - Sau khi kiểm tra toàn bộ dãy, dựa vào biến danDau
*   để xuất kết quả.
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
*     Xuất các phần tử trong mảng
*
*     danDau = true
*
*     Cho i chạy từ 0 đến n - 2
*         Nếu a[i] * a[i + 1] >= 0 thì
*             danDau = false
*         Kết thúc nếu
*     Kết thúc vòng lặp
*
*     Nếu danDau == true thì
*         Xuất "Day so tren la day dan dau."
*     Ngược lại
*         Xuất "Day so tren khong phai la day dan dau."
*     Kết thúc nếu
* Kết thúc
*/

namespace NNLT.Buoi10
{
    class Cau03
    {
         //Kiểm tra dãy đan dấu
        public static void DayDanDau()
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

            bool danDau = true;

             //Duyệt các cặp phần tử kề nhau
            for (int i = 0; i < n - 1; i++)
            {
                //Kiểm tra hai phần tử kề nhau có trái dấu không
                if (a[i] * a[i + 1] >= 0)
                {
                    danDau = false;
                }
            }

            if (danDau)
            {
                Console.WriteLine("Day so tren la day dan dau.");
            }
            else
            {
                Console.WriteLine("Day so tren khong phai la day dan dau.");
            }


        }
    }
}

/*
TEST CASE 1:
Input:
n = 7
a = -2 5 -3 4 -6 2 -1

Output:
Day so tren la day dan dau.


TEST CASE 2:
Input:
n = 5
a = -2 5 3 -4 6

Output:
Day so tren khong phai la day dan dau.


TEST CASE 3:
Input:
n = 5
a = -1 2 -3 4 -5

Output:
Day so tren la day dan dau.


TEST CASE 4:
Input:
n = 5
a = -1 -2 0 3 4

Output:
Day so tren khong phai la day dan dau.
*/