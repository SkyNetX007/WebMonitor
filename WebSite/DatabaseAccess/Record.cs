using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSite.DatabaseAccess
{
    [Table("all_lines")]
    public class Record
    {
        [Key, Column("ID")]
        public int ID { get; set; }

        [Column("TIME")]
        public DateTime TIME { get; set; }

        //[Column("POS")]
        //public string POS { get; set; }

        //[Column("PASSED")]
        //public bool PASSED { get; set; }

        //[Column("DIAMETER")]
        //public double DIAMETER { get; set; }

        [Column("BALL_DIAMETER")]
        public double BALL_DIAMETER { get; set; }

        [Column("DEVICE_DIAMETER")]
        public double DEVICE_DIAMETER { get; set; }

        [Column("BALL_ID")]
        public int BALL_ID { get; set; }

        [Column("DEVICE_ID")]
        public string DEVICE_ID {  get; set; }

        [Column("SPEED")]
        public double SPEED { get; set; }

        [Column("PASS_RATE")]
        public double PASS_RATE {  get; set; }

        [Column("RECORD")]
        public int RECORD_CNT { get; set; }

        [Column("BATCH")]
        public string BATCH { get; set; }

        //public Record(int ID, DateTime TIME, double DIAMETER, string POS, bool PASSED)
        //{
        //    this.ID = ID; this.TIME = TIME; this.DIAMETER = DIAMETER; this.POS = POS; this.PASSED = PASSED;
        //}
        public Record(int ID, DateTime time, double ballDiameter, double deviceDiameter, string deviceID, int ballID, double speed, double passRate, int recordCnt, string batch)
        {
            this.ID = ID;
            this.TIME = time;
            this.BALL_DIAMETER = ballDiameter;
            this.DEVICE_DIAMETER = deviceDiameter;
            this.DEVICE_ID = deviceID;
            this.BALL_ID = ballID;
            this.SPEED = speed;
            this.PASS_RATE = passRate;
            this.RECORD_CNT = recordCnt;
            this.BATCH = batch;
        }

        public Record() { }
    }
}
