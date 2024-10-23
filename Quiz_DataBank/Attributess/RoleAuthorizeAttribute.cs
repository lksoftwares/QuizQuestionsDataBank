
//using Azure.Core;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Filters;
//using Microsoft.IdentityModel.Tokens;
//using System.Linq;
//using Newtonsoft.Json;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Authorization;
//using System.Net.Http;
//using System.Text;
//using System.Net;
//using Microsoft.AspNetCore.Mvc.Controllers;
//public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
//{
//    private string[] AuthorizeRole = [];

//    public RoleAuthorizeAttribute(params string[] v)
//    {
//        this.AuthorizeRole = v;
//    }

//    public void OnAuthorization(AuthorizationFilterContext context)
//    {
//        var descriptor = context?.ActionDescriptor as ControllerActionDescriptor;

//        var req = context.HttpContext.Request;

//        var action = descriptor.ActionName;
//        var ControllerBase = descriptor.ControllerName;

//        HttpContext context1 = context.HttpContext;
//        req.EnableBuffering();
//        var b = req.Body;

//        using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8, true, 1024, true))

//        {
//            var json = reader.ReadToEnd();
//            dynamic role1 = JsonConvert.DeserializeObject(json);
//            //   if (AuthorizeRole.Contains(role1.AuthorizeRole.ToString()))
//            if (role1 == null || role1.AuthorizeRole == null || string.IsNullOrEmpty(role1.AuthorizeRole.ToString()))
//            {
//                context.Result = new BadRequestObjectResult(new
//                {
//                    error = "unauthorized",
//                    message = "Please enter AuthorizeRole first."
//                });
//                return;
//            }

//            if (AuthorizeRole.Any(role=>role == role1.AuthorizeRole.ToString()) )
//            {

//                req.Body.Position = 0;

//                return;
//            }
//        }


//        context.Result = new BadRequestObjectResult(new { error = "unauthorize", message = "Role Not Matched" });



//    }

//}












using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using System.Text;

public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _authorizedRoles;

    public RoleAuthorizeAttribute(params string[] roles)
    {
        _authorizedRoles = roles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var request = context.HttpContext.Request;
        request.EnableBuffering(); 

        using (var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
        {
            var bodyContent = reader.ReadToEnd();
            var roleData = JToken.Parse(bodyContent);

            bool isAuthorized = roleData.Type switch
            {
                JTokenType.Object => ValidateRole((JObject)roleData),
                JTokenType.Array => ((JArray)roleData).All(item => ValidateRole((JObject)item)),
                _ => false
            };

            request.Body.Position = 0; 

            if (!isAuthorized)
            {
                context.Result = new BadRequestObjectResult(new
                {
                    error = "unauthorized",
                    message = "Role not matched or missing."
                });
            }
        }
    }

    private bool ValidateRole(JObject roleObj)
    {
        var role = roleObj["AuthorizeRole"]?.ToString();
        return !string.IsNullOrEmpty(role) && _authorizedRoles.Contains(role);
    }
}
