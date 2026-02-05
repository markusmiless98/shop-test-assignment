using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowDemo
{
    internal class ShopExample
    {
        public static void DrawShop()
        {
            List<string> topText1 = new List<string> { "# Fina butiken #", "Allt inom kläder" };
            var windowTop1 = new Window("", 2, 1, topText1);
            windowTop1.Draw();

            List<string> topText2 = new List<string> { "Tröja", "Fin tröja i ull", "Pris: 149 kr", "Tryck A för att köpa" };
            var windowTop2 = new Window("Erbjudande 1", 2, 6, topText2);
            windowTop2.Draw();

            List<string> topText3 = new List<string> { "Byxor", "Lagom långa byxor", "Pris: 299 kr", "Tryck B för att köpa" };
            var windowTop3 = new Window("Erbjudande 2",28, 6, topText3);
            windowTop3.Draw();

            List<string> topText4 = new List<string> { "Läderskor", "Extra flotta", "Pris: 450 kr", "Tryck C för att köpa" };
            var windowTop4 = new Window("Erbjudande 3", 56, 6, topText4);
            windowTop4.Draw();


        }
    }
}
