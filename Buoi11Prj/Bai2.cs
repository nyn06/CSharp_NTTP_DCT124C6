// Người Thực Hiện: Lưu Huyền Thư
// MSSV: 3124411300
// Ngày thực hiện: 9/9/2006
/* Ý tưởng thực hiện
- Cho chạy vòng lặp for duyệt qua từng phần tử trong mảng 
- Cho kiểm tra điều kiện nếu nó là số chẵn thì xóa 
*/
using System;
namespace NNLCSharp.Buoi11
{
    class Bai2
    {
        // hàm nhập mảng 
        public static void NhapMang(int[] a, int n)
        {
            for (int i = 0; i < n; i++)
            {
                Console.Write("Nhap phan thu thu a[i]: ");
                a[i] = int.Parse(Console.ReadLine()!);
            }
        }
        // hàm xuất mảng 
        public static void XuatMang(int[] a, int n)
        {
            for(int i=0; i<n; i++)
            {
                Console.Write(a[i] + " ");
            }
        }
        // hàm xóa 1 phẩn tử tại 1 vị trí xác định 
        public static void Xoa(int[] a, int vt, ref int n)
        {
            for (int i = vt + 1; i < n; i++)
            {
                a[i - 1] = a[i];
            }
            n--;
        }
        // hàm xóa các phần tử chẳn 
        public static void Xoachan(int[] a, ref int n)
        {
            for(int i=0; i<n; i++)
            {
                // cho duyệt qua từng phần tử và kiểm tra nếu là số chẵn thì xóa tại vị trí đó 
                if ( a[i] % 2 == 0)
                {
                    // gọi hàm xóa để xóa phần tử tại vị trí i
                    Xoa(a, i, ref n);
                    i--;
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
            Xoachan(a, ref n);
            Console.WriteLine("\nMang sau khi xoa so chan la: ");
            XuatMang(a, n);

            Console.WriteLine("\nMang ban dau: ");
            XuatMang(b, m); 
            Xoachan(b, ref m);
            Console.WriteLine("\nMang sau khi xoa so chan la:");
            XuatMang(b, m);

            Console.WriteLine("\nMang ban dau: ");
            XuatMang(c, k); 
            Xoachan(c, ref k);
            Console.WriteLine("\nMang sau khi xoa so chan la: ");
            XuatMang(c, k);

        }
        public static void Main2(string[] args)
        {
            // khai báo n
            int n;
            Console.Write("Nhap so luong phan tu: ");
            // cho nhập n từ bàn phím
            n = int.Parse(Console.ReadLine()!);
            // khai báo mảng 1 chiều
            int [] a = new int[n];
            NhapMang(a, n);  // cho nhập mảng từ bàn phím 
            Console.WriteLine("Mang vua nhap la: ");
            XuatMang(a,n);  // in mảng vừa nhập ra màn hình 
            Xoachan(a, ref n);  // xóa các phần tử chẳn 
            Console.WriteLine("\nMang sau khi xoa so chan la: ");
            XuatMang(a, n);  // in ra mảng sau khi đã xóa phần tử chẵn 
            test();
        }

    }
}
