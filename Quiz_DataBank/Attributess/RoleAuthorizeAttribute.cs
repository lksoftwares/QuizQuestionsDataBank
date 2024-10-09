
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using System.Text;
using System.Net;
using Microsoft.AspNetCore.Mvc.Controllers;
public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private string[] AuthorizeRole = [];

    public RoleAuthorizeAttribute(params string[] v)
    {
        this.AuthorizeRole = v;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var descriptor = context?.ActionDescriptor as ControllerActionDescriptor;

        //context.HttpContext.Items
        var req = context.HttpContext.Request;

        var action = descriptor.ActionName;
        var ControllerBase = descriptor.ControllerName;
        
        HttpContext context1 = context.HttpContext;
        req.EnableBuffering();
        // req.Body.Position = 0;
        var b = req.Body;
        // var requestCode = req.Headers["Request-Code"].FirstOrDefault();

        using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8, true, 1024, true))

        //  using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8))
        {
            var json = reader.ReadToEnd();
            dynamic role1 = JsonConvert.DeserializeObject(json);
         //   if (AuthorizeRole.Contains(role1.AuthorizeRole.ToString()))

                if (AuthorizeRole.Any(role=>role == role1.AuthorizeRole.ToString()) )
            {

                req.Body.Position = 0;
                // req.Headers["Request-Code"]= requestCode;
                //  context.HttpContext = context1;
                return;
            }
        }


        context.Result = new BadRequestObjectResult(new { error = "unauthorize", message = "hello" });



    }

}

















//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Filters;
//using System.Linq;

//public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
//{
//    private readonly string[] allowedRoles;

//    public RoleAuthorizeAttribute(params string[] roles)
//    {
//        allowedRoles = roles;
//    }

//    public void OnAuthorization(AuthorizationFilterContext context)
//    {
//        var user = context.HttpContext.User;

//        if (!user.Identity.IsAuthenticated)
//        {
//            context.Result = new UnauthorizedResult();
//            return;
//        }

//        if (!allowedRoles.Any(role => user.IsInRole(role)))
//        {
//            context.Result = new ForbidResult();
//        }
//    }
//}
