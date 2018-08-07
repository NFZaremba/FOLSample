using FutureOfLatinos.Data.Providers;
using FutureOfLatinos.Models;
using FutureOfLatinos.Models.Domain;
using FutureOfLatinos.Services.Cryptography;
using FutureOfLatinos.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FutureOfLatinos.Services
{
    public class ThirdPartyUserService: BaseService, IThirdPartyUserService
    {
        private IAuthenticationService _authenticationService;
        private ICryptographyService _cryptographyService;
        private IDataProvider _dataProvider;
        private IConfigService _configService;
        private const int HASH_ITERATION_COUNT = 1;
        private const int RAND_LENGTH = 15;

        public ThirdPartyUserService(IAuthenticationService authService, ICryptographyService cryptographyService, IDataProvider dataProvider, IConfigService configService)
        {
            _authenticationService = authService;
            _cryptographyService = cryptographyService;
            _dataProvider = dataProvider;
            _configService = configService;
        }

        // [CREATE]
        public int Create(ThirdPartyUserLogin userModel)
        {
            int result = 0;
            string salt;
            string passwordHash;
            string password = userModel.Password;
            bool isConfirmed = true;
            bool isActive = true;

            salt = _cryptographyService.GenerateRandomString(RAND_LENGTH);
            passwordHash = _cryptographyService.Hash(password, salt, HASH_ITERATION_COUNT);
            //DB provider call to create user and get us a user id
            this.DataProvider.ExecuteNonQuery(
                 "Users_Insert",
                inputParamMapper: delegate (SqlParameterCollection paramCol)
                {
                    SqlParameter parm = new SqlParameter();
                    parm.ParameterName = "@Id";
                    parm.SqlDbType = SqlDbType.Int;
                    parm.Direction = ParameterDirection.Output;
                    paramCol.Add(parm);
                    paramCol.AddWithValue("@Email", userModel.Email);
                    paramCol.AddWithValue("@Pass", passwordHash);
                    paramCol.AddWithValue("@Salt", salt);
                    paramCol.AddWithValue("@isConfirmed", isConfirmed);
                    paramCol.AddWithValue("@isActive", isActive);
                    paramCol.AddWithValue("@FirstName", userModel.FirstName);
                    paramCol.AddWithValue("@MiddleInitial", userModel.MiddleInitial);
                    paramCol.AddWithValue("@LastName", userModel.LastName);
                    paramCol.AddWithValue("@Location", userModel.Location);
                    paramCol.AddWithValue("@ThirdpartyTypeId", userModel.ThirdPartTypeId);
                    paramCol.AddWithValue("@AccountId", userModel.AccountId);
                },
                    returnParameters: delegate (SqlParameterCollection paramCol)
                    {
                        result = (int)paramCol["@Id"].Value;
                    }
            );
            return result;
        }

        public ThirdPartyUserLogin GetByEmail(string Email)
        {
            ThirdPartyUserLogin model= null;
            this.DataProvider.ExecuteCmd(
              "ThridPartyUsers_GetByEmail",
              inputParamMapper delegate(SqlParameterCollection paramCol)
              {
                  paramCol.AddWithValue("@Email", Email);
              }
                
            );
        }
    }
}
