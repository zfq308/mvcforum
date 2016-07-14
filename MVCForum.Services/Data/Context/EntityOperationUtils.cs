using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
namespace MVCForum.Services.Data.Context
{
   
    /// <summary> Entity Framework公共的增删改方法。返回的是受影响的行数 </summary>
    public class EntityOperationUtils
    {
        //新增
        public static int InsertObject(object obj)
        {
            Type t = obj.GetType();
            int effect = -1;
            using (MVCForumContext con = new MVCForumContext())
            {
                DbSet set = con.Set(t);
                set.Add(obj);
                effect = con.SaveChanges();
                return effect;
            }
        }

        //批量新增
        public static int InsertObjects(IEnumerable<object> objs)
        {
            int effect = 0;

            var et = objs.GetEnumerator();
            if (et.MoveNext())
            {
                Type t = et.Current.GetType();
                using (MVCForumContext con = new MVCForumContext())
                {
                    DbSet set = con.Set(t);
                    foreach (var o in objs)
                    {
                        set.Add(o);
                    }
                    effect = con.SaveChanges();
                }
            }

            return effect;
        }

        //修改
        public static int ModifyObject(object obj)
        {
            int effect = -1;
            using (MVCForumContext con = new MVCForumContext())
            {
                DbEntityEntry entry = con.Entry(obj);
                entry.State = EntityState.Modified;
                effect = con.SaveChanges();
                return effect;
            }
        }

        //批量修改
        public static int ModifyObjects(IEnumerable<object> objs)
        {
            int effect = 0;
            var et = objs.GetEnumerator();
            if (et.MoveNext())
            {
                Type t = et.Current.GetType();
                using (MVCForumContext con = new MVCForumContext())
                {
                    foreach (var o in objs)
                    {
                        DbEntityEntry entry = con.Entry(o);
                        entry.State = EntityState.Modified;
                    }
                    effect = con.SaveChanges();
                }
            }

            return effect;
        }

        //删除
        public static int DeleteObject(object obj)
        {
            int effect = -1;
            using (MVCForumContext con = new MVCForumContext())
            {
                DbEntityEntry entry = con.Entry(obj);
                entry.State = EntityState.Deleted;
                effect = con.SaveChanges();
                return effect;
            }
        }

        //批量删除
        public static int DeleteObjects(IEnumerable<object> objs)
        {
            int effect = 0;

            var et = objs.GetEnumerator();
            if (et.MoveNext())
            {
                Type t = et.Current.GetType();
                using (MVCForumContext con = new MVCForumContext())
                {
                    foreach (var o in objs)
                    {
                        DbEntityEntry entry = con.Entry(o);
                        entry.State = EntityState.Deleted;
                    }
                    effect = con.SaveChanges();
                }
            }
            return effect;
        }
    }
}