using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WindowDemo.ADModel;

namespace WindowDemo
{
    internal class ItemJson
    {
        [JsonRequired]
        public int CustommerId { get; set; }
        [JsonRequired]
        public int ItemId { get; set; }
        public string ItemName { get; set; }

        public int ItemAmount { get; set; }

        public ItemJson(int cus_id, int item_id, string name = "Game", int amount = 1)
        {
            CustommerId = cus_id;
            ItemId = item_id;
            ItemName = name;
            ItemAmount = amount;
        }

        public ItemJson(ItemJson json)
        {
            CustommerId = json.CustommerId;
            ItemId = json.ItemId;
            ItemName = json.ItemName;
            ItemAmount = json.ItemAmount;
        }

        public ItemJson(string alternative)
        {
            string[] arr = alternative.Split(',');
            if (arr.Count() != 4)
            {
                // Failure
            }
            else
            {
                int i = 0;
                foreach (var item in arr)
                {
                    if (i == 0)
                    {
                        CustommerId = LocConvertToInt(item);
                    }
                    else if (i == 1)
                    {
                        ItemId = LocConvertToInt(item);
                    }
                    else if (i == 2)
                    {
                        string e = item.Remove(0, 12);
                        e = e.Remove(e.Length - 1);
                        ItemName = e;
                    }
                    else if (i == 3)
                    {
                        ItemAmount = LocConvertToInt(item);
                    }
                    i++;
                }
                //Console.WriteLine("Customer" + CustommerId + " bought a whopping " + ItemAmount + " " + ItemName);
            }
        }

        protected virtual int LocConvertToInt(string text)
        {
            text = new string(text.Where(c => char.IsDigit(c)).ToArray());

            int e = 0;
            if (int.TryParse(text, out e))
            {
                return e;
            }

            return e;
        }
    }
}
