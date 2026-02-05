using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowDemo.Models
{
    public partial class ProduktSales
    {
        public int Id { get; set; }
        public int ProduktID { get; set; }

        public int SoldAmnt { get; set; }

        public DateTime SaleDate { get; set; }

        public int? CustomerID { get; set; } = -1; // Default value, not logged in
    }
    public partial class ProdukSaleSimple
    {
        public int Id { get; set; }
        public int ProdukID { get; set; }
        public int SoldAmnt { get; set; }
    }
}
