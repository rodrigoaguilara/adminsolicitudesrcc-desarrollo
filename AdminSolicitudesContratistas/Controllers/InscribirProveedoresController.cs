using AdminSolicitudesContratistas.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AdminSolicitudesContratistas.Controllers
{
    public class InscribirProveedoresController : Controller
    {
        // GET: InscribirProveedores
        [CheckAuthorization]
        public ActionResult Index()
        {
            return View();
        }



        [ValidateAntiForgeryToken]
        [CheckAuthorization]
        [HttpPost]
        public JsonResult JsonBuscarProveedorInscrito(string rutProveedor)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    var jsonResult = new JsonResult();
                    var listInscritos = db.SP_MOP_BUSCAR_PROVEEDORES_MOP(rutProveedor, "", "", 0, "").ToList();
                    int cant = listInscritos.Count;

                    if (cant > 0)
                    {
                        jsonResult.Data = listInscritos;
                    }
                    else
                    {
                        var listNuevos = db.SP_MOP_BUSCAR_PROVEEDOR_PARA_INSCRIPCION(rutProveedor).ToList();
                        int cantNuevo = listNuevos.Count;

                        if (cantNuevo > 0) {
                            jsonResult.Data = listNuevos;
                        }
                        else { 
                            jsonResult.Data = "EMPTY"; 
                        }
                        
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


        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JsonTraerEstadosProveedor(string rutProveedor)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    MOPSolicitud sol = new MOPSolicitud();
                    //var list = db.SP_MOPFindProveedorEstado(rutProveedor).ToList();
                    var list = db.SP_MOP_TRAER_ESTADOS_PROVEEDOR(rutProveedor).ToList();
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



        [ValidateAntiForgeryToken]
        [CheckAuthorization]
        [HttpPost]
        public JsonResult JsonInscribirProveedorRegistroMOP(string rutProveedor, string codEstado,  DateTime fechaInscripcion,
                                                           string codTipoRegistro, int numSolicitudAsociada)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    string usuarioResp = Session["SessionNombreUsuario"].ToString();
                    //var OK = db.SP_MOP_INSCRIBIR_PROVEEDOR_REGISTRO_MOP(rutProveedor, codEstado, fechaInscripcion, usuarioResp, codTipoRegistro, numSolicitudAsociada);
                    //var jsonResult = new JsonResult();
                    //jsonResult.Data = OK;

                    //jsonResult.MaxJsonLength = Int32.MaxValue;
                    //jsonResult.ContentType = "application/json";
                    //return jsonResult;

                    var resultCode = new ObjectParameter("ResultCode", typeof(int));

                    // 2. Llamar al SP, pasando el ObjectParameter al final.
                    db.SP_MOP_INSCRIBIR_PROVEEDOR_REGISTRO_MOP(rutProveedor, codEstado, fechaInscripcion, usuarioResp, codTipoRegistro, numSolicitudAsociada, resultCode);

                    // 3. Obtener el valor del parámetro de salida.
                    var OK = Convert.ToInt32(resultCode.Value);

                    bool varSuccess = false;
                    string varMessage = "";

                    if (OK == 1)
                    {
                        //return Json(new { success = true, message = "Cambio de estado guardado correctamente." });
                        varSuccess = true;
                        varMessage = "Cambio de estado guardado correctamente.";
                    }
                    else if(OK == -1)
                    {
                        //return Json(new { success = false, message = "No se pudo guardar el cambio de estado." });
                        varSuccess = false;
                        varMessage = "No se pudo guardar el cambio de estado.";
                    }
                    else if(OK == -2)
                    {
                        //return Json(new { success = false, message = "El Proveedor ya se encuentra previamente registrado." });
                        varSuccess = false;
                        varMessage = "El Proveedor ya se encuentra previamente registrado.";
                    }

                    //switch (OK)
                    //{
                    //    case 1:
                    //        return Json(new { success = true, message = "Cambio de estado guardado correctamente." });
                    //        break;
                    //    case -1:
                    //        return Json(new { success = true, message = "Cambio de estado guardado correctamente." });
                    //        break;
                    //    case -2:
                    //        return Json(new { success = true, message = "Cambio de estado guardado correctamente." });
                    //        break;
                    //}

                    return Json(new { success = varSuccess, message = varMessage });

                }
            }
            catch (Exception)
            {
                throw;
            }

        }





    }
}