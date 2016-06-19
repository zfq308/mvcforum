using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Services.Data.Mapping
{

    public class AiLvHuoDongDetailMapping : EntityTypeConfiguration<AiLvHuoDongDetail>
    {
        public AiLvHuoDongDetailMapping()
        {
            HasKey(x => x.DetailsId);
            Property(x => x.DetailsId).IsRequired();
            Property(x => x.Id).IsRequired()
                                    .HasColumnAnnotation("Index",
                                    new IndexAnnotation(new IndexAttribute("IX_AiLvHuoDongDetail_HuoDongId", 1) { IsUnique = false }));
            Property(x => x.UserId).IsRequired();
            Property(x => x.UserGender).IsRequired();
            Property(x => x.UserTelphone).IsRequired().HasMaxLength(11);
            Property(x => x.CreateTime).IsRequired();
            Property(x => x.FeeSource).IsRequired().HasMaxLength(20);
            Property(x => x.FeeId).IsRequired().HasMaxLength(20);
            Property(x => x.FeeNumber).IsRequired();
            Property(x => x.FeeStatus).IsRequired();
        }
    }

}