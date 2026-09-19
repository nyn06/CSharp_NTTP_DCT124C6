/*
* CHƯƠNG TRÌNH TÌM GIÁ TRỊ CHẴN ĐẦU TIÊN, CUỐI CÙNG
* Tác giả: Huỳnh Thị Mỹ Tiên
* Ngày viết: 12/09/2026
*
* Phát biểu đề bài:
* Cho dãy số nguyên có n phần tử (n <= 500).
* Hãy tìm giá trị chẵn đầu tiên và cuối cùng của dãy số nguyên.
* Không tính các giá trị chẵn âm.
*
* Ý tưởng:
* - Nhập số lượng phần tử n của mảng.
* - Kiểm tra n phải lớn hơn 0 và không vượt quá 500.
* - Khai báo mảng có n phần tử.
* - Nhập các phần tử của mảng.
* - Duyệt mảng từ trái sang phải để tìm giá trị chẵn dương đầu tiên.
* - Duyệt mảng từ phải sang trái để tìm giá trị chẵn dương cuối cùng.
* - Sử dụng biến bool để kiểm tra có tìm thấy giá trị chẵn hay không.
*
* Mã giả:
* Bắt đầu
*     Nhập n
*     Nếu n < 1 hoặc n > 500 thì
*         Xuất "Du lieu khong hop le!"
*         Kết thúc
*     Kết thúc nếu
*
*     Khai báo mảng a có n phần tử
*     Cho i chạy từ 0 đến n - 1
*         Nhập a[i]
*     Kết thúc vòng lặp
*
*     timDau = false
*     Cho i chạy từ 0 đến n - 1
*         Nếu a[i] > 0 và a[i] chia hết cho 2 thì
*             Xuất vị trí và giá trị a[i]
*             timDau = true
*             Thoát vòng lặp
*         Kết thúc nếu
*     Kết thúc vòng lặp
*
*     Nếu timDau == false thì
*         Xuất "Khong co gia tri chan trong mang."
*     Kết thúc nếu
*
*     timCuoi = false
*     Cho i chạy từ n - 1 giảm đến 0
*         Nếu a[i] > 0 và a[i] chia hết cho 2 thì
*             Xuất vị trí và giá trị a[i]
*             timCuoi = true
*             Thoát vòng lặp
*         Kết thúc nếu
*     Kết thúc vòng lặp
*
*     Nếu timCuoi == false thì
*         Xuất "Khong co gia tri chan trong mang."
*     Kết thúc nếu
* Kết thúc
*/

namespace NNLT.Buoi09
{
    class Cau02
    {
        //Tìm giá trị chẵn đầu tiên và cuối cùng 
        public static void TimGiaTriChan()
        {
            Console.Write("Moi ban nhap so luong phan tu: ");
            int n = int.Parse(Console.ReadLine()!);

            //Kiểm tra số lượng phần tử
            if (n < 1 || n > 500)
            {
                Console.WriteLine("Du lieu khong hop le!");
                return;
            }

            int[] a = new int[n];

            //Nhập các phần tử của mảng
            for(int i = 0; i < n; i++)
            {
                Console.Write("Phan tu " + i + ": ");
                a[i] = int.Parse(Console.ReadLine()!);
            }

            bool TimDau = false;

            //Tìm giá trị chẵn đâu tiên 
            for(int i = 0; i < n; i++)
            {
                if (a[i] > 0 && a[i] % 2 == 0)
                {
                    Console.WriteLine("Gia tri chan dau tien o vi tri " + i + " co gia tri " + a[i]);
                    
                    TimDau = true;

                    break;
                }
            }

            if (!TimDau)
            {
                Console.WriteLine("Khong co gia tri chan trong mang.");   
            }

            bool TimCuoi = false;

            //Tìm giá trị chẵn cuối cùng
            for(int i = n - 1; i >= 0; i--)
            {
                if (a[i] > 0 && a[i] % 2 == 0)
                {
                    Console.WriteLine("Gia tri chan cuoi cung o vi tri " + i + " co gia tri " + a[i]);
                    
                    TimCuoi = true;

                    break;
                }
            }

            if (!TimCuoi)
            {
                Console.WriteLine("Khong co gia tri chan trong mang.");
            }
        }
    }
}

/*
TEST CASE 1:
Input:
n = 5
a = -2 5 8 9 16

Output:
Gia tri chan dau tien o vi tri 2 co gia tri 8.
Gia tri chan cuoi cung o vi tri 4 co gia tri 16.


TEST CASE 2:
Input:
n = 5
a = -2 3 -8 7 -10

Output:
Khong co gia tri chan trong mang.
Khong co gia tri chan trong mang.


TEST CASE 3:
Input:
n = 6
a = 3 4 7 10 9 12

Output:
Gia tri chan dau tien o vi tri 1 co gia tri 4.
Gia tri chan cuoi cung o vi tri 5 co gia tri 12.


TEST CASE 4:
Input:
n = 4
a = 2 5 7 9

Output:
Gia tri chan dau tien o vi tri 0 co gia tri 2.
Gia tri chan cuoi cung o vi tri 0 co gia tri 2.


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