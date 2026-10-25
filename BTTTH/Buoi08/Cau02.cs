/*
* CHƯƠNG TRÌNH TÍNH TOÁN BIỂU THỨC
* Tác giả: Huỳnh Thị Mỹ Tiên
* Ngày viết: 08/09/2026
*
* Phát biểu đề bài:
* Nhập vào số nguyên dương n.
* Liệt kê các số hoàn hảo trong đoạn [1, n] theo thứ tự giảm dần.
*
* Ý tưởng:
* - Nhập số nguyên dương n từ bàn phím.
* - Duyệt các số từ n giảm dần đến 1.
* - Với mỗi số, tìm các ước dương nhỏ hơn chính nó.
* - Tính tổng các ước tìm được.
* - Nếu tổng các ước bằng chính số đó thì đó là số hoàn hảo.
* - In các số hoàn hảo theo thứ tự giảm dần.
*
* Mã giả:
* Bắt đầu
*     Nhập n
*     Xuất "Cac so hoan hao trong doan [1, n]: "
*     Cho so chạy từ n giảm đến 1
*         tong = 0
*         Cho i chạy từ 1 đến so - 1
*             Nếu so chia hết cho i thì
*                 tong = tong + i
*             Kết thúc nếu
*         Kết thúc vòng lặp
*         Nếu tong == so thì
*             Xuất so
*         Kết thúc nếu
*     Kết thúc vòng lặp
* Kết thúc
*/

using System;

namespace NNLT.Buoi08
{
    class Cau02
    {
        //Liệt kê số hoàn hảo
        public static void SoHoanHao()
        {
            int n; 
            int tong;
            int dem = 0;
        
            Console.Write("Moi ban nhap so n: ");
            n = int.Parse(Console.ReadLine()!);

            if (n <= 0)
            {
                Console.WriteLine("Du lieu khong hop le!");
                return;
            }       

            //Duyệt các số từ n giảm về 1
            for (int so = n; so >= 1; so-- )
            {
                tong = 0;

                for(int i = 1; i < so; i++)
                {
                    if (so % i == 0)
                    {
                        tong = tong + i;
                    } 
                }

                //Kiểm tra số hoàn hảo
                if (tong == so)
                {
                    if (dem == 0)
                    {
                        Console.Write("Cac so hoan hao trong doan [1, " + n + "]: ");
                    }

                    Console.Write(so + " ");
                    dem++;
                }
            }
            //Không có số hoàn hảo
            if (dem == 0)
            {
                Console.WriteLine("Khong co so hoan hao trong doan [1, " + n + "]");
            }
        }
    }
}

/*
TEST CASE 1:
Input:
n = 10

Output:
Cac so hoan hao trong doan [1, 10]: 6


TEST CASE 2:
Input:
n = 30

Output:
Cac so hoan hao trong doan [1, 30]: 28 6


TEST CASE 3:
Input:
n = 100

Output:
Cac so hoan hao trong doan [1, 100]: 28 6


TEST CASE 4:
Input:
n = 500

Output:
Cac so hoan hao trong doan [1, 500]: 496 28 6


TEST CASE 5:
Input:
n = 1

Output:
Khong co so hoan hao trong doan [1, 1]

TEST CASE 5:
Input:
n = -5

Output:
Du lieu khong hop le!
*/