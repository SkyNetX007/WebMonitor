using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSite.DatabaseAccess
{
    [Table("users")]
    public class User
    {
        [Key, Column("ID")]
        public int ID { get; set; }

        [Column("USERNAME")]
        public string USERNAME { get; set; }

        [Column("ROLE")]
        public string ROLE { get; set; }

        [Column("PASSWD")]
        public string PASSWD { get; set; }

        public User(int ID, string USERNAME, string ROLE, string PASSWD)
        {
            this.ID = ID;
            this.USERNAME = USERNAME;
            this.ROLE = ROLE;
            this.PASSWD = PASSWD;
        }

        public User() { }
    }
}
