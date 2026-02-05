using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WindowDemo.Models;

namespace WindowDemo
{
    internal class CustommerJson
    {
        [JsonRequired]
        public int Id { get; set; }
        public string Name { get; set; }
        
        public CustommerJson(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public CustommerJson(string alternative)
        {
            string[] arr = alternative.Split(',');
            if (arr.Count() != 2)
            {
                // Failure
            }
            else
            {
                Id = LocConvertToInt(arr[0]);
                Name = arr[1].Remove(0, 8);
                Name = Name.Remove(Name.Length - 1);
            }
        }

        public CustommerJson(Customer cust)
        {
            if (cust.PurchaseId == null)
            {
                return;
            }
            Id = cust.PurchaseId ?? 0;
            Name = cust.Name;
        }

        private int LocConvertToInt(string text)
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
