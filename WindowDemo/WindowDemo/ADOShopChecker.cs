using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using WindowDemo.ADModel;
using WindowDemo.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WindowDemo
{
    internal class ADOShopChecker
    {
        static string sql_request = @"SELECT Title, Genre, NumPlayers, Age, Publisher, Information, Pris, LagerSaldo
                              FROM dbo.BoardGames
                              RIGHT JOIN dbo.BoardGameInfo ON dbo.BoardGames.ProduktID=dbo.BoardGameInfo.GameID
                              ORDER BY dbo.BoardGames.ProduktID";
        static string slq_check_request = @"SELECT Title, Genre, NumPlayers, Age, Publisher, Information, Pris, LagerSaldo
                                    FROM dbo.BoardGameInfo
                                    LEFT JOIN dbo.BoardGames ON (dbo.BoardGames.ProduktID=dbo.BoardGameInfo.GameID)
                                    RIGHT JOIN dbo.ProduktCategory ON (dbo.ProduktCategory.ProduktId=dbo.BoardGames.CategoryId)
                                    WHERE dbo.BoardGames.CategoryId={0,0}";
        static string slq_get_product = @"SELECT * FROM dbo.BoardGames WHERE dbo.BoardGames.ProduktId={0}";
        static string sql_user_request = @"SELECT [Name]
                              ,[Password]
                              ,[PurchaseId]
                              ,[username]
                              FROM [dbo].[Customer] where username = '";
        static string sql_all_user_request = @"SELECT [Name]
                              ,[Password]
                              ,[PurchaseId]
                              ,[username]
                              FROM [dbo].[Customer]";

        static string sql_sel_products_request = @"SELECT TOP (3) [ProduktId]
                                    FROM [dbo].[SelectedProdukts]";

        static string sql_get_categories = @"SELECT * FROM dbo.ProduktCategory";

        static string sql_serv { get; set; } = "Server=tcp:serv-sev.database.windows.net,1433;Initial Catalog=MyOs;Persist Security Info=False;User ID={0};Password={1};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        
        public static string SetServWithPass()
        {
            List<string> user_pass = JsonHandler.GetLogInFromFile();

            string user = user_pass[0];
            string pass = user_pass[1];

            return string.Format(sql_serv, user,pass);
        }

        static public List<String> CheckForSpecific(int i, int category = 1)
        {
            string ser_order = string.Format(slq_check_request, category);

            Thread.Sleep(1000);

            List<string> return_value = new List<string>();
            List<BoardGameInformation> bgi_list = new List<BoardGameInformation>();
            using (var connection = new SqlConnection(SetServWithPass()))
            {
                connection.Open();

                using (var command = new SqlCommand(ser_order, connection))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    using (reader)
                    {
                        int temp_i = 0;
                        while (reader.Read())
                        {
                            var bgi = new BoardGameInformation
                            {
                                Id = temp_i += 1,
                                Title = reader.GetString(reader.GetOrdinal("Title")),
                                Genre = reader.GetString(reader.GetOrdinal("Genre")),
                                NumPlayers = reader.GetInt32(reader.GetOrdinal("NumPlayers")),
                                Age = reader.GetInt32(reader.GetOrdinal("Age")),
                                Publisher = reader.GetString(reader.GetOrdinal("Publisher")),
                                Information = reader.GetString(reader.GetOrdinal("Information")),
                                Pris = reader.GetDouble(reader.GetOrdinal("Pris")),
                                LagerSaldo = reader.GetInt32(reader.GetOrdinal("LagerSaldo"))
                            };

                            bgi_list.Add(bgi);
                        }
                    }

                    if (bgi_list.Count > 0)
                    {
                        // In case above don't work, go through each item
                        foreach (var item in bgi_list)
                        {
                            if (item.Id == i)
                            {
                                BoardGameInformation bgi_tar = item;
                                return_value = bgi_tar.GetInformation();

                                return return_value;
                            }
                        }
                    }
                }

                return null;
            }
        }
        static public BoardGameInformation CheckForSpecificInfo(int i, int category = 1)
        {
            string ser_order = string.Format(slq_check_request, category);

            Thread.Sleep(1000);

            List<string> return_value = new List<string>();
            List<BoardGameInformation> bgi_list = new List<BoardGameInformation>();
            using (var connection = new SqlConnection(SetServWithPass()))
            {
                connection.Open();

                using (var command = new SqlCommand(ser_order, connection))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    using (reader)
                    {
                        int temp_i = -1;
                        while (reader.Read())
                        {
                            var bgi = new BoardGameInformation
                            {
                                Id = temp_i += 1,
                                Title = reader.GetString(reader.GetOrdinal("Title")),
                                Genre = reader.GetString(reader.GetOrdinal("Genre")),
                                NumPlayers = reader.GetInt32(reader.GetOrdinal("NumPlayers")),
                                Age = reader.GetInt32(reader.GetOrdinal("Age")),
                                Publisher = reader.GetString(reader.GetOrdinal("Publisher")),
                                Information = reader.GetString(reader.GetOrdinal("Information")),
                                Pris = reader.GetDouble(reader.GetOrdinal("Pris")),
                                LagerSaldo = reader.GetInt32(reader.GetOrdinal("LagerSaldo"))
                            };

                            bgi_list.Add(bgi);
                        }
                    }

                    if (bgi_list.Count > 0)
                    {
                        // In case above don't work, go through each item
                        foreach (var item in bgi_list)
                        {
                            if (item.Id == i)
                            {
                                BoardGameInformation bgi_tar = item;

                                return bgi_tar;
                            }
                        }
                    }
                }

                return null;
            }
        }

        static public BoardGameInformation GetProductInfoById(int id)
        {
            List<BoardGameInformation> bgi_list = GetAllProdukts();

            foreach (var item in bgi_list)
            {
                if (item.Id == id)
                {
                    return item;
                }
            }

            return null;
        }

        static public List<BoardGameInformation> GetAllProdukts()
        {
            List<BoardGameInformation> board_games = new List<BoardGameInformation>();


            using (var connection = new SqlConnection(SetServWithPass()))
            {
                board_games = connection.Query<BoardGameInformation>(sql_request).ToList();
            }

            int i = 1;
            // Temp solution
            foreach (var item in board_games)
            {
                item.Id = i;
                i++;
            }

            return board_games;
        }

        static public List<BoardGameInformation> GetAllBoardGameInfo(int type = 1)
        {
            List<BoardGameInformation> board_games = new List<BoardGameInformation>();


            using (var connection = new SqlConnection(SetServWithPass()))
            {
                board_games = connection.Query<BoardGameInformation>(string.Format(slq_check_request, type)).ToList();
            }

            return board_games;
        }

        static public int GetIdForProdukt()
        {
            int i = 1;

            using (var connection = new SqlConnection(SetServWithPass()))
            {
                i += connection.Query<BoardGameInformation>(string.Format(slq_check_request, 1)).ToList().Count;
                i += connection.Query<BoardGameInformation>(string.Format(slq_check_request, 2)).ToList().Count;
                i += connection.Query<BoardGameInformation>(string.Format(slq_check_request, 3)).ToList().Count;
            }

            return i;
        }

        static public List<BoardGameInformation> GetSelectedProdukts()
        {
            List<BoardGameInformation> board_games = new List<BoardGameInformation>();

            List<int> ids = GetSelectedProduktsIds();

            foreach (var item in GetAllProdukts())
            {
                if (ids.Contains(item.Id))
                {
                    board_games.Add(item);
                }
            }

            return board_games;
        }
        static public List<int> GetSelectedProduktsIds()
        {
            List<int> ids = new List<int>();

            using (var connection = new SqlConnection(SetServWithPass()))
            {
                ids = connection.Query<int>(sql_sel_products_request).ToList();
            }

            return ids;
        }

        static public BoardGame GetSpecificProdukt(int i)
        {
            BoardGame board_game = new BoardGame();
            using (var connection = new SqlConnection(SetServWithPass()))
            {
                board_game = connection.Query<BoardGame>(string.Format(slq_get_product, i)).ToList()[0];
            }

            if (board_game != null)
            {
                return board_game;
            }
            else
            {
                return null;
            }
        }

        static public bool CheckForUser(string username)
        {
            
                if (string.IsNullOrEmpty(username))
                {
                    return false;
                }
                string temp_req = sql_user_request + username + "'";
                using (var connection = new SqlConnection(SetServWithPass()))
                {
                    List<Customer> customers = connection.Query<Customer>(temp_req).ToList();
                    if (customers.Count > 0)
                    {
                        Console.WriteLine("Found user!");
                        Thread.Sleep(2000);
                        return true;
                    }
                }

                return false;
        }

        static public List<Customer> GetAllCustomers()
        {
            List<Customer> cust_list = new List<Customer>();
            using (var connection = new SqlConnection(SetServWithPass()))
            {
                cust_list = connection.Query<Customer>(sql_all_user_request).ToList();
            }
            return cust_list;
        }

        static public Customer CompareLogIn(string user, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
                {
                    return null;
                }
                List<Customer> customer = new List<Customer>();
                string temp_req = sql_user_request + user + "'";
                using (var connection = new SqlConnection(SetServWithPass()))
                {
                    customer = connection.Query<Customer>(temp_req).ToList();
                    if (customer != null)
                    {
                        if (customer.Count > 0)
                        {
                            foreach (var c in customer)
                            {
                                if (c.Password == password)
                                {
                                    return c;
                                }
                            }
                        }
                        // Know largely how to use it but didn't work out in short timeframe
                        // var cus = customer.Where(x => (x.username != null || x.username.Equals(string.Empty)) && x.username == user);
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error; " + e);
                Thread.Sleep(3000);
                return null;
            }
        }

        static string sql_produkt_sale_info_request = @"SELECT * FROM dbo.ProduktSales";
        static string sql_produkt_sale_info_request_alt = @"SELECT Title, SUM(SoldAmnt) as 'SoldAmnt', Customer.Name, CustomerID, dbo.ProduktSales.SaleDate
                                    FROM dbo.BoardGames
                                    RIGHT JOIN dbo.BoardGameInfo ON dbo.BoardGames.ProduktID=dbo.BoardGameInfo.GameID
                                    RIGHT JOIN dbo.ProduktSales ON dbo.BoardGames.ProduktID=dbo.ProduktSales.ProduktID
                                    JOIN dbo.Customer ON dbo.ProduktSales.CustomerID=dbo.Customer.PurchaseId
                                    GROUP BY Title, SoldAmnt, Customer.Name, CustomerID, dbo.ProduktSales.SaleDate
                                    ORDER BY CustomerID";
        static string sql_produkt_sale_info_request_altb = @"SELECT Title, dbo.BoardGameInfo.GameID, SoldAmnt, Customer.Name, CustomerID, SaleDate
                                    FROM dbo.BoardGames
                                    RIGHT JOIN dbo.BoardGameInfo ON dbo.BoardGames.ProduktID=dbo.BoardGameInfo.GameID
                                    RIGHT JOIN dbo.ProduktSales ON dbo.BoardGames.ProduktID=dbo.ProduktSales.ProduktID
                                    JOIN dbo.Customer ON dbo.ProduktSales.CustomerID=dbo.Customer.PurchaseId
                                    ORDER BY CustomerID";

        static string sql_add_sale_info_request = @"INSERT INTO ProduktSales (ProduktID, SoldAmnt, SaleDate, CustomerID) VALUES ({0},{1}, '{2}', {3})";

        static public void AddProduktSale(int prod_id, int amnt, string date, int cust_id)
        {
            using (var connection = new SqlConnection(SetServWithPass()))
            {
                using (var command = new SqlCommand(string.Format(sql_add_sale_info_request, prod_id, amnt, date, cust_id), connection))
                {
                    Console.WriteLine(command.CommandText);
                    Thread.Sleep(1000);
                    command.Connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        static public List<CustomerPurchase> GetAllProduktSales()
        {
            List<CustomerPurchase> cust_list = new List<CustomerPurchase>();
            using (var connection = new SqlConnection(SetServWithPass()))
            {
                cust_list = connection.Query<CustomerPurchase>(sql_produkt_sale_info_request_altb).ToList();
            }

            return cust_list;
        }

        static public List<ProduktCategory> GetCategories()
        {
            List<ProduktCategory> cust_list = new List<ProduktCategory>();
            using (var connection = new SqlConnection(SetServWithPass()))
            {
                cust_list = connection.Query<ProduktCategory>(sql_get_categories).ToList();
            }

            return cust_list;
        }

        static string slq_get_product_sales = @"SELECT * FROM dbo.ProduktSales";

        static public List<ProduktSales> GetAllSales()
        {
            List<ProduktSales> cust_sales = new List<ProduktSales>();

            using (var connection = new SqlConnection(SetServWithPass()))
            {
                cust_sales = connection.Query<ProduktSales>(slq_get_product_sales).ToList();
            }

            return cust_sales;
        }

        static string slq_get_product_sales_highest = @"SELECT SUM(SoldAmnt) as SoldAmnt, ProduktID FROM ProduktSales
                                                WHERE SoldAmnt >= 1
                                                GROUP BY ProduktID
                                                ORDER BY SoldAmnt DESC";

        static public List<ProdukSaleSimple> GetSalesByAmnt()
        {
            List<ProdukSaleSimple> cust_sales = new List<ProdukSaleSimple>();

            using (var connection = new SqlConnection(SetServWithPass()))
            {
                cust_sales = connection.Query<ProdukSaleSimple>(slq_get_product_sales_highest).ToList();
            }

            foreach (var cust in cust_sales)
            {
                Console.WriteLine(cust.ProdukID + " " + cust.SoldAmnt + " - " + cust.Id);
                Thread.Sleep(100);
            }

            return cust_sales;
        }
    }
}
