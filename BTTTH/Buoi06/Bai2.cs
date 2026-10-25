/*
* Tác giả : TRẦN THẢO NGUYÊN
* Ngày viết: 18/9/2026
*
* Phát biểu đề bài:
* Nhập vào số nguyên dương n và vị trí k.
* Hãy tìm chữ số ở vị trí thứ k tính từ trái sang phải
* của số n.
* Đồng thời tính tổng các chữ số lẻ của n,
* tìm chữ số lẻ nhỏ nhất và lớn nhất.
*
* Ý tưởng:
* - Nhập n và k, yêu cầu n > 0 và k > 0.
* - Đếm số chữ số của n.
* - Dựa vào số chữ số và vị trí k để loại bỏ các chữ số
*   bên phải, sau đó lấy chữ số cần tìm.
* - Duyệt từng chữ số của n bằng phép chia lấy dư 10.
* - Nếu chữ số là số lẻ thì cộng vào tổng,
*   đồng thời cập nhật số lẻ nhỏ nhất và lớn nhất.
*
* Mã giả:
* Nhập n
* Trong khi n <= 0:
*     Nhập lại n
*
* Nhập k
* Trong khi k <= 0:
*     Nhập lại k
*
* Đếm số chữ số d của n
* Tính số lần cần chia = d - k
*
* Lặp số lần cần chia:
*     Chia số tạm cho 10
*
* Lấy chữ số cần tìm = số tạm % 10
* In chữ số ở vị trí k
*
* tong = 0
* max = -1
* min = 10
*
* Trong khi n > 0:
*     s = n % 10
*     Nếu s là số lẻ:
*         tong = tong + s
*         Nếu s > max:
*             max = s
*         Nếu s < min:
*             min = s
*     n = n / 10
*
* In tổng các chữ số lẻ
* In chữ số lẻ nhỏ nhất và lớn nhất
*
* TEST CASE:
*
* Test case 1:
* Input:
* n = 12345
* k = 2
*
* Expected output:
* Chu so vi tri 2 (tinh tu trai) cua 12345 la 2
* Tong cac chu so le cua 12345 la 9
* 12345 co chu so le nho nhat la 1 va lon nhat la 5
*
* Test case 2:
* Input:
* n = 24680
* k = 3
*
* Expected output:
* Chu so vi tri 3 (tinh tu trai) cua 24680 la 6
* Tong cac chu so le cua 24680 la 0
*
* Test case 3:
* Input:
* n = 987654
* k = 4
*
* Expected output:
* Chu so vi tri 4 (tinh tu trai) cua 987654 la 6
* Tong cac chu so le cua 987654 la 21
* 987654 co chu so le nho nhat la 1 va lon nhat la 9
*/
using System;
namespace Buoi6Prj
{
    public class Bai2
    {
        public static void Xulychuoi()
        {
            Console.Write("Nhap so nguyen duong n: ");
            int n = int.Parse(Console.ReadLine()!);
            while (n <= 0)
            {
                Console.Write("n phai lon hon 0. Vui long nhap lai n: ");
                n = int.Parse(Console.ReadLine()!);
            }       
            Console.Write("Nhap k: ");
            int k = int.Parse(Console.ReadLine()!);
            while (k <= 0)
            {
                Console.Write("k phai lon hon 0. Vui long nhap lai k: ");
                k = int.Parse(Console.ReadLine()!);
            } 
            int sobandau = n;
            int tam = n;
            int d = 0;
            while (tam > 0)
            {
                tam /= 10; 
                d++;
            }

            int soLan = d-k;
            int tam2 = n;
            for(int i=0; i<soLan; i++)
            {
                tam2 /= 10;
            }

            int ChuSo = tam2 % 10;
            Console.WriteLine($"Chu so vi tri {k} (tinh tu trai) cua {sobandau} la {ChuSo}");
 
            // tính tổng 
            int tong = 0;
            int max = -1;
            int min = 10;
            while(n>0)
            {
                int s = n % 10;
                if(s%2!=0)
                {
                   tong += s;
                   if(s > max)
                   {
                       max = s;
                   }
                   if(s < min)
                   {
                    min = s;
                   }
                }
                n /=10;
            }
            Console.WriteLine($"Tong cac chu so le cua {sobandau} la {tong}");
            Console.WriteLine($"{sobandau} co chu so le nho nhat la {min} va lon nhat la {max}");
        }
    }
}