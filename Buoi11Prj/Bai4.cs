// Người thực hiện: Lưu Huyền Thư
// MSSV: 3124411300
// Ngày thực hiện: 10/9/2026
/* Ý tưởng thực hiện 
- Cho chạy vòng lặp for ngược từ vị trí phần tử cao nhất rồi in ra màn hình  
*/
using System;
namespace NNLCSharp.Buoi11
{
    class Bai4
    {
        public static void NhapMang(int[] a, int n)
        {
            for (int i = 0; i < n; i++)
            {
                Console.Write("Nhap phan thu thu a[i]: ");
                a[i] = int.Parse(Console.ReadLine()!);
            }
        }
        public static void XuatMang(int[] a, int n)
        {
            for(int i=0; i<n; i++)
            {
                Console.Write(a[i] + " ");
            }
        }
        public static void Daonguoc(int[] a, int n)
        {
            for (int i = n -1 ; i >= 0; i--)
            {
                Console.Write(a[i] + " ");
            }
        }
        public static void test()
        {
            int[] a = { 42, 13, 7, 29, 55, 3, 18, 36, 10, 24 };
            int[] b = { 8, 3, 15, 1, 9, 6, 12, 4, 20, 7};
            int[] c = { 25, 7, 14, 2, 19, 31, 5, 11, 28, 9};
            int n = a.Length;
            int m = b.Length;
            int k = c.Length;
            Console.WriteLine("Mang ban dau: ");
            XuatMang(a, n);
            Console.WriteLine("\nMang sau khi dao nguoc la: ");
            Daonguoc(a, n);

            Console.WriteLine("\nMang ban dau: ");
            XuatMang(b, m); 
            Console.WriteLine("\nMang sau khi dao nguoc la:");
            Daonguoc(b, m);

            Console.WriteLine("\nMang ban dau: ");
            XuatMang(c, k); 
            Console.WriteLine("\nMang sau khi dao nguoc la: ");
            Daonguoc(c, k);

        }
        public static void Main4(string[] args)
        {
            int n;
            Console.Write("Nhap so luong phan tu: ");
            n = int.Parse(Console.ReadLine()!);
            int [] a = new int[n];
            NhapMang(a, n);
            Console.WriteLine("\nMang vua nhap la: ");
            XuatMang(a,n);
            Console.WriteLine("\nMang sau khi dao nguoc la: ");
            Daonguoc(a, n);
            test();
        }

    }
}
