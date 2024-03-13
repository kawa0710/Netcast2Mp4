using System;
using System.Collections.Generic;

namespace Web.Models
{
    public partial class SendMail
    {
        public int SmlSn { get; set; }
        public string SmlSubject { get; set; }
        public string SmlBody { get; set; }
        public string SmlTo { get; set; }
        public DateTime SmlCat { get; set; }
        public DateTime SmlSendAt { get; set; }
    }
}
