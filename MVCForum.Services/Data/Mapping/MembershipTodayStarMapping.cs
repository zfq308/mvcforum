using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Services.Data.Mapping
{
    /// <summary>
    /// 用户今日之星特权实体映射类
    /// </summary>
    public class MembershipTodayStarMapping : EntityTypeConfiguration<MembershipTodayStar>
    {
        public MembershipTodayStarMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.UserId).IsRequired();
            Property(x => x.CreateDate).IsRequired();
            Property(x => x.Operator).IsRequired();
            Property(x => x.Status).IsRequired();
            Property(x => x.JobId).IsOptional();
            Property(x => x.StartTime).IsRequired();
            Property(x => x.StopTime).IsRequired();

        }
    }

}