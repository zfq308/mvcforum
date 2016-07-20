using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{

    public partial class CategoryNotification : Entity
    {
        public CategoryNotification()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public virtual Category Category { get; set; }
        public virtual MembershipUser User { get; set; }
    }

}