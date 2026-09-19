/*
* CHƯƠNG TRÌNH TÌM ĐOẠN LIÊN TIẾP DÀI NHẤT CÓ TỔNG BẰNG K
* Tác giả: Huỳnh Thị Mỹ Tiên
* Ngày viết: 18/09/2026
*
* Phát biểu đề bài:
* Cho dãy số thực có n phần tử (n <= 500).
* Hãy tìm đoạn liên tiếp dài nhất trong dãy có tổng các phần tử
* bằng số thực k được nhập từ bàn phím.
* Nếu có nhiều đoạn có cùng độ dài lớn nhất thì lấy đoạn xuất hiện
* đầu tiên trong dãy.
*
* Ý tưởng:
* - Nhập số lượng phần tử n của mảng.
* - Kiểm tra n phải lớn hơn 0 và không vượt quá 500.
* - Khai báo mảng số thực có n phần tử.
* - Nhập các phần tử của mảng.
* - Xuất các phần tử trong dãy.
* - Nhập số k cần tìm.
* - Khai báo vị trí đầu và vị trí cuối của đoạn dài nhất.
* - Khai báo biến doDaiMax để lưu độ dài lớn nhất.
* - Duyệt từng vị trí i làm vị trí bắt đầu của đoạn.
* - Với mỗi vị trí i, tính tổng các phần tử từ i đến j.
* - Nếu tổng bằng k thì tính độ dài của đoạn.
* - Nếu độ dài tìm được lớn hơn độ dài lớn nhất thì cập nhật
*   vị trí đầu, vị trí cuối và độ dài lớn nhất.
* - Sau khi duyệt hết mảng, kiểm tra có tìm được đoạn hay không.
* - Nếu không có đoạn nào thì xuất thông báo.
* - Nếu có thì xuất vị trí đầu, vị trí cuối và các phần tử
*   trong đoạn dài nhất.
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
*     Nhập k
*
*     viTriDau = -1
*     viTriCuoi = -1
*     doDaiMax = 0
*
*     Cho i chạy từ 0 đến n - 1
*         tong = 0
*
*         Cho j chạy từ i đến n - 1
*             tong = tong + a[j]
*
*             Nếu tong bằng k thì
*                 doDai = j - i + 1
*
*                 Nếu doDai > doDaiMax thì
*                     doDaiMax = doDai
*                     viTriDau = i
*                     viTriCuoi = j
*                 Kết thúc nếu
*             Kết thúc nếu
*         Kết thúc vòng lặp
*     Kết thúc vòng lặp
*
*     Nếu viTriDau == -1 thì
*         Xuất "Khong co doan nao co tong bang k"
*     Ngược lại
*         Xuất vị trí đầu và vị trí cuối của đoạn
*         Cho i chạy từ viTriDau đến viTriCuoi
*             Xuất a[i]
*         Kết thúc vòng lặp
*     Kết thúc nếu
* Kết thúc
*/

using System;

namespace NNLT.Buoi10
{
    class Cau04
    {
        public static void TimDoan()
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

            //Nhập giá trị k cần tìm
            Console.Write("Moi ban nhap so k: ");
            double k = double.Parse(Console.ReadLine()!);

            //Khởi tạo vị trí đầu, cuối và độ dài lớn nhất
            int viTriDau = -1;
            int viTriCuoi = -1;
            int doDaiMax = 0;
            
            double tong = 0;

            //Duyệt từng vị trí làm vị trí bắt đầu của đoạn
            for (int i = 0; i < n; i++)
            {
                tong = 0;

                for (int j = i; j < n; j++)
                {
                    tong = tong + a[j];

                    //Kiểm tra tổng của đoạn có bằng k hay không
                    if (Math.Abs(tong - k) < 0.000001)
                    {
                        int doDai = j - i + 1;

                        //Cập nhật nếu tìm được đoạn dài hơn
                        if (doDai > doDaiMax)
                        {
                            doDaiMax = doDai;
                            viTriDau = i;
                            viTriCuoi = j;
                        }
                    }
                }
            }

            if (viTriDau == -1)
            {
                Console.WriteLine("Khong co doan nao co tong bang " + k);
            }

            else
            {
                //Xuất vị trí và các phần tử của đoạn dài nhất
                Console.Write("Doan [" + viTriDau + ", " + viTriCuoi + "] dai nhat co tong bang " + k + ": ");
                    
                for (int i = viTriDau; i <= viTriCuoi; i++)
                {
                        Console.Write(a[i] + " ");
                }        
            }
        }
    }
}

/*
TEST CASE 1:
Input:
n = 7
a = -2 5 -3 4 -6 2 -1
k = 0

Output:
Day so co 7 phan tu: -2 5 -3 4 -6 2 -1
Moi ban nhap so k: 0
Doan [0, 5] dai nhat co tong bang 0: -2 5 -3 4 -6 2


TEST CASE 2:
Input:
n = 5
a = 1 2 3 4 5
k = 6

Output:
Day so co 5 phan tu: 1 2 3 4 5
Moi ban nhap so k: 6
Doan [0, 2] dai nhat co tong bang 6: 1 2 3


TEST CASE 3:
Input:
n = 6
a = 2 1 3 4 2 5
k = 10

Output:
Day so co 6 phan tu: 2 1 3 4 2 5
Moi ban nhap so k: 10
Doan [0, 3] dai nhat co tong bang 10: 2 1 3 4


TEST CASE 4:
Input:
n = 7
a = -2 3 4 -1 2 5 -3
k = 8

Output:
Day so co 7 phan tu: -2 3 4 -1 2 5 -3
Moi ban nhap so k: 8
Doan [0, 6] dai nhat co tong bang 8: -2 3 4 -1 2 5 -3


TEST CASE 5:
Input:
n = 5
a = 1.5 2.5 3 4 5
k = 7

Output:
Day so co 5 phan tu: 1.5 2.5 3 4 5
Moi ban nhap so k: 7
Doan [0, 2] dai nhat co tong bang 7: 1.5 2.5 3


TEST CASE 6:
Input:
n = 5
a = 1 2 4 5 7
k = 20

Output:
Day so co 5 phan tu: 1 2 4 5 7
Moi ban nhap so k: 20
Khong co doan nao co tong bang 20
*/