using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AdminSolicitudesContratistas.Models;


namespace AdminSolicitudesContratistas.Controllers
{
    public class OpcionesMenuPerfilController : Controller
    {
        [CheckAuthorization]
        // GET: OpcionesMenuPerfil
        public ActionResult Index()
        {
            return View();
        }
    }
}