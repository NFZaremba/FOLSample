using FutureOfLatinos.Models;
using FutureOfLatinos.Models.Domain;
using FutureOfLatinos.Models.Requests;
using FutureOfLatinos.Models.ViewModels;
using FutureOfLatinos.Services;
using FutureOfLatinos.Services.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FutureOfLatinos.Test.Services
{
    [TestClass]
    public class UserServiceTest
    {
        // This will represent the MoQ (pretend) Object that will be used
        // for continuous testing
        public readonly IUserService _userService;
        public readonly IUserService _userService1;
        public readonly IUserService _userService2; //AuthTokenViewModel
        public readonly IUserService _userService3; //AuthTokenAddRequest
        public readonly IUserService _userService4; //EmailConfirmationRequest/ Update
        public readonly IUserService _userService5; //GetByEmail

        /// <summary>
        /// Using the constructor of the class
        /// it will be where we run our test setup
        /// 1. Create some fake data that will represent a database
        /// 2. Create - Make the fake insert method that will act on the fake data
        /// 3. Read - Make GetAll and GetById fake methods that will act on the fake data
        /// 4. Update - Make fake Update method that will act on the fake data
        /// 5. Delete - Make fake Delete method that will act on the fake data
        /// </summary>
        public UserServiceTest()
        {
            // This is a date & time we'll use for Created and Modified Dates
            DateTime arrangeNow = DateTime.Now;

            // This will represent the fake database for the purpose of testing
            List<Registration> RegistrationList = new List<Registration> {
                new Registration{Id=1, Email="john.smith@gmail.com", Password="Alligator1", Salt="182h8f443h8", isConfirmed=false, isActive=false, CreatedDate=arrangeNow, ModifiedDate=arrangeNow },
                new Registration{Id=2, Email="jason.jones@yahoo.com", Password="password2", Salt="kdfn383ufie", isConfirmed=true, isActive=true, CreatedDate=arrangeNow, ModifiedDate=arrangeNow },
                new Registration{Id=3, Email="jeffrey.lawson@usc.edu", Password="EnormousElephant33", Salt="dmo2dcpjd2", isConfirmed=true, isActive=false, CreatedDate=arrangeNow, ModifiedDate=arrangeNow },
            };

            // AuthToken
            List<AuthTokenViewModel> AuthTokenList = new List<AuthTokenViewModel>
            {
                new AuthTokenViewModel{Id=1, UserId=1, Email="nfz32@comcast.net", ConfirmationAuthToken= "CF0A8C1C-F2D0-41A1-A12C-53D9BE513A1C", isConfirmed=false, isActive=false},
            };

            // AuthTokenAddRequest
            List<AuthTokenAddRequest> AuthTokenAddRequestList = new List<AuthTokenAddRequest>
            {
                new AuthTokenAddRequest{UserId=1, Email="nfz32@comcast.net", ConfirmationToken= new Guid("CF0A8C1C-F2D0-41A1-A12C-53D9BE513A1C")},
            };

            // EmailConfirmationRequest
            List<EmailConfirmationUpdateRequest> EmailConfirmationUpdateRequestList = new List<EmailConfirmationUpdateRequest>
            {
                new EmailConfirmationUpdateRequest{Id=1, isConfirmed= true},
            };

            // GetByEmail
            List<EmailViewModel> emailViewModelList = new List<EmailViewModel>
            {
                new EmailViewModel{Id=1, Email= "nfz32@comcast.net"},
            };

            // Build the mock object that represents your service
            var mock = new Mock<IUserService>();
            var mock1 = new Mock<IUserService>();
            var mock2 = new Mock<IUserService>(); //AuthToken
            var mock3 = new Mock<IUserService>(); //AuthTokenAddRequest
            var mock4 = new Mock<IUserService>(); //EmailConfirmationUpdateRequest
            var mock5 = new Mock<IUserService>(); //GetByEmail


            // Setup the calls to duplicate the service's behavior
            // Create
            mock.Setup(m => m.Create(It.IsAny<RegistrationAddRequest>())).Returns(
                (RegistrationAddRequest addRequestModel) =>
                {
                    // Immulate the ModelState.IsValid code in ASP.NET
                    List<ValidationResult> validationResult = ValidateModel<RegistrationAddRequest>(addRequestModel);
                    if (validationResult.Count > 0)
                        throw new ValidationException(validationResult[0], null, addRequestModel);

                    // Create the Domain Model to add to the fake database
                    Registration newModel = new Registration
                    {
                        Id = RegistrationList.Count + 1,
                        Email = addRequestModel.Email,
                        Password = addRequestModel.Password,
                    };

                    // add the new model to the fake database
                    RegistrationList.Add(newModel);
                    // return the new Id
                    return newModel.Id;
                }
            );

            mock1.Setup(m => m.LogIn(It.IsAny<string>(), It.IsAny<string>())).Returns(
                (string email, string pass) =>
                {
                    Registration model = RegistrationList.Where(m => m.Email == email).FirstOrDefault();
                    if (model == null)
                    {
                        return null;
                    }
                    else if (model.Password == pass)
                    {
                        string[] roles = null;
                        IUserAuthData response = new UserBase
                        {
                            Id = model.Id,
                            Name = "FakeUser" + model.Id.ToString(),
                            Roles = roles ?? new[] { "User", "Super", "Content Manager" }
                        };
                        return response;
                    }
                    else { return null; }
                }
            );

            //GetByAuthToken 
            mock2.Setup(m => m.GetByAuthTokenID(It.IsAny<string>())).Returns(
                (string confirmationAuthToken) =>
                {
                    //Get the model from the fake database
                    AuthTokenViewModel model = AuthTokenList.Where(m => m.ConfirmationAuthToken == confirmationAuthToken).FirstOrDefault();

                    //This will be the return from the method
                    AuthTokenViewModel newModel = null;

                    //Make sure we found the id in our fake database
                    if (model != null)
                    {
                        //Make a cope 
                        //Since C# doesn't have a spread operator we have to do this manually
                        newModel = new AuthTokenViewModel
                        {
                            Id = model.Id,
                            UserId = model.UserId,
                            Email = model.Email,
                            ConfirmationAuthToken = model.ConfirmationAuthToken,
                            isConfirmed = model.isConfirmed,
                            isActive = model.isActive,
                        };
                    }
                    return newModel;
                }
            );

            //CreateAuthToken
            mock3.Setup(m => m.CreateAuthToken(It.IsAny<AuthTokenAddRequest>())).Returns(
                (AuthTokenAddRequest addRequestModel) =>
                {
                    // Immulate the ModelState.IsValid code in ASP.NET
                    List<ValidationResult> validationResult = ValidateModel<AuthTokenAddRequest>(addRequestModel);
                    if (validationResult.Count > 0)
                        throw new ValidationException(validationResult[0], null, addRequestModel);

                    // Create the Domain Model to add to the fake database
                    AuthTokenAddRequest newModel = new AuthTokenAddRequest
                    {
                        UserId = AuthTokenAddRequestList.Count + 1,
                        Email = addRequestModel.Email,
                        ConfirmationToken = addRequestModel.ConfirmationToken,
                    };

                    // add the new model to the fake database
                    AuthTokenAddRequestList.Add(newModel);
                    // return the new Id
                    return newModel.UserId;
                }
            );

            //Update isConfirmed
            mock4.Setup(m => m.UpdateIsConfirmed(It.IsAny<EmailConfirmationUpdateRequest>())).Callback(
                (EmailConfirmationUpdateRequest updateRequestModel) =>
                {
                    //Immulate the ModelState.IsValid code in ASP.NET
                    List<ValidationResult> validationResult = ValidateModel<EmailConfirmationUpdateRequest>(updateRequestModel);
                    if (validationResult.Count > 0)
                        throw new ValidationException(validationResult[0], null, updateRequestModel);

                    //Get the model from the fake database
                    AuthTokenViewModel updateModel = AuthTokenList.Where(m => m.Id == updateRequestModel.Id).Single();

                    //Transfer the values from the updateRequestModel to the updateModel inside the fake database
                    updateModel.isConfirmed = updateRequestModel.isConfirmed;
                }
            );

            //Read - Get By Email
            mock5.Setup(m => m.GetByEmail(It.IsAny<string>())).Returns(
                (string email) =>
                {
                    //Get the model from the fake database
                    EmailViewModel model = emailViewModelList.Where(m => m.Email == email).FirstOrDefault();

                    //This will be the return from the method
                    EmailViewModel newModel = null;

                    //Make sure we found the id in our fake database
                    if (model != null)
                    {
                        //Make a copy
                        //Since C# doesn't have a spread operator we have to do this manually
                        newModel = new EmailViewModel
                        {
                            Id = model.Id,
                            Email = model.Email,
                        };
                    }
                    return newModel;
                }
            );
            // Once all the setup it completed
            // Assign the mock object to the
            // private member
            _userService = mock.Object;
            _userService1 = mock1.Object;
            _userService2 = mock2.Object; //AuthToken
            _userService3 = mock3.Object; //AuthTokenAddRequest
            _userService4 = mock4.Object; //EmailConfirmationUpdateRequest
            _userService5 = mock5.Object; //EmailConfirmationUpdateRequest
        }

        /// <summary>
        /// Immulating the ModelState.IsValid Functionality
        /// </summary>
        /// <typeparam name="T">Represents the generic type you're asking to validate</typeparam>
        /// <param name="requestModel">Based on the generic type this will represent the model</param>
        /// <returns>A list of validation results</returns>
        private static List<ValidationResult> ValidateModel<T>(T requestModel)
        {
            List<ValidationResult> validationResults = new List<ValidationResult>();
            ValidationContext ctx = new ValidationContext(requestModel, null, null);
            Validator.TryValidateObject(requestModel, ctx, validationResults, true);
            return validationResults;
        }

        [TestMethod]
        public void Insert_Test()
        {
            // Arrange
            RegistrationAddRequest addRequestModel = new RegistrationAddRequest
            {
                Email = "nitin23@nitin.com",
                Password = "pass1234",
            };
            // Act
            int result = _userService.Create(addRequestModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(int), "Id has to be int");
            Assert.IsTrue(result > 0, "The insert result has to be greater the 0");
        }

        [TestMethod]
        public void Login_Test()
        {
            // Arrange
            LoginRequest model = new LoginRequest
            {
                Email = "john.smith@gmail.com",
                Password = "Alligator1",
            };

            // Act
            IUserAuthData result = _userService1.LogIn(model.Email, model.Password);

            // Assert
            Assert.IsInstanceOfType(result, typeof(IUserAuthData), "response has to be bool");
        }

        [TestMethod]
        public void Login_Test_Email_False()
        {
            // Arrange
            LoginRequest model = new LoginRequest
            {
                Email = "john122.smith@gmail.com",
                Password = "Alligator231",
            };

            // Act
            IUserAuthData result = _userService1.LogIn(model.Email, model.Password);

            // Assert
            Assert.IsInstanceOfType(result, typeof(IUserAuthData), "response has to be bool");
        }

        [TestMethod]
        public void Login_Test_Password_False()
        {
            // Arrange
            LoginRequest model = new LoginRequest
            {
                Email = "john.smith@gmail.com",
                Password = "Alligator231",
            };

            // Act
            IUserAuthData result = _userService1.LogIn(model.Email, model.Password);

            // Assert
            Assert.IsInstanceOfType(result, typeof(IUserAuthData), "response has to be bool");
        }

        [TestMethod, ExpectedException(typeof(ValidationException))]
        public void Insert_Missing_DisplayName_Test()
        {
            // Arrange
            RegistrationAddRequest addRequestModel = new RegistrationAddRequest
            {
                //DisplayName = "Display Name 4",
                Password = "Description4",
            };

            // Act
            int result = _userService.Create(addRequestModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(int), "Id has to be int");
            Assert.IsTrue(result > 0, "The insert result has to be greater the 0");
        }

        [TestMethod] // AuthToken
        public void SelectByAuthTokenID_Test()
        {
            //Act
            AuthTokenViewModel model = _userService2.GetByAuthTokenID("CF0A8C1C-F2D0-41A1-A12C-53D9BE513A1C");
            //Assert
            Assert.IsNotNull(model, "Check that the token exists in the table"); //token needs to exist
            Assert.IsInstanceOfType(model, typeof(AuthTokenViewModel), "The type returned by SelectByAuthToken is incorrect");// token needs to be same type
        }

        [TestMethod] // AuthTokenAddRequest
        public void CreateAuthToken_Test()
        {
            // Arrange
            AuthTokenAddRequest addRequestModel = new AuthTokenAddRequest
            {
                Email = "nfz32@comcast.net",
                ConfirmationToken = new Guid("CF0A8C1C-F2D0-41A1-A12C-53D9BE513A1C"),
            };
            // Act
            int result = _userService3.CreateAuthToken(addRequestModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(int), "Id has to be int");
            Assert.IsTrue(result > 0, "The insert result has to be greater the 0");
        }

        [TestMethod] // Update IsConfirmed
        public void Update_Test()
        {
            //Arrange
            AuthTokenViewModel model = _userService2.GetByAuthTokenID("CF0A8C1C-F2D0-41A1-A12C-53D9BE513A1C");

            EmailConfirmationUpdateRequest updateModel = new EmailConfirmationUpdateRequest
            {
                Id = model.Id, //modifiedModel.Id = testModel(id=2)
                isConfirmed = true,
            };

            //Act
            _userService4.UpdateIsConfirmed(updateModel);
            AuthTokenViewModel modifiedModel = _userService2.GetByAuthTokenID("CF0A8C1C-F2D0-41A1-A12C-53D9BE513A1C");

            //Assert
            Assert.IsTrue(model.Id == updateModel.Id, "Id's don't match");
            Assert.IsFalse(model.isConfirmed == updateModel.isConfirmed, "Line1 change failed");
        }

        [TestMethod] // GetByEmail
        public void SelectByEmail_Test()
        {
            //Act
            EmailViewModel model = _userService5.GetByEmail("nfz32@comcast.net");

            //Assert
            Assert.IsNotNull(model, "The selected id does not exist"); //id (which is assigned to model) needs to exist
            Assert.IsInstanceOfType(model, typeof(EmailViewModel), "The type returned by SelectById is incorrect"); //id needs to be sam
        }
    }
}