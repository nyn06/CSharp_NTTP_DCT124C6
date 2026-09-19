/*
 * CHƯƠNG TRÌNH TÍNH BIỂU THỨC NHANH
 * Tác giả : Huỳnh Thị Mỹ Tuyền
 * Ngày viết: 15/09/2026

 * f(x) = 1 + 2*x + 3*x^2 - 4*x^3
 * chỉ dùng phép cộng, trừ, nhân và không quá 8 phép toán
 */
 using System;
namespace NNLTCSharp.Buoi2
{
    class BieuThucNhanh1
    {
        public static void Bai3()
        {
            //khai báo biến
            double x, f, x2, x3;

            //nhập
            Console.Write("Nhap so thuc x: ");
            x = double.Parse(Console.ReadLine());

            //xử lý 
            x2 = x*x; //x^2, 1 phép nhân
            x3 = x2*x; //x^3, 1 phép nhân
            f = 1 + 2*x + 3*x2 - 4*x3; //có 3 phép nhân, 2 phép cộng, 1 phép trừ
            //Tổng cộng 8 phép toán -> phù hợp yêu cầu

            //xuất
            Console.WriteLine("f({0}) = {1}", x, f);
        }
    }
}