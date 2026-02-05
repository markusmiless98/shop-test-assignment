using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowDemo.ADModel
{
    internal class BoardGameInformation
    {
        // This is for full query
        public int Id { get; set; }
        public string? Title { get; set; }

        public string? Genre { get; set; }

        public int? NumPlayers { get; set; }

        public int? Age { get; set; }

        public string? Publisher { get; set; }

        public string? Information { get; set; }

        public double? Pris { get; set; }

        public int? LagerSaldo { get; set; }


        public virtual List<string> GetInformation()
        {
            var list = new List<string>();

            list.Add(Title);
            if (Genre != null)
            {
                list.Add("Type: " + Genre);
            }
            if (Age != null)
            {
                list.Add("For age: " + Age + " or older.");
            }
            else
            {
                list.Add("For any age.");
            }
            list.Add("Publisher(s): " + Publisher);
            if (Pris != null)
            {
                string temp = Pris.ToString() + " kr | ";
                temp += (LagerSaldo >= 1) ? LagerSaldo.ToString() + " left in stock." : "Sold out";
                list.Add(temp);
            }
            else
            {
                if (LagerSaldo != null)
                {
                    list.Add(LagerSaldo.ToString() + " left in stock");
                }
            }
            if (Information != null)
            {
                list.Add("--- Description ---");
                if (Information.Length >= 10)
                {
                    string new_txt = "";
                    int i = 0;
                    foreach (var chara in Information)
                    {
                        i++;
                        new_txt += chara.ToString();
                        if (i >= 60)
                        {
                            new_txt += "-";
                            list.Add(new_txt);
                            new_txt = "";
                            i = 0;
                        }
                    }
                    if (new_txt.Length > 0 || i > 0)
                    {
                        list.Add(new_txt);
                    }
                }
                else
                {
                    list.Add(Information);
                }
            }

            return list;
        }
        public virtual List<string> GetShortInformation(bool include_desc = true)
        {
            var list = new List<string>();

            list.Add(Title);

            list.Add("Publisher(s): " + Publisher);
            if (Pris != null)
            {
                string temp = Pris.ToString() + " kr | ";
                temp += (LagerSaldo >= 1) ? LagerSaldo.ToString() + " left in stock." : "Sold out";
                list.Add(temp);
            }
            else
            {
                if (LagerSaldo != null)
                {
                    list.Add(LagerSaldo.ToString() + " left in stock");
                }
            }
            if (Information != null && include_desc)
            {
                list.Add("--- Description ---");
                if (Information.Length >= 10)
                {
                    string new_txt = "";
                    int i = 0;
                    foreach (var chara in Information)
                    {
                        i++;
                        new_txt += chara.ToString();
                        if (i >= 61)
                        {
                            new_txt += "...";
                            break;
                        }
                    }
                    list.Add(new_txt);
                }
                else
                {
                    list.Add(Information);
                }
            }

            return list;
        }
    }
}
