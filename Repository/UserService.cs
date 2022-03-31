using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoginMicroservice.Contexts;
using LoginMicroservice.Models;
using System.Text;
using System.IO;
using System.Security.Cryptography;
//using LoginMicroservice.Models;

namespace LoginMicroservice.Repository
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _configuration;
        private readonly LoginContext _dbContext;
        public UserService(IConfiguration configuration, LoginContext dbcontext)
        {
            _configuration = configuration;
            _dbContext = dbcontext;
        }
        public string DecryptString(string cipherText)
        {
            var keyString = _configuration["AESKey"];
            var fullCipher = Convert.FromBase64String(cipherText);

            var iv = new byte[16];
            var cipher = new byte[16];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, iv.Length);
            var key = Encoding.UTF8.GetBytes(keyString);

            using (var aesAlg = Aes.Create())
            {
                using (var decryptor = aesAlg.CreateDecryptor(key, iv))
                {
                    string result;
                    using (var msDecrypt = new MemoryStream(cipher))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                result = srDecrypt.ReadToEnd();
                            }
                        }
                    }

                    return result;
                }
            }
        }

        public static string EncryptString(string text, string keyString)
        {
            var key = Encoding.UTF8.GetBytes(keyString);

            using (var aesAlg = Aes.Create())
            {
                using (var encryptor = aesAlg.CreateEncryptor(key, aesAlg.IV))
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(text);
                        }

                        var iv = aesAlg.IV;

                        var decryptedContent = msEncrypt.ToArray();

                        var result = new byte[iv.Length + decryptedContent.Length];

                        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                        Buffer.BlockCopy(decryptedContent, 0, result, iv.Length, decryptedContent.Length);

                        return Convert.ToBase64String(result);
                    }
                }
            }
        }

        public bool IsValidUserInformation(Login model)
        {
            //var encrypted = EncryptString(model.Password, _configuration["AESKey"]);
            string password = _dbContext.Users.Where(u => u.EmailAddress == model.EmailAddress).Select(u => u.Password).FirstOrDefault();
            string DecryptedPassword = DecryptString(password);
            if(DecryptedPassword == model.Password)
            //if (_dbContext.Users.Where(u => u.EmailAddress == model.EmailAddress && u.Password == encrypted).Any())
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public bool IsUserAlreadyExists(string EmailAddress)
        {
            return _dbContext.Users.Where(u => u.EmailAddress == EmailAddress).Any();
        }

        public ServiceResponse AddNewUser(User model)
        {
            var encrypted = EncryptString(model.Password, _configuration["AESKey"]);

            User Model = new User();
            Model.EmailAddress = model.EmailAddress;
            Model.Password = encrypted;
            Model.Gender = model.Gender;
            Model.FirstName = model.FirstName;
            Model.LastName = model.LastName;
            Model.Company = model.Company;
            Model.Address = model.Address;
            Model.Zip = model.Zip;
            Model.City = model.City;
            Model.Country = model.Country;
            Model.Role = model.Role;
            Model.IsApproved = false;

            _dbContext.Users.Add(Model);
            _dbContext.SaveChanges();

            return new ServiceResponse { IsSuccess = true, UserId = Model.Id };

        }

        public ServiceResponse IsApprovedUser(string EmailAddress)
        {
           
            return (from u in _dbContext.Users where u.EmailAddress == EmailAddress select new ServiceResponse {IsSuccess = u.IsApproved, UserId = u.Id }).FirstOrDefault();
        }

        public bool IsUserApproved(string EmailAddress)
        {
            return _dbContext.Users.Where(u => u.EmailAddress == EmailAddress && u.IsApproved).Any();
        }

        public void UpdateUserProfile(User model)
        {
            User Model = _dbContext.Users.Where(u => u.EmailAddress == model.EmailAddress).FirstOrDefault();

            Model.Gender = model.Gender;
            Model.FirstName = model.FirstName;
            Model.LastName = model.LastName;
            Model.Company = model.Company;
            Model.Address = model.Address;
            Model.Zip = model.Zip;
            Model.City = model.City;
            Model.Country = model.Country;
            Model.BusinessInformation = model.BusinessInformation;

            _dbContext.SaveChanges();
        }

        public void UpdateUserPassword(UpdatePassword model)
        {
            User Model = _dbContext.Users.Where(u => u.Id == model.UserId).FirstOrDefault();

            Model.Password = model.Password;
            _dbContext.SaveChanges();
        }
        public ServiceResponse AddTokenToServer(string Token, string EmailAddress)
        {
            int UserId = _dbContext.Users.Where(u => u.EmailAddress == EmailAddress).Select(a => a.Id).FirstOrDefault();
            if (UserId > 0)
            {
                Usertoken model = new Usertoken();
                model.UserID = UserId;
                model.Token = Token;
                _dbContext.Usertokens.Add(model);
                _dbContext.SaveChanges();
                return new ServiceResponse { IsSuccess = true, UserId = UserId };
            } else { return new ServiceResponse { IsSuccess = false, UserId = 0 }; }
        }

        public bool Logout(string EmailAddress)
        {
            List<Usertoken> userToken = (from ut in _dbContext.Usertokens join u in _dbContext.Users on ut.UserID equals u.Id where u.EmailAddress == EmailAddress select ut).ToList();
            if (userToken.Count > 0)
            {
                _dbContext.Usertokens.RemoveRange(userToken);
                _dbContext.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }

        }

        public void UpdateUserStatus(bool IsApproved, int UserId)
        {
            User Model = _dbContext.Users.Where(u => u.Id == UserId).FirstOrDefault();
            Model.IsApproved = IsApproved;
            _dbContext.SaveChanges();
        }

        public List<User> GetUsersList()
        {
            return (from u in _dbContext.Users
                    select new User
                    {
                        Id = u.Id,
                        EmailAddress = u.EmailAddress,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Gender = u.Gender,
                        Country = u.Country,
                        Address = u.Address,
                        Company = u.Company,
                        Zip = u.Zip,
                        City = u.City,
                        IsApproved = u.IsApproved
                    }).ToList();
        }

        public void DeleteUser(int UserId)
        {
            List<Usertoken> userToken = (from ut in _dbContext.Usertokens  where ut.UserID == UserId select ut).ToList();
            User user = (from u in _dbContext.Users where u.Id == UserId select u).FirstOrDefault();

            _dbContext.Usertokens.RemoveRange(userToken);
            _dbContext.Users.Remove(user);
            _dbContext.SaveChanges();
        }

        public void UpdateUserRole(int UserId, string Role)
        {
            User Model = (from u in _dbContext.Users where u.Id == UserId select u).FirstOrDefault();
            Model.Role = Role;
            _dbContext.SaveChanges();
        }

        public User GetUserProfile(string EmailAddress)
        {
            return (from u in _dbContext.Users where u.EmailAddress == EmailAddress select new User
            { 
            EmailAddress = u.EmailAddress,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Gender = u.Gender,
            Company = u.Company,
            Address = u.Address,
            Zip = u.Zip,
            Country = u.Country,
            City = u.City
            }).FirstOrDefault();
        }

        public bool CheckTokenValidated(string EmailAddress, string Token)
        {
            return (from ut in _dbContext.Usertokens join u in _dbContext.Users on ut.UserID equals u.Id where u.EmailAddress == EmailAddress && ut.Token == Token select ut.ID).Any();
        }
    }
}
