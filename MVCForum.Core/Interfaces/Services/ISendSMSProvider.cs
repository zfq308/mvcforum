using System;
using System.Linq;
using System.Collections.Generic;
using System.Web;

namespace MVCForum.Domain.Interfaces
{

    public interface ISendSMSService
    {
        string Send(string Content, string MobileNumber);
    }

}