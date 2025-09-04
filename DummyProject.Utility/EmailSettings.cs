using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyProject.Utility
{
    public class EmailSettings
    {
        public String PrimaryDomain { get; set; }//server domain
        public int PrimaryPort { get; set; }// server port number
        public String SecondryDomain { get; set; }
        public int SecondryPort { get;set; }
        public String UsernameEmail { get; set; }//email address used for authentication
        public String UsernamePassword { get; set; }//Password to authenticate the email user
        public String FromEmail { get; set; }// sender's email address
        public String ToEmail { get; set; }//recipient's email address
        public String CcEmail { get; set; }//Email addresses for carbon copy

    }
}
