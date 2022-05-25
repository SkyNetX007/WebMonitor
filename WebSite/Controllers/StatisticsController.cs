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
    public class StatisticsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        private IDBAccess DBAccess;

        public StatisticsController(IDBAccess dbaccess)
        {
            DBAccess = dbaccess;
        }

        public ActionResult<string> Basic(string pos = "_all", string filters = "{\"filters\":[]}")
        {
            var ins = DBAccess.GetRecord().ToList();
            if (pos != "_all")
            {
                ins.RemoveAll(n => n.DEVICE_ID != pos);
            }
            filterSet fs = JsonConvert.DeserializeObject<filterSet>(filters);
            foreach (filter f in fs.filters)
            {
                if (f.element == "BATCH")
                {
                    Func<Record, String> func = x => x.BATCH;
                    ins.RemoveAll(n => func(n) != f.batch);
                }
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
            List<double> inputList = new List<double>();
            foreach (Record record in ins)
            {
                inputList.Add(record.BALL_DIAMETER);
            }
            string MeanData = MathNet.Numerics.Statistics.Statistics.Mean(inputList).ToString("####0.00000");
            string MedianData = MathNet.Numerics.Statistics.Statistics.Median(inputList).ToString("####0.00000");
            string MinimumData = MathNet.Numerics.Statistics.Statistics.Minimum(inputList).ToString("####0.00000");
            string MaximumData = MathNet.Numerics.Statistics.Statistics.Maximum(inputList).ToString("####0.00000");
            string PopulationVarianceData = MathNet.Numerics.Statistics.Statistics.PopulationVariance(inputList).ToString("####0.00000");
            string VarianceData = MathNet.Numerics.Statistics.Statistics.Variance(inputList).ToString("####0.00000");
            string StandardDeviationData = MathNet.Numerics.Statistics.Statistics.StandardDeviation(inputList).ToString("####0.00000");
            string PopulationStandardDeviationData = MathNet.Numerics.Statistics.Statistics.PopulationStandardDeviation(inputList).ToString("####0.00000");
            
            string result = "";
            result += $"{{";
            result += $"\"MeanData\":\"{MeanData}\"";
            result += $",\"MedianData\":\"{MedianData}\"";
            result += $",\"MinimumData\":\"{MinimumData}\"";
            result += $",\"MaximumData\":\"{MaximumData}\"";
            result += $",\"PopulationVarianceData\":\"{PopulationVarianceData}\"";
            result += $",\"VarianceData\":\"{VarianceData}\"";
            result += $",\"StandardDeviationData\":\"{StandardDeviationData}\"";
            result += $",\"PopulationStandardDeviationData\":\"{PopulationStandardDeviationData}\"";
            result += $"}}";

            return result;
        }
    }
}
