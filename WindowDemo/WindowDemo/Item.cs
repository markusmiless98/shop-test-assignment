using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using WindowDemo.ADModel;

namespace WindowDemo
{
    internal class Item
    {
        int Id { get; set; }
        int num { get; set; }
        string? Name { get; set; }

        public Item(int i, int amount, string name)
        {
            Id = i;
            num = amount;
            Name = name;
        }

        public int GetId()
        {
            return Id;
        }

        public string GetTitle()
        {
            return Name;
        }

        public int GetAmnt()
        {
            return num;
        }

        public int GetPrice()
        {
            List<BoardGameInformation> list_bgi = ADOShopChecker.GetAllProdukts();

            foreach (var bgi in list_bgi)
            {
                if (bgi.Title == GetTitle()) // For now
                {
                    return Convert.ToInt32(bgi.Pris);
                }
            }

            return 0;
        }

        public void AdjustAmount(int amnt)
        {
            num += amnt;
            if (num < 0)
            {
                num = 0;
            }
        }
    }
}
