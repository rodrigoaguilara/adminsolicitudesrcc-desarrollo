using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace AdminSolicitudesContratistas.Models
{
    public class CheckAuthorization : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            //var codUsuario = HttpContext.Current.Session["username"];
            var rutProv = HttpContext.Current.Session["SessionNombreUsuario"];
            var user = HttpContext.Current.User;

            if (rutProv == null || !HttpContext.Current.Request.IsAuthenticated)
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    if (user.Identity.IsAuthenticated == false)
                    {
                        filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        filterContext.HttpContext.Response.End();
                    }

                }
                else
                {
                    filterContext.Result = new RedirectResult(System.Web.Security.FormsAuthentication.LoginUrl);

                }
            }
            else
            {

                //Code HERE for page level authorization  

            }
        }


    }
}