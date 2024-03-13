using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace MySendMail
{
    public partial class Netcast2Mp4Entities : DbContext
    {
        public Netcast2Mp4Entities(string connectionString)
            : base(connectionString)
        {

        }
    }
}
