using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Services.Data.Mapping
{

    public class VerifyCodeMapping : EntityTypeConfiguration<VerifyCode>
    {
        public VerifyCodeMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.MobileNumber).IsRequired().HasMaxLength(11);
            Property(x => x.Status).IsRequired();
            Property(x => x.VerifyNumber).IsRequired().HasMaxLength(6);
            Property(x => x.FailNumber).IsRequired();
            Property(x => x.DateCreated).IsRequired();
            Property(x => x.LastUpdate).IsRequired();
            Property(x => x.ReturnMessage).IsOptional().HasMaxLength(200);
        }
    }

}