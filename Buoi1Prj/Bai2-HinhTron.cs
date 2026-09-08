/*
* CHƯƠNG TRÌNH TÍNH TOÁN DIỆN TÍCH VÀ CHU VI HÌNH TRÒN
* Tác giả : Huỳnh Thị Mỹ Tuyền
* Ngày viết: 08/09/2026
*
* Phát biểu đề bài: Nhập bán kính hình tròn, tính diện tích và chu vi hình tròn theo công thức: S = 3,14*r^2; P = 2*3,14*r. In kết quả với 1 chứ số thập phân.
* Ý tưởng: Nhập bán kính r, tính diện tích s và chu vi p theo công thức, xuất S và P với 1 chữ số thập phân.
* Mã giả: 
    bắt đầu
        Nhập r
        s <- 3,14*r^2
        p <- 2*3,14*r
        Xuất s,p
    kết thúc 
*/
using System;

namespace NNLTCSharp.Buoi1
{
    class HinhTron
    {
        public static void Bai2()
        {
            //khai báo biến
            double r, s, p;

            //Nhập dữ liệu
            Console.Write("Nhap ban kinh hinh tron: ");
            r=double.Parse(Console.ReadLine()); // chuyển dữ liệu nhập là kiểu chuỗi sang kiểu số thực

            //Xử lý
            s=3.14*Math.Pow(r,2);
            p=2*3.14*r;

            //Xuất dữ liệu
            Console.WriteLine("Dien tich S = {0:#.0}",s);
            Console.WriteLine("Chu vi P = {0:#.0}",p);

            //Dừng chương trình chờ nhập phím
            Console.Read();
        }
    }
}