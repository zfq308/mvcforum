using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel.Activity;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Services.Data.Mapping
{

    public class FollowMapping: EntityTypeConfiguration<Follow>
    {
        public FollowMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.UserId).IsRequired();
            Property(x => x.FriendUserId).IsRequired();
            Property(x => x.CreateTime).IsRequired();
            Property(x => x.UpdateTime).IsRequired();
            Property(x => x.OpsFlag).IsOptional().HasMaxLength(50);
        }
    }

}