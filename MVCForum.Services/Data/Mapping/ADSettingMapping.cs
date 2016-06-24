using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Services.Data.Mapping
{

    public class ADSettingMapping : EntityTypeConfiguration<ADSetting>
    {
        public ADSettingMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.ImageName).IsRequired().HasMaxLength(100);
            Property(x => x.ImageSaveURL).IsRequired().HasMaxLength(100);
            Property(x => x.Link).IsRequired().HasMaxLength(300);
            Property(x => x.CreateTime).IsRequired();
        }
    }

}