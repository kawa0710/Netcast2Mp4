using System;
using System.Collections.Generic;

namespace Web.Models
{
    public partial class Channel
    {
        public Channel()
        {
            Netcast = new HashSet<Netcast>();
        }

        public int ChnSn { get; set; }
        public int UsrSn { get; set; }
        public byte ChnStatus { get; set; }
        public string ChnInfo { get; set; }
        public DateTime ChnCat { get; set; }

        public virtual User UsrSnNavigation { get; set; }
        public virtual ICollection<Netcast> Netcast { get; set; }
    }
}
