﻿using Microsoft.AspNetCore.Mvc;
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


        //取N条JSON记录
        public ActionResult<string> GetJSON(int N=100, string pos = "_all", string filterElement = "_none", string filterType = "BETWEEN", string incUp = "true", string incLow = "true", double upperLimit = 0, double lowerLimit = 0)
        {
            var ins = DBAccess.GetRecord().ToList();

            if (pos != "_all")
            {
                ins.RemoveAll(n => n.POS != pos);
            }
            if (filterElement != "_none")
            {
                //改变比较数值类型
                Func<Record, double> func = x => 0;
                if (filterElement == "DIAMETER")
                {
                    func = x => x.DIAMETER;
                }

                if (filterType == "BETWEEN")
                {
                    if (incUp == "false")
                        ins.RemoveAll(n => func(n) == upperLimit);
                    if (incLow == "false")
                        ins.RemoveAll(n => func(n) == lowerLimit);
                    ins.RemoveAll(n => (func(n) < lowerLimit || func(n) > upperLimit));
                }
                else if (filterType == "EXCEPT")
                {
                    if (incUp == "false")
                        ins.RemoveAll(n => func(n) == upperLimit);
                    if (incLow == "false")
                        ins.RemoveAll(n => func(n) == lowerLimit);
                    ins.RemoveAll(n => (func(n) > lowerLimit && func(n) < upperLimit));
                }
                else if (filterType == "GREATER")
                {
                    if (incLow == "false")
                        ins.RemoveAll(n => func(n) == lowerLimit);
                    ins.RemoveAll(n => func(n) < lowerLimit);
                }
                else if (filterType == "LESS")
                {
                    if (incUp == "false")
                        ins.RemoveAll(n => func(n) == upperLimit);
                    ins.RemoveAll(n => func(n) > upperLimit);
                }
            }
            if (ins.Count < N || N == -1)
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
                TimeRecords += $"{{\"POS\":\"{pos}\",\"RecordTime\":\"{recordTime}\",\"Status\":{status}}},";
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

        public ActionResult<string> GetTableResult(string line = "_none", string filterElement = "_none", string filterType = "BETWEEN", string incUp = "true", string incLow = "true", double upperLimit = 0, double lowerLimit = 0, int N = 100)
        {
            var ins = DBAccess.GetRecord().ToList();
            if (line != "_none")
            {
                ins.RemoveAll(n => n.POS != line);
            }
            if (filterElement != "_none")
            {
                //改变比较数值类型
                Func<Record, double> func = x => 0;
                if (filterElement == "DIAMETER")
                {
                    func = x => x.DIAMETER;
                }

                if (filterType == "BETWEEN")
                {
                    if (incUp == "false")
                        ins.RemoveAll(n => func(n) == upperLimit);
                    if (incLow == "false")
                        ins.RemoveAll(n => func(n) == lowerLimit);
                    ins.RemoveAll(n => (func(n) < lowerLimit || func(n) > upperLimit));
                }
                else if (filterType == "EXCEPT")
                {
                    if (incUp == "false")
                        ins.RemoveAll(n => func(n) == upperLimit);
                    if (incLow == "false")
                        ins.RemoveAll(n => func(n) == lowerLimit);
                    ins.RemoveAll(n => (func(n) > lowerLimit && func(n) < upperLimit));
                }
                else if (filterType == "GREATER")
                {
                    if (incLow == "false")
                        ins.RemoveAll(n => func(n) == lowerLimit);
                    ins.RemoveAll(n => func(n) < lowerLimit);
                }
                else if (filterType == "LESS")
                {
                    if (incUp == "false")
                        ins.RemoveAll(n => func(n) == upperLimit);
                    ins.RemoveAll(n => func(n) > upperLimit);
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
