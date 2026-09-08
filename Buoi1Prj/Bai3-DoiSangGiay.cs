/*
* CHƯƠNG TRÌNH ĐỔI THỜI GIAN SANG GIÂY
* Tác giả : Huỳnh Thị Mỹ Tuyền
* Ngày viết: 08/09/2026
*
* Phát biểu đề bài: Nhập số giờ, số phút, số giây. Chuyển thời gian đó sang tổng số giây. In kết quả ra màn hình.
* Ý tưởng: Nhập h giờ, m phút, s giây. Tính tổng số giây = h*3600 + m*60 + s. Xuất kết quả ra màn hình.
* Mã giả:
    bắt đầu
        Nhập h,m,s
        TongsoGiay <- h*3600+m*60+s
        Xuất TongsoGiay
    kết thúc
*/
using System;
namespace NNLTCSharp.Buoi1
{
    class DoiSangGiay
    {
        public static void Bai3()
        {
            //khai báo biến
            int h, m, s, TongsoGiay;

            //Nhập
            Console.Write("Nhap so gio: ");
            h=int.Parse(Console.ReadLine()); 
            Console.Write("Nhap so phut: ");
            m=int.Parse(Console.ReadLine()); 
            Console.Write("Nhap so giay: ");
            s=int.Parse(Console.ReadLine()); 

            //xử lý
            TongsoGiay=h*3600+m*60+s;

            //Xuất
            Console.WriteLine("Tong so giay cua {0}:{1}:{2} là {3} giay",h,m,s,TongsoGiay);

            //Dừng chương trình
            Console.Read();
        }
    }
}