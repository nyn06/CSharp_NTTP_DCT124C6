using System;

namespace NNLT.Buoi08
{
    class Program
    {
        public static void Main(string[] args)
        {
            int luaChon;

            do
            {
                Console.WriteLine("========== MENU BUOI 08 ==========");
                Console.WriteLine("1. Cau 01 - Tinh giai thua");
                Console.WriteLine("2. Cau 02 - Liet ke so hoan hao");
                Console.WriteLine("3. Cau 03 - Rut gon phan so");
                Console.WriteLine("4. Cau 04 - Liet ke chu so");
                Console.WriteLine("0. Thoat");
                Console.WriteLine("==================================");

                Console.Write("Moi ban chon: ");
                luaChon = int.Parse(Console.ReadLine()!);

                switch (luaChon)
                {
                    case 1:
                        Cau01.TinhToHop();
                        break;

                    case 2:
                        Cau02.SoHoanHao();
                        break;

                    case 3:
                        Cau03.RutGonPhanSo();
                        break;

                    case 4:
                        Cau04.LKChuSo();
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