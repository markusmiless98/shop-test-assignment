using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowDemo
{
    public partial class CustomerPurchase
    {
        public string Title { get; set; }
        public int GameID {  get; set; }
        public int SoldAmnt { get; set; }
        public string Name { get; set; }
        public int CustomerID { get; set; }

        public DateTime SaleDate { get; set; }

    }
}
