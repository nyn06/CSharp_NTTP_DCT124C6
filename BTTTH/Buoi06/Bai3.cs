/*
* CHƯƠNG TRÌNH PHÂN TÍCH THỪA SỐ NGUYÊN TỐ
* Tác giả : TRẦN THẢO NGUYÊN
* Ngày viết: 18/9/2026
*
* Phát biểu đề bài:
* Nhập vào số nguyên n > 1.
* Hãy phân tích n thành tích các thừa số nguyên tố
* và biểu diễn kết quả dưới dạng lũy thừa.
*
* Ý tưởng:
* - Nhập n và kiểm tra n > 1.
* - Duyệt các số i từ 2 đến n.
* - Với mỗi i, kiểm tra xem n có chia hết cho i hay không.
* - Nếu chia hết, chia n liên tục cho i và đếm số lần chia.
* - Nếu i là thừa số nguyên tố của n thì in i^dem.
* - Dùng biến daIn để kiểm soát việc in dấu " x "
*   giữa các thừa số.
*
* Mã giả:
* Nhập n
* Trong khi n <= 1:
*     Nhập lại n
*
* In n + " = "
* daIn = false
*
* For i = 2 đến n:
*     dem = 0
*
*     Trong khi n chia hết cho i:
*         n = n / i
*         dem = dem + 1
*
*     Nếu dem > 0:
*         Nếu daIn = true:
*             In " x "
*         In i^dem
*         daIn = true
*
* TEST CASE:
*
* Test case 1:
* Input:
* n = 12
*
* Expected output:
* 12 = 2^2 x 3^1
*
* Test case 2:
* Input:
* n = 60
*
* Expected output:
* 60 = 2^2 x 3^1 x 5^1
*
* Test case 3:
* Input:
* n = 100
*
* Expected output:
* 100 = 2^2 x 5^2
*
* Test case 4:
* Input:
* n = 17
*
* Expected output:
* 17 = 17^1
*/
using System;
namespace Buoi6Prj
{
    public class Bai3
    {
        public static void Phantichthuaso()
        {
            Console.Write("Nhap so nguyen n:");
            int n = int.Parse(Console.ReadLine()!);
            while (n <= 1)
            {
                Console.Write("n phai lon hon 1. Vui long nhap lai n: ");
                n = int.Parse(Console.ReadLine()!);
            }  
            Console.Write(n + " = ");
            bool daIn = false;
            for(int i = 2; i <= n; i++)
            {
                int dem = 0;
                while(n % i ==0)
                {
                    n /=i;
                    dem++;
                }
                if(dem > 0)
                {
                    if(daIn)
                    {
                        Console.Write(" x ");
                    }
                    Console.Write(i + "^" + dem + " ");
                    daIn = true;
                }
            }
        }
    }
}