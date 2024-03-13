using System;
using System.Collections.Generic;

namespace Web.Models
{
    public partial class Flim
    {
        public int FlmSn { get; set; }
        public int NtcSn { get; set; }
        public string FlmFileName { get; set; }
        public byte FlmStatus { get; set; }
        public DateTime FlmCat { get; set; }
        public DateTime FlmMat { get; set; }
        public bool FlmIsActive { get; set; }
        public string FlmSha256 { get; set; }

        public virtual Netcast NtcSnNavigation { get; set; }
    }
}
