// Người thực hiện: Lưu Huyền Thư 
// MSSV: 3124411300
// Ngày thực hiện: 10/9/2026
/* Ý tưởng thực hiện 
- Nhập mảng 2 chiều
- Dùng dòng lệnh for lồng nhau để duyệt qua từng phần tử 
- Tính tổng cộng dồn theo từng cột   
- Dùng biến max để lưu giá trị tổng lớn nhất và dùng mảng vt để lưu các cột có tổng lớn nhất
*/
using System;
namespace NNLTCSharp.Buoi12
{
    class Bai4
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
        public static void Timtong(int[,] a)
        {
            int m = a.GetLength(1);
            int n = a.GetLength(0);
            int[] vt = new int[m];
            int d = 0;
            int max= int.MinValue;
            for(int j = 0; j < m; j++)
            {
                int s =0;
                for(int i = 0; i < n; i++)
                {
                    s += a[i,j];
                }
                if(s > max)
                {
                    max = s;
                    d=0;
                    vt[d] = j+1;
                    d++;
                }
                else if(s == max)
                {
                    vt[d] = j+1;
                    d++;
                }
            }
            Console.Write("Cac cot co tong lon nhat la: ");
            for(int i = 0; i < d; i++)
            {
                Console.Write(vt[i] + " ");
            }
            Console.WriteLine("\nVoi tong la: {0}", max);
        }

         public static void test()
        {
            int[,] a = { { 1, 3, 5 }, { 7, 9, 11 } };
            XuatMang(a);
            Timtong(a);
            int[,] b = { { 1, 3, 5 }, { 7, 9, 10 } };
            XuatMang(b);
            Timtong(b);
            int[,] c = { { 2, 4, 6 }, { 8, 10, 12 } };
            XuatMang(c);
            Timtong(c);
            int[,] d = { { 1, 3, 5 }, { 7, 0, 9 } };
            XuatMang(d);
            Timtong(d);
        }
        public static void Main(string[] args)
        {
            int[,] a;
            a = new int[0,0];
            NhapMang(ref a);
            XuatMang(a);
            Timtong(a);
            test();
        }
    }
} 