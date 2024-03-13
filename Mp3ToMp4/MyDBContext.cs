using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Mp3ToMp4
{
    public partial class Netcast2Mp4Entities : DbContext
    {
        public Netcast2Mp4Entities(string connectionString)
            : base(connectionString)
        {

        }
    }
}
