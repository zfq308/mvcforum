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
    /// 用户属性变更记录映射类
    /// </summary>
    public class MembershipPropertyChangeMapping : EntityTypeConfiguration<MembershipPropertyChange>
    {
        public MembershipPropertyChangeMapping()
        {
            HasKey(x => x.ChangeId);
            Property(x => x.ChangeId).IsRequired();
            Property(x => x.CreateDate).IsRequired();
            Property(x => x.PropertyName).IsRequired();
            Property(x => x.PropertyTypeName).IsRequired();
            Property(x => x.SourceValue).IsRequired();
            Property(x => x.TargetValue).IsRequired();
            Property(x => x.ApprovedFlag).IsRequired();
            Property(x => x.ApprovedTime).IsRequired();
            Property(x => x.ApprovedMan).IsRequired();
            Property(x => x.ApproveMessage).IsRequired();
            Property(x => x.UserId).IsRequired()
             .HasColumnAnnotation("Index",
                                    new IndexAnnotation(new IndexAttribute("IX_MembershipPropertyChange_UserId", 1) { IsUnique = false }));

        }

    }

}