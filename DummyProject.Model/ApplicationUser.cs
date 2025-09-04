
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyProject.Model
{
    public class ApplicationUser: IdentityUser//we inherit identityUser to add columns to existng table if we don't do that it'll create new table
    {
        [Required]
        public string Name { get; set; }
        [Display(Name = "Street Address")]
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        [Display(Name ="Postal Code")]
        public string PostalCode { get; set; }
        [Display(Name = "Company")]
        public int? CompanyId { get; set; }//it can be null
        [ForeignKey("CompanyId")]        
        public Company Company { get; set; }
        [NotMapped]
        public string Role {  get; set; }
    }
}
