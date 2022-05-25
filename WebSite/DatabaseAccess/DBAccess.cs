using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebSite.DatabaseAccess
{
    public class DBAccess : IDBAccess
    {
        public DBContext Context;

        public DBAccess(DBContext context)
        {
            Context = context;
        }
        
        // 生产数据
        // 插入数据
        public bool CreateRecord(Record ins)
        {
            Context.Record.Add(ins);
            return Context.SaveChanges() > 0;
        }

        // 取全部记录
        public IEnumerable<Record> GetRecord()
        {
            return Context.Record.ToList();
        }

        public IEnumerable<Status> GetStatusRecord()
        {
            return Context.Status.ToList();
        }

        // 取某设备记录
        public IEnumerable<Record> GetRecordByPos(string pos)
        {
            return Context.Record.Where(s => s.DEVICE_ID == pos).ToList();
        }

        public IEnumerable<Status> GetStatusByPos(string pos)
        {
            return Context.Status.Where(s => s.DEVICE_ID == pos).ToList();
        }

        // 取某id记录
        public Record GetRecordByID(int id)
        {
            return Context.Record.SingleOrDefault(s => s.ID == id);
        }

        // 根据id更新整条记录
        public bool UpdateRecord(Record ins)
        {
            Context.Record.Update(ins);
            return Context.SaveChanges() > 0;
        }
        
        // 根据id更新名称
        public bool UpdateValByID(int id, string val)
        {
            var state = false;
            var ins = Context.Record.SingleOrDefault(s => s.ID == id);

            if (ins != null)
            {
                ins.DEVICE_ID = val;
                state = Context.SaveChanges() > 0;
            }

            return state;
        }

        // 根据id删掉记录
        public bool DeleteRecordByID(int id)
        {
            var ins = Context.Record.SingleOrDefault(s => s.ID == id);
            Context.Record.Remove(ins);
            return Context.SaveChanges() > 0;
        }

        // 用户数据
        // 插入数据
        public bool CreateUser(User ins)
        {
            Context.User.Add(ins);
            return Context.SaveChanges() > 0;
        }

        // 取某id记录
        public User GetUserByID(int id)
        {
            return Context.User.SingleOrDefault(s => s.ID == id);
        }

        // 根据id更新整条记录
        public bool UpdateUser(User ins)
        {
            Context.User.Update(ins);
            return Context.SaveChanges() > 0;
        }

        // 根据id删掉记录
        public bool DeleteUserByID(int id)
        {
            var ins = Context.User.SingleOrDefault(s => s.ID == id);
            Context.User.Remove(ins);
            return Context.SaveChanges() > 0;
        }
    }
}
