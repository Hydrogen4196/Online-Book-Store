using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyProject.Model.ViewModels
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShopingCart> listCart {  get; set; }//product in user's cart
        public OrderHeader OrderHeader { get; set; }
    }
}
