using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyProject.Model.ViewModels
{
    public class ProductVM
    {
        public Product Product { get; set; }//access product model
        public IEnumerable<SelectListItem> CategoryList { get; set; }//make two properties to get all data
        public IEnumerable<SelectListItem> CoverTypeList { get; set; }

    }
}
