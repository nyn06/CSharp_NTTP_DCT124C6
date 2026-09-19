/*
* CHƯƠNG TRÌNH TÍNH DIỆN TÍCH HÌNH TAM GIÁC
* Tác giả : Huỳnh Thị Mỹ Tuyền
* Ngày viết: 08/09/2026
*
* Phát biểu đề bài: Nhập ba cạnh của tam giác. Tính diện tích tam giác theo công thức Heron. In kết quả với 2 số lẻ thập phân.
* Ý tưởng: Nhập ba cạnh a, b, c. Tính nửa chu vi p = (a + b + c)/2. Tính diện tích S = Math.Sqrt(p*(p - a)*(p - b)*(p - c)). Xuất kết quả với 2 số lẻ thập phân.
* Mã giả:
    bắt đầu
        Nhập a,b,c
        p <- (a + b + c)/2
        S <- Math.Sqrt(p*(p - a)*(p - b)*(p - c))
        Xuất S
    kết thúc
*/
using System;
namespace NNLTCSharp.Buoi1
{
    class DienTichTamGiac
    {
        public static void Bai5()
        {
            double a, b, c, p, S;

            //Nhập
            Console.Write("Nhap do dai canh a: ");
            a=double.Parse(Console.ReadLine());
            Console.Write("Nhap do dai canh b: ");
            b=double.Parse(Console.ReadLine());
            Console.Write("Nhap do dai canh c: ");
            c=double.Parse(Console.ReadLine());

            //Xử lý
            p=(a+b+c)/2;
            S=Math.Sqrt(p*(p-a)*(p-b)*(p-c));

            //Xuất
            Console.WriteLine("Dien tich tam giac S = {0:#.00}",S);
            Console.Read();
        }
    }
}