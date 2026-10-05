using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AdminSolicitudesContratistas.Models;
using AdminSolicitudesContratistas.Models.ViewModels;

namespace AdminSolicitudesContratistas.Controllers
{
    public class UsuariosMOPController : Controller
    {
        // GET: UsuariosMOP
        [CheckAuthorization]
        public ActionResult Index()
        {
            return View();
        }


        #region CARGA DE PARÁMETROS


        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult JsonTraerParametrosUsuario()
        {
            try
            {
                var viewModel = new ParametrosUsuarioViewModel();
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    // 1. Cargar Tipos de Solicitud
                    int codigoPadreSol = 2241;
                    viewModel.TiposSolicitud = db.SP_MOP_LISTAR_PARAMETROS_POR_TIPO(codigoPadreSol)
                        .Select(p => new FiltroItemViewModel { Codigo = p.CODIGO, Descripcion = p.DESCRIPCION })
                        .ToList();

                    // 2. Cargar Tipos de Registro
                    int codigoPadreReg = 2237;
                    viewModel.TiposRegistro = db.SP_MOP_LISTAR_PARAMETROS_POR_TIPO(codigoPadreReg)
                        .Select(p => new FiltroItemViewModel { Codigo = p.CODIGO, Descripcion = p.DESCRIPCION })
                        .ToList();

                    // 3. Cargar Perfiles
                    viewModel.Perfiles = db.SP_MOP_ADM_LISTAR_PERFILES()
                        .Select(p => new PerfilViewModel { CodPerfil = p.CodPerfil, DescPerfil = p.DescPerfil })
                        .ToList();

                    // 4. Cargar Regiones
                    viewModel.Regiones = db.SP_MOP_LISTAR_REGIONES()
                        .Select(r => new RegionFiltroViewModel { CodRegion = r.COD_REGION, NombreRegion = r.NOMBRE_REGION })
                        .ToList();
                }
                return Json(viewModel);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { success = false, message = "Error al cargar los parámetros." });
            }
        }


        [CheckAuthorization]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JsonTraerPerfilesMOP()
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    //MOPSolicitud sol = new MOPSolicitud();
                    var list = db.SP_MOP_ADM_LISTAR_PERFILES().ToList();
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


        #region CARGA USUARIOS


        [CheckAuthorization]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JsonTraerUsuariosMOP()
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    //MOPSolicitud sol = new MOPSolicitud();
                    var list = db.SP_MOP_ADM_LISTAR_USUARIOS_MOP().ToList();
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


        #region CARGA DATOS DE USUARIOS

        [ValidateAntiForgeryToken]
        [CheckAuthorization]
        [HttpPost]
        public JsonResult JsonTraerDatosUsuarioMOP(string usernameAD)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    //MOPSolicitud sol = new MOPSolicitud();
                    var list = db.SP_MOP_ADM_TRAER_DATOS_USUARIO_MOP(usernameAD).ToList();
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


        [CheckAuthorization]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JsonBuscarFuncionarioMOPWS(string funcionarioAD)
        {
            try
            {
                var jsonResult = new JsonResult();
                //VALIDAR USUARIO EXISTENTE EN DB CONTRATISTA
                if (SQLUsuariosMOP.UsuarioMOPExistenteDB(funcionarioAD) == 1)
                {
                    jsonResult.Data = "EXISTE";

                }
                else
                {
                    WS_AD.AccesoActiveDirectory wsValida = new WS_AD.AccesoActiveDirectory();
                    //var func = wsValida.ValidaPorRunAD(runFuncionarioMOP);                
                    var func = wsValida.ValidaPorUsuarioAD(funcionarioAD);
                    jsonResult.Data = func;


                }



                jsonResult.MaxJsonLength = Int32.MaxValue;
                jsonResult.ContentType = "application/json";
                return jsonResult;
            }
            catch (Exception)
            {

                throw;
            }

        }





        #endregion




        #region ADMINISTRAR USUARIOS
        

        [ValidateAntiForgeryToken]
        [CheckAuthorization]
        [HttpPost]
        // Se elimina el parámetro bool nuevoUsuario de la firma
        public JsonResult JsonGuardarUsuarioMOP(string usernameAD, string nombreUsuario, string emailUsuario, bool vigente,
            string perfilUsuario, string tipoSolicitud, string tipoRegistro, int regiones)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    // El parámetro 'superAdmin' no se usa en el JS, lo pongo como 'false' por defecto
                    bool superAdmin = false;

                    // 1. La llamada al SP devuelve un 'int?'.
                    int? resultado = db.SP_MOP_ADM_GUARDAR_USUARIO_MOP(usernameAD, nombreUsuario, emailUsuario, vigente, superAdmin, perfilUsuario, tipoSolicitud, tipoRegistro, regiones)
                                        .FirstOrDefault();

                    // 2. Comparamos directamente la variable 'resultado'.
                    if (resultado.HasValue && resultado.Value == 2)
                    {
                        // Devolvemos el JSON que espera tu JavaScript.
                        // Como tu JS espera el número 2, se lo enviamos directamente.
                        return Json("2");
                    }
                    else
                    {
                        // Si falla o devuelve otro valor, puedes devolver un código de error o un objeto.
                        // Devolver un objeto es más robusto.
                        return Json(new { success = false, message = "Ocurrió un error al guardar el usuario." });
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }



        public static string GuardarLogAdminUsuarios(MOPLog log, string funcionario)
        {
            string resp = "OK";
            string accion = ""; //GUARDAR NUEVO USUARIO || EDITAR USUARIO
            try
            {
                //string usuario = Session["SessionNombreUsuario"].ToString();
                SQLMethods.GuardarMOPLog(log.RutProveedor, int.Parse(log.idSol.ToString()), int.Parse(log.idDoc.ToString()), log.CodEstado, log.CodTipo, log.Comentario_1, log.Comentario_3, log.Comentario_3, funcionario);
            }
            catch (Exception x)
            {
                resp = "Error al intentar actualizar el detalle de Solicitud: " + x.InnerException.Message;
            }
            return resp;
        }




        #endregion











    }
}