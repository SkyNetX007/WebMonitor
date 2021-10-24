using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSite.DatabaseAccess
{
    [Table("devicestatus")]
    public class Status
    {
        [Key, Column("DEVICE_ID")]
        public string DEVICE_ID { get; set; }

        [Column("ONLINE")]
        public bool ONLINE { get; set; }

        [Column("ERROR")]
        public bool ERROR { get; set; }

        public Status(string DEVICE_ID, bool ONLINE, bool ERROR)
        {
            this.DEVICE_ID = DEVICE_ID;
            this.ONLINE = ONLINE;
            this.ERROR = ERROR;
        }

        public Status() { }
    }
}
