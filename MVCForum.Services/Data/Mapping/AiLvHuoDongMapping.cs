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
    /// 爱驴活动数据映射类
    /// </summary>
    public class AiLvHuoDongMapping : EntityTypeConfiguration<AiLvHuoDong>
    {
        public AiLvHuoDongMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.MingCheng).IsRequired().HasMaxLength(50)
                                    .HasColumnAnnotation("Index",
                                    new IndexAnnotation(new IndexAttribute("IX_AiLvHuoDong_MingCheng", 1) { IsUnique = true }));
            Property(x => x.LeiBie).IsRequired();
            Property(x => x.YaoQiu).IsRequired();
            Property(x => x.StartTime).IsRequired();
            Property(x => x.StopTime).IsRequired();
            Property(x => x.BaoMingJieZhiTime).IsRequired();
            Property(x => x.DiDian).IsRequired().HasMaxLength(20);
            Property(x => x.LiuCheng).IsRequired().HasMaxLength(2000);
            Property(x => x.Feiyong).IsRequired();
            Property(x => x.FeiyongShuoMing).IsRequired().HasMaxLength(400);
            Property(x => x.ZhuYiShiXiang).IsRequired().HasMaxLength(400);
            Property(x => x.YuGuRenShu).IsRequired();
            Property(x => x.XingBieBiLi).IsRequired().HasMaxLength(20);
            Property(x => x.YaoQingMa).IsRequired().HasMaxLength(20);
            Property(x => x.ZhuangTai).IsRequired();

            Property(x => x.CreatedTime).IsRequired();

         

        }
    }

    public class ActivityRegisterMapping : EntityTypeConfiguration<ActivityRegister>
    {
        public ActivityRegisterMapping()
        {
            HasKey(x => x.DetailsId);
            Property(x => x.DetailsId).IsRequired();
            Property(x => x.Id).IsRequired()
                                    .HasColumnAnnotation("Index",
                                    new IndexAnnotation(new IndexAttribute("IX_ActivityRegister_HuoDongId", 1) { IsUnique = false }));
            Property(x => x.UserId).IsRequired();
            Property(x => x.UserGender).IsRequired();
            Property(x => x.UserMarriedStatus).IsRequired();
            Property(x => x.UserTelphone).IsRequired().HasMaxLength(11);
            Property(x => x.JoinPeopleNumber).IsRequired();
            Property(x => x.CreateTime).IsRequired();
            Property(x => x.FeeSource).IsRequired().HasMaxLength(20);
            Property(x => x.FeeId).IsRequired().HasMaxLength(20);
            Property(x => x.FeeNumber).IsRequired();
            Property(x => x.FeeStatus).IsRequired();
            Property(x => x.PayCompletedTime).IsRequired();

        }
    }
}