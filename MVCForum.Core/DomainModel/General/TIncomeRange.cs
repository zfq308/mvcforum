using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MVCForum.Domain.DomainModel.General
{

    public class TIncomeRange
    {
        public string IncomeRangeId;
        public string IncomeRangeName;

        public static List<TIncomeRange> LoadAllIncomeList()
        {
            var list = new List<TIncomeRange>();
            list.Add(new TIncomeRange() { IncomeRangeId = "1", IncomeRangeName = "1万以下" });
            list.Add(new TIncomeRange() { IncomeRangeId = "2", IncomeRangeName = "1万至5万" });
            list.Add(new TIncomeRange() { IncomeRangeId = "3", IncomeRangeName = "5万以上" });

            return list;
        }

        public static List<TIncomeRange> LoadForSearchIncomeList()
        {
            var list = new List<TIncomeRange>();
            list.Add(new TIncomeRange() { IncomeRangeId = "1", IncomeRangeName = "1万以下" });
            list.Add(new TIncomeRange() { IncomeRangeId = "2", IncomeRangeName = "1万以上" });
            list.Add(new TIncomeRange() { IncomeRangeId = "3", IncomeRangeName = "5万以上" });
            return list;
        }

        public static TIncomeRange LoadIncomeRangeByIncomeRangeId(string IncomeRangeId)
        {
            return LoadAllIncomeList().Where(x => x.IncomeRangeId == IncomeRangeId).FirstOrDefault();
        }
    }

}