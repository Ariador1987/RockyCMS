using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rocky.Models.ViewModels
{
    public class ProductUpsertVM
    {
        public IEnumerable<SelectListItem> SelectListItems { get; set; }
        public Product Product { get; set; }
    }
}
