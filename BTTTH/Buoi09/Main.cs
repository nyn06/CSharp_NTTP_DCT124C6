using System;

namespace NNLT.Buoi09
{
    class Program
    {
        public static void Main(string[] args)
        {
            int luaChon;

            do
            {
                Console.WriteLine("========== MENU BUOI 09 ==========");
                Console.WriteLine("1. Cau 01 - Liet ke cac so chan");
                Console.WriteLine("2. Cau 02 - Tim gia tri chan dau tien, cuoi cung");
                Console.WriteLine("3. Cau 03 - Dem dau");
                Console.WriteLine("4. Cau 04 - Tim kiem");
                Console.WriteLine("0. Thoat");
                Console.WriteLine("==================================");

                Console.Write("Moi ban chon: ");
                luaChon = int.Parse(Console.ReadLine()!);

                switch (luaChon)
                {
                    case 1:
                        Cau01.LKSoChan();
                        break;

                    case 2:
                        Cau02.TimGiaTriChan();
                        break;

                    case 3:
                        Cau03.DemDau();
                        break;

                    case 4:
                        Cau04.TimKiem();
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