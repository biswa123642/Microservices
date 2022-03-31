using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using LoginMicroservice.Models;
using LoginMicroservice.Repository;

namespace LoginMicroservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller,IActionFilter
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        public AuthController(IConfiguration configuration, IUserService userService)
        {
            _configuration = configuration;
            _userService = userService;
        }



        [AllowAnonymous]
        [HttpPost(nameof(UserLogin))]
        public IActionResult UserLogin(Login data)
        {
            try
            {
                bool isValid = _userService.IsUserAlreadyExists(data.EmailAddress);
                if (isValid)
                {
                    if (_userService.IsValidUserInformation(data))
                    {
                        if (_userService.IsUserApproved(data.EmailAddress))
                        {
                            var tokenString = GenerateJwtToken(data.EmailAddress);
                            ServiceResponse resp = _userService.AddTokenToServer(tokenString, data.EmailAddress);
                            if (resp.IsSuccess)
                            {
                                return new JsonResult(new RequestResponse{ UserId = resp.UserId, Message = "User Logged In Successfully", Token = tokenString, Success = true, StatusCode = (int)StatuCode.Success }) { StatusCode = StatusCodes.Status200OK };
                            } else
                            {
                                return new JsonResult(new RequestResponse { Message = "Unable to generate token for user", Success = false, StatusCode = (int)StatuCode.Error });
                            }
                        }
                        return new JsonResult(new RequestResponse { Message = "User not verified", Success = false, StatusCode = (int)StatuCode.Error });
                    }
                    else
                    {
                        return new JsonResult(new RequestResponse { Message = "Invalid Credentials", Success = false , StatusCode = (int)StatuCode.InvalidCredentials });
                    }

                }
                else {
                    return new JsonResult(new RequestResponse { Message = "No user found", Success = false , StatusCode = (int)StatuCode.NotFound });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new RequestResponse { Message = String.Format("Error while user login {0}", ex.InnerException), Success = false, StatusCode = (int)StatuCode.Error }) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        [AllowAnonymous]
        [HttpPost(nameof(RegisterNewUser))]
        public IActionResult RegisterNewUser(User data)
        {
            try
            {
                bool userAlreadyExists = _userService.IsUserAlreadyExists(data.EmailAddress);
                if (!userAlreadyExists) 
                { 
                  ServiceResponse resp = _userService.AddNewUser(data);
                  return new JsonResult(new RequestResponse { Message = "User Added succesfully" , UserId = resp.UserId, Success = true, StatusCode = (int)StatuCode.Success }) { StatusCode = StatusCodes.Status200OK };
                }
                else
                {
                    return new JsonResult(new RequestResponse { Message = "A user already exists with same email address", Success = false, StatusCode = (int)StatuCode.AlreadyExists });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new RequestResponse { Message = string.Format("Error while adding user {0}", ex.InnerException), Success = false, StatusCode = (int)StatuCode.Error }) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [HttpPatch(nameof(UpdateUserDetails))]
        public IActionResult UpdateUserDetails(User data)
        {
            try
            {
                _userService.UpdateUserProfile(data);
                return new JsonResult(new RequestResponse { Message = "User profile updated successfully", Success = true, StatusCode = (int)StatuCode.Success }) { StatusCode = StatusCodes.Status200OK };
            }
            catch(Exception ex)
            {
                return new JsonResult(new RequestResponse { Message = string.Format("Error while updating user {0}", ex.InnerException), Success = false, StatusCode = (int)StatuCode.Error }) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        [AllowAnonymous]
        [HttpPatch(nameof(UpdateUserPassword))]
        public IActionResult UpdateUserPassword(UpdatePassword data)
        {
            try
            {
                _userService.UpdateUserPassword(data);
                return new JsonResult(new RequestResponse { Message = "Password updated successfully", Success = true, StatusCode = (int)StatuCode.Success }) { StatusCode = StatusCodes.Status200OK };
            }
            catch(Exception ex)
            {
                return new JsonResult(new RequestResponse { Message = string.Format("Error while updating user password {0}", ex.InnerException), Success = false, StatusCode = (int)StatuCode.Error }) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost(nameof(Logout))]
        public IActionResult Logout(string EmailAddress)
        {
            try
            {
                bool Result = _userService.Logout(EmailAddress);
                if (Result)
                {
                    return new JsonResult(new RequestResponse { Message = "User Loggedout successfully", Success = true, StatusCode = (int)StatuCode.Success }) { StatusCode = StatusCodes.Status200OK };
                }
                else
                {
                    return new JsonResult(new RequestResponse { Message = "No user found with the given email address to logout", Success = false, StatusCode = (int)StatuCode.NotFound });
                }
            }
            catch(Exception ex)
            {
                return new JsonResult(new RequestResponse { Message = string.Format("Error occured while logging out", ex.InnerException), Success = false , StatusCode = (int)StatuCode.Error }) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet(nameof(GetUserProfile))]
        public IActionResult GetUserProfile(string EmailAddress)
        {
            try
            {
                User result = _userService.GetUserProfile(EmailAddress);
                return new JsonResult(new {Result = result, Success = true, StatusCode = (int)StatuCode.Success }) { StatusCode = StatusCodes.Status200OK };
            }
            catch(Exception ex)
            {
                return new JsonResult(new RequestResponse { Message = string.Format("Error occured while fetching user profile", ex.InnerException), Success = false, StatusCode = (int)StatuCode.Error }) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        [AllowAnonymous]
        [HttpPatch(nameof(UpdateUserStatus))]
        public IActionResult UpdateUserStatus(bool Approve, int UserId, string SecretKey)
        {
            try
            {
                if (_configuration["AdminKey"] == SecretKey)
                {
                    _userService.UpdateUserStatus(Approve, UserId);
                    return new JsonResult(new RequestResponse { Message = "User status updated", Success = true, StatusCode = (int)StatuCode.Success }) { StatusCode = StatusCodes.Status200OK };
                }
                else
                {
                    return new JsonResult(new RequestResponse { Message = "Unauthorized", Success = false, StatusCode = (int)StatuCode.Error });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new RequestResponse { Message = string.Format("Error occured while updating the user status", ex.InnerException), Success = false, StatusCode = (int)StatuCode.Error }) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        [AllowAnonymous]
        [HttpGet(nameof(IsApprovedUser))]
        public IActionResult IsApprovedUser(string EmailAddress, string SecretKey)
        {
            try
            {
                if (_configuration["AdminKey"] == SecretKey)
                {
                    ServiceResponse resp = _userService.IsApprovedUser(EmailAddress);
                    return new JsonResult(new { Success = true, UserId = resp.UserId, IsApprovedUser = resp.IsSuccess, StatusCode = (int)StatuCode.Success }) { StatusCode = StatusCodes.Status200OK };
                }
                else
                {
                    return new JsonResult(new RequestResponse { Message = "Unauthorized", Success = false, StatusCode = (int)StatuCode.Error });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new RequestResponse { Message = string.Format("Error occured while checking the user status", ex.InnerException), Success = false, StatusCode = (int)StatuCode.Error }) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        [AllowAnonymous]
        [HttpGet(nameof(GetUsers))]
        public IActionResult GetUsers(string SecretKey)
        {
            try
            {
                if (_configuration["AdminKey"] == SecretKey)
                {
                    List<User> result = _userService.GetUsersList();
                    return new JsonResult(new { UserList = result, Success = true, StatusCode = (int)StatuCode.Success }) { StatusCode = StatusCodes.Status200OK };
                } else
                {
                    return new JsonResult(new RequestResponse { Message = "Unauthorized call", Success = false , StatusCode = (int)StatuCode.Error });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new RequestResponse{ Message = string.Format("Error occured while fetching users list", ex.InnerException), Success = false, StatusCode = (int)StatuCode.Error }) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        [AllowAnonymous]
        [HttpPatch(nameof(DeleteUser))]
        public IActionResult DeleteUser(int UserId, string SecretKey)
        {
            try
            {
                if (_configuration["AdminKey"] == SecretKey)
                {
                    _userService.DeleteUser(UserId);
                    return new JsonResult(new RequestResponse { Message = "User deleted successfully", Success = true, StatusCode = (int)StatuCode.Success }) { StatusCode = StatusCodes.Status200OK };
                }
                else
                {
                    return new JsonResult(new RequestResponse { Message = "Unauthorized", Success = false, StatusCode = (int)StatuCode.Error });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new RequestResponse { Message = string.Format("Error occured while fetching users list", ex.InnerException), Success = false, StatusCode = (int)StatuCode.Error }) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        [AllowAnonymous]
        [HttpPatch(nameof(UpdateUserRole))]
        public IActionResult UpdateUserRole(int UserId, string Role, string SecretKey)
        {
            try
            {
                if (_configuration["AdminKey"] == SecretKey)
                {
                    _userService.UpdateUserRole(UserId, Role);
                    return new JsonResult(new RequestResponse { Message = "User role updated successfully", Success = true, StatusCode = (int)StatuCode.Success }) { StatusCode = StatusCodes.Status200OK };
                }
                else
                {
                    return new JsonResult(new RequestResponse { Message = "Unauthorized", Success = false, StatusCode = (int)StatuCode.Error });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new RequestResponse { Message = string.Format("Error occured while updating user role", ex.InnerException), Success = false, StatusCode = (int)StatuCode.Error }) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        private string GenerateJwtToken(string userName)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", userName) }),
                Expires = DateTime.UtcNow.AddHours(3),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
       
    }
}
