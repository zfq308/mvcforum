using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MVCForum.Domain.DomainModel.General
{
    public class TEducation
    {
        public string EducationId;
        public string EducationName;

        public static TEducation LoadEducationByEducationId(string EducationId)
        {
            return LoadAllEducationList().Where(x => x.EducationId == EducationId).FirstOrDefault();
        }

        public static List<TEducation> LoadAllEducationList()
        {
            var list = new List<TEducation>();
            list.Add(new TEducation() { EducationId = "1", EducationName = "高中以下" });
            list.Add(new TEducation() { EducationId = "2", EducationName = "高中，中专" });
            list.Add(new TEducation() { EducationId = "3", EducationName = "大专" });
            list.Add(new TEducation() { EducationId = "4", EducationName = "本科" });
            list.Add(new TEducation() { EducationId = "5", EducationName = "硕士" });
            list.Add(new TEducation() { EducationId = "6", EducationName = "博士" });
            list.Add(new TEducation() { EducationId = "7", EducationName = "其他" });

            return list;
        }
    }

}