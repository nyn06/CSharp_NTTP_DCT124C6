using System;

namespace NNLT.Buoi10
{
    class Program
    {
        public static void Main(string[] args)
        {
            int luaChon;

            do
            {
                Console.WriteLine("\n========== MENU ==========");
                Console.WriteLine("1. Cau 01 - Trung binh cong phan tu cuc tri");
                Console.WriteLine("2. Cau 02 - Ke nhau trai dau");
                Console.WriteLine("3. Cau 03 - Day dan dau");
                Console.WriteLine("4. Cau 04 - Tim doan");
                Console.WriteLine("0. Thoat");
                Console.WriteLine("==========================");

                Console.Write("Moi ban chon: ");
                luaChon = int.Parse(Console.ReadLine()!);

                switch (luaChon)
                {
                    case 1:
                        Cau01.TrungBinhCongCucTri();
                        break;

                    case 2:
                        Cau02.KeNhauTraiDau();
                        break;

                    case 3:
                        Cau03.DayDanDau();
                        break;

                    case 4:
                        Cau04.TimDoan();
                        break;

                    case 0:
                        Console.WriteLine("Ket thuc chuong trinh!");
                        break;

                    default:
                        Console.WriteLine("Lua chon khong hop le!");
                        break;
                }
                Console.WriteLine();
                Console.WriteLine();

            } while (luaChon != 0);
        }
    }
}