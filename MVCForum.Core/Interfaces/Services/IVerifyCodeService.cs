using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{

    public partial interface IVerifyCodeService
    {
       
        void CompleteVerifyCode(Guid id);
        void CompleteVerifyCode(VerifyCode verifyCode);
        void Delete(VerifyCode verifyCode);
        void Delete(List<VerifyCode> verifyCodeList);
        void DeleteInvalidVerifyCode();
        void SendVerifyCode(VerifyCode verifyCode);
        VerifyCode Get(Guid id);
        int GetCountByMobileNumber(string mobileNumber);
        void CheckInvalidVerifyCode();

    }
}