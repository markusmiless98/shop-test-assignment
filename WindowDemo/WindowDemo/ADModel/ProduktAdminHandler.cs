using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using WindowDemo.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WindowDemo.ADModel
{
    internal class ProduktAdminHandler
    {       
        static string sql_request = @"INSERT INTO dbo.BoardGames (Title, Genre, NumPlayers, Age, Publisher, ProduktID, CategoryId) VALUES('{0,0}', '{1,0}', {2,0}, {3,0}, '{4,0}', {5,0}, {6,0})";
        static string sql_adj_request = @"UPDATE dbo.BoardGames SET {0,0} = {1,0} WHERE ProduktId={2,0}";
        static string sql_cat_request = @"INSERT INTO dbo.ProduktCategory(ProduktId, Kategori) VALUES({0},'{1}')";
        static string sql_info_request = @"INSERT INTO BoardGameInfo (Information, Pris, LagerSaldo, GameID) VALUES ('A produkt to be sold.', 99, 100,{0})";
        static string sql_select_prod_req = @"UPDATE SelectedProdukts SET ProduktId={0} WHERE ProduktId={1}";

        // Can now add produkts heck yeah
        static public void PerformAddProdukt(string n, string g, int p, int ag, string pu)
        {
            string namn = "";
            string gen = "Annat";
            int selection = 0;
            int selection_age = 0;
            string publi = "";
            if (n != null && g != null && p != null && ag != null)
            {
                namn = n;
                gen = g;
                selection = p;
                selection_age = ag;
                if (!pu.IsNullOrEmpty())
                {
                    publi = pu;
                }
            }
            else
            {
                Console.WriteLine("Add Name");
                Console.WriteLine("");
                namn = Console.ReadLine();
                if (namn.Length <= 2)
                {
                    Console.WriteLine("\nName is too short");
                    Thread.Sleep(1000);
                    return;
                }
                Console.WriteLine("Select Genre/Category: A = Spel (Will Require Further Input), B = Clothes, C = Annat (Will Require Further Input)");
                Console.WriteLine("");
                ConsoleKeyInfo key = Console.ReadKey();
                int attempts_made = 5;

                switch (key.Key)
                {
                    case ConsoleKey.A:
                        Console.WriteLine("Selected A, Spel");
                        Console.WriteLine("Type in the genre, make sure to not misstype;");
                        Console.WriteLine("");
                        gen = Console.ReadLine();
                        while (gen.Length <= 4)
                        {
                            Console.WriteLine("Genre needs to be longer, you got {0} attempts left", attempts_made);
                            Console.WriteLine("");
                            gen = Console.ReadLine();
                            attempts_made--;
                            if (attempts_made <= 0)
                            {
                                Console.WriteLine("Too many attempts.");
                                attempts_made--;
                                break;
                            }
                        }
                        break;
                    case ConsoleKey.B:
                        Console.WriteLine("Selected B, Clothes");
                        gen = "Clothes";
                        break;
                    case ConsoleKey.C:
                        Console.WriteLine("Selected C, Annat");
                        Console.WriteLine("Type in the genre, make sure to not misstype;");
                        Console.WriteLine("");
                        gen = Console.ReadLine();
                        while (gen.Length <= 4)
                        {
                            Console.WriteLine("Genre needs to be longer, you got {0} attempts left", attempts_made);
                            Console.WriteLine("");
                            gen = Console.ReadLine();
                            attempts_made--;
                            if (attempts_made <= 0)
                            {
                                Console.WriteLine("Too many attempts.");
                                attempts_made--;
                                break;
                            }
                        }
                        break;
                    default:
                        Console.WriteLine("That wasn't an option, defaults to Annat");
                        break;
                }

                Console.WriteLine("Write number of users:");
                Console.WriteLine("");

                if (Int32.TryParse(Console.ReadLine(), out int i))
                {
                    selection = i;
                }
                else
                {
                    Console.WriteLine("That isn't a number");
                    Thread.Sleep(1000);
                    return;
                }

                Console.WriteLine("Write acceptable age (1-100)");
                Console.WriteLine("");

                if (Int32.TryParse(Console.ReadLine(), out int age))
                {
                    if (age <= 0)
                    {
                        Console.WriteLine("Too young");
                        Thread.Sleep(1100);
                        return;
                    }
                    if (age > 100)
                    {
                        Console.WriteLine("Too old");
                        Thread.Sleep(1100);
                        return;
                    }
                    selection_age = age;
                }
                else
                {
                    Console.WriteLine("That isn't a number");
                    Thread.Sleep(1000);
                    return;
                }

                Console.WriteLine("Write publisher (optional)");
                Console.WriteLine("");

                publi = Console.ReadLine();

                if (publi.Length <= 1)
                {
                    publi = "Multiple";
                }

            }
            BoardGame board = new BoardGame();
            board.Title = namn;
            board.Genre = gen;
            board.NumPlayers = selection;
            board.Age = selection_age;
            board.Publisher = publi;


            Console.WriteLine(@"Input; {0} {1} Players: {2} Age: {3} Publisher: {4}", namn, gen, selection, selection_age, publi);
            Thread.Sleep(100);
            Console.WriteLine("Is this correct? (Y/N)");

            ConsoleKeyInfo key_two = Console.ReadKey();

            switch (key_two.Key)
            {
                case ConsoleKey.Y:
                    Console.WriteLine("Selected Y, Trying to publish");
                    AddProdukt(namn,gen,selection,selection_age,publi);
                    break;
                case ConsoleKey.N:
                    Console.WriteLine("Selected N, Restarting...");
                    break;
                default:
                    PerformAddProdukt(board);
                    break;
            }
        }
        static public void PerformAddProdukt(BoardGame bgi)
        {
            PerformAddProdukt(bgi.Title, bgi.Genre, bgi.NumPlayers ?? 1, bgi.Age ?? 1, bgi.Publisher ?? "Multiple");
        }

        static public void AdjustProdukt(string adj_col, string adj_val, int adj_tar)
        {
            try
            {
                Console.WriteLine("Are you sure? (Y/N)");
                ConsoleKeyInfo key = Console.ReadKey();

                switch (key.Key)
                {
                    case ConsoleKey.Y:
                        using (var connection = new SqlConnection(ADOShopChecker.SetServWithPass()))
                        {
                            using (var command = new SqlCommand(string.Format(sql_adj_request, adj_col,adj_val,adj_tar), connection))
                            {
                                command.Connection.Open();
                                command.ExecuteNonQuery();
                            }
                        }
                        break;
                    case ConsoleKey.N:
                        break;
                    default:
                        AdjustProdukt(adj_col,adj_val,adj_tar);
                        break;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e);
                Thread.Sleep(5000);
                throw;
            }
        }
        static private void AddProdukt(string title, string genre, int num_player, int age, string publish)
        {
                Console.WriteLine("");
                Console.WriteLine("Are you sure? (Y/N)");

                ConsoleKeyInfo key = Console.ReadKey();
                // No keypress at present

                switch (key.Key)
                {
                    case ConsoleKey.Y:
                        int id_num = ADOShopChecker.GetIdForProdukt();
                        using (var connection = new SqlConnection(ADOShopChecker.SetServWithPass()))
                        {
                            using (var command = new SqlCommand(string.Format(sql_request, title, genre, num_player, age, publish, id_num, 0), connection))
                            {
                                command.Connection.Open();
                                command.ExecuteNonQuery();
                            }
                        }
                        Thread.Sleep(500);
                        Console.WriteLine("Buying into lager");
                        using (var connection = new SqlConnection(ADOShopChecker.SetServWithPass()))
                        {
                            using (var command_b = new SqlCommand(string.Format(sql_info_request, id_num), connection))
                            {
                                command_b.Connection.Open();
                                command_b.ExecuteNonQuery();
                            }
                        }
                        Console.WriteLine("Need to add category of produkt.");
                        Thread.Sleep(1000);
                        PerformAddCategory(0, false, id_num);
                        Thread.Sleep(500);
                        break;
                    case ConsoleKey.N:
                        break;
                    default:
                        AddProdukt(title,genre,num_player,age, publish);
                        break;
                }
                Thread.Sleep(500);
        }
        // Category Stuff
        static public void PerformAddCategory(int tar = 0, bool allow_canceling = true, int id_num = 1)
        {
            bool performing = true;
            List<ProduktCategory> categories = ADOShopChecker.GetCategories();
            while (performing)
            {
                Console.Clear();
                Console.WriteLine("Category for Latest Produkt");
                if (tar < 1)
                {
                    Console.WriteLine("A. Select an option:");
                    foreach (var item in categories)
                    {
                        Console.WriteLine("* " + item.ProduktId + ". " + item.Kategori);
                    }
                    Console.WriteLine("B. -Add New-");
                    if (allow_canceling) Console.Write("\nE. Go Back");
                    ConsoleKeyInfo key = Console.ReadKey();
                    switch (key.Key)
                    {
                        case ConsoleKey.A:
                            Console.WriteLine("Select Category:");
                            foreach (var item in categories)
                            {
                                Console.WriteLine(item.ProduktId + ". " + item.Kategori);
                            }
                            Console.WriteLine("");
                            int sel = 0;
                            if (Int32.TryParse(Console.ReadLine(), out sel))
                            {
                                if (sel < categories.Count())
                                {
                                    tar = sel;
                                    performing = false;
                                }
                                else
                                {
                                    Console.WriteLine("Category out of range");
                                    Thread.Sleep(700);
                                }
                            }

                            break;
                        case ConsoleKey.B:
                            Thread.Sleep(100);
                            Console.WriteLine("Write Category Name:");
                            Console.WriteLine("");
                            string name = "";

                            while (name.Length < 3)
                            {
                                name = Console.ReadLine();
                                if (name.Length < 3)
                                {
                                    Console.Write(" - name is too short");
                                    Console.WriteLine("");
                                }
                            }

                            using (var connection = new SqlConnection(ADOShopChecker.SetServWithPass()))
                            {
                                using (var command = new SqlCommand(string.Format(sql_cat_request, categories.Count + 1, name), connection))
                                {
                                    command.Connection.Open();
                                    command.ExecuteNonQuery();
                                }
                            }

                            using (var connection = new SqlConnection(ADOShopChecker.SetServWithPass()))
                            {
                                using (var command = new SqlCommand(string.Format(sql_adj_request, "CategoryId", categories.Count + 1, id_num), connection))
                                {
                                    command.Connection.Open();
                                    command.ExecuteNonQuery();
                                }
                            }

                            categories = ADOShopChecker.GetCategories();

                            break;
                        case ConsoleKey.E:
                            if (allow_canceling)
                            {
                                Console.WriteLine("Returning to last menu.");
                                Thread.Sleep(600);
                                performing = false;
                                return;
                            }
                            break;
                        default:
                            break;

                    }
                }
                else
                {
                    performing = false;
                }
            }
            Thread.Sleep(100);
            if (tar >= 1)
            {
                Thread.Sleep(100);
                Console.WriteLine("Attempting to add category");
                using (var connection = new SqlConnection(ADOShopChecker.SetServWithPass()))
                {
                    using (var command = new SqlCommand(string.Format(sql_adj_request, "CategoryId", tar, id_num), connection))
                    {
                        command.Connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
                Thread.Sleep(100);
            }
        }

        public static void ChangeSelectedProducts(int id_targ)
        {
            try
            {
                if (id_targ < 0) return;


                List<BoardGameInformation> list = ADOShopChecker.GetAllProdukts();
                if (id_targ > list.Count) return;
                List<int> sel_list = ADOShopChecker.GetSelectedProduktsIds();
                List<string> list_name = new List<string>();

                int i = 1;
                int selected = 0;
                list_name.Add("X. Go Back");
                foreach (BoardGameInformation info in list)
                {
                    string nam = i + ". " + info.Title;
                    if (info.Id == id_targ)
                    {
                        nam += " (Cur)";
                        selected = i;
                    }
                    else if (sel_list.Contains(i))
                    {
                        nam += " (X)";
                    }
                    list_name.Add(nam);
                    i++;
                }


                var full_window = new Window("Selectable Options", 25, 1, list_name);
                full_window.Draw();

                bool will_continue = false;

                while (!will_continue)
                {
                    int temp = 0;
                    Console.WriteLine("Press 'X' to return to selection");
                    Console.WriteLine("                            ");
                    Console.WriteLine("Select a number between (1-" + list.Count() + ") and press Enter");
                    ConsoleKeyInfo key = Console.ReadKey();
                    if (key.Key == ConsoleKey.X)
                    {
                        will_continue = true;
                        return;
                    }
                    string i_txt = "";

                    bool check_key = true;


                    i_txt = Console.ReadLine();

                    if (Int32.TryParse(i_txt, out temp))
                    {
                        Console.WriteLine("B:" + i_txt);
                        if (!sel_list.Contains(temp))
                        {
                            Console.WriteLine("C:" + i_txt);
                            i = temp;
                            will_continue = true;
                        }
                    }
                }

                if (ADOShopChecker.GetSpecificProdukt(i) != null)
                {
                    using (var connection = new SqlConnection(ADOShopChecker.SetServWithPass()))
                    {
                        using (var command = new SqlCommand(string.Format(sql_select_prod_req, i, selected), connection))
                        {
                            command.Connection.Open();
                            command.ExecuteNonQuery();
                        }
                    }
                }
                else
                {
                    Console.WriteLine("This produkt doesn't exist");
                }

                Thread.Sleep(2000);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception; " + e.ToString());
                Thread.Sleep(1000);
            }
        }
    }
}
