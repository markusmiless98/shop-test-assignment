using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using WindowDemo.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WindowDemo
{
    internal class JsonHandler
    {
        // Taken from example of:
        // https://learn.microsoft.com/en-us/troubleshoot/developer/visualstudio/csharp/language-compilers/read-write-text-file
        // But adjusted to fit needs of remembering purchases

        static string MainFile = "customer_json";
        static string SecondFile = "cart_json";
        static string LogInFile = "login_json";

        static string file_path = Directory.GetCurrentDirectory() + @"\";

        // Only used to add local files first time
        public static void SetUpSave()
        {
            try
            {
                List<CustommerJson> _data = new List<CustommerJson>();
                List<ItemJson> _dataItems = new List<ItemJson>();
                _dataItems.Add(new ItemJson(1, 1, "Othello",1));
                _dataItems.Add(new ItemJson(1, 2, "Chess",1));
                _dataItems.Add(new ItemJson(2, 1, "Othello",2));
                _data.Add(new CustommerJson(1, "Bengt"));
                _data.Add(new CustommerJson(2, "Hilda"));

                string json = JsonSerializer.Serialize(_data);
                if (!File.Exists(GetPath(true)))
                {
                    File.WriteAllText(GetPath(true), json);
                }
                GetUsersFromTable();
                json = JsonSerializer.Serialize(_dataItems);
                if (!File.Exists(GetPath(false))) {
                    File.WriteAllText(GetPath(false), json);
                }
                GetPurchasesFromTable();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Thread.Sleep(500);
                Console.WriteLine("Executing finally block.");
                Console.WriteLine(file_path);
                Thread.Sleep(1000);
            }
        }

        public static void UpdateSavedCart(List<Item> items, int cust_id)
        {
            foreach (var item in items)
            {
                ADOShopChecker.AddProduktSale(item.GetId(), item.GetAmnt(), DateTime.Now.ToShortDateString(), cust_id);
            }
            if (cust_id < 0)
            {
                // Shouldn't go through this when not logged in
                return;
            }
            if (File.Exists(GetPath(false)))
            {
                List<ItemJson> data = new List<ItemJson>();
                List<string> cur_data = ManualDeserializer(GetPath(false));

                // Goes through each customer and saves their data to a local json file
                foreach (var custommer in ManualDeserializer(GetPath(true)))
                {
                    CustommerJson temp_cus = new CustommerJson(custommer);
                    List<ItemJson> temp_list = new List<ItemJson>();
                    foreach (var item in ManualDeserializer(GetPath(false)))
                    {
                        ItemJson temp = new ItemJson(item);
                        if (temp.CustommerId == temp_cus.Id)
                        {
                            temp_list.Add(temp);
                            //Console.Write(temp.ItemAmount.ToString());
                        }
                        else
                        {
                            //Console.Write(temp.ItemAmount.ToString());
                        }
                    }
                    foreach (var product in items)
                    {
                        bool been_adj = false;
                        foreach (var old_product in temp_list)
                        {
                            if (old_product.ItemId == product.GetId() && !been_adj)
                            {
                                old_product.ItemAmount += product.GetAmnt();
                                been_adj = true;
                            }
                        }
                        if (!been_adj)
                        {
                            if (cust_id == temp_cus.Id)
                            {
                                temp_list.Add(new ItemJson(temp_cus.Id, product.GetId(), product.GetTitle(), product.GetAmnt()));
                            }
                        }
                    }

                    foreach (var prod in temp_list)
                    {
                        data.Add(prod);
                    }
                }

                string json = JsonSerializer.Serialize(data);
                File.WriteAllText(GetPath(false), json);
            }

            Thread.Sleep(1000);
        }

        public static void GetUsersFromTable()
        {

            List<CustommerJson> _data = new List<CustommerJson>();

            List<Customer> customers = ADOShopChecker.GetAllCustomers();
            foreach (var targ in customers)
            {
                CustommerJson temp = new(targ);
                _data.Add(temp);
            }

            string json = JsonSerializer.Serialize(_data);
            File.WriteAllText(GetPath(true), json);
            Thread.Sleep(1000);
            Console.WriteLine("We added people from the online!");
            Thread.Sleep(3000);
        }

        public static void GetPurchasesFromTable()
        {
            List<ItemJson> _data = new List<ItemJson>();

            foreach (var item in ADOShopChecker.GetAllProduktSales())
            {
                _data.Add(new ItemJson(item.CustomerID,item.GameID,item.Name,item.SoldAmnt));
            }

            string json = JsonSerializer.Serialize(_data);
            File.WriteAllText(GetPath(false), json);
            Thread.Sleep(1000);
            Console.WriteLine("We added purchases from the online!");
            Thread.Sleep(3000);

        }

        public static string GetUserName(int id = -1)
        {
            try
            {
                if (id < -1)
                {
                    throw new InvalidOperationException("Customer ID is negative number.");
                }
                else if (id == -1)
                {
                    return "Namn";
                }

                if (File.Exists(GetPath(true)))
                {
                    List<string> data = ManualDeserializer(GetPath(true));
                    foreach (var custommer in data)
                    {
                        CustommerJson temp = new CustommerJson(custommer);
                        if (temp.Id == id)
                        {
                            return temp.Name;
                        }
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception; " + e);
                Thread.Sleep(5000);
                throw;
            }
        }

        public static List<string> GetLogInFromFile()
        {
            List<string> i = new List<string>();

            string text_path = file_path + LogInFile + @".json";

            i = ManualDeserializer(text_path);
            // Gets User at 0 and password at 1

            return i;
        }

        // Reads file in console
        public static bool ReadFile(bool pause = false)
        {
            try
            {
                string path = GetPath(true);
                if (File.Exists(path))
                {
                    Console.WriteLine(File.ReadAllText(path));
                    if (pause)
                    {
                        Thread.Sleep(500);
                    }
                    
                    path = GetPath(false);
                    Console.WriteLine(File.ReadAllText(path));
                    if (pause)
                    {
                        Thread.Sleep(1000);
                    }

                    foreach (var custommer in ManualDeserializer(GetPath(true)))
                    {
                        CustommerJson temp_cus = new CustommerJson(custommer);
                        List<ItemJson> temp_list = new List<ItemJson>();
                        foreach (var item in ManualDeserializer(GetPath(false)))
                        {
                            ItemJson temp = new ItemJson(item);
                            if (temp.CustommerId == temp_cus.Id)
                            {
                                temp_list.Add(temp);
                            }
                        }

                        if (pause)
                        {
                            Thread.Sleep(500);
                        }
                        Console.WriteLine("Customer " + temp_cus.Id + " " + temp_cus.Name);
                        Console.WriteLine("Bought: ");
                        foreach (var item in temp_list)
                        {
                            Console.WriteLine("- " + item.ItemAmount + " x " + item.ItemName);
                            if (pause)
                            {
                                Thread.Sleep(250);
                            }
                        }

                        if (pause)
                        {
                            Thread.Sleep(250);
                        }
                    }

                    if (pause)
                    {
                        Thread.Sleep(4000);
                    }

                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                Thread.Sleep(5000);
                return false;
            }
            finally
            {
                Console.WriteLine("Executing finally block.");

                if (pause)
                {
                    Thread.Sleep(500);
                }

                Console.Clear();
                Console.WriteLine("Loaded files");
                Thread.Sleep(pause ? 2000 : 1000);
            }
        }

        public static List<string> ManualDeserializer(string path)
        {
            List<string> list = new List<string>();
            string full_file_str = File.ReadAllText(path);

            var charsToRemove = new string[] { "[", "]" };

            foreach (var c in charsToRemove)
            {
                full_file_str = full_file_str.Replace(c, string.Empty);
            }
            string[] full_str_res = full_file_str.Split("},{");
            charsToRemove = new string[] { "{", "}" };
            foreach (var c in charsToRemove)
            {
                full_str_res[0] = full_str_res[0].Replace(c, string.Empty);
                full_str_res[full_str_res.Length - 1] = full_str_res[full_str_res.Length - 1].Replace(c, string.Empty);
            }

            foreach (var item in full_str_res)
            {
                list.Add(item);
            }
            
            return list;
        }

        static public List<CustommerJson> GetCustomersJson()
        {
            List<CustommerJson> temp = new List<CustommerJson>();
            foreach (var item in ManualDeserializer(GetPath(true)))
            {
                temp.Add(new CustommerJson(item));
            }
            return temp;
        }

        static public List<ItemJson> GetItemJsonsFromCust(int id)
        {
            List<ItemJson> temp = GetItemJsons();
            List<ItemJson> fin_ans = new List<ItemJson>();

            foreach (var item in temp)
            {
                if (item.CustommerId == id)
                {
                    fin_ans.Add(item);
                }
            }

            return fin_ans;
        }

        static public List<ItemJson> GetItemJsons()
        {

            List<ItemJson> temp = new List<ItemJson>();
            foreach (var item in ManualDeserializer(GetPath(false)))
            {
                temp.Add(new ItemJson(item));
            }
            return temp;
        }
        
        static string GetPath(bool main = true)
        {
            string text_path = file_path;
            if (main)
            {
                text_path += MainFile;
            }
            else
            {
                text_path += SecondFile;
            }
            text_path += @".json";

            return text_path;
        }
    }
}
