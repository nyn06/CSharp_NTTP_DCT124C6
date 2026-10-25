 /*
 * CHƯƠNG TRÌNH TìM QUÍ CỦA THÁNG
 * Tác giả : Huỳnh Thị Mỹ Tuyền
 * Ngày viết: 15/09/2026

 * Nhập tháng, cho biết tháng đó thuộc quí nào.
 * Tháng 1, 2, 3       -> quí 1
 * Tháng 4, 5, 6       -> quí 2
 * Tháng 7, 8, 9       -> quí 3
 * Tháng 10, 11, 12    -> quí 4
 */
 using System;
 namespace NNLTCSharp.Buoi2
{
    class TimQui
    {
        public static void Bai4()
        {
            int thang, qui;

            Console.Write("Nhap thang: ");
            thang = int.Parse(Console.ReadLine());

            //dùng switch-case
            switch (thang)
            {
                case 1:
                case 2:
                case 3:
                    qui = 1;
                    Console.WriteLine("Thang {0} thuoc qui {1}", thang, qui);
                    break;
                case 4:
                case 5:
                case 6:
                    qui = 2;
                    Console.WriteLine("Thang {0} thuoc qui {1}", thang, qui);
                    break;
                case 7:
                case 8:
                case 9:
                    qui = 3;
                    Console.WriteLine("Thang {0} thuoc qui {1}", thang, qui);
                    break;
                case 10:
                case 11:
                case 12:
                    qui = 4;
                    Console.WriteLine("Thang {0} thuoc qui {1}", thang, qui);
                    break;
                default:
                    Console.WriteLine("Thang khong hop le!");
                    break;
            }
        }
    }
}