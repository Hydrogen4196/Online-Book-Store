using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyProject.Model
{
    public class OrderHeader
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }//user who is making order
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }
        [Required]
        public DateTime OrderDate { get; set; }//order date
        [Required]
        public DateTime ShippingDate { get; set; }//shiping date
        public double OrderTotal { get; set; }//total
        public string TrackingNumber { get; set; }
        public string Carrier { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime PaymentDate { get; set; }//for indUser
        public DateTime PaymentDueDate { get; set; }// for company user
        public string TransactionId { get; set; }

        //Dilivery Address---------------------
        [Required]
        public string  Name { get; set; }
        [Required]
        [Display(Name ="Street Address")]
        public string StreetAddress { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Region { get; set; }
        [Required]
        [Display(Name="Postal Code")]
        public string PostalCode { get; set; }
        [Required]
        [Display(Name="Phone Number")]
        public string PhoneNumber { get; set; }

    }
}
