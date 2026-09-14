// Người thực hiện: Lưu Huyền Thư 
// MSSV: 3124411300
// Ngày thực hiện: 10/9/2026
/* Ý tưởng thực hiện 
- Nhập mảng 2 chiều
- Dùng dòng lệnh for lồng nhau để duyệt qua từng phần tử 
- Dùng biến đếm để đếm số lượng phần tử âm dương va 0 thông qua câu lệnh if else
*/
using System;
namespace NNLTCSharp.Buoi12
{
    class Bai2
    {
        public static void NhapMang(ref double[,]a)
        {
            Console.Write("Moi ban nhap so dong: ");
            int n= int.Parse(Console.ReadLine()!);
            Console.Write("\nMoi ban nhao so cot: ");
            int m = int.Parse(Console.ReadLine()!);
            a = new double[n,m];
            for(int i = 0; i < n; i++)
            {
                for(int j = 0; j < m; j++)
                {
                    Console.Write("Phan tu a[{0},{1}]: ", i, j);
                    a[i, j] = double.Parse(Console.ReadLine()!);
                }
            }
        }
        public static void XuatMang(double[,] a)
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
        public static void Dem(double [,] a )
        {
            int n = a.GetLength(0);
            int m = a.GetLength(1);
            int sd = 0, sa = 0, s0 = 0;
            for(int i = 0; i < n; i++)
            {
                for( int j = 0; j < m; j++)
                {
                    if(a[i,j] < 0)
                    {
                        sa++;
                    }
                    else if (a[i,j] > 0)
                    {
                        sd++;
                    }
                    else
                    {
                        s0++;
                    }
                }
            }
            Console.WriteLine("Tren duong bien co " + sd +" so duong,"+ sa +" so am va "+ s0 +" so 0");
        }
         public static void test()
        {
            double[,] a ={{ -3.5,  2.7,  0.0 },{  4.2, -1.8,  6.5 },{  0.0, -7.3,  9.1 }};
            XuatMang(a);
            Dem(a);
            double[,] b ={{ -5.5,  2.3,  0.0,  7.8 },{  4.6, -3.2, -8.1,  1.5 },{  0.0,  9.7, -2.4,  6.3 }};
            XuatMang(b);
            Dem(b);
            double[,] c ={{ 2.0, -4.0, 6.0 },{ 8.0, 10.0, -12.0 }};
            XuatMang(c);
            Dem(c);
            double[,] d ={{ 1.0, 3.0, -5.0 },{ 7.0, 0.0, 9.0 }};
            XuatMang(d);
            Dem(d);
        }
        public static void Main2(string[] args)
        {
            double[,] a;
            a = new double[0,0];
            NhapMang(ref a);
            XuatMang(a);
            Dem(a);
            test();
        }
    }
} 