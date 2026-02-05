using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowDemo.ADModel;
using WindowDemo.Models;

namespace WindowDemo
{
    internal class Cart
    {
        public List<Item> items { get; set; } = new List<Item>();

        public List<Item> GetList()
        {
            return items;
        }

        public int GetCount()
        {
            if (items == null) return 0;
            return items.Count;
        }

        bool is_cached { get; set; } = false;
        int cur_price { get; set; } = 0;

        public int GetPrice()
        {
            if (is_cached)
            {
                return cur_price;
            }
            int i = 0;

            List<BoardGameInformation> list_bgi = ADOShopChecker.GetAllProdukts();
            
            foreach (var item in items)
            {
                foreach (var bgi in list_bgi)
                {
                    if (bgi.Title == item.GetTitle()) // For now
                    {
                        i += item.GetAmnt() * Convert.ToInt32(bgi.Pris);
                    }
                }
            }

            if (i != cur_price)
            {
                cur_price = i;
                is_cached = true;
            }

            return i;
        }

        public void AddToCart(BoardGameInformation bg, int id, int amnt = 0)
        {
            Item produkt = new(id, amnt, bg.Title);

            bool will_add = true;
            List<Item> list = new List<Item>();

            if (items.Count > 0)
            {
                foreach (var item in items)
                {
                    if (item.GetId() == produkt.GetId())
                    {
                        item.AdjustAmount(amnt);
                        will_add = false;
                        is_cached = false;
                    }
                }
            }
            if (will_add)
            {
                items.Add(produkt);
                is_cached = false;
            }
        }
    }
}
