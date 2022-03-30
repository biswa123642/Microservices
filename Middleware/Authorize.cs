using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoginMicroservice.Contexts;
using LoginMicroservice.Repository;

namespace LoginMicroservice.Middleware
{
    public class AuthorizeAttribute : IAuthorizationFilter
    {
        
        private readonly IUserService _userService;
        //public AuthorizeAttribute() { }
        public AuthorizeAttribute(IUserService userService)
        {
           
            _userService = userService;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!(context.ActionDescriptor as ControllerActionDescriptor).MethodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any())
            {
                var account = (context.HttpContext.Items["User"]);
                var token = (context.HttpContext.Items["Token"]);
                if (account == null || token == null)
                {
                    context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
                }
                else
                {
                    bool tokenVlidated = _userService.CheckTokenValidated(account.ToString(), token.ToString()) ;
                    if (!tokenVlidated)
                    {
                        context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
                    }
                }
            }
        }
    }
}
