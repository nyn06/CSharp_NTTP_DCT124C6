/*
 * CHƯƠNG TRÌNH TìM HAI CHỮ SỐ CUỐI CÙNG
 * Tác giả : Huỳnh Thị Mỹ Tuyền
 * Ngày viết: 15/09/2026

 * Nhập số nguyên n, tính 2 chữ số cuối cùng của n^278
 * để lấy 2 chữ số cuối, ta lấy n^278 % 100
 */
 using System;
 namespace NNLTCSharp.Buoi2
{
    class HaiChuSoCuoi
    {
        public static void Bai5()
        {
            int n, kq = 1; //gán biến kq bắt đầu = 1 để tính lũy thừa

            //nhập dữ liệu
            Console.Write("Nhap so nguyen n: ");
            n = int.Parse(Console.ReadLine());

            //xử lý
            for (int i = 1; i <= 278; i++)
            {
                kq = (kq * n) % 100; //lấy 2 chữ số cuối cùng
            }

            //xuất kết quả
            Console.WriteLine("{0}^278 co 2 chu so cuoi cung la {1}.", n, kq);
        }
    }
}