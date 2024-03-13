using System;
using System.Collections.Generic;

namespace Web.Models
{
    public partial class UserPassword
    {
        public int UsrSn { get; set; }
        public string UpwPwd { get; set; }
        public DateTime UpwCat { get; set; }

        public virtual User UsrSnNavigation { get; set; }
    }
}
