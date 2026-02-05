using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Identity.Client;
using WindowDemo.ADModel;
using WindowDemo.Models;

namespace WindowDemo
{
    internal class Program
    {
        const int num_menu_enums = 4;
        enum MenuEnum
        {
            MainMenu,
            Store,
            Cart,
            Admin,
            LogIn,
            Search
        }
        enum ActionType
        {
            Navigation,
            Selection,
            Store,
            Other
        }

        static MenuEnum cur_menu { get; set; } = MenuEnum.MainMenu;
        static ActionType action_selected { get; set; } = ActionType.Navigation;

        static Cart custommer_cart { get; set; } = new Cart();

        static int log_in_id { get; set; } = 0;
        static string log_in_name { get; set; } = "Namn"; // Saved locally to not have to look up name each time c:

        static int produkt_look { get; set; } = 1;

        static List<int> selected_products { get; set; } = new List<int>() { 1, 2, 5 };

        static void Main(string[] args)
        {
            if (!JsonHandler.ReadFile())
            {
                JsonHandler.SetUpSave();
            }
            else
            {
                log_in_name = JsonHandler.GetUserName(log_in_id);
            }
            selected_products = ADOShopChecker.GetSelectedProduktsIds();
            List<string> i = JsonHandler.GetLogInFromFile();

            do
            {
                Console.Clear();

                switch (cur_menu)
                {
                    case MenuEnum.MainMenu:
                        MainMenu();
                        break;
                    case MenuEnum.Store:
                        var db = new MyOsContext();
                        StoreMenu(db, produkt_look);
                        break;
                    case MenuEnum.Cart:
                        CartMenu();
                        break;
                    case MenuEnum.Admin:
                        AdminMenu();
                        action_selected = ActionType.Other;
                        break;
                    case MenuEnum.LogIn:
                        LogInMenu();
                        action_selected = ActionType.Other;
                        break;
                    case MenuEnum.Search:
                        SearchMenu();
                        break;
                    default:
                        MainMenu();
                        break;
                }
                
                //}
            }
            while (true);
        }

        // Used to make the top match the right thing
        static void TopBar(bool showSide = true)
        {
            if (cur_menu != MenuEnum.Admin)
            {
                Console.WriteLine("Welcome to the store! Your cart has: " + custommer_cart.GetList().Count().ToString() + " unique items.");
                if (custommer_cart.GetCount() > 0)
                {
                    Console.Write("Last Bought; " + custommer_cart.GetList()[custommer_cart.GetList().Count() - 1].GetTitle());
                }
            }
            else if (log_in_id == 0) // temp
            {
                Console.WriteLine("Store's Admin Page");
            }
            else
            {
                Console.WriteLine("Admin Page; Inacccessible");
            }
            LogInText();
            if (showSide)
            {
                SideMenu();
            }
        }

        static void SideMenu()
        {
            if (cur_menu  == MenuEnum.LogIn) { Console.WriteLine("Log in page"); }
            List<string> categoriesText = GetMenuText();

            var windowCategories = new Window("Menuer", 1, 2, categoriesText);
            windowCategories.Draw();
        }
        

        static void MainMenu()
        {
            TopBar();

            List<BoardGameInformation> pos_item = ADOShopChecker.GetAllProdukts();
            List<BoardGameInformation> sel_items = new List<BoardGameInformation>();

            int loc_y = 3;
            int i = 0;
            foreach (var item in selected_products)
            {
                CreateSelectedProductWindow(loc_y, item - 1, pos_item);
                if (selected_products[i] > pos_item.Count)
                {
                    sel_items.Add(pos_item[pos_item.Count - 1]);
                }
                else {
                    sel_items.Add(pos_item[item - 1]);
                }
                Thread.Sleep(100);
                loc_y += 7;
                i++;
            }
            List<string> temp = new List<string>();

            i = 1;
            foreach (var item in sel_items)
            {
                temp.Add(i + "." + item.Title);
                i++;
            }

            var window = new Window("Press Num to Go To", 1, 11, temp);
            window.Draw();

            Console.SetCursorPosition(0, 16);
            ConsoleKeyInfo key = Console.ReadKey();
            switch (key.Key)
            {
                case ConsoleKey.A:
                case ConsoleKey.B:
                case ConsoleKey.C:
                case ConsoleKey.D:
                case ConsoleKey.E:
                case ConsoleKey.F:
                case ConsoleKey.P:
                    MenuNavigation(key.Key);
                    break;
                case ConsoleKey.D1:
                    GoToProduct(selected_products[0]);
                    break;
                case ConsoleKey.D2:
                    GoToProduct(selected_products[1]);
                    break;
                case ConsoleKey.D3:
                    GoToProduct(selected_products[2]);
                    break;
                default:
                    break;
            }

            Thread.Sleep(50);
        }

        static void GoToProduct(int i)
        {
            if (i < 1)
            {
                return;
            }
            action_selected = ActionType.Selection;
            Selection = i;


            var prod = ADOShopChecker.GetSpecificProdukt(i);
            if (prod != null)
            {
                Console.Clear();
                cur_menu = MenuEnum.Store;
                produkt_look = prod.Category.ProduktId;
                Thread.Sleep(500);
            }
        }

        static void CreateSelectedProductWindow(int y, int id, List<BoardGameInformation> possible_items)
        {
            if (id < 0) { return; }
            while (id > possible_items.Count)
            {
                id--;
            }
            List<string> menu_text = new List<string>();
            BoardGameInformation sel_item = possible_items[id];
            if (sel_item != null)
            {
                menu_text = sel_item.GetShortInformation();
            }
            
            var window = new Window("Item " + ((y + 4) / 7), 28, y, menu_text);
            window.Draw();
        }

        static int Selection { get; set; } = 0;

        static void LogInText()
        {
            string txt = "";
            List<string> log_in_info = new List<string>();
            if (log_in_id >= 0)
            {
                txt = "Inloggad";
                log_in_info.Add(log_in_name ?? "Namn");
            }
            else
            {
                txt = "Utloggad";
                log_in_info.Add("Måste logga in.");
            }
            //List<string> items = new_list[i - 1].GetInformation();
            
            var log_category = new Window(txt, 70, 0, log_in_info);
            log_category.Draw();
        }

        // TBA: Letting custommer add to cart
        static void StoreMenu(MyOsContext db, int produkts = 1)
        {
            if (db == null) return;
            TopBar(action_selected == ActionType.Other ? false : true);

            Cart temp_cart = custommer_cart;

            using (db)
            {
                int i = 0;

                List<string> menu_text = new List<string>();

                if (action_selected == ActionType.Other)
                {
                    menu_text = new List<string>() { "A. Navigera", "B. Välj Produkt", "C. Ändra kategori" };
                }
                else
                {
                    menu_text = new List<string>() { "A. Välj Produkt", "B. Gå Tillbaka" };
                }

                List<string> items = new List<string>();
                int x = 25;
                int y = 2;
                int length_check = ADOShopChecker.GetAllBoardGameInfo(produkts).Count(); // Not most performant
                int max_length = 4;

                if (action_selected != ActionType.Selection)
                {
                    int num_of_window = 1;
                    // used to be db.BoardGames rather than 
                    foreach (var p in ADOShopChecker.GetAllBoardGameInfo(produkts))
                    {
                        i++;

                        if (p.Title.Length > max_length)
                        {
                            max_length = p.Title.Length;
                        }

                        items.Add(i.ToString() + ". " + p.Title);
                        if (i % 4 == 0 || i >= length_check)
                        {
                            var gameCategory = new Window("Games " + num_of_window.ToString(), x, y, items);
                            if (x == 25)
                            {
                                x += 10 + max_length;
                            }
                            else
                            {
                                x = 25;
                                y += 6;
                            }
                            gameCategory.Draw();
                            items = new List<string>();
                            num_of_window += 1;
                        }
                    }
                }

                if (action_selected == ActionType.Other)
                {
                    var choiceCat = new Window("Actions", 1, 3, menu_text);
                    choiceCat.Draw();
                    /*
                    Console.Write("Välj action med 1 eller 2, sen tryck ENTER.");
                    Console.WriteLine("");
                    */
                    int first_selection = 0;
                    Console.SetCursorPosition(0, 15);
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.A:
                            Console.Write("Going back...");
                            Thread.Sleep(800);
                            action_selected = ActionType.Navigation;
                            break;
                        case ConsoleKey.B:
                            int selection = Selection;
                            Console.WriteLine("Välj produkt (nummer)");
                            Console.WriteLine("");
                            if (Int32.TryParse(Console.ReadLine(), out selection))
                            {
                                if (selection > i)
                                {
                                    return;
                                }
                                Selection = selection;
                                action_selected = ActionType.Selection;
                                CreateProduktInfoWindow(Selection);
                            }
                            break;
                        case ConsoleKey.C:
                            Console.WriteLine("Välj Kategori (1-" + ADOShopChecker.GetCategories().Count() + ")");
                            SelectStoreCategory();
                            break;
                        default:
                            Console.WriteLine("Fel input, pröva att skriva 'A' eller 'B'.");
                            Thread.Sleep(1700);
                            break;
                    }
                }
                else if (action_selected == ActionType.Selection)
                {
                    var choiceCat = new Window("Actions", 1, 3, menu_text);
                    choiceCat.Draw();
                    CreateProduktInfoWindow(Selection);

                    Console.SetCursorPosition(0, 13);
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.A:
                            Console.WriteLine("Välj antal (1-10)");
                            Console.WriteLine("");
                            int sel_amnt = 1;
                            if (Int32.TryParse(Console.ReadLine(), out sel_amnt))
                            {
                                List<BoardGameInformation> bgi_l = ADOShopChecker.GetAllProdukts();
                                BoardGameInformation bgi = ADOShopChecker.CheckForSpecificInfo(Selection - 1, produkts);
                                foreach (var item in bgi_l)
                                {
                                    if (item.Title == bgi.Title && item.Information == bgi.Information)
                                    {
                                        bgi.Id = item.Id;
                                    }
                                }
                                Console.Write("Antal; " + sel_amnt);
                                Thread.Sleep(500);
                                custommer_cart.AddToCart(bgi, bgi.Id, sel_amnt);
                                Thread.Sleep(500);
                                action_selected = ActionType.Other; // För nu
                                Selection = 0;
                                Console.Write("SÅLD");
                            }
                            break;
                        case ConsoleKey.B:
                            action_selected = ActionType.Other;
                            Thread.Sleep(500);
                            Selection = 0;
                            break;
                        default:
                            break;
                    }
                }
                else if (action_selected == ActionType.Navigation)
                {
                    Console.SetCursorPosition(0, 12);
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    MenuNavigation(key.Key);
                }
            }
        }

        static void CreateProduktInfoWindow(int i, int y = 25, int x = 2)
        {
            if (i < 1) return;
            
            List<string> items = ADOShopChecker.CheckForSpecific(i, produkt_look);

            var gameCategory = new Window(items[0], y, x, items);
            gameCategory.Draw();
        }
        static void CreateProduktInfoWindow(int i)
        {
            CreateProduktInfoWindow(i, 25, 2);
        }


        static void CartMenu()
        {
            bool menuing = true;
            bool can_checkout_cart = custommer_cart.GetCount() > 0;
            if (can_checkout_cart)
            {
                action_selected = ActionType.Other;
            }
            while (menuing)
            {
                Console.Clear();
                TopBar(action_selected == ActionType.Navigation);

                if (can_checkout_cart)
                {
                    Console.WriteLine("Thank you for buying stuff!");
                    Console.WriteLine("");
                    List<string> bought_itmes = new List<string>();

                    foreach (var item in custommer_cart.GetList())
                    {
                        bought_itmes.Add(item.GetAmnt() + " x " + item.GetTitle() + " - " + item.GetPrice() + " kr");
                    }
                    bought_itmes.Add(custommer_cart.GetPrice().ToString() + " kr");
                    var shop_window = new Window("Cart", 30, 2, bought_itmes);
                    shop_window.Draw();
                }
                else
                {
                    Console.WriteLine("Your cart is empty.");
                }
                Console.SetCursorPosition(0, 10);

                if (action_selected == ActionType.Other)
                {
                    List<string> items = new List<string> { "A. Navigate", "B. Checkout Cart", "C. Save to local file" };

                    var gameCategory = new Window("Actions", 2, 2, items);
                    gameCategory.Draw();

                    ConsoleKeyInfo key = Console.ReadKey(true);

                    switch (key.Key)
                    {
                        case ConsoleKey.A:
                            action_selected = ActionType.Navigation;
                            Thread.Sleep(100);
                            ConsoleKeyInfo keyb = Console.ReadKey(true);
                            TopBar();
                            MenuNavigation(keyb.Key);
                            break;
                        case ConsoleKey.B:
                            Thread.Sleep(100);
                            Console.WriteLine("Skriv in Address");
                            Console.WriteLine("");
                            string address = Console.ReadLine();
                            if (address.Length < 4)
                            {
                                break;
                            }

                            Console.WriteLine("Välj hur du vill beställa:");
                            Console.WriteLine("- A. Billigt och Bra (20 kr)");
                            Console.WriteLine("- B. Dyrt (300 kr)");

                            int extra_cost = 0;
                            while (extra_cost == 0)
                            {
                                ConsoleKeyInfo key_b = Console.ReadKey(true);
                                switch (key_b.Key)
                                {
                                    case ConsoleKey.A:
                                        extra_cost = 20;
                                        break;
                                    case ConsoleKey.B:
                                        extra_cost = 300;
                                        break;
                                    default:
                                        break;
                                }
                            }

                            Console.Clear();

                            gameCategory.Draw();
                            int pris = custommer_cart.GetPrice() + extra_cost;
                            Console.WriteLine(string.Format("Cost: {0} kr", custommer_cart.GetPrice().ToString()));
                            Thread.Sleep(250);
                            Console.WriteLine(string.Format("Frakt: {0} kr", extra_cost.ToString()));
                            Thread.Sleep(250);
                            Console.WriteLine(string.Format("PAYMENT TOTAL: {0} kr", pris.ToString()));

                            Thread.Sleep(3000);

                            if (log_in_id > -1)
                            {
                                JsonHandler.UpdateSavedCart(custommer_cart.GetList(), log_in_id);
                                custommer_cart = new Cart();
                            }
                            break;
                        case ConsoleKey.C:
                            Console.WriteLine("Will try to save to file...");
                            Thread.Sleep(100);
                            if (log_in_id > -1)
                            {
                                JsonHandler.UpdateSavedCart(custommer_cart.GetList(), log_in_id);
                                Thread.Sleep(100);
                            }
                            else
                            {
                                Console.WriteLine("Not logged in, try again when logged in");
                                Thread.Sleep(2500);
                            }
                            menuing = false;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);

                    MenuNavigation(key.Key);
                }
                if (!GetCurMenu(4))
                {
                    menuing = false;
                }
            }
        }

        static void SearchMenu()
        {
            bool menuing = true;

            while (menuing)
            {
                Console.Clear();
                Console.WriteLine("Search for Produkt by Name;");
                Console.WriteLine("");

                string i = Console.ReadLine();

                int y = 3;
                int x = 0;
                List<BoardGameInformation> check_list = new List<BoardGameInformation>();

                if (i.Length > 1)
                {
                    List<BoardGameInformation> list = ADOShopChecker.GetAllProdukts();
                    foreach (BoardGameInformation info in list)
                    {
                        if (info.Title.StartsWith(i) || info.Title.Contains(i))
                        {
                            check_list.Add(info);
                        }
                    }
                    if (check_list.Count < 1)
                    {
                        Console.WriteLine("Nothing found.");
                        Thread.Sleep(1000);
                        menuing = false;
                        break;
                    }
                    foreach (var item in check_list)
                    {
                        var windowCategories = new Window(item.Title, x,y, item.GetShortInformation(false));
                        windowCategories.Draw();
                        int len = 0;
                        foreach (var len_tar in item.GetShortInformation(false))
                        {
                            if (len_tar.Length > len)
                            {
                                len = len_tar.Length;
                            }
                        }
                        if (x + item.Title.Length > 50)
                        {
                            y += 5;
                            x = 0;
                        }
                        else
                        {
                            x += Math.Clamp(len + 5,0,50);
                        }
                    }

                    Console.WriteLine("Select Produkt (1-" + check_list.Count + ")");
                    Console.WriteLine("");

                    int sel_int = 0;
                    if (Int32.TryParse(Console.ReadLine(), out sel_int))
                    {
                        menuing = false;
                        Console.WriteLine(check_list[sel_int - 1].Title);
                        Thread.Sleep(2000);
                        GoToProduct(check_list[sel_int - 1].Id);
                        break;
                    }
                }

                Thread.Sleep(5000);
            }
        }

        static void AdminMenu()
        {
            bool menuin = true;
            bool cust_info = false;
            int sel_action = 0;
            do {
                Console.Clear();
                TopBar(action_selected == ActionType.Navigation);

                List<string> menu_text = new List<string>();
                menu_text = new List<string>() { "A. Navigera", "B. Add Produkt", "C. Adjust Produkt", "D. Customer List", "E. Adjust Showcased Products", "F. Add Category" };

                if (action_selected == ActionType.Other)
                {
                    var gameCategory = new Window("Actions", 1, 2, menu_text);
                    gameCategory.Draw();

                    Console.SetCursorPosition(0, 16);
                    AdminInfoMenu();
                    ConsoleKeyInfo key = Console.ReadKey(true);

                    switch (key.Key)
                    {
                        case ConsoleKey.A:
                            Console.WriteLine("Returning...");
                            action_selected = ActionType.Navigation;
                            Thread.Sleep(200);
                            Console.Clear();
                            TopBar();
                            Thread.Sleep(100);
                            ConsoleKeyInfo key2 = Console.ReadKey(true);
                            MenuNavigation(key2.Key);
                            break;
                        case ConsoleKey.B:
                            Thread.Sleep(100);
                            Console.Clear();
                            Thread.Sleep(50);
                            ProduktAdminHandler.PerformAddProdukt(null, null, -1, -1, null);
                            break;
                        case ConsoleKey.C:
                            Thread.Sleep(100);
                            action_selected = ActionType.Store;
                            break;
                        case ConsoleKey.D:
                            Thread.Sleep(100);
                            cust_info = false;
                            action_selected = ActionType.Selection;
                            sel_action = 0;
                            break;
                        case ConsoleKey.E:
                            Thread.Sleep(100);
                            cust_info = false;
                            action_selected = ActionType.Selection;
                            sel_action = 1;
                            break;
                        case ConsoleKey.F:
                            Thread.Sleep(100);
                            ProduktAdminHandler.PerformAddCategory();
                            Thread.Sleep(100);
                            break;
                        // Remove stuff
                        // Adjust produkt info             // Adjust produkt info
                        default:
                            break;
                    }

                }
                else if (action_selected == ActionType.Selection && sel_action == 0)
                {
                    Console.SetCursorPosition(0, 1);
                    List<string> menu_items = new List<string>();

                    List<Customer> customer = ADOShopChecker.GetAllCustomers();
                    List<CustomerPurchase> purchases = ADOShopChecker.GetAllProduktSales();

                    int y = 3;
                    int x = 17;

                    foreach (var cust in JsonHandler.GetCustomersJson())
                    {
                        menu_items.Add(cust.Name);
                        int item_adj = cust.Name.Length;
                        int y_adj = 3;

                        if (cust_info)
                        {
                            foreach (var item in purchases)
                            {
                                if (item.CustomerID == cust.Id)
                                {
                                    menu_items.Add(item.Title + "|" + item.SoldAmnt + "|" + item.SaleDate.ToShortDateString());
                                    if (item_adj < item.Title.Length + item.SoldAmnt.ToString().Length + 7)
                                    {
                                        item_adj = item.Title.Length + item.SoldAmnt.ToString().Length + 7;
                                    }
                                    y_adj++;
                                }
                            }
                        }

                        var shop_window = new Window("User ID: " + cust.Id, x, y, menu_items);
                        if (x >= 38)
                        {
                            y += y_adj;
                            x = 17;
                        }
                        else
                        {
                            x += 13 + item_adj;
                        }

                        shop_window.Draw();
                        menu_items = new List<string>();
                    }

                    // For now
                    Console.SetCursorPosition(0, cust_info ? 16 : 7);
                    Console.WriteLine("A. to get data from table.");
                    Console.WriteLine(!cust_info ? "B. to bring up list of purchases per person" : "B. to bring away the list of purchases per person");
                    Console.WriteLine("C. to return");
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.A:
                            JsonHandler.GetUsersFromTable();
                            break;
                        case ConsoleKey.B:
                            cust_info = !cust_info;
                            break;
                        case ConsoleKey.C:
                            action_selected = ActionType.Other;
                            break;
                        default:
                            break;
                    }
                    Thread.Sleep(2000);
                }
                else if (action_selected == ActionType.Selection)
                {
                    List<string> menu_items = new List<string>();
                    int i = 1;
                    foreach (var item in selected_products)
                    {
                        menu_items.Add(i + ". " + ADOShopChecker.GetSpecificProdukt(item).Title);
                        i++;
                    }

                    // For now
                    Console.WriteLine("A. to load data from table.");
                    Console.WriteLine("B. to return");
                    Console.WriteLine("Press 1-3 to edit respective produkt chosen");

                    var window = new Window("E", 2, 1, menu_items);
                    window.Draw();

                    ConsoleKeyInfo key = Console.ReadKey(true);

                    int sel = -1;
                    switch (key.Key)
                    {
                        case ConsoleKey.A:
                            selected_products = ADOShopChecker.GetSelectedProduktsIds();
                            break;
                        case ConsoleKey.B:
                            action_selected = ActionType.Other;
                            sel_action = 0;
                            break;
                        case ConsoleKey.D1:
                            sel = selected_products[0];
                            break;
                        case ConsoleKey.D2:
                            sel = selected_products[1];
                            break;
                        case ConsoleKey.D3:
                            sel = selected_products[2];
                            break;
                        default:
                            break;
                    }
                    if (sel > 0)
                    {
                        ProduktAdminHandler.ChangeSelectedProducts(sel);
                        selected_products = ADOShopChecker.GetSelectedProduktsIds();
                        Console.Clear();
                    }
                    Thread.Sleep(1000);
                }
                else if (action_selected == ActionType.Store)
                {
                    // Add
                    List<BoardGameInformation> list = ADOShopChecker.GetAllProdukts();

                    int x = 24;
                    int y = 2;
                    int length_check = list.Count();
                    int max_length = 5;

                    int num_of_window = 1;

                    List<string> items = new List<string>();

                    int i = 0;
                    // used to be db.BoardGames rather than 
                    foreach (var p in list)
                    {
                        i++;

                        if (p.Title.Length > max_length)
                        {
                            max_length = p.Title.Length;
                        }

                        items.Add(i.ToString() + ". " + p.Title);
                        if (i % 5 == 0 || i >= length_check)
                        {
                            var gameCategory = new Window("Games " + num_of_window.ToString(), x, y, items);
                            if (x == 24)
                            {
                                x += 10 + max_length;
                            }
                            else
                            {
                                x = 24;
                                y += 7;
                            }
                            gameCategory.Draw();
                            Thread.Sleep(200);
                            items = new List<string>();
                            num_of_window += 1;
                        }
                    }

                    if (i == 0)
                    {
                        action_selected = ActionType.Other;
                    }
                    else
                    {
                        int selection = 0;
                        Console.WriteLine("Select a number between 1-" + i);
                        Console.WriteLine("");
                        if (Int32.TryParse(Console.ReadLine(), out selection))
                        {
                            if (selection <= 0 || selection > i)
                            {
                                Console.WriteLine("Number not within range, going back to last menu.");
                                Thread.Sleep(1000);
                                return;
                            }
                            Selection = selection;
                            Console.Clear();
                            CreateProduktInfoWindow(Selection);

                            Console.WriteLine("What information do you wish to adjust?\nA. Title\nB. Genre\nC. Num of Users\nD. Adviced Age\nE. Publisher\n- Press any other button to cancel");

                            string info_col = "";
                            ConsoleKeyInfo key = Console.ReadKey(true);

                            switch (key.Key)
                            {
                                case ConsoleKey.A:
                                    info_col = "Title";
                                    break;
                                case ConsoleKey.B:
                                    info_col = "Genre";
                                    break;
                                case ConsoleKey.C:
                                    info_col = "NumPlayers";
                                    break;
                                case ConsoleKey.D:
                                    info_col = "Age";
                                    break;
                                case ConsoleKey.E:
                                    info_col = "Publisher";
                                    break;
                                default:
                                    Console.WriteLine("Returning...");
                                    action_selected = ActionType.Other;
                                    Thread.Sleep(700);
                                    return;
                            }

                            string info_adj = "";

                            if (info_col == "NumPlayers" || info_col == "Age")
                            {
                                Console.WriteLine("Write correct Number (needs to be a whole number):");
                                Console.WriteLine("");
                                int info_adj_int = 0;

                                if (Int32.TryParse(Console.ReadLine(), out info_adj_int))
                                {
                                    info_adj = info_adj_int.ToString();
                                }
                                else
                                {
                                    Console.WriteLine("That wasn't a whole number.");
                                    Thread.Sleep(100);
                                    action_selected = ActionType.Other;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Write correct info");
                                Console.WriteLine("");
                                info_adj += "'";
                                info_adj += Console.ReadLine();
                                info_adj += "'";
                            }

                            Console.WriteLine("Are you sure this is correct?: (Y/N)");

                            bool temp_bool = false;

                            while (temp_bool)
                            {
                                ConsoleKeyInfo key_fin = Console.ReadKey(true);

                                switch (key_fin.Key)
                                {
                                    case ConsoleKey.Y:
                                        temp_bool = false;
                                        BoardGame board = new BoardGame();
                                        board = ADOShopChecker.GetSpecificProdukt(Selection);
                                        ProduktAdminHandler.AdjustProdukt(info_col, info_adj, board.ProduktId ?? Selection);
                                        break;
                                    case ConsoleKey.N:
                                        temp_bool = false;
                                        action_selected = ActionType.Other;
                                        break;
                                    default:
                                        Console.WriteLine("Press either Y or N");
                                        break;
                                }
                            }

                            Thread.Sleep(2000);
                        }
                    }
                    Thread.Sleep(1000);
                }
                else
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    MenuNavigation(key.Key);
                }
                
                Thread.Sleep(100);
                Console.Clear();
                if (!GetCurMenu(5))
                {
                    menuin = false;
                }
            }
            while (menuin);
        }
        static void LogInMenu()
        {
            bool menuing = true;
            while (menuing)
            {
                Console.Clear();
                if (action_selected == ActionType.Other)
                {
                    TopBar(false);
                    List<BoardGameInformation> new_list = ADOShopChecker.GetAllBoardGameInfo();
                    //List<string> items = new_list[i - 1].GetInformation();
                    List<string> items = new List<string>() { "A. Navigera" };

                    items.Add(log_in_id >= 0 ? "B. Log out" : "B. Log in");

                    var gameCategory = new Window("Log in", 1, 2, items);
                    gameCategory.Draw();

                    ConsoleKeyInfo key = Console.ReadKey(true);

                    switch (key.Key)
                    {
                        case ConsoleKey.A:
                            Console.Write("Going back...");
                            Thread.Sleep(500);
                            action_selected = ActionType.Navigation;
                            Console.Clear();
                            break;
                        case ConsoleKey.B:
                            Console.WriteLine("Loading log in");
                            Thread.Sleep(500);
                            action_selected = ActionType.Selection;
                            break;
                        default:
                            Console.WriteLine("Fel input, pröva att skriva 'A' eller 'B'.");
                            Thread.Sleep(1000);
                            break;
                    }
                }
                else if (action_selected == ActionType.Selection && log_in_id < 0)
                {
                    Console.WriteLine($"Log in Page\n");

                    Console.WriteLine("What is your username?:");
                    
                    string user = Console.ReadLine();
                    if (user == null || user.Length < 1)
                    {
                        Console.WriteLine("Error Empty field!");
                    }
                    else
                    {
                        if (ADOShopChecker.CheckForUser(user))
                        {
                            Console.WriteLine("User found!");
                            Thread.Sleep(100);
                            Console.WriteLine("Write your password:");

                            string pass = Console.ReadLine();
                            Customer cus = ADOShopChecker.CompareLogIn(user, pass);
                            if (cus != null)
                            {
                                log_in_id = cus.PurchaseId ?? -1;
                                log_in_name = cus.Name;
                                Console.WriteLine("Logged in successfully");
                                Thread.Sleep(200);
                                if (log_in_id > 0)
                                {
                                    cur_menu = MenuEnum.MainMenu;
                                    action_selected = ActionType.Navigation;
                                }
                                else
                                {
                                    cur_menu = MenuEnum.Admin;
                                    action_selected = ActionType.Navigation;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Wrong password");
                                Thread.Sleep(600);
                            }
                        }
                        else
                        {
                            Console.WriteLine("User not found.");
                            Console.WriteLine("Do you wish to try again? Y or N");

                            ConsoleKeyInfo key = Console.ReadKey(true);
                            switch (key.Key)
                            {
                                case ConsoleKey.Y:
                                    Console.WriteLine("Reloading...");
                                    Thread.Sleep(200);
                                    action_selected = ActionType.Other; // För nu
                                    break;
                                case ConsoleKey.N:
                                    action_selected = ActionType.Other;
                                    Console.WriteLine("Returning to previous menu");
                                    Thread.Sleep(600);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                    Thread.Sleep(100);
                }
                else if (action_selected == ActionType.Selection)
                {
                    Console.WriteLine($"Log out Page\n");

                    Console.WriteLine("Do you want to log out? (Y/N):");

                    ConsoleKeyInfo key = Console.ReadKey(true);

                    switch (key.Key)
                    {
                        case ConsoleKey.Y:
                            log_in_id = -1;
                            log_in_name = "";
                            Console.WriteLine("Logging out...");
                            Thread.Sleep(500);
                            break;
                        case ConsoleKey.N:
                            Console.WriteLine("Returning to previous menu");
                            Thread.Sleep(600);
                            break;
                        default:
                            break;
                    }

                    action_selected = ActionType.Other;

                    Thread.Sleep(100);
                }
                else
                {
                    TopBar();
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    MenuNavigation(key.Key);
                    if (!GetCurMenu(0))
                    {
                        menuing = false;
                    }
                }
            }

            Thread.Sleep(50);
        }

        static void AdminInfoMenu()
        {
            List<string> menu_info = new List<string>();

            menu_info = new List<string>();
            List<ProdukSaleSimple> list_of_prod = new List<ProdukSaleSimple>();

            int i = 1;
            foreach (var item in ADOShopChecker.GetAllSales())
            {
                bool will_add = true;
                foreach (var prod in list_of_prod)
                {
                    if (prod.ProdukID == item.ProduktID)
                    {
                        prod.SoldAmnt += item.SoldAmnt;
                        will_add = false;
                    }
                }
                if (will_add)
                {
                    list_of_prod.Add(new ProdukSaleSimple()
                    {
                        Id = i,
                        ProdukID = item.ProduktID,
                        SoldAmnt = item.SoldAmnt,
                    });
                }
                i++;
            }

            list_of_prod.Sort(SortBySold);

            foreach (var item in list_of_prod)
            {
                BoardGame bg = ADOShopChecker.GetSpecificProdukt(item.ProdukID);
                BoardGameInformation bgi = ADOShopChecker.GetProductInfoById(item.ProdukID);
                if (menu_info.Count < 5)
                {
                    menu_info.Add(bg.Title + " x " + item.SoldAmnt + " - " + (bgi.Pris * item.SoldAmnt) + " kr total");
                }
            }


            var windowCategories = new Window("Mest Sålda (Antal)", 33, 3, menu_info);
            windowCategories.Draw();

            list_of_prod.Sort(SortBySaleGain);
            menu_info.Clear();
            foreach (var item in list_of_prod)
            {
                BoardGame bg = ADOShopChecker.GetSpecificProdukt(item.ProdukID);
                BoardGameInformation bgi = ADOShopChecker.GetProductInfoById(item.ProdukID);
                if (menu_info.Count < 5)
                {
                    menu_info.Add(bg.Title + " x " + item.SoldAmnt + " - " + (bgi.Pris * item.SoldAmnt) + " kr total");
                }
            }
            windowCategories = new Window("Mest Sålda (Vinst)", 33, 10, menu_info);
            windowCategories.Draw();
        }

        static int SortBySold(ProdukSaleSimple a, ProdukSaleSimple b)
        {
            if (a.SoldAmnt < b.SoldAmnt)
            {
                return 1;
            }
            if (b.SoldAmnt < a.SoldAmnt)
            {
                return -1;
            }

            return 0;
        }
        static int SortBySaleGain(ProdukSaleSimple a, ProdukSaleSimple b)
        {
            BoardGameInformation bgi_a = ADOShopChecker.GetProductInfoById(a.ProdukID);
            BoardGameInformation bgi_b = ADOShopChecker.GetProductInfoById(b.ProdukID);
            int a_price = (int)bgi_a.Pris * a.SoldAmnt;
            int b_price = (int)bgi_b.Pris * b.SoldAmnt;

            if (a_price < b_price)
            {
                return 1;
            }
            if (a_price > b_price)
            {
                return -1;
            }

            if (a.SoldAmnt < b.SoldAmnt)
            {
                return 1;
            }
            else if (b.SoldAmnt < a.SoldAmnt)
            {
                return -1;
            }
            
            return 0;
        }

        static List<string> GetMenuText()
        {
            List<string> menu_text = new List<string>() { "A. Startsida", "B. Shoppen", "C. Varukorg", "D. Admin", "E. Search", "P. Log In-Out" };

            if ((int)cur_menu <= menu_text.Count)
            {
                int cur_tar = (int)cur_menu;
                menu_text[((int)cur_menu)] += " (here)";
            }

            return menu_text;
        }

        static bool GetCurMenu(int i)
        {
            return (int)cur_menu == i;
        }

        static void SelectStoreCategory()
        {
            Console.Clear();
            TopBar(false);

            List<string> list = new List<string>();

            foreach (var item in ADOShopChecker.GetCategories())
            {
                list.Add(item.ProduktId + ". " + item.Kategori);
            }

            var windowCategories = new Window("Menuer", 2, 2, list);
            windowCategories.Draw();
            Console.WriteLine("Select Category with number 1-" + list.Count);
            Console.WriteLine("");
            int se = 1;
            if (Int32.TryParse(Console.ReadLine(), out se))
            {
                if (se <= list.Count)
                {
                    produkt_look = se;
                }
            }
            else
            {
                Console.WriteLine("Not a category");
                Thread.Sleep(1000);
                SelectStoreCategory();
            }
        }

        static void MenuNavigation(ConsoleKey e)
        {
            switch (e)
            {
                case ConsoleKey.A:
                    if (!GetCurMenu(0))
                    {
                        Console.Clear();
                        cur_menu = MenuEnum.MainMenu;
                        Thread.Sleep(25);
                        Console.SetCursorPosition(0, 0);
                    }
                    break;
                case ConsoleKey.B:
                    if (!GetCurMenu(1))
                    {
                        Console.Clear();
                        cur_menu = MenuEnum.Store;
                        action_selected = ActionType.Other;
                        SelectStoreCategory();
                        Thread.Sleep(25);
                        Console.SetCursorPosition(0, 0);
                    }
                    break;
                case ConsoleKey.C:
                    if (!GetCurMenu(4))
                    {
                        Console.Clear();
                        cur_menu = MenuEnum.Cart;
                        Thread.Sleep(25);
                        Console.SetCursorPosition(0, 0);
                    }
                    break;
                case ConsoleKey.D:
                    if (!GetCurMenu(5) && log_in_id == 0) // very secure obv
                    {
                        Console.Clear();
                        cur_menu = MenuEnum.Admin;
                        action_selected = ActionType.Other;
                        Thread.Sleep(25);
                        Console.SetCursorPosition(0, 0);
                    }
                    break;
                case ConsoleKey.E:
                    if (!GetCurMenu(7)) 
                    {
                        Console.Clear();
                        cur_menu = MenuEnum.Search;
                        action_selected = ActionType.Navigation;
                        Thread.Sleep(25);
                        Console.SetCursorPosition(0, 0);
                    }
                    break;
                case ConsoleKey.P:
                    if (!GetCurMenu(6))
                    {
                        Console.Clear();
                        cur_menu = MenuEnum.LogIn;
                        action_selected = ActionType.Other;
                        Thread.Sleep(25);
                        Console.SetCursorPosition(0, 0);
                    }
                    break;
                default:
                    break;
            }
        }

    }
}