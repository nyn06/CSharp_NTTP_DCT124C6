// Người thực hiện: Lưu Huyền Thư 
// MSSV: 3124411300
// Ngày thực hiện: 10/9/2026
/* Ý tưởng thực hiện 
- Nhập mảng 2 chiều
- Dùng dòng lệnh for lồng nhau để duyệt qua từng phần tử 
- Dùng if else để kiểm tra chẵn lẻ nếu xuất hiện số chẵn thì không phải là ma trận toàn lẻ
*/
using System;
namespace NNLTCSharp.Buoi12
{
    class Bai3
    {
        public static void NhapMang(ref int[,]a)
        {
            Console.Write("Moi ban nhap so dong: ");
            int n= int.Parse(Console.ReadLine()!);
            Console.Write("\nMoi ban nhao so cot: ");
            int m = int.Parse(Console.ReadLine()!);
            a = new int[n,m];
            for(int i = 0; i < n; i++)
            {
                for(int j = 0; j < m; j++)
                {
                    Console.Write("Phan tu a[{0},{1}]: ", i, j);
                    a[i, j] = int.Parse(Console.ReadLine()!);
                }
            }
        }
        public static void XuatMang(int[,] a)
        {
            int n = a.GetLength(0);
            int m = a.GetLength(1);
            Console.WriteLine("Mang co {0} dong va {1} cot la: ", n, m);
            for(int i = 0; i < n; i++)
            {
                for(int j = 0; j < m; j++)
                {
                    Console.Write(a[i, j] + " ");
                }
                Console.WriteLine();
            }
        }
        public static void Kiemtra(int [,] a )
        {
            int n = a.GetLength(0);
            int m = a.GetLength(1);
            int d=0;
            for(int i = 0; i < n; i++)
            {
                for( int j = 0; j < m; j++)
                {
                    if(a[i,j] % 2 == 0)
                    {
                        d++;
                    }
                }
            }
            if(d > 0)
            {
                Console.WriteLine("Mang khong phai la ma tran toan le");
            }
            else
            {
                Console.WriteLine("Mang la ma tran toan le");
            }
        }
        public static void test()
        {
            int[,] a = { { 1, 3, 5 }, { 7, 9, 11 } };
            XuatMang(a);
            Kiemtra(a);
            int[,] b = { { 1, 3, 5 }, { 7, 9, 10 } };
            XuatMang(b);
            Kiemtra(b);
            int[,] c = { { 2, 4, 6 }, { 8, 10, 12 } };
            XuatMang(c);
            Kiemtra(c);
            int[,] d = { { 1, 3, 5 }, { 7, 0, 9 } };
            XuatMang(d);
            Kiemtra(d);
        }
        public static void Main3(string[] args)
        {
            int[,] a;
            a = new int[0,0];
            NhapMang(ref a);
            XuatMang(a);
            Kiemtra(a);
            test();
        }
    }
} 