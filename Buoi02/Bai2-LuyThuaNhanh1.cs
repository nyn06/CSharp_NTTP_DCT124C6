/*
 * CHƯƠNG TRÌNH TÍNH LŨY THỪA NHANH
 * Tác giả : Huỳnh Thị Mỹ Tuyền
 * Ngày viết: 14/09/2026

 * Nhập hai số thực a. Tính a^2; a^5; a^17 chỉ dùng 6 phép nhân. Xuất kết quả.
 * a²  = a × a       ← 1
 * a⁴  = a² × a²     ← 2
 * a⁵  = a⁴ × a      ← 3
 * a¹⁰ = a⁵ × a⁵     ← 4
 * a¹⁵ = a¹⁰ × a⁵    ← 5
 * a¹⁷ = a¹⁵ × a²    ← 6
 */

using System;
namespace NNLTCSharp.Buoi2
{
    class LuyThuaNhanh1
    {
        public static void Bai2()
        {
            double a, a2, a4, a5, a10, a15, a17;

            Console.Write("Nhap so thuc a: ");
            a=double.Parse(Console.ReadLine());

            //Xử lý (6 phép nhân)
            a2 = a * a; //a^2, 1 phép nhân
            a4 = a2 * a2; //a^4, 1 phép nhân
            a5 = a4 * a; //a^5, 1 phép nhân
            a10 = a5 * a5; //a^10, 1 phép nhân
            a15 = a10 * a5; //a^15, 1 phép nhân
            a17 = a15 * a2; //a^17, 1 phép nhân
            //Tổng cộng 6 phép nhân -> phù hợp yêu cầu

            Console.WriteLine("Ket qua: {0}^2 = {1}, {0}^5 = {2}, {0}^17 = {3}", a, a2, a5, a17);
        }
    }
}