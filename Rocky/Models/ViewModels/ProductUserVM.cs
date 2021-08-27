using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rocky.Models.ViewModels
{
    public class ProductUserVM
    {
        // mozemo inicijalizirat listu ovdje u konstruktoru, i ovo je ispravan nacin izbjegavanja error u kontrolleru jer smo assignali novi 
        // objekt našen produkt listu
        public ProductUserVM()
        {
            ProductList = new List<Product>();
        }
        public ApplicationUser ApplicationUser { get; set; }
        public IList<Product> ProductList { get; set; }


    }
}
