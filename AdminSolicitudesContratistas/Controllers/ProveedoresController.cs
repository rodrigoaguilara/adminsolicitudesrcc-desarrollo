using AdminSolicitudesContratistas.Models;
using AdminSolicitudesContratistas.Models.GesintelMethods;
using AdminSolicitudesContratistas.Models.GesintelModel;
using AdminSolicitudesContratistas.Models.ViewModels;
//conexion api correo
using AdminSolicitudesContratistas.Services;
using ClosedXML.Excel;
using Microsoft.Ajax.Utilities;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.Objects;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Windows.Interop;

namespace AdminSolicitudesContratistas.Controllers
{
    public class ProveedoresController : Controller
    {
        // GET: Proveedores
        [CheckAuthorization]
        public ActionResult Index()
        {
            return View();
        }

        #region FILTROS INICIALES


        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JsonTraerFiltrosPrincipales()
        {
            try
            {
                var viewModel = new FiltrosBusquedaProveedorViewModel();

                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    // 1. Cargar Tipos de Registro
                    int codigoPadreTipoRegistro = 2237;
                    viewModel.TiposRegistro = db.SP_MOP_LISTAR_PARAMETROS_POR_TIPO(codigoPadreTipoRegistro)
                        .Select(p => new FiltroItemViewModel
                        {
                            Codigo = p.CODIGO,
                            Descripcion = p.DESCRIPCION
                        }).ToList();

                    // 2. Cargar Estados de Proveedor
                    viewModel.EstadosProveedor = db.SP_MOP_TRAER_ESTADOS_VIGENCIA_MOP()
                        .Select(e => new FiltroItemViewModel
                        {
                            Codigo = e.Codigo,
                            Descripcion = e.Descripcion
                        }).ToList();

                    // 3. Cargar Regiones
                    viewModel.Regiones = db.SP_MOP_LISTAR_REGIONES()
                        .Select(r => new RegionFiltroViewModel
                        {
                            CodRegion = r.COD_REGION,
                            NombreRegion = r.NOMBRE_REGION
                        }).ToList();
                }

                return Json(viewModel);
            }
            catch (Exception ex)
            {
                // Manejo de errores
                Response.StatusCode = 500;
                return Json(new { success = false, message = "Error al cargar los filtros iniciales: " + ex.Message.ToString() });
            }
        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JsonTraerListadoRegiones()
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    //MOPSolicitud sol = new MOPSolicitud();
                    var list = db.SP_MOP_LISTAR_REGIONES().ToList();
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



        #region DATOS PRINCIPALES PROVEEDORES


        [CheckAuthorization]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JsonTraerProveedores(string rutProveedor, string razonSocial, string codEstadoMOP, int region, string tipoRegistro)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    //MOPSolicitud sol = new MOPSolicitud();
                    var list = db.SP_MOP_BUSCAR_PROVEEDORES_MOP(rutProveedor, razonSocial, codEstadoMOP, region, tipoRegistro).ToList();
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


        /// <summary>
        /// /////
        /// </summary>
        /// <param name="rutProveedor"></param>
        /// <param name="codTipoRegistro"></param>
        /// <returns></returns>
        //NUEVO MÉTODO PARA TRAER TODO EL DETALLE DE PROVEEDORES EN UNA LLAMADA
        [ValidateAntiForgeryToken]
        [CheckAuthorization]
        [HttpPost]
        public JsonResult JsonTraerDetalleCompletoProveedor(string rutProveedor, string codTipoRegistro)
        {
            try
            {
                // El objeto principal que devolveremos
                var viewModel = new ProveedorDetalleCompletoViewModel();

                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    // 1. Obtener Detalle Principal
                    // Usamos FirstOrDefault porque esperamos un solo proveedor
                    var detalleDb = db.SP_MOP_TRAER_PROVEEDOR_POR_RUT(rutProveedor).FirstOrDefault();
                    if (detalleDb != null)
                    {
                        viewModel.DetallePrincipal = new DetalleProveedorPrincipalViewModel
                        {
                            Rut = detalleDb.Rut,
                            RazonSocial = detalleDb.RazonSocial,
                            Email = detalleDb.Email,
                            NombreFantasia = detalleDb.NombreFantasia
                        };
                    }

                    // 2. Obtener Historial de Razón Social
                    viewModel.HistorialRazonSocial = db.SP_MOP_TRAER_LOG_PROVEEDOR(rutProveedor, "CM_RS")
                        .Select(log => new HistorialLogViewModel
                        {
                            FECHA_LOG = log.FECHA_LOG,
                            COMENTARIO_1 = log.COMENTARIO_1,
                            COMENTARIO_2 = log.COMENTARIO_2,
                            COMENTARIO_3 = log.COMENTARIO_3,
                            USUARIO_RESPONSABLE = log.USUARIO_RESPONSABLE
                        }).ToList();

                    // 3. Obtener Estado MOP Actual
                    var estadoMopDb = db.SP_MOP_TRAER_ESTADO_PROVEEDOR_POR_TIPO_REG(rutProveedor, codTipoRegistro).FirstOrDefault();
                    if (estadoMopDb != null)
                    {
                        viewModel.EstadoMopActual = new EstadoMopViewModel
                        {
                            ESTADO = estadoMopDb.ESTADO,
                            MES_RENOVACION = estadoMopDb.MES_RENOVACION,
                            COD_ESTADO = estadoMopDb.COD_ESTADO,
                            FECHA_INGRESO_REG = estadoMopDb.FECHA_INGRESO_REG,
                            FECHA_SUSPENSION_REG = estadoMopDb.FECHA_SUSPENSION_REG,
                            NUM_SOLICITUD_ASOCIADA = estadoMopDb.NUM_SOLICITUD_ASOCIADA
                        };
                    }

                    // 4. Obtener Historial de Estado MOP
                    viewModel.HistorialEstadoMop = db.SP_MOP_TRAER_LOG_PROVEEDOR(rutProveedor, "CEV")
                        .Select(log => new HistorialLogViewModel
                        {
                            FECHA_LOG = log.FECHA_LOG,
                            COMENTARIO_1 = log.COMENTARIO_1,
                            COMENTARIO_2 = log.COMENTARIO_2,
                            COMENTARIO_3 = log.COMENTARIO_3,
                            USUARIO_RESPONSABLE = log.USUARIO_RESPONSABLE
                        }).ToList();

                    // 5. Obtener Capacidad Económica (si no es Consultor)
                    if (codTipoRegistro != "CNS")
                    {
                        // Capacidad Actual (Habilitada)
                        var capEconActualDb = db.SP_MOP_TRAER_CAPACIDAD_ECONOMICA_PROVEEDOR(rutProveedor, codTipoRegistro, "HAB").FirstOrDefault();
                        if (capEconActualDb != null)
                        {
                            viewModel.CapacidadEconomicaActual = new CapacidadEconomicaViewModel
                            {
                                CapacidadEconomica = capEconActualDb.CapacidadEconomica,
                                FechaEstadoFinanciero = capEconActualDb.FechaEstadoFinanciero
                            };
                        }

                        // Historial de Capacidad (Inhabilitados)
                        viewModel.HistorialCapacidadEconomica = db.SP_MOP_TRAER_CAPACIDAD_ECONOMICA_PROVEEDOR(rutProveedor, codTipoRegistro, "INH")
                            .Select(h => new CapacidadEconomicaViewModel
                            {
                                CapacidadEconomica = h.CapacidadEconomica,
                                FechaEstadoFinanciero = h.FechaEstadoFinanciero
                            }).ToList();
                    }

                    // 6. Obtener Especialidades y Categorías (Lógica consolidada)
                    string codTipoEsp = "", codTipoCat = "";
                    var especialidadesProveedor = db.SP_MOP_TRAER_ESPECIALIDADES_CATEGORIA_PROVEEDOR(rutProveedor, codTipoRegistro, codTipoEsp, codTipoCat).ToList();

                    foreach (var esp in especialidadesProveedor)
                    {
                        var espCompleta = new EspecialidadCompletaViewModel
                        {
                            Id = esp.Id,
                            TipoEspecialidad = esp.TipoEspecialidad,
                            CodTipoEspecialidad = esp.CodTipoEspecialidad,
                            CodTipoCategoriaSeleccionada = esp.CodTipoCategoria,
                            // Obtenemos aquí mismo las categorías disponibles para esta especialidad
                            CategoriasDisponibles = db.SP_MOP_TRAER_CATEGORIAS_POR_ESPECIALIDAD(codTipoRegistro, esp.CodTipoEspecialidad, codTipoCat)
                                .Select(cat => new CategoriaDisponibleViewModel
                                {
                                    CodTipoCategoria = cat.CodTipoCategoria,
                                    TipoCategoria = cat.TipoCategoria
                                }).ToList()
                        };
                        viewModel.EspecialidadesCompletas.Add(espCompleta);
                    }

                    // 7. Obtener Historial de Especialidades
                    viewModel.HistorialEspecialidades = db.SP_MOP_TRAER_HISTORIAL_ESPECIALIDADES_Y_CATEGORIAS(rutProveedor, codTipoRegistro)
                       .Select(log => new HistorialLogViewModel
                       {
                           FECHA_LOG = log.FECHA, 
                    COMENTARIO_1 = log.COMENTARIO_1,
                           COMENTARIO_2 = log.COMENTARIO_2,
                           USUARIO_RESPONSABLE = log.USUARIO_RESPONSABLE
                       }).ToList();


                    // 8. Obtener Documentos del Proveedor
                    string visibleAlProveedor = "", palabrasClave = "", codAreaAnalisis = "", codTipoDocumento = "";
                    int año = 0;
                    viewModel.Documentos = db.SP_MOP_LISTAR_DOCUMENTOS_DEL_PROVEEDOR(rutProveedor, visibleAlProveedor, palabrasClave, año, codAreaAnalisis, codTipoDocumento)
                        .Select(doc => new DocumentoProveedorViewModel
                        {
                            NOMBRE_ARCHIVO = doc.NOMBRE_ARCHIVO,
                            DESCRIPCION = doc.DESCRIPCION,
                            ID_SOL = doc.ID_SOL,
                            AREA_ANALISIS = doc.AREA_ANALISIS,
                            TIPO_DOCUMENTO = doc.TIPO_DOCUMENTO,
                            TIPO = doc.TIPO,
                            AÑO = doc.AÑO,
                            PALABRAS_CLAVE = doc.PALABRAS_CLAVE,
                            VISIBLE_AL_PROVEEDOR = doc.VISIBLE_AL_PROVEEDOR,
                            ID_ARCHIVO_GESINTEL = doc.ID_ARCHIVO_GESINTEL
                        }).ToList();
                }

                // Devolvemos el objeto completo
                var jsonResult = new JsonResult();
                jsonResult.Data = viewModel;
                jsonResult.MaxJsonLength = Int32.MaxValue; // Buena práctica mantenerlo por si los datos son muchos
                jsonResult.ContentType = "application/json";
                return jsonResult;
            }
            catch (Exception ex)
            {
                // Es mejor manejar la excepción que simplemente lanzarla.
                // Puedes loguear el error (ex.ToString())
                Response.StatusCode = 500; // Internal Server Error
                return Json(new { success = false, message = "Ocurrió un error en el servidor al obtener los datos." });
            }
        }




        [ValidateAntiForgeryToken]
        [CheckAuthorization]
        [HttpPost]
        public JsonResult JsonGuardarCambioRazonSocial(string rutProveedor, string razonSocial, string nombreFantasia)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    string usuarioResp = Session["SessionNombreUsuario"].ToString();
                    var OK = db.SP_MOP_GUARDAR_CAMBIO_RAZON_SOCIAL(rutProveedor, razonSocial, nombreFantasia);
                    var jsonResult = new JsonResult();
                    jsonResult.Data = OK;

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



        #region ESPECIALIDADES Y CATEGORIAS
        //ESPECIALIDADES Y CATEGORÍAS


        




        [ValidateAntiForgeryToken]
        //[CheckAuthorization]
        [HttpPost]
        public JsonResult JsonGuardarCategoriasPorEspecialidadProveedor(string rutProveedor, string codTipoReg, string codEspecialidad, string codCategoria)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    string usuarioResp = Session["SessionNombreUsuario"].ToString();
                    var OK = db.SP_MOP_GUARDAR_CATEGORIAS_POR_ESPECIALIDAD_PROVEEDOR(rutProveedor, codTipoReg, codEspecialidad, codCategoria);
                    var jsonResult = new JsonResult();
                    jsonResult.Data = OK;

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
        //[CheckAuthorization]
        [HttpPost]
        public JsonResult JsonTraerHistorialEspecialidadesYCategorias(string rutProveedor, string codTipoReg)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    var list = db.SP_MOP_TRAER_HISTORIAL_ESPECIALIDADES_Y_CATEGORIAS(rutProveedor, codTipoReg).ToList();
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


        [HttpPost]
        public ActionResult PartialRenderEspecialidades(List<EspecialidadCompletaViewModel> model) 
        {
            
            if (model == null)
            {
                model = new List<EspecialidadCompletaViewModel>();
            }
            return PartialView("EspecialidadesProveedor", model);
        }


        #endregion


        #region CAPACIDAD ECONOMICA

        //CAPACIDAD ECONOMICA


        [ValidateAntiForgeryToken]
        //[CheckAuthorization]
        [HttpPost]
        public JsonResult JsonTraerHistorialCapacidadEconomicaProveedor(string rutProveedor, string codTipoReg)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    string codEstado = "INH"; //PARA TRAER HISTORIAL 
                    var list = db.SP_MOP_TRAER_CAPACIDAD_ECONOMICA_PROVEEDOR(rutProveedor, codTipoReg, codEstado).ToList();
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
        public JsonResult JsonGuardarNuevaCapacidadEconomicaProveedor(string rutProveedor, string codTipoReg, int montoCapacidad, DateTime fechaEstadoFinanciero)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {

                    var OK = db.SP_MOP_GUARDAR_NUEVA_CAPACIDAD_ECONOMICA(rutProveedor, codTipoReg, montoCapacidad, fechaEstadoFinanciero);
                    var jsonResult = new JsonResult();
                    jsonResult.Data = OK;

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

        #region ESTADO PROVEEDOR

        //ESTADO PROVEEDOR

        

        [ValidateAntiForgeryToken]
        [CheckAuthorization]
        [HttpPost]
        public JsonResult JsonTraerDocumentoResolucionParaCambioEstado(int numSolicitud, string rutProveedor)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    //string codEstado = "INH"; //PARA TRAER HISTORIAL 
                    var list = db.SP_MOP_TRAER_DOCUMENTO_RESOLUCION_PARA_CAMBIO_ESTADO_PROV(numSolicitud, rutProveedor).ToList();
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
        public JsonResult JsonGuardarCambioEstadoProveedor(string rutProveedor, string codEstado, string codEstadoAnterior, DateTime fechaInscripcion,
                                                           string codTipoReg, int numSolicitudAsociada)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    string usuarioResp = Session["SessionNombreUsuario"].ToString();
                    //var OK = db.SP_MOP_GUARDAR_CAMBIO_ESTADO_PROVEEDOR(rutProveedor, codEstado, codEstadoAnterior, fechaInscripcion, usuarioResp, codTipoReg, numSolicitudAsociada);
                    var resultCode = new ObjectParameter("ResultCode", typeof(int));

                    // 2. Llamar al SP, pasando el ObjectParameter al final.
                    db.SP_MOP_GUARDAR_CAMBIO_ESTADO_PROVEEDOR(rutProveedor, codEstado, codEstadoAnterior,
                                                             fechaInscripcion, usuarioResp, codTipoReg,
                                                             numSolicitudAsociada, resultCode);

                    // 3. Obtener el valor del parámetro de salida.
                    var OK = Convert.ToInt32(resultCode.Value);


                    if (OK == 1) 
                    {
                        return Json(new { success = true, message = "Cambio de estado guardado correctamente." });
                    }
                    else
                    {
                        return Json(new { success = false, message = "No se pudo guardar el cambio de estado." });
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        [ValidateAntiForgeryToken]
        //[CheckAuthorization]
        [HttpPost]
        public JsonResult JsonTraerSolicitudesParaCambioEstado(string rutProveedor, string codTipoRegistro)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    var list = db.SP_MOP_TRAER_SOLICITUDES_PARA_CAMBIO_ESTADO(rutProveedor, codTipoRegistro).ToList();
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
        //[CheckAuthorization]
        [HttpPost]
        public JsonResult JsonTraerMesesRenovacion()
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    //MOPSolicitud sol = new MOPSolicitud();
                    //var list = db.SP_MOP_TRAER_PROVEEDOR_POR_RUT(rutProveedor).ToList();
                    List<Mes> listMeses = new List<Mes>();
                    listMeses = SQLMethods.TraerListadoMeses();

                    var jsonResult = new JsonResult();
                    int cant = listMeses.Count;

                    if (cant > 0)
                    {
                        jsonResult.Data = listMeses;
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
        [HttpPost]
        public JsonResult JsonTraerEstadosVigencia()
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    //MOPSolicitud sol = new MOPSolicitud();
                    var list = db.SP_MOP_TRAER_ESTADOS_VIGENCIA_MOP().ToList();
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
        //[CheckAuthorization]
        [HttpPost]
        public JsonResult JsonTraerEstadoProveedorPorTipoRegistro(string rutProveedor, string codTipoReg)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    var list = db.SP_MOP_TRAER_ESTADO_PROVEEDOR_POR_TIPO_REG(rutProveedor, codTipoReg).ToList();
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


        #region DOCUMENTOS DEL PROVEEDOR

        //DOCUMENTOS DEL PROVEEDOR







        [ValidateAntiForgeryToken]
        //[CheckAuthorization]
        [HttpPost]
        public JsonResult JsonTraerDocumentosDelProveedor(string rutProveedor)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    string visibleAlProveedor = "", palabrasClave = "", codAreaAnalisis = "", codTipoDocumento = "";
                    int año = 0;
                    var list = db.SP_MOP_LISTAR_DOCUMENTOS_DEL_PROVEEDOR(rutProveedor, visibleAlProveedor, palabrasClave, año, codAreaAnalisis, codTipoDocumento).ToList();
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
        //[CheckAuthorization]
        [HttpPost]
        public JsonResult JsonTraerTipoDocumento(string areaDocumento)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    var list = db.SP_MOP_TRAER_TIPO_DOCUMENTO_POR_AREA(areaDocumento).ToList();
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



        //////////////////////////////////
        /////////////////////////////////////
        //SUBIDA DE ARCHIVO
        [CheckAuthorization]
        [HttpPost]
        public ActionResult SubirArchivoProveedor(string hiddenRutProveedorSubirDoc, HttpPostedFileBase fileSubirDocumentoProveedor, string txtDescripcionSubirDocumentoProveedor,
            string cmbAreaSubirDocumentoProveedor, string cmbTipoDocSubirDocumentoProveedor, int txtAñoDocSubirDocumentoProveedor,
            string cmbVisibleAlProveedorSubirDocProv, string txtPalabrasClaveDocumentoProveedor)
        {

            int idTipoDocumento;

            if (!int.TryParse(Request.Form["idDoc"], out idTipoDocumento))
            {
                System.Diagnostics.Debug.WriteLine(
                    "No se recibió un ID de documento válido."
                );
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(
                    "ID del documento: " + idTipoDocumento
                );
            }

            if ("S".Equals(cmbVisibleAlProveedorSubirDocProv, StringComparison.OrdinalIgnoreCase) && idTipoDocumento != 3123)
            {


                /*
                 * RCC-TEST-002
                 * Prueba de envío con plantilla HTML institucional.
                 * Ambiente: Desarrollo.
                 * IMPORTANTE: Retirar antes de publicar.
                 */
                try
                {
                    using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                    {
                        var datosProveedor = db.SP_MOP_TRAER_PROVEEDOR_POR_RUT(hiddenRutProveedorSubirDoc).ToList();
                        var datosEstadoProveedor = db.SP_MOP_TRAER_ESTADOS_PROVEEDOR(hiddenRutProveedorSubirDoc).ToList();
                        var jsonResult = new JsonResult();
                        int cant = datosProveedor.Count;



                        if (cant > 0)
                        {
                            var proveedor = datosProveedor[0];
                            var EstadoProveedor = datosEstadoProveedor[0];
                            try
                            {
                                // Generar HTML con datos ficticios
                                //string RazonSocial,
                                //string RutProveedor,
                                //string TipoRegistro,
                                //string NombreDocumento,
                                //DateTime FechaPublicacion,
                                //string CorreoRegistro,
                                //string Asunto,
                                //string Mensaje)
                                string fileName = Path.GetFileName(fileSubirDocumentoProveedor.FileName);
                                string fechaMensaje = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                                string tipoDoc = cmbTipoDocSubirDocumentoProveedor;
                                string swAsunto;
                                string swCuerpo;
                                //se agrega el correo de registro
                                string CorreoRegistro = ConfigurationManager.AppSettings["CorreoRegistro"];

                                switch (idTipoDocumento)
                                {
                                    case 3117:
                                        // Oficio Suspensión Consultor
                                        swAsunto = ConfigurationManager.AppSettings["asuntoP-01"];
                                        swCuerpo = ConfigurationManager.AppSettings["P-01"];
                                        break;

                                    case 3118:
                                        // Oficio Eliminación Consultor
                                        swAsunto = ConfigurationManager.AppSettings["asuntoP-02"];
                                        swCuerpo = ConfigurationManager.AppSettings["P-02"];
                                        break;

                                    case 3119:
                                        // Oficio Suspensión Contratista
                                        swAsunto = ConfigurationManager.AppSettings["asuntoP-03"];
                                        swCuerpo = ConfigurationManager.AppSettings["P-03"];
                                        break;

                                    case 3120:
                                        // Oficio Eliminación Contratista
                                        swAsunto = ConfigurationManager.AppSettings["asuntoP-04"];
                                        swCuerpo = ConfigurationManager.AppSettings["P-04"];
                                        break;

                                    case 3121:
                                        // Resolución
                                        swAsunto = ConfigurationManager.AppSettings["asuntoP-05"];
                                        swCuerpo = ConfigurationManager.AppSettings["P-05"];
                                        break;

                                    case 3122:
                                        // Observaciones / Alcances
                                        swAsunto = ConfigurationManager.AppSettings["asuntoP-06"];
                                        swCuerpo = ConfigurationManager.AppSettings["P-06"];
                                        break;

                                    default:
                                        // Tipo de documento no contemplado
                                        swAsunto = "";
                                        swCuerpo = "";
                                        break;
                                }



                                string cuerpoHtml = PlantillaCorreoService.PlantillaCorreos(
                                    proveedor.RazonSocial,
                                    proveedor.Rut,
                                    EstadoProveedor.TipoRegistro,
                                    fileName,
                                    fechaMensaje,
                                    CorreoRegistro,
                                    swAsunto,
                                    swCuerpo
                                );

                                // Consumir API REST corporativa
                                var correoService = new CorreoApiService();

                                string respuesta = Task.Run(() =>
                                    correoService.EnviarCorreoAsync(
                                        "rodrigo.aguilar@mop.gov.cl",
                                        "rodrigo.aguilar@mop.gov.cl",
                                        swAsunto,
                                        cuerpoHtml,
                                        ""
                                    )
                                ).GetAwaiter().GetResult();

                                System.Diagnostics.Debug.WriteLine(
                                    "Respuesta API correo: " + respuesta
                                );

                                return Json(new
                                {
                                    success = true,
                                    message = "API de correo respondió correctamente.",
                                    respuestaApi = respuesta
                                });
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine(
                                    "Error API correo: " + ex.ToString()
                                );

                                return Json(new
                                {
                                    success = false,
                                    message = "Error al enviar correo de prueba."
                                });
                            }
                        }
                        else
                        {
                            jsonResult.Data = "EMPTY";
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return Json(new { success = false, message = "prueba" });

            //try
            //{
            //    if (Request.IsAjaxRequest())
            //    {
            //        if (fileSubirDocumentoProveedor != null)
            //        {
            //            //VALIDACIÓN EXTENSIÓN                    
            //            if (fileSubirDocumentoProveedor.FileName.EndsWith("pdf"))
            //            {
            //                //VALIDACION DESCRIPCION
            //                if (txtDescripcionSubirDocumentoProveedor.Length > 0)
            //                {
            //                    //VAIDACION TIPO DOCUMENTO
            //                    if (cmbTipoDocSubirDocumentoProveedor != null)
            //                    {
            //                        //VALIDACION AÑO
            //                        //if (txtAñoDocSubirDocumentoProveedor.ToString().Length > 0) {                               }
            //                        //VALIDACIÓN PALABRAS CLAVE
            //                        if (txtPalabrasClaveDocumentoProveedor.Length > 0)
            //                        {

            //                            ProveedorView prov = new ProveedorView();
            //                            string rutProveedor = hiddenRutProveedorSubirDoc;
            //                            prov = SQLMethods.TraerDatosProveedor(rutProveedor);

            //                            string fechaTmp = DateTime.Now.ToString("ddMMyyyyHHmmss");
            //                            string fileName = Path.GetFileName(fileSubirDocumentoProveedor.FileName);
            //                            string folder = rutProveedor.Replace("-", "_") + "_" + fechaTmp;
            //                            string folderPath = Path.Combine(Server.MapPath("~/Content/TempUploadedFiles/"), folder);
            //                            string path = Path.Combine(Server.MapPath("~/Content/TempUploadedFiles/"), folder, fileName);

            //                            Directory.CreateDirectory(folderPath);

            //                            string descripcion = txtDescripcionSubirDocumentoProveedor;
            //                            string areaDoc = cmbAreaSubirDocumentoProveedor;
            //                            string tipoDoc = cmbTipoDocSubirDocumentoProveedor;
            //                            int año = txtAñoDocSubirDocumentoProveedor;
            //                            string visibleAlProv = cmbVisibleAlProveedorSubirDocProv; //S o N
            //                            string palabrasClave = txtPalabrasClaveDocumentoProveedor;

            //                            //SE GUARDA ARCHIVO EN RUTA TMP PARA LUEGO SUBIRLO A GESINTEL, Y LUEGO SE ELIMINA
            //                            fileSubirDocumentoProveedor.SaveAs(path);

            //                            //SUBIDA A GESINTEL
            //                            AuthenticateResponse token = Models.GesintelMethods.Token.gesintelLoginToken();
            //                            if (token.Status == "OK")
            //                            {
            //                                SendDocumentPayload doc = new SendDocumentPayload();
            //                                SendDocumentResponse resp = new SendDocumentResponse();

            //                                doc.Type = "MOP";
            //                                doc.Code = prov.EntCode; //entcode
            //                                doc.Name = fileSubirDocumentoProveedor.FileName; //filename
            //                                doc.Rut = rutProveedor; //rutproveedor
            //                                switch (prov.TipoProveedor)
            //                                {
            //                                    case 18:
            //                                        doc.ProviderType = "Person";//Tipo Proveedor: NATURAL=PERSON
            //                                        break;
            //                                    case 19:
            //                                        doc.ProviderType = "Entity";//Tipo Proveedor: JURÍDICA - ENTITY
            //                                        break;
            //                                }
            //                                doc.ProviderName = prov.RazonSocial; //Razón Social

            //                                //Byte[] byteFile = File.ReadAllBytes(rutaArchivoAConvertir);
            //                                Byte[] byteFile = System.IO.File.ReadAllBytes(path);
            //                                string base64File = Convert.ToBase64String(byteFile);
            //                                doc.Content = base64File;

            //                                resp = Models.GesintelMethods.RestMethods.SendDocument(token.Token, doc);

            //                                if (resp.status == "OK")
            //                                {
            //                                    MOPArchivoPrivado arch = new MOPArchivoPrivado();
            //                                    string tmpFileName = fileSubirDocumentoProveedor.FileName;
            //                                    int lastDotIndex = tmpFileName.LastIndexOf('.');
            //                                    string soloNombre = tmpFileName.Substring(0, lastDotIndex);
            //                                    /////////////////////////////////////////////////////////////
            //                                    /////////////////////////////////////////////////////////////
            //                                    arch.NomArchivo = soloNombre;
            //                                    //string ext = tmpFileName.Substring(lastDotIndex, tmpFileName.Length); //extensión de archivo
            //                                    string ext = tmpFileName.Substring(lastDotIndex + 1); //extensión de archivo                                                                                                    
            //                                    arch.Tipo = ext;
            //                                    arch.Fecha = DateTime.Now;
            //                                    arch.Descripcion = descripcion;
            //                                    arch.Tamano = (decimal)fileSubirDocumentoProveedor.ContentLength;
            //                                    arch.IdArchivo = long.Parse(resp.Folio);
            //                                    arch.UsuarioIngresoReg = Session["SessionNombreUsuario"].ToString();//identityName -- RUT PROVEEDOR
            //                                    arch.FechaIngresoReg = DateTime.Now;
            //                                    arch.UsuarioModReg = Session["SessionNombreUsuario"].ToString();
            //                                    arch.FechaModReg = DateTime.Now;
            //                                    arch.ParentId = prov.Id;//idProveedor
            //                                    arch.IdInstitucion = 21; //idMandanteMOP
            //                                    arch.FechaInicioVigencia = DateTime.Now;
            //                                    arch.FechaFinVigencia = DateTime.Now.AddYears(1);
            //                                    arch.IdDocSolicitado = 0; //0
            //                                    arch.RutProveedor = rutProveedor;
            //                                    arch.TipoDocumento = "INT";

            //                                    //SE INSERTA REGISTRO EN DB
            //                                    int idArchivoPrivado = SQLMethods.InsertarMOPArchivoPrivado(arch);

            //                                    MOPMetaDatosDocumentos mmd = new MOPMetaDatosDocumentos();
            //                                    mmd.IdDoc = idArchivoPrivado;
            //                                    mmd.VisibleProv = visibleAlProv; //S o N
            //                                    mmd.PalabrasClave = palabrasClave;
            //                                    mmd.Anio = año;
            //                                    mmd.CodTipoDocumento = tipoDoc;
            //                                    mmd.CodAreaAnalisis = areaDoc;
            //                                    mmd.FechaIngresoReg = DateTime.Now;
            //                                    mmd.UsuarioIngresoReg = Session["SessionNombreUsuario"].ToString();

            //                                    //SE REGISTRA EN DB
            //                                    SQLMethods.InsertarMetadatosDocumento(mmd);

            //                                    //AL FINAL, SE ELIMINA EL ARCHIVO DE LA RUTA TEMP
            //                                    if (System.IO.File.Exists(path))
            //                                    {
            //                                        //System.IO.File.Delete(path);
            //                                        System.IO.Directory.Delete(folderPath, true);//con true borra el directorio y todo su contenido
            //                                    }


            //                                }
            //                                else
            //                                {
            //                                    //ViewBag.MsgSubirArchivo = "Error al conectar a Repositorio Gesintel - Error de subida: " + resp.status;
            //                                    return Json(new { success = false, message = "Error al conectar a Repositorio Gesintel - Error de subida: " + resp.status });
            //                                }
            //                            }
            //                            else
            //                            {
            //                                //ViewBag.MsgSubirArchivo = "Error al conectar a Repositorio Gesintel - Token error: " + token.Status;
            //                                return Json(new { success = false, message = "Error al conectar a Repositorio Gesintel - Token error: " + token.Status });
            //                            }

            //                        }
            //                        else
            //                        {
            //                            //ViewBag.MsgSubirArchivo = "Debe indicar las palabras clave";
            //                            return Json(new { success = false, message = "Debe indicar las palabras clave" });
            //                        }
            //                    }
            //                    else
            //                    {
            //                        //ViewBag.MsgSubirArchivo = "Debe indicar un tipo de documento";
            //                        //return View("Index");
            //                        return Json(new { success = false, message = "Debe indicar un tipo de documento" });
            //                    }
            //                }
            //                else
            //                {
            //                    //ViewBag.MsgSubirArchivo = "Debe indicar una descripción";                                
            //                    return Json(new { success = false, message = "Debe indicar una descripción" });
            //                }

            //            }
            //            else
            //            {
            //                //ViewBag.MsgSubirArchivo = "La extensión del archivo debe ser PDF";
            //                //return View("Index");                            
            //                return Json(new { success = false, message = "La extensión del archivo debe ser PDF" });
            //            }


            //        }
            //        else
            //        {
            //            //ViewBag.MsgSubirArchivo = "Debe adjuntar un archivo";                        
            //            return Json(new { success = false, message = "Debe adjuntar un archivo" });
            //        }


            //        string msg = "Archivo Subido Correctamente";
            //        return Json(new { success = false, message = msg });



            //    }
            //    else
            //    {
            //        string msg = "No AJAX Request";
            //        return Json(new { success = false, message = msg });
            //    }





            //}
            //catch (Exception x)
            //{
            //    //ViewBag.MsgSubirArchivo = "File upload failed!!: " + x.InnerException.Message.ToString();                
            //    return Json(new { success = false, message = "File upload failed!!: " + x.InnerException.Message.ToString() });
            //}
        }




        #endregion


        #region LOGS

        //REGISTRO DE LOG


        [ValidateAntiForgeryToken]
        //[CheckAuthorization]
        [HttpPost]
        public JsonResult JsonTraerMOPLog(string rutProveedor, string codTipoLog)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    //string codEstado = "INH"; //PARA TRAER HISTORIAL 
                    var list = db.SP_MOP_TRAER_LOG_PROVEEDOR(rutProveedor, codTipoLog).ToList();
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
        //[CheckAuthorization]
        [HttpPost]
        public JsonResult JsonGuardarMOPLog(string rutProveedor, int idSol, int idDoc, string codEstado, string codTipoOperacion, string comentario1,
                                                string comentario2, string comentario3)
        {
            try
            {
                var jsonResult = new JsonResult();
                string usuario = Session["SessionNombreUsuario"].ToString();
                int resp = SQLMethods.GuardarMOPLog(rutProveedor, idSol, idDoc, codEstado, codTipoOperacion, comentario1, comentario2, comentario3, usuario);
                jsonResult.Data = resp;
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



        

        #region REPORTES Y EXCEL

        [HttpPost]
        public ActionResult ReporteProveedores(string data)
        {
            var map = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
            var file = map["file"];
            var stream = (MemoryStream)downloadExcelStream(data);


            string today = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString();
            //var filename = string.Format("{0}.{1}.csv", "Listado de Solicitudes", today);
            var filename = string.Format("{0} {1}.xlsx", file, today);
            stream.Position = 0;

            return File(stream.ToArray(), "application/octet-stream", filename);
        }


        public Object downloadExcelStream(string data)
        {
            MemoryStream stream = new MemoryStream();
            var map = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
            //var json = map.ContainsKey("base64") ? FuncionesGenerales.base64_to_str(map["base64"]) : "";
            var json = map.ContainsKey("stringDatos") ? map["stringDatos"] : "";
            var name = map["file"];
            //var allRegs = miscelaneos.ListadoRegiones();
            //var allServ = miscelaneos.ListadoServicios();

            using (XLWorkbook wb = new XLWorkbook())
            {
                var lista = JsonConvert.DeserializeObject<List<ProveedoresExcel>>(json);
                int i = 2;
                var ws = wb.Worksheets.Add(name);
                ws.Cell("A1").Value = "RUT";
                ws.Cell("B1").Value = "Razón Social";
                ws.Cell("C1").Value = "Región";
                ws.Cell("D1").Value = "Estado MOP";
                ws.Cell("E1").Value = "Tipo de Registro";
                ws.Cell("F1").Value = "Mes de Renovación";
                ws.Cell("G1").Value = "Fecha Ingreso Registro";
                ws.Range("A1:G1").Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent1, 0.5);
                ws.Range("A1:G1").Style.Font.Bold = true;

                try
                {
                    foreach (var item in lista)
                    {
                        ws.Cell("A" + i).Value = item.Rut;
                        ws.Cell("B" + i).Value = item.RazonSocial;
                        ws.Cell("C" + i).Value = item.Region;
                        ws.Cell("D" + i).Value = item.EstadoMOP;
                        ws.Cell("E" + i).Value = item.TipoRegistro;
                        ws.Cell("F" + i).Value = item.MesRenovacion;
                        ws.Cell("G" + i).Value = item.FechaIngresoReg;
                        i++;
                    }

                    //17 columnas
                    IXLRange range = ws.Range(ws.Cell(1, 1).Address, ws.Cell(i, 7).Address);
                    range.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    range.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    wb.SaveAs(stream);
                }
                catch (Exception e)
                {
                    ws.Cell("A" + i).Value = "ERROR: Ocurrió un problema en la generacion del Excel";
                    ws.Cell("A" + i + 1).Value = e.Message;
                }
            }
            return (Object)stream;
        }




        #endregion









    }
}