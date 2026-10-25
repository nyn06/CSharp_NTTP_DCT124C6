/*
 * CHƯƠNG TRÌNH TÍNH TỔNG ĐOẠN
 * Tác giả : Huỳnh Thị Mỹ Tuyền
 * Ngày viết: 13/09/2026

 * Nhập hai số nguyên a, b. Tính tổng các số nguyên từ a đến b. Xuất kết quả.
 * S = (b(b+1)/2) - ((a-1)((a-1)+1)/2)
 */

using System;
namespace NNLTCSharp.Buoi2
{
    class TongDoan
    {
        public static void Bai1()
        {
            //Khai báo biến
            int a, b, tong;

            //Nhập
            Console.Write("Nhap a: ");
            a=int.Parse(Console.ReadLine()!);
            Console.Write("Nhap b: ");
            b=int.Parse(Console.ReadLine()!);

            //Xử lý
            tong=(b*(b+1)/2) - ((a-1)*a/2); //rút gọn vế ((a-1)((a-1)+1)/2) = ((a-1)*a/2)

            //Xuất
            Console.WriteLine("Tong cua cac so trong doan [{0},{1}] la {2}.", a, b, tong);
        }
    }
}