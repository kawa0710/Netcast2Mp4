using System;
using System.Collections.Generic;

namespace Web.Models
{
    public partial class User
    {
        public User()
        {
            Channel = new HashSet<Channel>();
        }

        public int UsrSn { get; set; }
        public string UsrEmail { get; set; }
        public Guid UsrGuid { get; set; }
        public DateTime UsrCat { get; set; }
        public string UsrCuser { get; set; }
        public DateTime UsrMat { get; set; }
        public string UsrMuser { get; set; }
        public bool UsrIsActive { get; set; }

        public virtual UserPassword UserPassword { get; set; }
        public virtual ICollection<Channel> Channel { get; set; }
    }
}
