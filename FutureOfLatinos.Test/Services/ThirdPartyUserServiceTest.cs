using FutureOfLatinos.Models.Domain;
using FutureOfLatinos.Services;
using FutureOfLatinos.Services.Cryptography;
using FutureOfLatinos.Services.Interfaces;
using FutureOfLatinos.Web.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FutureOfLatinos.Test.Services
{
    [TestClass]
    public class ThirdPartyUserServiceTest
    {
        //public IThirdPartyUserService _svc;

        [TestMethod]
        public void InsertThirdPartyUserTest()
        {
            ThirdPartyUserLogin model = new ThirdPartyUserLogin();
            model.Email = "newGoogleUser@gmail.com";
            model.Password = "newpassword";
            model.FirstName = "FirstName";
            model.MiddleInitial = "M";
            model.LastName = "LastName";
            model.Location = "http://www.industryexplorers.com/resources/Microsoft-logo_transparent.png";
            model.CreatedBy = "API Unit Test";
            model.ThirdPartyTypeId = 2;
            model.AccountId = "11111111111223243424233432";

            // Act
            ThirdPartyUserService _svc = new ThirdPartyUserService(new Base64StringCryptographyService());
            int result = _svc.Create(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(int), "Id has to be int");
            Assert.IsTrue(result > 0, "The insert result has to be greater the 0");
        }

        [TestMethod]
        public void GetByEmail_Test()
        {
            // Act
            ThirdPartyUserService _svc = new ThirdPartyUserService(new Base64StringCryptographyService());
            ThirdPartyUserLogin result = _svc.GetByEmail("newGoogleUser@gmail.com");

            // Assert
            Assert.IsInstanceOfType(result, typeof(ThirdPartyUserLogin), "Must be login model");
            //Assert.IsTrue(result > 0, "The insert result has to be greater the 0");
        }
    }
}
