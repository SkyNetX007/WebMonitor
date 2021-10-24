using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSite.DatabaseAccess;
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

    public class chartInfo
    {
        public string deviceID;
        public int num;
        public List<double> records;

        public chartInfo() { }
        public chartInfo(Record firstRecord)
        {
            num = 1; deviceID = firstRecord.DEVICE_ID;
            records = new List<double>();
            records.Add(firstRecord.SPEED);
        }
        public bool isSameLine(Record r)
        {
            return (r.DEVICE_ID == deviceID);
        }
    }

    public class config
    {
        public string name = "default";
        public string deviceList;
        public double warningVal;
    }

    public class configSet
    {
        public config[] configs;
    }

    public class DBController : Controller
    {
        private IDBAccess DBAccess;

        public DBController(IDBAccess dbaccess)
        {
            DBAccess = dbaccess;
        }

        //插入数据
        public ActionResult<string> Create(int id, DateTime time, double ballDiameter, double deviceDiameter, string deviceID, int ballId, double speed, double passRate)
        {
            var _record = new Record(id, time, ballDiameter, deviceDiameter, deviceID, ballId, speed, passRate);

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
            ViewData["deviceList"] = GetDeviceCnf("Device.cnf").deviceList;
            ViewData["warningVal"] = GetDeviceCnf("Device.cnf").warningVal;
            return View();
        }

        //图表查询
        public ActionResult<string> Charts(string pos = "_all")
        {
            ViewData["POS"] = pos;
            ViewData["deviceList"] = GetDeviceCnf("Device.cnf").deviceList;
            return View();
        }


        //获取JSON记录
        public ActionResult<string> GetJSON(string pos = "_all", string filters = "{\"filters\":[]}", int N = 100)
        {
            var ins = DBAccess.GetRecord().ToList();
            if (pos != "_all")
            {
                ins.RemoveAll(n => n.DEVICE_ID != pos);
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
                        func = x => x.BALL_DIAMETER;
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
                //result += $"{{\"ID\":\"{results[num].ID}\",\"TIME\":\"{results[num].TIME}\",\"DIAMETER\":\"{results[num].BALL_DIAMETER}\",\"POS\":\"{results[num].POS}\",\"PASSED\":\"{results[num].PASSED}\"}},";
                result += $"{{";
                result += $"\"ID\":\"{results[num].ID}\"";
                result += $",\"TIME\":\"{results[num].TIME}\"";
                result += $",\"BALL_DIAMETER\":\"{results[num].BALL_DIAMETER}\"";
                result += $",\"DEVICE_DIAMETER\":\"{results[num].DEVICE_DIAMETER}\"";
                result += $",\"BALL_ID\":\"{results[num].BALL_ID}\"";
                result += $",\"DEVICE_ID\":\"{results[num].DEVICE_ID}\"";
                result += $",\"SPEED\":\"{results[num].SPEED}\"";
                result += $",\"PASS_RATE\":\"{results[num].PASS_RATE}\"";
                result += $"}},";
            }
            result = result.TrimEnd(',');
            result = "[" + result + "]";
            ViewData["json"] = result;
            return result;
        }

        //设备状态
        public ActionResult<string> GetStatus()
        {
            string deviceList = GetDeviceCnf("Device.cnf").deviceList;
            List<string> Device = deviceList.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();

            List<Record> ins;
            string TimeRecords = "";
            string result = "";

            foreach (var pos in Device)
            {
                ins = DBAccess.GetRecordByPos(pos).ToList();
                int status;
                if (ins.Count == 0) continue;
                DateTime recordTime = ins[ins.Count - 1].TIME;
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

                List<Status> statusIns = DBAccess.GetStatusByPos(pos).ToList();
                if (statusIns.Count == 0) continue;
                if (statusIns[0].ONLINE) { status = 1; } else { status = 0; }
                if (statusIns[0].ERROR) { status = 2; }
                TimeRecords += $"{{\"POS\":\"{pos}\",\"RecordTime\":\"{recordTime}\",\"Status\":{status},\"TotalRecords\":{ins.Count}}},";
            }
            result = "[" + TimeRecords.TrimEnd(',') + "]";
            return result;
        }

        public ActionResult<string> DataCheck(double upperLimit = 4.8)
        {
            return "";
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

        public config GetDeviceCnf(string config = "Device.cnf")
        {
            string deviceCnf = "";
            StreamReader DeviceConfig = new StreamReader(config, Encoding.Default);
            while (DeviceConfig.Peek() != -1)
            {
                //读取文件中的一行字符
                deviceCnf = DeviceConfig.ReadLine();
            }

            configSet cs = JsonConvert.DeserializeObject<configSet>(deviceCnf);
            config c = cs.configs[0];
            return c;
        }

        public ActionResult<string> getChartInfo(int N = 50, string POS = "_all")
        {
            var ins = DBAccess.GetRecord().ToList();
            ins.Reverse();
            List<chartInfo> set = new List<chartInfo>();
            foreach (var i in ins)
            {
                bool newLineRecord = true;
                if (set.Count == 0)
                {
                    chartInfo ci = new(i);
                    set.Add(ci);
                    continue;
                }
                foreach (chartInfo info in set)
                {
                    if (info.isSameLine(i))
                    {
                        newLineRecord = false;
                        if (info.num<N)
                        {
                            info.records.Add(i.SPEED);
                            info.num++;
                        }
                        break;
                    }
                }
                if (newLineRecord)
                {
                    set.Add(new chartInfo(i));
                }
            }
            string result = JsonConvert.SerializeObject(set);
            return result;
        }
    }
}
