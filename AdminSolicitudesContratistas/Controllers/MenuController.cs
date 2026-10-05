using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


using AdminSolicitudesContratistas.Models;
using AdminSolicitudesContratistas.Models.ViewModels;

namespace AdminSolicitudesContratistas.Controllers
{
    public class MenuController : Controller
    {
        // GET: Menu
        public ActionResult Index()
        {
            return View();
        }


        #region LISTAR MENU


        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult JsonTraerMenuCompleto(string username)
        {
            try
            {
                using (var db = new dsSolicitudesMOP())
                {
                    // 1. Traemos la lista plana desde el SP consolidado
                    var listaPlana = db.SP_MOP_ADM_OBTENER_MENU_COMPLETO_USUARIO(username).ToList();

                    // 2. Agrupamos por Menú Padre usando LINQ
                    var menuCompleto = listaPlana
                        .GroupBy(m => new { m.CodMenu, m.DescMenu })
                        .Select(grupo => new MenuItemViewModel
                        {
                            CodMenu = grupo.Key.CodMenu,
                            DescMenu = grupo.Key.DescMenu,
                            Opciones = grupo.Select(o => new OpcionMenuViewModel
                            {
                                DescOpcion = o.DescOpcion,
                                Controlador = o.Controlador
                            }).ToList()
                        }).ToList();

                    return Json(menuCompleto);
                }
            }
            catch (Exception ex)
            {
                // Loguear el error si es necesario
                return Json(new List<MenuItemViewModel>());
            }
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken] 
        //public JsonResult JsonTraerMenuCompleto(string username)
        //{
        //    try
        //    {
        //        ///////////////////////////

        //        var menuCompleto = new List<MenuItemViewModel>();
        //        using (var db = new dsSolicitudesMOP())
        //        {
        //            // 1. Obtenemos la lista de menús principales usando tu SP existente.
        //            var menusPrincipales = db.SP_MOP_ADM_LISTAR_MENU().ToList();

        //            // 2. Para cada menú, obtenemos sus opciones correspondientes.
        //            foreach (var menuInfo in menusPrincipales)
        //            {
        //                var menuItem = new MenuItemViewModel
        //                {
        //                    CodMenu = menuInfo.CodMenu,
        //                    DescMenu = menuInfo.DescMenu
        //                };

        //                // Obtenemos las opciones para ESTE menú y ESTE usuario usando tu otro SP.
        //                var opciones = db.SP_MOP_ADM_LISTAR_OPCIONES_POR_MENU(menuInfo.CodMenu, username)
        //                    .Select(o => new OpcionMenuViewModel
        //                    {
        //                        DescOpcion = o.DescOpcion,
        //                        Controlador = o.Controlador
        //                    }).ToList();

        //                // Solo añadimos el menú al resultado si el usuario tiene al menos una opción visible en él.
        //                if (opciones.Any())
        //                {
        //                    menuItem.Opciones = opciones;
        //                    menuCompleto.Add(menuItem);
        //                }
        //            }
        //        }
        //        return Json(menuCompleto);
        //    }
        //    catch (Exception)
        //    {
        //        // En caso de error, devolvemos un array vacío para no romper el cliente.
        //        return Json(new List<MenuItemViewModel>());
        //    }
        //}





        #endregion


        #region MÉTODOS ANTIGUOS SIN USO
        //[ValidateAntiForgeryToken]
        //[CheckAuthorization]
        [HttpPost]
        public JsonResult JsonListarMenu()
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    var list = db.SP_MOP_ADM_LISTAR_MENU().ToList();
                    var jsonResult = new JsonResult();
                    int cant = list.Count;

                    if (cant > 0)
                    {
                        jsonResult.Data = list;
                    }
                    else
                    {
                        jsonResult.Data = "EMPTY";
                    }

                    jsonResult.MaxJsonLength = Int32.MaxValue;
                    jsonResult.ContentType = "application/json";
                    return jsonResult;
                }
            }
            catch (Exception)
            {

                throw;
            }

        }


        //[ValidateAntiForgeryToken]
        //[CheckAuthorization]
        [HttpPost]
        public JsonResult JsonListarOpcionesPorMenu(string codMenu, string username)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    //MOPSolicitud sol = new MOPSolicitud();
                    //var list = db.SP_MOPFindListaSolicitudesCompleto(rutProveedor, idSolicitud, tipoSolicitud, estado, tipoRegistro, region, razonSocial, tipoBusqueda, ano).ToList();
                    var list = db.SP_MOP_ADM_LISTAR_OPCIONES_POR_MENU(codMenu, username).ToList();
                    var jsonResult = new JsonResult();
                    int cant = list.Count;

                    if (cant > 0)
                    {
                        jsonResult.Data = list;
                    }
                    else
                    {
                        jsonResult.Data = "EMPTY";
                    }

                    jsonResult.MaxJsonLength = Int32.MaxValue;
                    jsonResult.ContentType = "application/json";
                    return jsonResult;
                }
            }
            catch (Exception)
            {

                throw;
            }

        }



        #endregion








    }
}