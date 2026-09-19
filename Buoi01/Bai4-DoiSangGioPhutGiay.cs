/*
* CHƯƠNG TRÌNH ĐỔI THỜI GIAN SANG GIỜ PHÚT GIÂY
* Tác giả : Huỳnh Thị Mỹ Tuyền
* Ngày viết: 08/09/2026
*
* Phát biểu đề bài: Nhập số giây. Chuyển thời gian đó sang giờ, phút giây. In kết quả ra màn hình.
* Ý tưởng: Nhập t giây. Tính h giờ = t / 3600, m phút = (t % 3600) / 60, s giây = (t % 3600) % 60. Xuất kết quả ra màn hình.
* Mã giả:
    bắt đầu
        Nhập t
        h <- t / 3600
        m <- (t % 3600) / 60
        s <- (t % 3600) % 60
        Xuất h,m,s
    kết thúc
*/
using System;
namespace NNLTCSharp.Buoi1
{
    class DoiSangGioPhutGiay
    {
        public static void Bai4()
        {
            //khai báo biến
            int h, m, s, t;
            
            //Nhập
            Console.Write("Nhap vao tong so giay: ");
            t=int.Parse(Console.ReadLine());

            //xử lý
            h=t/3600; //chia lấy phần nguyên của t để ra số giờ
            m=(t%3600)/60; //chia lấy số dư của t rồi chia lấy phần nguyên để ra số phút
            s=(t%3600)%60; //phần dư còn lại sẽ là số giây

            //Xuất
            Console.WriteLine("{0} giay co dang {1}:{2:00}:{3:00}", t, h, m, s);
            Console.Read();
        }
    }
}