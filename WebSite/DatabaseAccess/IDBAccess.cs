using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebSite.DatabaseAccess
{
    public interface IDBAccess
    {
        //插入数据
        bool CreateRecord(Record ins);

        //取全部记录
        IEnumerable<Record> GetRecord();

        //取某设备记录
        IEnumerable<Record> GetRecordByPos(string pos);

        //取某id记录
        Record GetRecordByID(int id);

        //根据id更新整条记录
        bool UpdateRecord(Record ins);

        //根据id更新名称
        bool UpdateValByID(int id, string val);

        //根据id删掉记录
        bool DeleteRecordByID(int id);
    }
}
