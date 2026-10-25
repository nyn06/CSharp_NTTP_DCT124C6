/*
* CHƯƠNG TRÌNH TÍNH TOÁN BIỂU THỨC
* Tác giả: Huỳnh Thị Mỹ Tiên
* Ngày viết: 13/09/2026
*
* Phát biểu đề bài:
* Cho dãy số thực có n phần tử (n <= 500) và số thực x.
* Hãy cho biết vị trí đầu tiên, cuối cùng và số lần xuất hiện
* của giá trị x trong dãy số.
* Nếu x không tồn tại thì thông báo giá trị x không tồn tại
* trong dãy số.
*
* Ý tưởng:
* - Nhập số lượng phần tử n của mảng.
* - Kiểm tra n phải lớn hơn 0 và không vượt quá 500.
* - Khai báo mảng số thực có n phần tử.
* - Nhập các phần tử của mảng.
* - Xuất các phần tử trong mảng.
* - Nhập giá trị x cần tìm.
* - Duyệt mảng từ đầu đến cuối.
* - Nếu phần tử bằng x thì tăng biến đếm.
* - Nếu tìm thấy x lần đầu thì lưu vị trí đầu tiên.
* - Mỗi lần tìm thấy x thì cập nhật vị trí cuối cùng.
* - Nếu x không xuất hiện thì thông báo x không tồn tại.
* - Nếu x có xuất hiện thì xuất số lần, vị trí đầu tiên
*   và vị trí cuối cùng.
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
*     Nhập x
*
*     viTriDau = -1
*     viTriCuoi = -1
*     dem = 0
*
*     Cho i chạy từ 0 đến n - 1
*         Nếu a[i] == x thì
*             dem = dem + 1
*
*             Nếu viTriDau == -1 thì
*                 viTriDau = i
*             Kết thúc nếu
*
*             viTriCuoi = i
*         Kết thúc nếu
*     Kết thúc vòng lặp
*
*     Nếu dem == 0 thì
*         Xuất "Gia tri x khong ton tai trong day so."
*     Ngược lại
*         Xuất số lần xuất hiện
*         Xuất vị trí đầu tiên
*         Xuất vị trí cuối cùng
*     Kết thúc nếu
* Kết thúc
*/

using System;

namespace NNLT.Buoi09
{
    class Cau04
    {
        //Tìm kiếm giá trị x trong mảng
        public static void TimKiem()
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
            
            Console.WriteLine("* Tim kiem");

            //Nhập giá trị x cần tìm
            Console.Write("Moi ban nhap gia tri x can tim: ");
            double x = double.Parse(Console.ReadLine()!);

            int viTriDau = -1;
            int viTriCuoi = -1;
            int dem = 0;

            //Duyệt mảng để tìm x
            for (int i = 0; i < n; i++)
            {
                //Kiểm tra phần tử có bằng x không
                if (a[i] == x)
                {
                    //Đếm số lần xuất hiện
                    dem++;

                    //Lưu vị trí đầu tiên
                    if (viTriDau == -1)
                    {
                        viTriDau = i;
                    }

                    //Cập nhật vị trí cuối cùng
                    viTriCuoi = i;
                }
            }

            //Kiểm tra x có tồn tại trong mảng không
            if (dem == 0)
            {
                Console.WriteLine("Gia tri " + x + " khong ton tai trong day so.");
            }
            else
            {
                Console.WriteLine("Gia tri " + x + " xuat hien " + dem
                    + " lan o vi tri dau tien " + viTriDau
                    + " va cuoi cung " + viTriCuoi + ".");
            }
        }
    }
}

/*
TEST CASE 1:
Input:
n = 5
a = -2.9 5 -4 -2.9 -5
x = -2.9

Output:
Gia tri -2.9 xuat hien 2 lan o vi tri dau tien 0 va cuoi cung 3.


TEST CASE 2:
Input:
n = 5
a = 1 2 3 4 5
x = 6

Output:
Gia tri 6 khong ton tai trong day so.


TEST CASE 3:
Input:
n = 4
a = 2.5 3.5 2.5 4
x = 2.5

Output:
Gia tri 2.5 xuat hien 2 lan o vi tri dau tien 0 va cuoi cung 2.
*/