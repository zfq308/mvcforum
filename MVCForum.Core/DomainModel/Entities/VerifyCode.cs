using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{

    public partial class VerifyCode : Entity
    {
        public VerifyCode()
        {
            Id = GuidComb.GenerateComb();
            DateCreated = DateTime.UtcNow;
        }
        public Guid Id { get; set; }
        public DateTime DateCreated { get; set; }
        public string MobileNumber { get; set; }
        public string VerifyNumber { get; set; }
        public int Status { get; set; }
        public int FailNumber { get; set; }
        public DateTime LastUpdate { get; set; }
    }

}