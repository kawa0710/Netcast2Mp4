using System;
using System.Collections.Generic;

namespace Web.Models
{
    public partial class Image
    {
        public int ImgSn { get; set; }
        public int NtcSn { get; set; }
        public string ImgGuid16 { get; set; }
        public string ImgExt { get; set; }

        public virtual Netcast NtcSnNavigation { get; set; }
    }
}
