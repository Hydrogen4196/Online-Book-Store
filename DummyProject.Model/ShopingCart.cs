using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyProject.Model
{
    public class ShopingCart
    {
        public ShopingCart()                                  //count can't be zero in case of cart
        {
            Count = 1;
        }
        public int Id { get; set; }
        public string  ApplicationUserId { get; set; }            //to check user
        [ForeignKey("ApplicaionUserId")]
        public ApplicationUser ApplicationUser { get; set; }
        public int ProductId { get; set; }                    //to check product
        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        public int Count { get; set; }                        //to count
        [NotMapped]                                           // price will not be stored in database 
        public double Price { get; set; }                     //double shows we can store work with dynamic value 
    }
}
