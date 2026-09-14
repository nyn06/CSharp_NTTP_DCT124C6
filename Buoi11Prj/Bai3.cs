// Người thực hiện: Lưu Huyền Thư
// MSSV: 3124411300
// Ngày thực hiện: 10/9/2026
/* Ý tưởng thực hiện 
- Cho chạy vòng lặp for duyệt qua từng phần tử tìm 2 số chẵn liên tiếp a[i], a[i+1] 
- Nới rộng độ dài dãy thêm 1 sau đó cho người dùng nhập vào số lẻ muốn thêm và thêm vào giữa a[i] và a[i+1]
*/
using System;
using System.Security.Cryptography;
namespace NNLCSharp.Buoi11
{
    class Bai3
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
        public static int[]  NoiRongMang(int[] a, int sl)
        {
            int [] b = new int [a.Length + sl];
            for (int i = 0; i <= a.Length-1; i++)
            {
                b[i ] = a[i];
            }
            return b;
        }
        public static int[] Themphantu( int [] a, int vt, int giatri)
        {
            a= NoiRongMang(a, 1);
            for(int i=a.Length -1; i>=vt; i--)
            {
                a[i] = a[i-1];
            }
            a[vt] = giatri;
            return a;
        }
        public static void Themle(ref int[] a,ref int n)
        {
            for(int i=0; i < n-1; i++)
            {
                if ( a[i] % 2 == 0 && a[i+1] % 2 == 0)
                {
                    Console.Write("Nhap so le ban muon them vao giua " + a[i] + " va " + a[i+1] + ": ");
                    int giatri = int.Parse(Console.ReadLine()!);
                    a = Themphantu(a, i+1, giatri);
                    n++;
                }
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
            Themle(ref a, ref n);
            Console.WriteLine("\nMang sau khi them so le la: ");
            XuatMang(a, n);

            Console.WriteLine("\nMang ban dau: ");
            XuatMang(b, m); 
            Themle(ref b, ref m);
            Console.WriteLine("\nMang sau khi them so le la:");
            XuatMang(b, m);

            Console.WriteLine("\nMang ban dau: ");
            XuatMang(c, k); 
            Themle(ref c, ref k);
            Console.WriteLine("\nMang sau khi them so le la: ");
            XuatMang(c, k);

        }
        public static void Main3(string[] args)
        {
            int n;
            Console.Write("Nhap so luong phan tu: ");
            n = int.Parse(Console.ReadLine()!);
            int [] a = new int[n];
            NhapMang(a, n);
            Console.WriteLine("Mang vua nhap la: ");
            XuatMang(a,n);
            Themle(ref a, ref n);
            Console.WriteLine("\nMang sau khi them so le la: ");
            XuatMang(a, n);
            test();
        }

    }
}
