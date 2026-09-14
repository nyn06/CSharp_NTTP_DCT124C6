/*
* CHƯƠNG TRÌNH TÍNH ĐIỂM TRUNG BÌNH
* Tác giả : Nguyễn Thiên Phú
* MSSV: 3124411232
* Ngày viết: 10/09/2026
*
* Phát biểu đề bài: Nhập vào 3 điểm Toán, Lý, Hóa.
* Tính điểm trung bình và phân loại học sinh.
*
* Ý tưởng:
* - Nhập điểm Toán, Lý, Hóa.
* - Tính điểm trung bình theo công thức:
*   DTB = (Toán * 2 + Lý * 3 + Hóa) / 6.
* - Nếu 8 <= DTB <= 10 thì xếp loại Giỏi.
* - Nếu 6.5 <= DTB < 8 thì xếp loại Khá.
* - Nếu 5 <= DTB < 6.5 thì xếp loại Trung bình.
* - Nếu DTB < 5 thì xếp loại Yếu.
* - Xuất điểm trung bình và xếp loại.
*
* Mã giả:
* - Nhập điểm Toán, Lý, Hóa.
* - DTB = (Toán * 2 + Lý * 3 + Hóa) / 6.
* - Nếu DTB >= 8 thì xếp loại Giỏi.
* - Ngược lại nếu DTB >= 6.5 thì xếp loại Khá.
* - Ngược lại nếu DTB >= 5 thì xếp loại Trung bình.
* - Ngược lại xếp loại Yếu.
* - Xuất DTB và xếp loại.
*/

using System;

namespace Buoi04
{
    class Bai01
    {
        public static void ChayBai01()
        {
             // Khai bao bien
            double toan, ly, hoa;
            double dtb;
            string xepLoai;

            // Nhap du lieu
            Console.Write("Moi ban nhap diem toan: ");
            toan = double.Parse(Console.ReadLine());
            
            Console.Write("Moi ban nhap diem ly: ");
            ly = double.Parse(Console.ReadLine());

            Console.Write("Moi ban nhap diem hoa: ");
            hoa = double.Parse(Console.ReadLine());

            // Xu ly
            if (toan < 0 || toan > 10 || ly < 0 || ly > 10 || hoa < 0 || hoa > 10)
            {
                Console.WriteLine("Diem khong hop le.");
                return;
            }
            dtb = (toan * 2 + ly * 3 + hoa) / 6;

            if (dtb >= 8 && dtb <= 10)
            {
                xepLoai = "Gioi";
            }
            else if (dtb >= 6.5 && dtb < 8)
            {
                xepLoai = "Kha";
            }
            else if (dtb >= 5 && dtb < 6.5)
            {
                xepLoai = "Trung binh";
            }
            else
            {
                xepLoai = "Yeu";
            }

            // Xuat ket qua
            Console.WriteLine("Ban co diem trung binh {0} duoc xep loai {1}.", dtb, xepLoai);
        }
    }
}

/*
- TEST CASE 1:
Input: Toan = 7, Ly = 6, Hoa = 8
Output: Ban co diem trung binh 6.666666666666667 duoc xep loai Kha.

- TEST CASE 2:
Input: Toan = 6, Ly = 6, Hoa = 6
Output: Ban co diem trung binh 6 duoc xep loai Trung binh.

- TEST CASE 3:
Input: Toan = 4, Ly = 4, Hoa = 4
Output: Ban co diem trung binh 4 duoc xep loai Yeu.

- TEST CASE 4:
Input: Toan = 10, Ly = 10, Hoa = 10
Output: Ban co diem trung binh 10 duoc xep loai Gioi.

- TEST CASE 5:
Input: Toan = 7, Ly = 6, Hoa = -5
Output: Diem khong hop le.

- TEST CASE 6:
Input: Toan = 15, Ly = 20, Hoa = 8
Output: Diem khong hop le.
*/
