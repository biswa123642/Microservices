using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoginMicroservice.Contexts;
using LoginMicroservice.Models;
//using LoginMicroservice.Models;

namespace LoginMicroservice.Repository
{
    public class UserService : IUserService
    {
       
        private readonly LoginContext _dbContext;
        public UserService(LoginContext dbcontext)
        {
            _dbContext = dbcontext;
        }
        public bool IsValidUserInformation(Login model)
        {
            if (_dbContext.Users.Where(u => u.EmailAddress == model.EmailAddress && u.Password == model.Password).Any())
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

        public ServiceResponse AddNewUser(RegisterUser model)
        {
            User Model = new User();
            Model.EmailAddress = model.EmailAddress;
            Model.Password = model.Password;
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

        public void UpdateUserProfile(UpdateUser model)
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

        public List<UserDetail> GetUsersList()
        {
            return (from u in _dbContext.Users
                    select new UserDetail
                    {
                        UserId = u.Id,
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

        public UpdateUser GetUserProfile(string EmailAddress)
        {
            return (from u in _dbContext.Users where u.EmailAddress == EmailAddress select new UpdateUser
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
