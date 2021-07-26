using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSite.DatabaseAccess
{
    [Table("steelball")]
    public class Record
    {
        [Column("ID")]
        public int ID { get; set; }

        [Column("TIME")]
        public DateTime TIME { get; set; }

        [Column("POS")]
        public string POS { get; set; }

        [Column("PASSED")]
        public bool PASSED { get; set; }

        [Column("DIAMETER")]
        public double DIAMETER { get; set; }

        public Record(int ID, DateTime TIME, double DIAMETER, string POS, bool PASSED)
        {
            this.ID = ID; this.TIME = TIME; this.DIAMETER = DIAMETER; this.POS = POS; this.PASSED = PASSED;
        }
    }
}
