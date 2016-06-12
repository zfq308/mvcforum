using Microsoft.VisualStudio.TestTools.UnitTesting;
using MVCForum.Domain.DomainModel;
using MVCForum.Services;
using MVCForum.Services.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCForum.Services.Tests
{
    [TestClass()]
    public class VerifyCodeServiceTests
    {
        private VerifyCodeService mVerifyCodeService;
        private MVCForumContext _context;

        [TestInitialize()]
        public void Setup()
        {
            _context = new MVCForumContext();
            mVerifyCodeService = new VerifyCodeService(_context);
        }


        [TestMethod()]
        public void CheckInvalidVerifyCodeTest()
        {

        }

        [TestMethod()]
        public void CompleteVerifyCodeTest()
        {

        }

        [TestMethod()]
        public void CompleteVerifyCodeTest1()
        {

        }

        [TestMethod()]
        public void DeleteTest()
        {

        }

        [TestMethod()]
        public void DeleteTest1()
        {

        }

        [TestMethod()]
        public void DeleteInvalidVerifyCodeTest()
        {

        }

        [TestMethod()]
        public void GetTest()
        {

        }

        [TestMethod()]
        public void GetCountByMobileNumberTest()
        {

        }

        [TestMethod()]
        public void SendVerifyCodeTest()
        {
            mVerifyCodeService.SendVerifyCode(new VerifyCode("13686886937", VerifyCodeStatus.Waiting, "5678"));

        }
    }
}