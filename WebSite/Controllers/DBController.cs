using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSite.DatabaseAccess;

namespace WebSite.Controllers
{
    public class DBController : Controller
    {
        private IDBAccess DBAccess;
        
        public DBController(IDBAccess dbaccess)
        {
            DBAccess = dbaccess;
        }
        
        //插入数据
        public ActionResult<string> Create(int ID, DateTime TIME, double DIAMETER, string POS, bool PASSED)
        {
            var _record = new Record(ID, TIME, DIAMETER, POS, PASSED);

            var result = DBAccess.CreateRecord(_record);

            if (result)
            {
                return "插入成功";
            }
            else
            {
                return "插入失败";
            }

        }

        //取全部记录
        public ActionResult<string> Gets()
        {
            List<List<string>> tableResult = new List<List<string>>();
            var ins = DBAccess.GetRecord();
            if (ins.Count() != 0)
            {
                foreach (var s in ins)
                {
                    List<string> tmpTableResult = new List<string>();
                    tmpTableResult.Add($"{s.ID}");
                    tmpTableResult.Add($"{s.TIME}");
                    tmpTableResult.Add($"{s.DIAMETER}");
                    tmpTableResult.Add($"{s.POS}");
                    tmpTableResult.Add($"{s.PASSED}");
                    tableResult.Add(tmpTableResult);
                }
            }
            tableResult.Reverse();
            ViewData["tableResult"] = tableResult;
            return View();
        }

        //图表查询
        public ActionResult<string> Charts()
        {
            return View();
        }
        

        //取N条JSON记录
        public ActionResult<string> GetJSON(int N=100, string pos ="All" )
        {
            List<string> Device = new List<string>();
            StreamReader DeviceConfig = new StreamReader("Device.cnf", Encoding.Default);
            String line;
            while ((line = DeviceConfig.ReadLine()) != null)
            {
                Device.Add(line.ToString());
            }

            List<Record> ins = DBAccess.GetRecord().ToList();
            if (pos != "All")
            {
                ins = DBAccess.GetRecordByPos(pos).ToList();
            }
            
            if (ins.Count() < N)
                N = ins.Count();
            var results = new Record[N];
            if (ins.Count() != 0)
            {
                int len = ins.Count() - 1;
                for (int num = 0; num < N; num++)
                {
                    results[num] = ins[len];
                    len--;
                    if (len < 0) break;
                }
            }
            string result = "";
            for (int num = 0; num < N; num++)
            {
                result += $"{{\"ID\":\"{results[num].ID}\",\"TIME\":\"{results[num].TIME}\",\"DIAMETER\":\"{results[num].DIAMETER}\",\"POS\":\"{results[num].POS}\",\"PASSED\":\"{results[num].PASSED}\"}},";
            }
            result = result.TrimEnd(',');
            result = "[" + result + "]";
            ViewData["json"] = result;
            return result;
        }

        public ActionResult<string> GetStatus()
        {
            List<string> Device = new List<string>();
            StreamReader DeviceConfig = new StreamReader("Device.cnf", Encoding.Default);
            String line;
            while ((line = DeviceConfig.ReadLine()) != null)
            {
                Device.Add(line.ToString());
            }

            List<Record> ins;
            string TimeRecords = "";
            string result = "";

            foreach (var pos in Device)
            {
                ins = DBAccess.GetRecordByPos(pos).ToList();
                int status;
                if (ins.Count == 0) continue;
                DateTime recordTime = ins[0].TIME;
                DateTime currentTime = DateTime.Now;
                TimeSpan DiffSeconds = new TimeSpan(currentTime.Ticks - recordTime.Ticks);
                if (DiffSeconds.TotalSeconds > 30)
                {
                    status = 0;
                }
                else
                { 
                    status = 1;
                }
                TimeRecords += $"{{\"POS\":\"{pos}\",\"RecordTime\":\"{recordTime}\",\"Status\":{status}}},";
            }
            result = "[" + TimeRecords.TrimEnd(',') + "]";
            return result;
        }

        //取某id记录
        public ActionResult<string> Get(int id)
        {
            var result = "没有数据";
            var _record = DBAccess.GetRecordByID(id);

            if (_record != null)
            {
                result = _record.ToString();
            }

            return result;
        }
    }
}
