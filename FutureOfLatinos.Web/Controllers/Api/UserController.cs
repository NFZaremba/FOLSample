using FutureOfLatinos.Models;
using FutureOfLatinos.Models.Domain;
using FutureOfLatinos.Models.Requests;
using FutureOfLatinos.Models.Responses;
using FutureOfLatinos.Models.ViewModels;
using FutureOfLatinos.Services;
using FutureOfLatinos.Services.Interfaces;
using FutureOfLatinos.Services.Security;
using FutureOfLatinos.Web.helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;

namespace FutureOfLatinos.Web.Controllers.Api
{
    [RoutePrefix("api/Users")]
    public class UserController : ApiController
    {
        private static readonly log4net.ILog log = LogHelper.GetLogger();
        public IUserService _userService;
        private IAuthenticationService _auth;
        private IPrincipal _principal;
        public IConfigService _configService;
        private IThirdPartyUserService _thirdPartyUserService;

        public UserController(IUserService userService, IAuthenticationService auth, IPrincipal principal, IConfigService configService, IThirdPartyUserService thirdPartyUserService)
        {
            _userService = userService;
            _auth = auth;
            _principal = principal;
            _configService = configService;
            _thirdPartyUserService = thirdPartyUserService;
        }

        [Route("login"), HttpPost, AllowAnonymous]
        public HttpResponseMessage Login(LoginRequest model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var webClient = new WebClient();
                    string verification = webClient.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", _configService.ConvertConfigValue_String("Google_Recaptcha"), model.recaptchaResponse));
                    var recaptchaResponse = (JObject.Parse(verification)["success"].Value<bool>());

                    // checking for third party login
                    ThirdPartyUserLogin check = _thirdPartyUserService.GetByEmail(model.Email);
                    if (check.ThirdPartyTypeId > 0)
                    {
                        ErrorResponse resp = new ErrorResponse("Uncessful Login Attempt, user is registered with third party service");
                        return Request.CreateResponse(HttpStatusCode.OK, resp);
                    }
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
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [Route("current"), HttpGet]
        public HttpResponseMessage Current()
        {
            return Request.CreateResponse(HttpStatusCode.OK, _principal.Identity.GetCurrentUser());
        }

        [Route("person"), HttpGet]
        public HttpResponseMessage Person()
        {
            int cur = _principal.Identity.GetCurrentUser().Id;
            ItemsResponse<int> resp = new ItemsResponse<int>();
            resp.Items = _userService.GetPerson(cur);
            return Request.CreateResponse(HttpStatusCode.OK, resp);
        }

        [Route("logout"), HttpGet]
        public HttpResponseMessage Logout()
        {
            _auth.LogOut();
            return Request.CreateResponse(HttpStatusCode.OK, new FutureOfLatinos.Models.Responses.SuccessResponse());
        }

        [Route("register"), HttpPost, AllowAnonymous]
        public async Task<HttpResponseMessage> Post(RegistrationAddRequest model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var webClient = new WebClient();
                    string verification = webClient.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", _configService.ConvertConfigValue_String("Google_Recaptcha"), model.recaptchaResponse));
                    var recaptchaResponse = (JObject.Parse(verification)["success"].Value<bool>());

                    if(recaptchaResponse == true)
                    {
                        int id = _userService.Create(model);
                        var email = model.Email;

                        var AuthTokenID = _userService.AuthorizationToken(id);
                        var ConfirmationToken = AuthTokenID.ConfirmationToken;
                        var SendEmail = _userService.GetEmail(ConfirmationToken, email);
                        int authTokenId = _userService.CreateAuthToken(AuthTokenID);

                        ItemResponse<int> resp = new ItemResponse<int>();
                        var response = await SendGridEmail.SendEmail(SendEmail);
                        return Request.CreateResponse(HttpStatusCode.OK, resp);
                    }
                    else
                    {
                        ErrorResponse resp = new ErrorResponse("Uncessful Registration Attempt");
                        return Request.CreateResponse(HttpStatusCode.OK, resp);
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [Route("token/{token}"), HttpGet, AllowAnonymous]
        public HttpResponseMessage GetByAuthTokenID(string token)
        {
            try
            {
                ItemResponse<AuthTokenViewModel> resp = new ItemResponse<AuthTokenViewModel>();
                resp.Item = _userService.GetByAuthTokenID(token);

                // Start Expiration date
                var id = resp.Item.UserId;  // grab userId

                int expireInSeconds = _configService.ConvertConfigValue_Int("Expiration_Date"); // This value should come from Configuration
                TimeSpan ts = DateTime.UtcNow - resp.Item.CreatedDate; // span between the two dateTimes
                if (ts.TotalSeconds > expireInSeconds) // if Total Seconds is more than expire date. then it is expired
                {
                    _userService.Delete(id);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, resp);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, resp);
                }
                // End of Expiration date
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [Route("email/{email}/"), HttpGet, AllowAnonymous]
        public async Task<HttpResponseMessage> GetByEmailAsync(string email)
        {
            try
            {
                ItemResponse<EmailViewModel> resp = new ItemResponse<EmailViewModel>();
                resp.Item = _userService.GetByEmail(email);
                var Id = resp.Item.Id;

                if (resp.Item.isConfirmed == false)
                {
                    var AuthTokenID = _userService.AuthorizationToken(Id);
                    var ConfirmationToken = AuthTokenID.ConfirmationToken;
                    var SendEmail = _userService.GetEmail(ConfirmationToken, email);
                    int authTokenId = _userService.CreateAuthToken(AuthTokenID);

                    var response = await SendGridEmail.SendEmail(SendEmail);
                    return Request.CreateResponse(HttpStatusCode.OK, resp);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, resp);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [Route("forgotpassword/{email}/"), HttpGet, AllowAnonymous]
        public async Task<HttpResponseMessage> ForgotPasswordEmailAsync(string email)
        {
            try
            {
                ItemResponse<EmailViewModel> resp = new ItemResponse<EmailViewModel>();
                resp.Item = _userService.GetByEmail(email);
                var Id = resp.Item.Id;
                var AuthTokenID = _userService.AuthorizationToken(Id);
                var ConfirmationToken = AuthTokenID.ConfirmationToken;
                var SendEmail = _userService.ForgotPasswordEmail(ConfirmationToken, email);
                int authTokenId = _userService.CreateAuthToken(AuthTokenID);
                var response = await SendGridEmail.SendEmail(SendEmail);
                return Request.CreateResponse(HttpStatusCode.OK, resp);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [Route("{resetpassword}"), HttpPut, AllowAnonymous]
        public HttpResponseMessage Put(PasswordUpdateRequest model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _userService.UpdatePassword(model);
                    SuccessResponse resp = new SuccessResponse();
                    return Request.CreateResponse(HttpStatusCode.OK, resp);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [Route("{id:int}"), HttpPut, AllowAnonymous]
        public HttpResponseMessage Put(EmailConfirmationUpdateRequest model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _userService.UpdateIsConfirmed(model);
                    SuccessResponse resp = new SuccessResponse();
                    return Request.CreateResponse(HttpStatusCode.OK, resp);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [Route("{id:int}"), HttpDelete, AllowAnonymous]
        public HttpResponseMessage Delete(int id)
        {
            try
            {
                _userService.Delete(id);
                SuccessResponse resp = new SuccessResponse();
                return Request.CreateResponse(HttpStatusCode.OK, resp);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [Route("validate"), HttpPost, AllowAnonymous]
        public HttpResponseMessage validate(LoginRequest model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    IUserAuthData success = _userService.validatePassword(model.Email, model.Password);
                    if (success != null)
                    {
                        ItemResponse<IUserAuthData> resp = new ItemResponse<IUserAuthData>();
                        resp.Item = success;
                        return Request.CreateResponse(HttpStatusCode.OK, resp);
                    }
                    else
                    {
                        ErrorResponse resp = new ErrorResponse("Password does not match");
                        return Request.CreateResponse(HttpStatusCode.OK, resp);
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [Route("updatepassword"), HttpPut, AllowAnonymous]
        public HttpResponseMessage Update(PasswordUpdateRequest model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    IUserAuthData currentUser = _principal.Identity.GetCurrentUser();
                    model.Id = currentUser.Id;
                    _userService.Update(model);
                    SuccessResponse resp = new SuccessResponse();
                    return Request.CreateResponse(HttpStatusCode.OK, resp);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }
    }
}