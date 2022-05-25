using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace WebSite.DatabaseAccess
{
    //public class DBContext : DbContext
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        { }

        public DbSet<Record> Record { get; set; }
        public DbSet<Status> Status { get; set; }
        public DbSet<User> User { get; set; }
    }
}
