using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoginMicroservice.Models;

namespace LoginMicroservice.Repository
{
    public interface IUserService
    {
        bool IsValidUserInformation(Login model);
        ServiceResponse AddNewUser(User model);

        bool IsUserAlreadyExists(string Email);

        bool IsUserApproved(string Email);
        ServiceResponse IsApprovedUser(string Email);
        void UpdateUserProfile(User model);
        void UpdateUserPassword(UpdatePassword model);

        bool Logout(string Email);
        ServiceResponse AddTokenToServer(string Token, string Email);

        void UpdateUserStatus(bool IsApproved, int UserId);
        List<User> GetUsersList();

        void DeleteUser(int UserId);
        void UpdateUserRole(int UserId, string Role);
        User GetUserProfile(string Email);
        bool CheckTokenValidated(string Email, string token);
    }
}
