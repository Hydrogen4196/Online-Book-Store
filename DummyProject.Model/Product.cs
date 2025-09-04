using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyProject.Model
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Author { get; set; }
        [Required]
        public string ISBN { get; set; }
        [Required]
        [Range(10, 1000)]
        public double ListPrice { get; set; }  //main price 1000
        [Required]
        [Range(10, 1000)]
        public double Price { get; set; }//550
        [Required]
        [Range(10, 1000)]
        public double Price50 { get; set; }//50 rp discount 500
        [Required]
        [Range(10, 1000)]
        public double Price100 { get; set; }//100 rp discount 450





        [Display(Name ="Image Url")]
        public string ImageUrl { get; set; }



        [Required]
        [Display(Name =("Category"))]
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        [Required]
        [Display(Name = "Cover Type")]
        public int CoverTypeId { get; set; }
        public CoverType CoverType { get; set; }
    }
}
