using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSite.DatabaseAccess;
using System.Text.Json;
using Newtonsoft.Json;

namespace WebSite.Controllers
{
    public class filter
    {
        public string element;
        public string type;
        public double upperLimit;
        public string incUpper;
        public double lowerLimit;
        public string incLower;
    }

    public class filterSet
    {
        public filter[] filters;
    }

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
            ViewData["deviceList"] = GetDevicelist("Device.cnf");
            return View();
        }

        //图表查询
        public ActionResult<string> Charts(string pos = "_all")
        {
            ViewData["POS"] = pos;
            ViewData["deviceList"] = GetDevicelist("Device.cnf");
            return View();
        }


        //获取JSON记录
        public ActionResult<string> GetJSON(string pos = "_all", string filters = "{\"filters\":[]}", int N = 100)
        {
            var ins = DBAccess.GetRecord().ToList();
            if (pos != "_all")
            {
                ins.RemoveAll(n => n.POS != pos);
            }
            filterSet fs = JsonConvert.DeserializeObject<filterSet>(filters);
            foreach (filter f in fs.filters)
            {
                if (f.element != "_none")
                {
                    //改变比较数值类型
                    Func<Record, double> func = x => 0;
                    if (f.element == "DIAMETER")
                    {
                        func = x => x.DIAMETER;
                    }
                    else if (f.element == "DATE")
                    {
                        func = x => ((x.TIME - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime()).TotalSeconds * 1000);
                    }

                    if (f.type == "BETWEEN")
                    {
                        if (f.incUpper == "false")
                            ins.RemoveAll(n => func(n) == f.upperLimit);
                        if (f.incLower == "false")
                            ins.RemoveAll(n => func(n) == f.lowerLimit);
                        ins.RemoveAll(n => (func(n) < f.lowerLimit || func(n) > f.upperLimit));
                    }
                    else if (f.type == "EXCEPT")
                    {
                        if (f.incUpper == "false")
                            ins.RemoveAll(n => func(n) == f.upperLimit);
                        if (f.incLower == "false")
                            ins.RemoveAll(n => func(n) == f.lowerLimit);
                        ins.RemoveAll(n => (func(n) > f.lowerLimit && func(n) < f.upperLimit));
                    }
                    else if (f.type == "GREATER")
                    {
                        if (f.incLower == "false")
                            ins.RemoveAll(n => func(n) == f.lowerLimit);
                        ins.RemoveAll(n => func(n) < f.lowerLimit);
                    }
                    else if (f.type == "LESS")
                    {
                        if (f.incUpper == "false")
                            ins.RemoveAll(n => func(n) == f.upperLimit);
                        ins.RemoveAll(n => func(n) > f.upperLimit);
                    }
                }
            }
            if (ins.Count < N || N == -1)
                N = ins.Count;
            var results = new Record[N];
            if (ins.Count != 0)
            {
                int len = ins.Count - 1;
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

        //设备状态
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
                DateTime recordTime = ins[ins.Count-1].TIME;
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
                TimeRecords += $"{{\"POS\":\"{pos}\",\"RecordTime\":\"{recordTime}\",\"Status\":{status},\"TotalRecords\":{ins.Count}}},";
            }
            result = "[" + TimeRecords.TrimEnd(',') + "]";
            return result;
        }

        public ActionResult<string> DataCheck(double upperLimit = 4.8)
        {
            var ins = DBAccess.GetRecord();
            var result = "";
            //StreamWriter LogFile = new StreamWriter("Status.log");
            if (ins.Count() != 0)
            {
                foreach (var s in ins)
                {
                    if (s.DIAMETER > upperLimit)
                    {
                        TimeSpan DiffSeconds = new TimeSpan(DateTime.Now.Ticks - s.TIME.Ticks);
                        if (DiffSeconds.TotalSeconds < 60)
                        {
                            result += $"{{\"POS\":\"{s.POS}\",\"ID\":{s.ID},\"DIAMETER\":{s.DIAMETER}}},";
                        }
                        //LogFile.WriteLine($"{{{s.TIME}:\"POS\":\"{s.POS}\",\"ID\":{s.ID},\"DIAMETER\":{s.DIAMETER}}},");
                    }
                }
                result = "[" + result.TrimEnd(',') + "]";
            }
            else
            {
                result = "[]";
            }
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

        public string GetDevicelist(string config = "Device.cnf")
        {
            string deviceLists = "";
            StreamReader DeviceConfig = new StreamReader(config, Encoding.Default);
            String line;
            while ((line = DeviceConfig.ReadLine()) != null)
            {
                deviceLists = deviceLists + line + ",";
            }
            return deviceLists.TrimEnd(',');
        }
    }
}
