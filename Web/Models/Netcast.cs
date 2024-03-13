using System;
using System.Collections.Generic;

namespace Web.Models
{
    public partial class Netcast
    {
        public Netcast()
        {
            Flim = new HashSet<Flim>();
            Image = new HashSet<Image>();
        }

        public int NtcSn { get; set; }
        public int ChnSn { get; set; }
        public string NtcInfo { get; set; }
        public string NtcGuid { get; set; }

        public virtual Channel ChnSnNavigation { get; set; }
        public virtual ICollection<Flim> Flim { get; set; }
        public virtual ICollection<Image> Image { get; set; }
    }
}
