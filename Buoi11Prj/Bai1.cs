using System;
namespace NNLCSharp.Buoi11
{
    class Bai1
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
        public static void SapXepMang(int[] a, int n)
        {
            for (int i = 0; i < n-1; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    if (a[i] % 2 == 0 && a[j] % 2 ==0 && a[i] > a[j])
                    {
                        int temp = a[i];
                        a[i] = a[j];
                        a[j] = temp;
                    }
                    if (a[i] % 2 != 0 && a[j] % 2 != 0 && a[i] < a[j])
                    {
                        int temp = a[i];
                        a[i] = a[j];
                        a[j] = temp;
                    }
                    if (a[i] % 2 != 0 && a[j] % 2 == 0)
                    {
                        int temp = a[i];
                        a[i] = a[j];
                        a[j] = temp;
                    }
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
            SapXepMang(a, n);
            Console.WriteLine("\nMang sau khi sap xep chan truoc le sau, chan tang le giam la: ");
            XuatMang(a, n);
            Console.WriteLine("\nMang ban dau: ");
            XuatMang(b, m); 
            SapXepMang(b, m);
            Console.WriteLine("\nMang sau khi sap xep chan truoc le sau, chan tang le giam la: ");
            XuatMang(b, m);
             Console.WriteLine("\nMang ban dau: ");
            XuatMang(c, k); 
            SapXepMang(c, k);
            Console.WriteLine("\nMang sau khi sap xep chan truoc le sau, chan tang le giam la: ");
            XuatMang(c, k);

        }
        public static void Main1(string[] args)
        {
            int n;
            Console.Write("Nhap so luong phan tu: ");
            n = int.Parse(Console.ReadLine()!);
            int [] a = new int[n];
            NhapMang(a, n);
            Console.WriteLine("Mang vua nhap la: ");
            XuatMang(a,n);
            SapXepMang(a, n);
            Console.WriteLine("\nMang sau khi sap xep chan truoc le sau, chan tang le giam la: ");
            XuatMang(a, n);
            test();
        }

    }
}
