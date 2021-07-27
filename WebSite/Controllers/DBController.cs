using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
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
            var result = "没有数据";
            var ins = DBAccess.GetRecord();
            if (ins.Count() != 0)
            {
                result = "";
                foreach (var s in ins)
                {
                    result += $"{s.ID} \t {s.TIME} \t {s.DIAMETER} \t {s.POS} \t {s.PASSED} \n";
                }   
            }
            ViewData["result"] = result;
            return View();
        }

        //取全部记录
        public ActionResult<string> Charts()
        {
            var results = new Record[10];
            var ids = new int[10];
            var diameters = new double[10];
            var ins = DBAccess.GetRecord().ToList();
            if (ins.Count() != 0)
            {
                int len = ins.Count() - 1;
                for (int num = 0; num < 10; num++)
                {
                    results[num] = ins[len];
                    ids[num] = ins[len].ID;
                    diameters[num] = ins[len].DIAMETER;
                    len--;
                    if (len < 0) break;
                }
            }
            string result = "";
            for (int num = 0; num < 10; num++)
            {
                result += $"{{\"IDS\":\"{ids[num]}\",\"DIAMETERS\":\"{diameters[num]}\"}},";
            }
            result = result.TrimEnd(',');
            result = "[" + result + "]";
            ViewData["ids"] = string.Join(",",ids);
            ViewData["diameters"] = string.Join(",", diameters);
            ViewData["json"] = result;
            return View();
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
