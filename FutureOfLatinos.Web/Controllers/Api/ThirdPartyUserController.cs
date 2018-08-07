using FutureOfLatinos.Data.Providers;
using FutureOfLatinos.Models;
using FutureOfLatinos.Models.Domain;
using FutureOfLatinos.Models.Responses;
using FutureOfLatinos.Services;
using FutureOfLatinos.Services.Cryptography;
using FutureOfLatinos.Services.Interfaces;
using FutureOfLatinos.Web.helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FutureOfLatinos.Web.Controllers.Api
{
    [RoutePrefix("api/thirdpartylogin")]
    public class ThirdPartyUserController : ApiController
    {
        private static readonly log4net.ILog log = LogHelper.GetLogger();
        private IAuthenticationService _authenticationService;
        private ICryptographyService _cryptographyService;
        private IDataProvider _dataProvider;
        private IConfigService _configService;
        private IThirdPartyUserService _thirdPartyService;
        private IUserService _userService;
        private const int HASH_ITERATION_COUNT = 1;
        private const int RAND_LENGTH = 15;

        public ThirdPartyUserController(IAuthenticationService authService, ICryptographyService cryptographyService, IThirdPartyUserService thirdPartyService, IUserService userService)
        {
            _authenticationService = authService;
            _cryptographyService = cryptographyService;
            //_dataProvider = dataProvider;
            //_configService = configService;
            _thirdPartyService = thirdPartyService;
            _userService = userService;
        }

        [Route("login"), HttpPost, AllowAnonymous]
        public HttpResponseMessage SignIn(ThirdPartyUserLogin model)
        {
            try
            {
                //check if user exists
                ThirdPartyUserLogin check = _thirdPartyService.GetByEmail(model.Email);

                if (check == null)
                {
                    //Register User
                    int result = _thirdPartyService.Create(model);

                    //If the registration does not work
                    if (result == null || result <= 0)
                    {
                        ErrorResponse resp = new ErrorResponse("Unsuccessful Registration Attempt");
                        return Request.CreateResponse(HttpStatusCode.OK, resp);
                    }
                    //If succesfull, log the user in
                    else
                    {
                        IUserAuthData success = _userService.LogIn(model.Email, model.Password);

                        if (success != null)
                        {
                            List<int> pID = _userService.GetPerson(success.Id);
                            LoginResponse<IUserAuthData> resp = new LoginResponse<IUserAuthData>();
                            resp.Item = success;
                            resp.PersonID = pID;
                            return Request.CreateResponse(HttpStatusCode.OK, resp);
                        }
                        else
                        {
                            ErrorResponse resp = new ErrorResponse("Uncessful Login Attempt");
                            return Request.CreateResponse(HttpStatusCode.OK, resp);
                        }
                    }
                }
                else //check for 3rd Party Type
                {
                    if (check.ThirdPartyTypeId > 0)
                    {
                        IUserAuthData success = _userService.LogIn(model.Email, model.Password);

                        if (success != null)
                        {
                            List<int> pID = _userService.GetPerson(success.Id);
                            LoginResponse<IUserAuthData> resp = new LoginResponse<IUserAuthData>();
                            resp.Item = success;
                            resp.PersonID = pID;
                            return Request.CreateResponse(HttpStatusCode.OK, resp);
                        }
                        else
                        {
                            ErrorResponse resp = new ErrorResponse("Uncessful Login Attempt");
                            return Request.CreateResponse(HttpStatusCode.OK, resp);
                        }
                    }
                    else
                    {
                        ErrorResponse resp = new ErrorResponse("Uncessful Login Attempt. User is already registered.");
                        return Request.CreateResponse(HttpStatusCode.OK, resp);
                    }
                }
            }//Mdoel State Valid
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
        }
    }
}
