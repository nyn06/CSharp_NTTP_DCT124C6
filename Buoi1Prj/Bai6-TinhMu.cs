/*
* CHƯƠNG TRÌNH HÀM MŨ
* Tác giả : Huỳnh Thị Mỹ Tuyền
* Ngày viết: 08/09/2026
*
* Phát biểu đề bài: Nhập hai số thực a và số nguyên n. Tính a^n.
* Ý tưởng: Nhập hai số a và n. Sử dụng hàm Math.Pow(a, n) để tính a^n. Xuất kết quả.
* Mã giả:
    bắt đầu
        Nhập a,n
        Kết quả <- Math.Pow(a,n)
        Xuất Kết quả
    kết thúc
*/
using System;
namespace NNLTCSharp.Buoi1
{
    class TinhMu
    {
        public static void Bai6()
        {
            double a, Kq;
            int n;

            Console.Write("Nhap so thuc a: ");
            a=double.Parse(Console.ReadLine());
            Console.Write("Nhap so nguyen n: ");
            n=int.Parse(Console.ReadLine());

            //Xử lý
            Kq=Math.Pow(a,n);

            Console.WriteLine("Ket qua {0}^{1} = {2}",a,n,Kq);
            Console.Read();
        }
    }
}