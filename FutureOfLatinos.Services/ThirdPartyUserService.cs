using Amazon.S3;
using Amazon.S3.Transfer;
using FutureOfLatinos.Data;
using FutureOfLatinos.Data.Providers;
using FutureOfLatinos.Models;
using FutureOfLatinos.Models.Domain;
using FutureOfLatinos.Services.Cryptography;
using FutureOfLatinos.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
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
        private string bucketname = "sabio-training/C53";
        private IAmazonS3 awsS3Client = new AmazonS3Client(Amazon.RegionEndpoint.USWest2);

        public ThirdPartyUserService(ICryptographyService cryptographyService)
        {
            //_authenticationService = authService;
            _cryptographyService = cryptographyService;
            //_dataProvider = dataProvider;
            //_configService = configService;
        }

        // [CREATE]
        public int Create(ThirdPartyUserLogin userModel)
        {
            
            TransferUtility utility = new TransferUtility(awsS3Client);
            TransferUtilityUploadRequest request = new TransferUtilityUploadRequest();
            var newGuid = Guid.NewGuid().ToString("");
            var newFileName = "ThirdParty_ProfilePic_" + newGuid;
            string ProfileUrl = userModel.Location;
            var client = new WebClient();
            var content = client.DownloadData(ProfileUrl);
            var stream = new MemoryStream(content);
            request.BucketName = bucketname;
            request.Key = newFileName;
            request.InputStream = stream;
        
            utility.Upload(request);

            userModel.Password = userModel.AccountId;

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
                 "ThirdPartyUsers_Register",
                inputParamMapper: delegate (SqlParameterCollection paramCol)
                {
                    List<SqlParameter> parm = new List<SqlParameter>()
                    {
                            new SqlParameter("@UserId",SqlDbType.Int),
                            new SqlParameter("@PersonId",SqlDbType.Int),
                            new SqlParameter("@FileStorageId",SqlDbType.Int)
                    };
                    foreach (var item in parm)
                    {
                        item.Direction = ParameterDirection.Output;
                    }
                    paramCol.AddRange(parm.ToArray());
                    paramCol.AddWithValue("@Email", userModel.Email);
                    paramCol.AddWithValue("@Pass", passwordHash);
                    paramCol.AddWithValue("@Salt", salt);
                    paramCol.AddWithValue("@isConfirmed", isConfirmed);
                    paramCol.AddWithValue("@isActive", isActive);
                    paramCol.AddWithValue("@FirstName", userModel.FirstName);
                    paramCol.AddWithValue("@MiddleInitial", userModel.MiddleInitial);
                    paramCol.AddWithValue("@LastName", userModel.LastName);
                    paramCol.AddWithValue("@FileTypeId", 1);
                    paramCol.AddWithValue("@UserFileName", "ThirdParty_ProfileImg");
                    paramCol.AddWithValue("@SystemFileName", "ThirdParty_ProfileImg");
                    paramCol.AddWithValue("@Location", "https://sabio-training.s3.us-west-2.amazonaws.com/C53/" + newFileName);
                    paramCol.AddWithValue("@CreatedBy", userModel.Email);
                    paramCol.AddWithValue("@ThirdPartyTypeId", userModel.ThirdPartyTypeId);
                    paramCol.AddWithValue("@AccountId", userModel.AccountId);
                },
                    returnParameters: delegate (SqlParameterCollection paramCol)
                    {
                        result = (int)paramCol["@UserId"].Value;
                    }
            );
            return result;
        }

        public ThirdPartyUserLogin GetByEmail(string Email)
        {
            ThirdPartyUserLogin model= null;
            this.DataProvider.ExecuteCmd(
              "ThridPartyUsers_GetByEmail",
              inputParamMapper: delegate(SqlParameterCollection paramCol)
              {
                  paramCol.AddWithValue("@Email", Email);
              },
              singleRecordMapper: delegate(IDataReader reader, short set)
              {
                  model = new ThirdPartyUserLogin();
                  int index = 0;
                  model.UserId = reader.GetSafeInt32(index++);
                  model.Email = reader.GetSafeString(index++);
                  model.isConfirmed = reader.GetSafeBool(index++);
                  model.isActive = reader.GetSafeBool(index++);
                  model.ThirdPartyTypeId = reader.GetSafeInt32(index++);
                  model.AccountId = reader.GetSafeString(index++);
              }
            );
            return model;
        }
    }
}
