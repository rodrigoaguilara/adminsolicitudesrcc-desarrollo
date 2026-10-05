using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using AdminSolicitudesContratistas.Models;

namespace AdminSolicitudesContratistas.Controllers
{
    public class HomeController : Controller
    {
     
        
        [CheckAuthorization]
        public ActionResult Index()
        {
            return View();
        }





        [HttpGet]
        //[CheckAuthorization]
        public ActionResult Login(string returnURL)
        {
            //var userinfo = new USUARIOS();
            try
            {

                EnsureLoggedOut();
                // Store the originating URL so we can attach it to a form field  
                

                return View();

            }
            catch
            {
                throw;
            }
        }


        [HttpPost]
        public ActionResult Login(string username, string password) {
            try
            {
                
                //VALIDAR USUARIO EXISTENTE EN DB CONTRATISTA
                if (SQLUsuariosMOP.UsuarioMOPExistenteDB(username) == 1)
                {
                    //EXISTE
                    //VALIDAR CREDENCIALES EN ACTIVE DIRECTORY
                    WS_AD.AccesoActiveDirectory wsValida = new WS_AD.AccesoActiveDirectory();

                    string resp = wsValida.ValidaUsuarioAD(username, password);

                    if (resp == "SI existe usuario")
                    {

                        //TRAER DATOS DE USUARIO
                        //WSFuncionarios.fichaFuncionarioPorEmailCompletedEventArgs wsDatosFuncionarios = new WSFuncionarios.fichaFuncionarioPorEmailCompletedEventArgs();
                        UsuarioMOPView usr = new UsuarioMOPView();
                        usr = SQLMethods.TraerDatosUsuarioMOP(username);


                        Session["SessionNombreUsuario"] = usr.Nombre;
                        Session["SessionPerfilUsuario"] = usr.Perfil;
                        Session["SessionCodPerfil"] = usr.CodPerfil;

                        Session["SessionUserTipoRegistro"] = usr.UserTipoRegistro;
                        Session["SessionUserTipoSolicitud"] = usr.UserTipoSolicitud;
                        Session["SessionUserRegion"] = usr.UserRegion;

                        //Session["SessionNombreUsuario"] = username;
                        Session["SessionUsernameAD"] = username;
                        
                        //SETEO DE CONFIGURACIÓN DE FORMS AUTHENTICATION
                        FormsAuthentication.SetAuthCookie(username, false);

                        //int minutosTimeOut = int.Parse(System.Web.Configuration.WebConfigurationManager.AppSettings["sessionTimeout"]);
                        int minutosTimeOut = System.Web.HttpContext.Current.Session.Timeout;
                        int minutosTimeOutms = minutosTimeOut * 60000; // 1 min = 60000 ms
                        Session["SessionMinutosTimeoutms"] = minutosTimeOutms;


                        return RedirectToAction("Index", "Home");
                        //return Redirect("../home/index");
                    }
                    else
                    {
                        TempData["ErrorMSG"] = "Acceso Denegado, las credenciales ingresadas no son correctas";
                        return View();
                    }
                }
                else
                {
                    //NO EXISTE 
                    TempData["ErrorMSG"] = "Acceso Denegado, el usuario no existe en el Sistema de Registro de Contratistas o no se encuentra vigente";
                    return View();
                }
            }
            catch (Exception x)
            {
                TempData["ErrorMSG"] = "Error al intentar ingresar: " + x.Message.ToString();
                return View();
                //throw;
            }
        
        }

        //GET: EnsureLoggedOut  
        private void EnsureLoggedOut()
        {
            // If the request is (still) marked as authenticated we send the user to the logout action  
            if (Request.IsAuthenticated)
                Logout();
        }

        [HttpGet]
        public ActionResult Logout()
        {

            // First we clean the authentication ticket like always  
            //required NameSpace: using System.Web.Security;  
            FormsAuthentication.SignOut();

            // Second we clear the principal to ensure the user does not retain any authentication  
            //required NameSpace: using System.Security.Principal;  
            HttpContext.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);

            Session.Clear();
            System.Web.HttpContext.Current.Session.RemoveAll();

            return RedirectToAction("Login", "Home");
        }


        //public ActionResult CheckSession()
        //{
        //    var username = Session["SessionNombreUsuario"];
        //    var isAuthenticated = User.Identity.IsAuthenticated;


        //    if (username == null || !isAuthenticated)
        //    {
        //        // Session has expired or user is not authenticated
        //        // Perform the necessary action, such as redirecting to the login page
        //        return RedirectToAction("Login", "Home");
        //        //return View("Login");
        //    }
        //    //else
        //    //{
        //    //    // Session is active and user is authenticated
        //    //    // Proceed with the action
        //    //    // ...
        //    //}
        //    return View();
        //}




        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }



        






    }
}