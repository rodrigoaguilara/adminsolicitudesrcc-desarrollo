using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AdminSolicitudesContratistas.Models;
using AdminSolicitudesContratistas.Models.GesintelModel;
using AdminSolicitudesContratistas.Models.GesintelMethods;
using AdminSolicitudesContratistas.Models.ViewModels;
using System.IO;


using Newtonsoft.Json;
using System.IO.Compression;
using ClosedXML.Excel;
using System.Configuration;
//using System.IO.Compression.ZipArchive;

namespace AdminSolicitudesContratistas.Controllers
{
    public class SolicitudesController : Controller
    {

        static int staticMaxFileLength = System.Convert.ToInt32(ConfigurationManager.AppSettings.Get("MaxFileLength"));
        // GET: Solicitudes
        [CheckAuthorization]
        public ActionResult Index()
        {
            return View();
        }





        #region BÚSQUEDA DE SOLICITUDES




        [ValidateAntiForgeryToken]
        [CheckAuthorization]
        [HttpPost]
        public JsonResult JsonBuscarSolicitudes(string rutProveedor, int idSolicitud, string tipoSolicitud, string estado, string tipoRegistro, string userRegion, string razonSocial, short ano)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {

                    var list = db.SP_MOP_BUSCAR_SOLICITUDES(rutProveedor.Trim(), idSolicitud, tipoSolicitud, estado, tipoRegistro, userRegion, razonSocial.Trim(), ano).ToList();
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


        #region DATOS DE SOLICITUD



        [ValidateAntiForgeryToken]
        [CheckAuthorization]
        [HttpPost]
        public JsonResult JsonTraerDetalleCompletoSolicitud(int numSolicitud)
        {
            try
            {
                FuncionesGenerales.LogTraceProduccion($"Iniciando carga Solicitud {numSolicitud}");
                var viewModel = new SolicitudDetalleCompletoViewModel();

                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    // 1. Obtener Datos Principales de la Solicitud
                    FuncionesGenerales.LogTraceProduccion("1. Consultando SP_MOP_TRAER_SOLICITUD_POR_ID");
                    var solicitudDb = db.SP_MOP_TRAER_SOLICITUD_POR_ID(numSolicitud).FirstOrDefault();

                    if (solicitudDb == null)
                    {
                        // Si la solicitud no existe, devolvemos un error claro.
                        Response.StatusCode = 404; // Not Found
                        return Json(new { success = false, message = "La solicitud con el N° " + numSolicitud + " no fue encontrada." });
                    }


                    // Mapeamos los datos principales al ViewModel
                    viewModel.Principal = new SolicitudPrincipalViewModel
                    {
                        IdSolPadre = solicitudDb.IdSolPadre ?? 0,
                        RazonSocial = solicitudDb.RazonSocial,
                        RutProveedor = solicitudDb.RutProveedor,
                        EmailProveedor = solicitudDb.EmailProveedor,
                        Comentario = solicitudDb.Comentario,
                        // Usamos el operador ?. para evitar errores si las fechas son nulas
                        FechaIngreso = solicitudDb.FechaIngreso?.ToString("dd/MM/yyyy HH:mm:ss"),
                        FechaRecepcion = solicitudDb.FechaRecepcion.ToString("dd/MM/yyyy HH:mm:ss"),
                        Estado = solicitudDb.Estado,
                        CodEstado = solicitudDb.CodEstado,
                        TipoReg = solicitudDb.TipoReg,
                        TipoSol = solicitudDb.TipoSol,
                        CodTipoSol = solicitudDb.CodTipoSol,
                        CodTipoReg = solicitudDb.CodTipoReg,
                        IdRegion = solicitudDb.IdRegion ?? 0
                    };

                    FuncionesGenerales.LogTraceProduccion("2. Datos principales mapeados OK");

                    // 2. Obtener Estado Actual del Proveedor (reutilizamos la lógica de la llamada original)
                    var estadoProveedorDb = db.SP_MOP_TRAER_ESTADO_PROVEEDOR_POR_TIPO_REG(solicitudDb.RutProveedor, solicitudDb.CodTipoReg).FirstOrDefault();
                    viewModel.Principal.EstadoProveedor = estadoProveedorDb != null ? estadoProveedorDb.ESTADO : "SIN REGISTROS";

                    // 3. Obtener Documentos Adjuntos
                    viewModel.DocumentosAdjuntos = db.SP_MOP_DETALLE_SOLICITUD_POR_TIPO(numSolicitud, "ADJT", "")
                        .Select(d => new DocumentoAdjuntoViewModel
                        {
                            idDet = d.idDet,
                            NumArchivo = d.NumArchivo,
                            NomDoc = d.NomDoc,
                            Comentario = d.Comentario,
                            IdArchivoGesintel = d.IdArchivoGesintel,
                            CodAreaAnalisis = d.CodAreaAnalisis,
                            AreaAnalisis = d.AreaAnalisis,
                            Estado = d.Estado,
                            CodEstado = d.CodEstado
                        }).ToList();

                    // 4. Obtener Log de la Solicitud
                    viewModel.LogSolicitud = db.SP_MOP_TRAER_LOG_SOLICITUD(numSolicitud)
                        .Select(l => new LogSolicitudViewModel
                        {
                            Estado = l.Estado,
                            Fecha = l.Fecha,
                            Comentario_1 = l.Comentario_1,
                            Comentario_2 = l.Comentario_2,
                            Comentario_3 = l.Comentario_3,
                            CuentaUsuario = l.CuentaUsuario
                        }).ToList();

                    // 5. Obtener Documentos de Resolución (Replicando la lógica con Gesintel)
                    var resolucionesDb = db.SP_MOP_TRAER_DOCUMENTO_RESOLUCION_INTERNO_MOP(numSolicitud).ToList();
                    if (resolucionesDb.Any())
                    {
                        FuncionesGenerales.LogTraceProduccion($"3. Procesando {resolucionesDb.Count} resoluciones. Llamando a Gesintel Token.");
                        AuthenticateResponse token = Token.gesintelLoginToken(); // Un solo token para todas las validaciones
                        if (token == null) FuncionesGenerales.LogTraceProduccion("¡ADVERTENCIA! El token de Gesintel volvió nulo.");
                        foreach (var res in resolucionesDb)
                        {
                            FuncionesGenerales.LogTraceProduccion($"4. Validando archivo ID: {res.IdArchivoGesintel}");
                            string valGesintel = ValidarArchivoEnGesintel((int)res.IdArchivoGesintel, solicitudDb.RutProveedor, token);
                            viewModel.DocumentosResolucion.Add(new DocumentoResolucionViewModel
                            {
                                FechaNota = res.FechaNota,
                                AreaAnalisis = res.AreaAnalisis,
                                Comentario = res.Comentario,
                                NomArchivo = res.NomArchivo,
                                IdArchivoGesintel = res.IdArchivoGesintel,
                                DisponibleEnGesintel = (valGesintel == "OK") ? 1 : 0
                            });
                        }
                    }

                    // 6. Obtener Observaciones
                    viewModel.Observaciones = db.SP_MOP_TRAER_OBSERVACIONES_SOLICITUD(numSolicitud)
                        .Select(o => new ObservacionViewModel
                        {
                            AreaAnalisis = o.AreaAnalisis,
                            FechaNota = o.FechaNota,
                            Comentario = o.Comentario
                        }).ToList();

                    // 7. Obtener Solicitudes Relacionadas (si tiene una solicitud padre)
                    int idPadre = viewModel.Principal.IdSolPadre;
                    if (idPadre > 0)
                    {
                        viewModel.SolicitudesRelacionadas = db.SP_MOP_BUSCA_SOL_RELACIONADAS(idPadre)
                           .Select(sr => new SolicitudRelacionadaViewModel
                           {
                               ID = sr.ID,
                               FechaCreacion = sr.FechaCreacion,
                               TipoSolRel = sr.TipoSolRel
                           }).ToList();
                    }
                }

                FuncionesGenerales.LogTraceProduccion("5. Todo el proceso terminó con éxito.");

                return Json(viewModel);
            }
            catch (Exception ex)
            {
                // Es una buena práctica registrar el error para poder depurarlo.
                // Logger.LogError(ex, "Error en JsonTraerDetalleCompletoSolicitud");
                FuncionesGenerales.LogErrorProduccion("JsonTraerDetalleCompletoSolicitud", numSolicitud, ex);
                Response.StatusCode = 500;
                //return Json(new { success = false, message = "Ocurrió un error en el servidor al obtener el detalle de la solicitud." });
                return Json(new { success = false, message = ex.Message });
            }
        }



        [ValidateAntiForgeryToken]
        [CheckAuthorization]
        [HttpPost]
        public JsonResult JsonTraerDatosSolicitud(int numSolicitud)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    //Session["numSolicitudVista"] = numSolicitud;
                    MOPSolicitud sol = new MOPSolicitud();
                    var list = db.SP_MOP_TRAER_SOLICITUD_POR_ID(numSolicitud).ToList();
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
        [HttpPost]
        public JsonResult JsonTraerDocumentosAdjuntosSolicitud(int numSolicitud, string rutProveedor)
        {
            try
            {

                var listRep = TraerDetalleSolicitudRepositorio(numSolicitud);
                var jsonResult = new JsonResult();
                int cantRep = listRep.Count;
                DocumentosResolucionProveedorRespSP doc = new DocumentosResolucionProveedorRespSP();

                #region antigua validación (OPERATIVA)

                if (cantRep > 0)
                {
                    jsonResult.Data = listRep;
                }
                else
                {
                    jsonResult.Data = "EMPTY";
                }
                #endregion


                #region nueva validación
                //AuthenticateResponse token = Token.gesintelLoginToken();

                //foreach (var item in listRep)
                //{
                //    int id = int.Parse(item.IdArchivoGesintel.ToString());
                //    string valGesintel = ValidarArchivoEnGesintel(id, rutProveedor, token);

                //    if (valGesintel == "OK")
                //    {
                //        item.DisponibleEnGesintel = 1;
                //    }
                //    else
                //    {
                //        doc.DisponibleEnGesintel = 0;
                //    }
                //}

                //if (cantRep > 0)
                //{
                //    jsonResult.Data = listRep;
                //}
                //else
                //{
                //    jsonResult.Data = "EMPTY";
                //}

                #endregion


                jsonResult.MaxJsonLength = Int32.MaxValue;
                jsonResult.ContentType = "application/json";
                return jsonResult;

            }
            catch (Exception)
            {

                throw;
            }

        }

        private List<SP_MOP_DETALLE_SOLICITUD_POR_TIPO_Result> TraerDetalleSolicitudRepositorio(int idSolicitud)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    var listRep = db.SP_MOP_DETALLE_SOLICITUD_POR_TIPO(idSolicitud, "ADJT", "").ToList();
                    //List<MOPDetalleSolicitudTMP> list = new List<MOPDetalleSolicitudTMP>();
                    return listRep;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JsonTraerLogSolicitud(int numSolicitud)
        {
            try
            {

                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    //Session["numSolicitudVista"] = numSolicitud;
                    //MOPSolicitud sol = new MOPSolicitud();
                    var list = db.SP_MOP_TRAER_LOG_SOLICITUD(numSolicitud).ToList();
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
        [HttpPost]
        public JsonResult JsonTraerObservacionesSolicitud(int numSolicitud)
        {
            try
            {

                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    //Session["numSolicitudVista"] = numSolicitud;
                    //MOPSolicitud sol = new MOPSolicitud();
                    var list = db.SP_MOP_TRAER_OBSERVACIONES_SOLICITUD(numSolicitud).ToList();
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
        [HttpPost]
        public JsonResult JsonTraerDocumentoResolucion(int numSolicitud, string rutProveedor)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    //MOPSolicitud sol = new MOPSolicitud();
                    //var list = db.SP_MOPFindDocumentoResolucion(IdSolicitud).ToList();
                    var list = db.SP_MOP_TRAER_DOCUMENTO_RESOLUCION_INTERNO_MOP(numSolicitud).ToList();
                    var jsonResult = new JsonResult();
                    int cant = list.Count;
                    DocumentosResolucionProveedorRespSP doc = new DocumentosResolucionProveedorRespSP();
                    //string rutProv = Session["SessionRutProveedor"]?.ToString();
                    AuthenticateResponse token = Token.gesintelLoginToken();
                    foreach (var item in list)
                    {
                        int id = int.Parse(item.IdArchivoGesintel.ToString());
                        string valGesintel = ValidarArchivoEnGesintel(id, rutProveedor, token);

                        if (valGesintel == "OK")
                        {
                            item.DisponibleEnGesintel = 1;
                        }
                        else
                        {
                            doc.DisponibleEnGesintel = 0;
                        }

                    }


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
        [HttpPost]
        public JsonResult JsonTraerSolicitudesRelacionadas(int IdSolicitud)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    MOPSolicitud sol = new MOPSolicitud();
                    var list = db.SP_MOP_BUSCA_SOL_RELACIONADAS(IdSolicitud).ToList();
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


        #region DESCARGA DE ARCHIVOS

        [CheckAuthorization]
        [HttpGet]
        public ActionResult BajarArchivoGesintel(int idArchivoGesintel, string rutProveedor)
        {
            string contexto = $"BajarArchivoGesintel(ID: {idArchivoGesintel}, RUT: {rutProveedor})";
            try
            {
                FuncionesGenerales.LogTraceProduccion($"{contexto} - ---INICIO DESCARGA DE ARCHIVOS---");

                byte[] resp;
                FuncionesGenerales.LogTraceProduccion($"{contexto} - Solicitando Token Gesintel");

                try
                {
                    AuthenticateResponse token = Token.gesintelLoginToken();

                    if (token == null || string.IsNullOrEmpty(token.Token))
                    {
                        FuncionesGenerales.LogTraceProduccion($"{contexto} - ERROR: Token obtenido es nulo o vacío");
                        return Json(new { success = false, message = "Error de autenticación con el repositorio de archivos." }, JsonRequestBehavior.AllowGet);
                    }

                    // Lógica de búsqueda de Folio (MOP / HIST)
                    FuncionesGenerales.LogTraceProduccion($"{contexto} - Buscando nuevo folio (MOP/HIST)");
                    GetDocumentsByTypePayload mopPayload = new GetDocumentsByTypePayload { Folio = idArchivoGesintel, Type = "MOP" };
                    GetDocumentsByTypePayload histPayload = new GetDocumentsByTypePayload { Folio = idArchivoGesintel, Type = "HIST" };

                    int nuevoIdArchivoGesintel = TraerNuevoFolioGesintel(mopPayload, rutProveedor, token);
                    if (nuevoIdArchivoGesintel == 0)
                    {
                        FuncionesGenerales.LogTraceProduccion($"{contexto} - No hallado en MOP, buscando en HIST");
                        nuevoIdArchivoGesintel = TraerNuevoFolioGesintel(histPayload, rutProveedor, token);

                        if (nuevoIdArchivoGesintel == 0)
                        {
                            FuncionesGenerales.LogTraceProduccion($"{contexto} - No hallado en HIST, usando ID original");
                            nuevoIdArchivoGesintel = idArchivoGesintel;
                        }
                    }
                    FuncionesGenerales.LogTraceProduccion($"{contexto} - Folio final a descargar: {nuevoIdArchivoGesintel}");

                    // Descarga del archivo físico
                    FuncionesGenerales.LogTraceProduccion($"{contexto} - Llamando a GetFileDocument");
                    GetFileDocumentPayload doc = new GetFileDocumentPayload { Folio = nuevoIdArchivoGesintel };
                    resp = RestMethods.GetFileDocument(token.Token, doc);

                    if (resp == null || resp.Length == 0)
                    {
                        FuncionesGenerales.LogTraceProduccion($"{contexto} - ERROR: El archivo descargado está vacío (0 bytes)");
                    }
                    else
                    {
                        FuncionesGenerales.LogTraceProduccion($"{contexto} - Archivo descargado OK. Tamaño: {resp.Length} bytes");
                    }

                    // Obtener metadatos del archivo
                    FuncionesGenerales.LogTraceProduccion($"{contexto} - Solicitando metadatos (BajarInfoArchivoAdjunto)");
                    GetDocumentResponse infoDoc = BajarInfoArchivoAdjunto(nuevoIdArchivoGesintel, rutProveedor);

                    if (infoDoc == null || infoDoc.DocumentProp == null)
                    {
                        FuncionesGenerales.LogTraceProduccion($"{contexto} - ERROR: No se pudo obtener info del documento o DocumentProp es nulo");
                        return Json(new { success = false, message = "No se pudo obtener la información del archivo." }, JsonRequestBehavior.AllowGet);
                    }

                    // Validación de RUT (Seguridad)
                    string rutGesintel = infoDoc.DocumentProp.ProviderProp?.Rut?.ToLower() ?? "";
                    string rutProvFormat = rutProveedor.ToLower().Replace("-", "");
                    FuncionesGenerales.LogTraceProduccion($"{contexto} - Validando RUT. Esperado: {rutProvFormat}, Recibido: {rutGesintel}");

                    if (infoDoc.Status == "ERROR" || rutGesintel != rutProvFormat)
                    {
                        FuncionesGenerales.LogTraceProduccion($"{contexto} - ERROR VALIDACIÓN: RUT no coincide o Status ERROR");
                        return Json(new { success = false, message = "El archivo no pertenece al proveedor solicitado o no se encuentra disponible." }, JsonRequestBehavior.AllowGet);
                    }

                    string nombreArchivo = infoDoc.DocumentProp.Name;
                    FuncionesGenerales.LogTraceProduccion($"{contexto} - Preparando descarga de: {nombreArchivo}");

                    return File(resp, System.Net.Mime.MediaTypeNames.Application.Octet, nombreArchivo);



                }
                catch (Exception t)
                {
                    Exception realError = t;
                    while (realError.InnerException != null) realError = realError.InnerException;

                    string msgDetallado = $"Excepción en Token: {t.Message} | Detalle Real: {realError.Message}";

                    FuncionesGenerales.LogTraceProduccion($"{contexto} - {msgDetallado}");
                    FuncionesGenerales.LogErrorProduccion(contexto, idArchivoGesintel, t);
                    throw;
                }
                
                

                

                
            }
            catch (Exception x)
            {
                // IMPORTANTE: Loguear el error real con el método que hicimos antes
                FuncionesGenerales.LogErrorProduccion(contexto, idArchivoGesintel, x);

                // No devolver null, devolver un error que el usuario o la consola puedan entender
                Response.StatusCode = 500;
                return Json(new { success = false, message = "Excepción interna: " + x.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        //[CheckAuthorization]
        //[HttpGet]
        //public ActionResult BajarArchivoGesintel(int idArchivoGesintel, string rutProveedor)
        //{
        //    try
        //    {
        //        byte[] resp;
        //        AuthenticateResponse token = Token.gesintelLoginToken();
        //        //VALIDACIÓN DE ARCHIVO EN REPOSITORIO MOP E HISTÓRICO
        //        GetDocumentsByTypePayload mopPayload = new GetDocumentsByTypePayload();
        //        mopPayload.Folio = idArchivoGesintel;
        //        mopPayload.Type = "MOP";

        //        GetDocumentsByTypePayload histPayload = new GetDocumentsByTypePayload();
        //        histPayload.Folio = idArchivoGesintel;
        //        histPayload.Type = "HIST";

        //        int nuevoIdArchivoGesintel = 0;
        //        //Se busca primero en el repositorio MOP
        //        nuevoIdArchivoGesintel = TraerNuevoFolioGesintel(mopPayload, rutProveedor, token);
        //        if (nuevoIdArchivoGesintel == 0)
        //        {
        //            nuevoIdArchivoGesintel = TraerNuevoFolioGesintel(histPayload, rutProveedor, token);
        //            //IMPLEMENTAR VALIDACION DE FOUND
        //            //if (nuevoIdArchivoGesintel == 0 && found == false) {
        //            if (nuevoIdArchivoGesintel == 0)
        //            {
        //                nuevoIdArchivoGesintel = idArchivoGesintel;
        //            }
        //        }

        //        GetFileDocumentPayload doc = new GetFileDocumentPayload();
        //        //doc.Folio = idArchivoGesintel;
        //        doc.Folio = nuevoIdArchivoGesintel;
        //        resp = RestMethods.GetFileDocument(token.Token, doc);

        //        //INFO DE ARCHIVO A BAJAR
        //        GetDocumentResponse infoDoc = new GetDocumentResponse();
        //        infoDoc = BajarInfoArchivoAdjunto(nuevoIdArchivoGesintel, rutProveedor);
        //        string nombreArchivo = "";
        //        //if (rutProveedor.StartsWith("0"))
        //        //{
        //        //    rutProveedor = rutProveedor.Substring(1, rutProveedor.Length - 1);
        //        //}
        //        //Validación para archivos que Gesintel tiene con RUT diferentes, esos archivos se marcarán como no existentes
        //        string rutGesintel = infoDoc.DocumentProp.ProviderProp.Rut.ToLower();
        //        string rutProvFormat = rutProveedor.ToLower().Replace("-", "");

        //        if (infoDoc.Status == "ERROR" || rutGesintel != rutProvFormat)
        //        {
        //            //resp = new byte[0];                    
        //            //ViewBag.Message = "Error al intentar descargar: No se encuentra archivo";
        //            //return HttpNotFound("Error al intentar descargar: No se encuentra archivo");
        //            return Json(new { success = false, message = "Error al intentar descargar: El archivo no se encuentra en el repositorio" }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            nombreArchivo = infoDoc.DocumentProp.Name;
        //        }

        //        //return File(resp, System.Net.Mime.MediaTypeNames.Application.Octet, nombreArchivo);
        //        // Set Content-Disposition header to force download
        //        //Response.AddHeader("Content-Disposition", "attachment; filename=" + nombreArchivo);
        //        return File(resp, System.Net.Mime.MediaTypeNames.Application.Octet, nombreArchivo);

        //    }
        //    catch (Exception x)
        //    {
        //        return null;
        //    }

        //}


        //[CheckAuthorization]
        [HttpPost]
        public ActionResult DescargaMasivaArchivosGesintel(string jsonArchivosGesintel, string rutProveedor, string numSolicitud, string nombreTipoDescarga)
        {
            string folderPath = "";

            try
            {
                
                //Se genera un único Token para la descarga
                AuthenticateResponse token = Token.gesintelLoginToken();
                ///////////////////////////////////
                int idArchivoGesintel = 0;
                string fechaTmp = DateTime.Now.ToString("ddMMyyyyHHmmss");
                string rutaDescargasTmp = Server.MapPath("~/Content/TempCompressedFiles/");
                // 1. Limpiamos basura de solicitudes anteriores (de otros usuarios/días)
                LimpiarArchivosAntiguos(rutaDescargasTmp);
                //string folder = rutProveedor.Replace("-", "_") + "_" + numSolicitud + "_" + fechaTmp;
                string folderName = rutProveedor.Replace("-", "_") + "_" + numSolicitud + "_" + fechaTmp;
                folderPath = Path.Combine(rutaDescargasTmp, folderName);
                //string path = Path.Combine(Server.MapPath("~/Content/TempUploadedFiles/"), folder, fileName);
                //SE CREA DIRECTORIO TMP 
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                ///////////////////////////////////
                byte[] resp;
                byte[] zippedFile = new byte[0];
                byte[] downloadedFile;
                List<string> listDocs = JsonConvert.DeserializeObject<List<string>>(jsonArchivosGesintel);
                foreach (var item in listDocs)
                {
                    idArchivoGesintel = int.Parse(item);
                    //VALIDACION DE DOCUMENTO DE HISTORICOS Y MOP
                    GetDocumentsByTypePayload mopPayload = new GetDocumentsByTypePayload();
                    mopPayload.Folio = idArchivoGesintel;
                    mopPayload.Type = "MOP";

                    GetDocumentsByTypePayload histPayload = new GetDocumentsByTypePayload();
                    histPayload.Folio = idArchivoGesintel;
                    histPayload.Type = "HIST";

                    int nuevoIdArchivoGesintel = 0;
                    //Se busca primero en el repositorio MOP
                    nuevoIdArchivoGesintel = TraerNuevoFolioGesintel(mopPayload, rutProveedor, token);
                    if (nuevoIdArchivoGesintel == 0)
                    {
                        nuevoIdArchivoGesintel = TraerNuevoFolioGesintel(histPayload, rutProveedor, token);
                        //IMPLEMENTAR VALIDACION DE FOUND
                        //if (nuevoIdArchivoGesintel == 0 && found == false) {
                        if (nuevoIdArchivoGesintel == 0)
                        {
                            nuevoIdArchivoGesintel = idArchivoGesintel;
                        }
                    }





                    GetFileDocumentPayload doc = new GetFileDocumentPayload();
                    doc.Folio = nuevoIdArchivoGesintel;
                    downloadedFile = RestMethods.GetFileDocument(token.Token, doc);


                    //INFO DE ARCHIVO A BAJAR
                    GetDocumentResponse infoDoc = new GetDocumentResponse();
                    infoDoc = BajarInfoArchivoAdjunto(nuevoIdArchivoGesintel, rutProveedor);
                    string nombreArchivo = "";
                    if (infoDoc.Status == "OK")
                    {
                        nombreArchivo = infoDoc.DocumentProp.Name;
                        string fullPathArchivo = Path.Combine(folderPath, nombreArchivo);
                        System.IO.File.WriteAllBytes(folderPath + "/" + nombreArchivo, downloadedFile);
                    }
                    else
                    {
                        /////////////
                    }


                }


                string zippedFileName = "Archivos Solicitud " + numSolicitud + " " + DateTime.Now.ToString("dd-MM-yyyy") + " - " + nombreTipoDescarga + ".zip";
                //string rutaZipped = Path.Combine(rutaDescargasTmp, zippedFileName);

                byte[] zipBytes = GenerateZipFile(folderPath);
                //if (System.IO.Directory.Exists(folderPath))
                //{
                //    System.IO.Directory.Delete(folderPath, true);//con true borra el directorio y todo su contenido
                //}
                return File(zipBytes, "application/zip", zippedFileName);


            }
            catch (Exception x)
            {
                return Json(new { success = false, message = "Error al intentar descargar: " + x.Message });
            }
            finally {
                try
                {
                    if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
                    {
                        Directory.Delete(folderPath, true);
                    }
                }
                catch (Exception ex)
                {
                    // Opcional: Loguear si falló el borrado (ej. por permisos)
                    //Debug.WriteLine("No se pudo limpiar la carpeta temporal: " + ex.Message);
                }
            }    

        }


        public byte[] GenerateZipFile(string folderPath)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (ZipArchive zip = new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
                    foreach (string file in Directory.GetFiles(folderPath))
                    {
                        string entryName = Path.GetFileName(file);
                        zip.CreateEntryFromFile(file, entryName);
                    }
                }

                stream.Seek(0, SeekOrigin.Begin);
                return stream.ToArray();
            }
        }

        private void LimpiarArchivosAntiguos(string rutaBase)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(rutaBase);
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    // Si la carpeta tiene más de 1 día de antigüedad, la borramos
                    if (dir.CreationTime < DateTime.Now.AddDays(-1))
                    {
                        dir.Delete(true);
                    }
                }
            }
            catch { /* Silencioso para no interrumpir el flujo principal */ }
        }


        public GetDocumentResponse BajarInfoArchivoAdjunto(int idArchivoGesintel, string rutProveedor)
        {
            try
            {
                AuthenticateResponse token = Token.gesintelLoginToken();
                GetDocumentResponse resp = new GetDocumentResponse();
                GetDocumentPayload doc = new GetDocumentPayload();
                //SendDocumentPayload doc2 = new SendDocumentPayload();
                //idDoc = 1001151;
                doc.Folio = idArchivoGesintel;
                resp = RestMethods.GetDocument(token.Token, doc);
                var jsonResult = new JsonResult();

                return resp;

            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public GetDocumentsByTypeResponse.Root BajarInfoArchivoAdjuntoGetDocumentByType(GetDocumentsByTypePayload doc, string rutProveedor, AuthenticateResponse token)
        {
            try
            {
                GetDocumentsByTypeResponse.Root resp = new GetDocumentsByTypeResponse.Root();
                resp = RestMethods.GetDocumentByType(token.Token, doc);

                if (resp.documents.Count > 0)
                {
                    foreach (GetDocumentsByTypeResponse.Document arch in resp.documents)
                    {
                        if (rutProveedor == arch.record.rut.ToUpper())
                        {

                        }
                    }
                }
                return resp;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public int TraerNuevoFolioGesintel(GetDocumentsByTypePayload doc, string rutProveedor, AuthenticateResponse token)
        {
            try
            {
                int nuevoFolio = 0;
                GetDocumentsByTypeResponse.Root resp = new GetDocumentsByTypeResponse.Root();
                resp = RestMethods.GetDocumentByType(token.Token, doc);

                if (resp.documents.Count > 0)
                {
                    foreach (GetDocumentsByTypeResponse.Document arch in resp.documents)
                    {
                        rutProveedor = rutProveedor.Replace("-", "").Replace(".", "");

                        if (rutProveedor.StartsWith("0"))
                        {
                            rutProveedor = rutProveedor.Substring(1, rutProveedor.Length - 1);
                        }
                        //(rutProveedor.Length < 9)? 

                        if (rutProveedor == arch.record.rut.ToUpper())
                        {
                            nuevoFolio = arch.folio;
                        }
                    }
                }
                return nuevoFolio;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public GetDocumentResponse BajarInfoArchivoAdjunto_Validacion(int idArchivoGesintel, string rutProveedor, AuthenticateResponse token)
        {
            try
            {
                GetDocumentResponse resp = new GetDocumentResponse();
                GetDocumentPayload doc = new GetDocumentPayload();
                doc.Folio = idArchivoGesintel;
                resp = RestMethods.GetDocument(token.Token, doc);
                var jsonResult = new JsonResult();

                return resp;

            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public string ValidarArchivoEnGesintel(int idArchivoGesintel, string rutProveedor, AuthenticateResponse token)
        {
            try
            {
                //INFO DE ARCHIVO A BAJAR
                GetDocumentResponse infoDoc = new GetDocumentResponse();
                infoDoc = BajarInfoArchivoAdjunto_Validacion(idArchivoGesintel, rutProveedor, token);
                string resp = "OK";
                if (infoDoc.Status != "OK")
                {
                    resp = "FAIL";
                }
                return resp;
            }
            catch (Exception)
            {

                throw;
            }

        }



        #endregion


        #region FILTROS Y PARÁMETROS


        //MÉTODOS PARA FILTROS DE SOLICITUDES
        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult JsonTraerTipoRegistroMOP()
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    //MOPSolicitud sol = new MOPSolicitud();
                    int idPadre = 2237; //tipo de Registro 

                    var list = db.SP_MOP_LISTAR_PARAMETROS_POR_TIPO(idPadre).ToList();
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
        [HttpPost]
        public JsonResult JsonTraerFiltrosBusquedaSolicitudes(string userTipoRegistro, string userTipoSolicitud)
        {
            try
            {
                var viewModel = new FiltrosSolicitudViewModel();
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    // 1. Cargar Tipos de Registro
                    viewModel.TiposRegistro = db.SP_MOP_SOL_LISTAR_FILTRO_TIPO_REGISTRO(userTipoRegistro)
                        .Select(p => new FiltroItemSolViewModel { CODIGO = p.CODIGO, DESCRIPCION = p.DESCRIPCION })
                        .ToList();

                    // 2. Cargar Tipos de Solicitud
                    viewModel.TiposSolicitud = db.SP_MOP_SOL_LISTAR_FILTRO_TIPO_SOLICITUD(userTipoSolicitud)
                        .Select(p => new FiltroItemSolViewModel { CODIGO = p.CODIGO, DESCRIPCION = p.DESCRIPCION })
                        .ToList();

                    // 3. Cargar Estados de Solicitud
                    int codigoPadreEstado = 2230;
                    viewModel.EstadosSolicitud = db.SP_MOP_LISTAR_PARAMETROS_POR_TIPO(codigoPadreEstado)
                        .Select(p => new FiltroItemSolViewModel { CODIGO = p.CODIGO, DESCRIPCION = p.DESCRIPCION })
                        .ToList();
                }
                return Json(viewModel);
            }
            catch (Exception)
            {
                Response.StatusCode = 500;
                return Json(new { success = false, message = "Error al cargar los filtros." });
            }
        }

        





        [ValidateAntiForgeryToken]
        //[CheckAuthorization]
        [HttpPost]
        public JsonResult JsonListarParametrosPorTipo(int codigoPadre)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    MOPSolicitud sol = new MOPSolicitud();
                    var list = db.SP_MOP_LISTAR_PARAMETROS_POR_TIPO(codigoPadre).ToList();
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
        public JsonResult JsonListarFiltroTipoRegistro(string codTipoRegistro)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    MOPSolicitud sol = new MOPSolicitud();
                    var list = db.SP_MOP_SOL_LISTAR_FILTRO_TIPO_REGISTRO(codTipoRegistro).ToList();
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
        public JsonResult JsonListarFiltroTipoSolicitud(string codTipoSolicitud)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    MOPSolicitud sol = new MOPSolicitud();
                    var list = db.SP_MOP_SOL_LISTAR_FILTRO_TIPO_SOLICITUD(codTipoSolicitud).ToList();
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


        #region REPORTES Y EXPORTAR A EXCEL


        [HttpPost]
        public ActionResult ReporteSolicitudes(string data)
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
                var lista = JsonConvert.DeserializeObject<List<SolicitudesExcel>>(json);
                int i = 2;
                var ws = wb.Worksheets.Add(name);
                ws.Cell("A1").Value = "N° Solicitud";
                ws.Cell("B1").Value = "Solicitud padre";
                ws.Cell("C1").Value = "RUT";
                ws.Cell("D1").Value = "Razón Social";
                ws.Cell("E1").Value = "Estado";
                ws.Cell("F1").Value = "Tipo de Solicitud";
                ws.Cell("G1").Value = "Tipo de Registro";
                ws.Cell("H1").Value = "Región";
                ws.Cell("I1").Value = "Fecha Recepción";
                ws.Cell("J1").Value = "Fecha Envío a Análisis";//fecha ingreso
                ws.Cell("K1").Value = "Fecha Análisis Técnico";
                ws.Cell("L1").Value = "Estado Análisis Técnico";
                ws.Cell("M1").Value = "Fecha Análisis Legal";
                ws.Cell("N1").Value = "Estado Análisis Legal";
                ws.Cell("O1").Value = "Fecha Análisis Contable";
                ws.Cell("P1").Value = "Estado Análisis Contable";
                ws.Cell("Q1").Value = "Fecha Respuesta";
                //ws.Cell("Q1").Value = "Días desde Ingreso";
                ws.Range("A1:Q1").Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent1, 0.5);
                ws.Range("A1:Q1").Style.Font.Bold = true;

                try
                {
                    foreach (var item in lista)
                    {
                        ws.Cell("A" + i).Value = item.ID;
                        ws.Cell("B" + i).Value = item.ID_SOL_PADRE;
                        ws.Cell("C" + i).Value = item.RUT;
                        ws.Cell("D" + i).Value = item.RAZON_SOCIAL;
                        ws.Cell("E" + i).Value = item.ESTADO;
                        ws.Cell("F" + i).Value = item.TIPO_SOL;
                        ws.Cell("G" + i).Value = item.TIPO_REG;
                        ws.Cell("H" + i).Value = item.REGION;
                        ws.Cell("I" + i).Value = item.FECHA_RECEPCION;
                        ws.Cell("J" + i).Value = item.FECHA_INGRESO;
                        ws.Cell("K" + i).Value = item.FECHA_ANALISIS_TEC;
                        ws.Cell("L" + i).Value = item.ESTADO_ANALISIS_TEC;
                        ws.Cell("M" + i).Value = item.FECHA_ANALISIS_LEG;
                        ws.Cell("N" + i).Value = item.ESTADO_ANALISIS_LEG;
                        ws.Cell("O" + i).Value = item.FECHA_ANALISIS_CONTABLE;
                        ws.Cell("P" + i).Value = item.ESTADO_ANALISIS_CONTABLE;
                        ws.Cell("Q" + i).Value = item.FECHA_RESPUESTA;
                        //ws.Cell("Q" + i).Value = item.DIAS_DESDE_INGRESO;
                        i++;
                    }

                    //17 columnas
                    IXLRange range = ws.Range(ws.Cell(1, 1).Address, ws.Cell(i, 17).Address);
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




        #region GUARDAR SOLICITUD

        //GUARDAR-PASAR DE ESTADO SOLICITUD CON SUBIDA DE ARCHIVO
        [CheckAuthorization]
        [HttpPost]
        public ActionResult GuardarSolicitud(int hiddenIdSolicitud, int hiddenIdSolPadre, string hiddenCodPerfilSolicitud, string hiddenCodEstadoSolicitud,
            string hiddenCodTipoSolicitud, HttpPostedFileBase fileSubirDocResolucion, string txtComentariosAceptarSolicitud, string cmbAccionesSolicitud,
            string hiddenRutProveedor, string hiddenListDocs, string hiddenEmailProveedor, string hiddenCodTipoRegistro, string hiddenFechaRecepcion, int hiddenCodRegion)
        {
            try
            {
                string funcionarioMOP = Session["SessionNombreUsuario"].ToString();
                if (Request.IsAjaxRequest())
                {
                    int numSolicitud = hiddenIdSolicitud;
                    int numSolicitudPadre = hiddenIdSolPadre;
                    string accion = cmbAccionesSolicitud;
                    string rutProveedor = hiddenRutProveedor;
                    string codPerfil = hiddenCodPerfilSolicitud;
                    string codEstadoSolicitud = hiddenCodEstadoSolicitud;
                    string codTipoSolicitud = hiddenCodTipoSolicitud;
                    string comentariosAnalista = txtComentariosAceptarSolicitud;
                    string nombreAreaAnalisis = "";
                    string respCorreo = "";
                    List<SP_MOP_DETALLE_SOLICITUD_POR_TIPO_Result> listAdjuntosResultadoAnalisis = TraerDetalleSolicitudRepositorio(numSolicitud);

                    //ASIGNACIÓN NOMBRE DEL AREA
                    switch (codPerfil)
                    {
                        case "ALEG":
                        case "LEG":
                            nombreAreaAnalisis = "Legal";
                            break;
                        case "ATEC":
                        case "TEC":
                        case "AREG":
                            nombreAreaAnalisis = "Técnica";
                            break;
                        case "ACON":
                        case "CON":
                            nombreAreaAnalisis = "Contable";
                            break;
                    }
                    string codPerfilTmp = "";
                    string codAreaAnalisisTmp = "";

                    if (codPerfil == "AREG" && accion == "ACP")//Sólo le cambia el perfil desde AREG a ATEC cuando vaya a responder analisis
                    {
                        codPerfilTmp = "ATEC";
                        codAreaAnalisisTmp = codPerfilTmp.Substring(1, codPerfilTmp.Length - 1);
                    }
                    else if (codPerfil != "ADM")
                    {
                        //if (codPerfil == "AREG" && (accion == "ACE" || accion == "CAL")) {
                        if ((codPerfil == "AREG" || codPerfil == "JREG") && (accion == "ACE" || accion == "CAL"))
                        {
                            codAreaAnalisisTmp = codPerfil;
                        }
                        else
                        {
                            codAreaAnalisisTmp = codPerfil.Substring(1, codPerfil.Length - 1);
                        }
                    }
                    else
                    {
                        codAreaAnalisisTmp = codPerfil;
                    }

                    ProveedorView prov = new ProveedorView();
                    prov = SQLMethods.TraerDatosProveedor(rutProveedor);

                    AuthenticateResponse token = Models.GesintelMethods.Token.gesintelLoginToken();



                    string msgRespuesta = "";
                    switch (accion)
                    {
                        case "ACP"://ACEPTAR ANÁLISIS VA CON DOC
                            #region RESPONDER ANÁLISIS

                            msgRespuesta = "Aceptado por área de análisis";
                            //VALIDACIÓN DOCUMENTO ADJUNTO
                            if (fileSubirDocResolucion != null)
                            {
                                if (fileSubirDocResolucion.FileName.Length > staticMaxFileLength)
                                {
                                    string msg2;
                                    msg2 = "El nombre del archivo no puede superar el largo máximo de " + staticMaxFileLength + " caracteres";
                                    return Json(new { success = false, message = msg2 });
                                }
                                else
                                {
                                    //SUBIDA DE ARCHIVO
                                    string fechaTmp = DateTime.Now.ToString("ddMMyyyyHHmmss");
                                    string fileName = Path.GetFileName(fileSubirDocResolucion.FileName);
                                    string folder = rutProveedor.Replace("-", "_") + "_" + fechaTmp;
                                    string folderPath = Path.Combine(Server.MapPath("~/Content/TempUploadedFilesSolicitudes/"), folder);
                                    string path = Path.Combine(Server.MapPath("~/Content/TempUploadedFilesSolicitudes/"), folder, fileName);

                                    //SE GUARDA ARCHIVO EN RUTA TMP PARA LUEGO SUBIRLO A GESINTEL, Y LUEGO SE ELIMINA
                                    Directory.CreateDirectory(folderPath);
                                    try
                                    {
                                        fileSubirDocResolucion.SaveAs(path);
                                    }
                                    catch (Exception x)
                                    {
                                        msgRespuesta = "Error al procesar el archivo antes de subir: " + x.InnerException.Message.ToString();
                                        return Json(new { success = false, message = msgRespuesta });
                                    }



                                    if (token.Status == "OK")
                                    {
                                        //ACTUALIZACIÓN DE AREA DE ESTADO EN MOPDetalleSolicitud

                                        foreach (var item in listAdjuntosResultadoAnalisis)
                                        {
                                            //Se integra validación con códigos nuevos y códigos antiguos de áreas de analisis 
                                            //NUEVO: ATEC (Analista Tecnico)
                                            //VIEJO: TEC

                                            if (item.CodAreaAnalisis == codPerfil || "A" + item.CodAreaAnalisis == codPerfil || "A" + item.CodAreaAnalisis == codPerfilTmp)
                                            {
                                                MOPDetalleSolicitud detSol = new MOPDetalleSolicitud();
                                                detSol.ID = item.idDet;
                                                detSol.Comentario = comentariosAnalista;//
                                                detSol.CodEstado = "RES";
                                                detSol.CodAreaAnalisis = codAreaAnalisisTmp;
                                                SQLSolicitudesMOP.ActualizarEstadoDetalleSolicitud(detSol);
                                            }
                                            //Se actualiza detalle de solicitud

                                        }




                                        SendDocumentPayload doc = new SendDocumentPayload();
                                        SendDocumentResponse resp = new SendDocumentResponse();

                                        doc.Type = "MOP";
                                        doc.Code = prov.EntCode; //entcode
                                        doc.Name = fileSubirDocResolucion.FileName; //filename
                                        doc.Rut = rutProveedor; //rutproveedor
                                        switch (prov.TipoProveedor)
                                        {
                                            case 18:
                                                doc.ProviderType = "Person";//Tipo Proveedor: NATURAL=PERSON
                                                break;
                                            case 19:
                                                doc.ProviderType = "Entity";//Tipo Proveedor: JURÍDICA - ENTITY
                                                break;
                                        }
                                        doc.ProviderName = prov.RazonSocial; //Razón Social

                                        //Byte[] byteFile = File.ReadAllBytes(rutaArchivoAConvertir);
                                        Byte[] byteFile = System.IO.File.ReadAllBytes(path);
                                        string base64File = Convert.ToBase64String(byteFile);
                                        doc.Content = base64File;

                                        resp = Models.GesintelMethods.RestMethods.SendDocument(token.Token, doc);

                                        if (resp.status == "OK")
                                        {
                                            MOPArchivoPrivado archRespAnalisis = new MOPArchivoPrivado();
                                            string tmpFileName = fileSubirDocResolucion.FileName;
                                            int lastDotIndex = tmpFileName.LastIndexOf('.');
                                            string soloNombre = tmpFileName.Substring(0, lastDotIndex);
                                            /////////////////////////////////////////////////////////////
                                            /////////////////////////////////////////////////////////////
                                            archRespAnalisis.NomArchivo = soloNombre;
                                            //string ext = tmpFileName.Substring(lastDotIndex, tmpFileName.Length); //extensión de archivo
                                            string ext = tmpFileName.Substring(lastDotIndex + 1); //extensión de archivo                                                                                                    
                                            archRespAnalisis.Tipo = ext;
                                            archRespAnalisis.Fecha = DateTime.Now;
                                            archRespAnalisis.Descripcion = "Documento resolución área: " + nombreAreaAnalisis;
                                            archRespAnalisis.Tamano = (decimal)fileSubirDocResolucion.ContentLength;
                                            archRespAnalisis.IdArchivo = long.Parse(resp.Folio);
                                            archRespAnalisis.UsuarioIngresoReg = Session["SessionNombreUsuario"].ToString();//identityName -- RUT PROVEEDOR
                                            archRespAnalisis.FechaIngresoReg = DateTime.Now;
                                            archRespAnalisis.UsuarioModReg = Session["SessionNombreUsuario"].ToString();
                                            archRespAnalisis.FechaModReg = DateTime.Now;
                                            archRespAnalisis.ParentId = prov.Id;//idProveedor
                                            archRespAnalisis.IdInstitucion = 21; //idMandanteMOP
                                            archRespAnalisis.FechaInicioVigencia = DateTime.Now;
                                            archRespAnalisis.FechaFinVigencia = DateTime.Now.AddYears(1);
                                            archRespAnalisis.IdDocSolicitado = 0; //0
                                            archRespAnalisis.RutProveedor = rutProveedor;
                                            archRespAnalisis.TipoDocumento = "NOTA";

                                            //SE INSERTA REGISTRO EN DB
                                            int idArchivoPrivado = SQLMethods.InsertarMOPArchivoPrivado(archRespAnalisis);

                                            //MOPMetaDatosDocumentos mmd = new MOPMetaDatosDocumentos();
                                            //mmd.IdDoc = idArchivoPrivado;
                                            //mmd.VisibleProv = "N"; //S o N
                                            //mmd.PalabrasClave = palabrasClave;
                                            //mmd.Anio = año;
                                            //mmd.CodTipoDocumento = tipoDoc;
                                            //mmd.CodAreaAnalisis = areaDoc;
                                            //mmd.FechaIngresoReg = DateTime.Now;
                                            //mmd.UsuarioIngresoReg = Session["SessionNombreUsuario"].ToString();

                                            //SE REGISTRA SUBIDA DE ARCHIVO EN DB
                                            //SQLMethods.InsertarMetadatosDocumento(mmd);

                                            MOPSolicitud solRan = new MOPSolicitud();
                                            solRan.ID = numSolicitud;
                                            solRan.CodEstado = "RAN";
                                            solRan.CodEstadoCHP = "";//se envia vacío para evitar mandar null, no se usa en este paso
                                            solRan.FechaRespuesta = DateTime.Now;
                                            solRan.FechaIngreso = DateTime.Now; //se manda para no mandar null pero no se usa en el SP en este paso
                                            solRan.SolPadreRespondida = "";
                                            SQLSolicitudesMOP.ActualizarEstadoSolicitud(solRan);





                                            //NOTAS DEL ARCHIVO SUBIDO
                                            if (idArchivoPrivado > 0)
                                            {
                                                MOPNotasSolicitud nota = new MOPNotasSolicitud();
                                                nota.IdSol = numSolicitud;
                                                nota.Comentario = comentariosAnalista;
                                                nota.FechaNota = DateTime.Now;
                                                nota.CodEstado = "RES";
                                                nota.CodAreaAnalisis = codAreaAnalisisTmp;
                                                nota.IdDoc = idArchivoPrivado;
                                                nota.Tipo = "INT";
                                                //SE REGISTRA EN DB
                                                SQLSolicitudesMOP.GuardarNotasSolicitud(nota);
                                            }


                                            //NOTAS DE LA SOLICITUD
                                            MOPNotasSolicitud notaAnalisis = new MOPNotasSolicitud();
                                            notaAnalisis.IdSol = numSolicitud;
                                            notaAnalisis.Comentario = "Analisis Respondido";
                                            notaAnalisis.FechaNota = DateTime.Now;
                                            notaAnalisis.CodEstado = "REOBS";
                                            notaAnalisis.CodAreaAnalisis = codAreaAnalisisTmp;
                                            notaAnalisis.IdDoc = 0;
                                            notaAnalisis.Tipo = "PRO";
                                            //SE REGISTRA EN DB
                                            SQLSolicitudesMOP.GuardarNotasSolicitud(notaAnalisis);


                                            //GUARDAR LOG
                                            MOPLog logRespAnalisis = new MOPLog();
                                            logRespAnalisis.Fecha = DateTime.Now;
                                            logRespAnalisis.RutProveedor = rutProveedor;
                                            logRespAnalisis.Usuario = Session["SessionNombreUsuario"].ToString();
                                            logRespAnalisis.idSol = numSolicitud;
                                            logRespAnalisis.idDoc = 0;
                                            logRespAnalisis.CodTipo = "LSOL";
                                            logRespAnalisis.CodEstado = "RES";
                                            logRespAnalisis.Comentario_1 = "Analisis Respondido";
                                            logRespAnalisis.Comentario_2 = "Área Análisis [" + nombreAreaAnalisis + "] responde solicitud N° " + numSolicitud.ToString();
                                            logRespAnalisis.Comentario_3 = comentariosAnalista;
                                            string v = SQLSolicitudesMOP.GuardarMOPLog(logRespAnalisis, funcionarioMOP);



                                            //ENVIO DE CORREO
                                            respCorreo = EnvioCorreosMOP.EnviarNotificacion_ResponderAnalisis(numSolicitud, nombreAreaAnalisis);



                                            //AL FINAL, SE ELIMINA EL ARCHIVO DE LA RUTA TEMP
                                            if (System.IO.File.Exists(path))
                                            {
                                                //System.IO.File.Delete(path);
                                                System.IO.Directory.Delete(folderPath, true);//con true borra el directorio y todo su contenido
                                            }


                                        }
                                        else
                                        {
                                            //GUARDAR LOG
                                            MOPLog logErrorGesintel = new MOPLog();
                                            logErrorGesintel.Fecha = DateTime.Now;
                                            logErrorGesintel.RutProveedor = rutProveedor;
                                            logErrorGesintel.Usuario = Session["SessionNombreUsuario"].ToString();
                                            logErrorGesintel.idSol = numSolicitud;
                                            logErrorGesintel.idDoc = 0;
                                            logErrorGesintel.CodTipo = "LSOL";
                                            logErrorGesintel.CodEstado = "ERROR";
                                            logErrorGesintel.Comentario_1 = "Error al subir documento a Gesintel";
                                            logErrorGesintel.Comentario_2 = "Error al conectar a Repositorio Gesintel - Error de subida: " + resp.status;
                                            logErrorGesintel.Comentario_3 = "";
                                            string v = SQLSolicitudesMOP.GuardarMOPLog(logErrorGesintel, funcionarioMOP);

                                            return Json(new { success = false, message = "Error al conectar a Repositorio Gesintel - Error de subida: " + resp.status });
                                        }
                                    }
                                    else
                                    {
                                        //GUARDAR LOG
                                        MOPLog logErrorToken = new MOPLog();
                                        logErrorToken.Fecha = DateTime.Now;
                                        logErrorToken.RutProveedor = rutProveedor;
                                        logErrorToken.Usuario = Session["SessionNombreUsuario"].ToString();
                                        logErrorToken.idSol = numSolicitud;
                                        logErrorToken.idDoc = 0;
                                        logErrorToken.CodTipo = "LSOL";
                                        logErrorToken.CodEstado = "ERROR";
                                        logErrorToken.Comentario_1 = "Error al Conectar a Gesintel";
                                        logErrorToken.Comentario_2 = "Error al conectar a Repositorio Gesintel - Token error: " + token.Status;
                                        logErrorToken.Comentario_3 = "";
                                        string v = SQLSolicitudesMOP.GuardarMOPLog(logErrorToken, funcionarioMOP);



                                        return Json(new { success = false, message = "Error al conectar a Repositorio Gesintel - Token error: " + token.Status });
                                    }

                                    //AL FINAL, SE ELIMINA EL ARCHIVO DE LA RUTA TEMP
                                    if (System.IO.File.Exists(path))
                                    {
                                        //System.IO.File.Delete(path);
                                        System.IO.Directory.Delete(folderPath, true);//con true borra el directorio y todo su contenido
                                    }

                                }





                            }
                            else
                            {
                                string msg2;

                                msg2 = "Para esta operación debe adjuntar un documento de resolución";
                                return Json(new { success = false, message = msg2 });
                            }

                            //VALIDACION LARGO NOMBRE DOC


                            #endregion

                            break;
                        case "CAL"://                            
                        case "ACE"://ACEPTAR SOLICITUD VA CON DOC                            
                            #region ACEPTAR SOLICITUD   
                            msgRespuesta = "Solicitud aceptada";

                            if (accion == "CAL")
                            {
                                msgRespuesta = "Solicitud finalizada con alcances";
                            }

                            //VALIDACIÓN DOCUMENTO ADJUNTO
                            if (fileSubirDocResolucion != null)
                            {

                                if (fileSubirDocResolucion.FileName.Length > staticMaxFileLength)
                                {
                                    string msg2;
                                    msg2 = "El nombre del archivo no puede superar el largo máximo de " + staticMaxFileLength + " caracteres";
                                    return Json(new { success = false, message = msg2 });
                                }
                                else
                                {



                                    string fechaTmp = DateTime.Now.ToString("ddMMyyyyHHmmss");
                                    string fileName = Path.GetFileName(fileSubirDocResolucion.FileName);
                                    string folder = rutProveedor.Replace("-", "_") + "_" + fechaTmp;
                                    string folderPath = Path.Combine(Server.MapPath("~/Content/TempUploadedFilesSolicitudes/"), folder);
                                    string path = Path.Combine(Server.MapPath("~/Content/TempUploadedFilesSolicitudes/"), folder, fileName);
                                    //SUBIDA DE ARCHIVO
                                    //SE GUARDA ARCHIVO EN RUTA TMP PARA LUEGO SUBIRLO A GESINTEL, Y LUEGO SE ELIMINA
                                    Directory.CreateDirectory(folderPath);
                                    try
                                    {
                                        fileSubirDocResolucion.SaveAs(path);
                                    }
                                    catch (Exception x)
                                    {
                                        msgRespuesta = "Error al procesar el archivo antes de subir: " + x.InnerException.Message.ToString();
                                        return Json(new { success = false, message = msgRespuesta });
                                    }

                                    //SUBIDA A GESINTEL
                                    if (token.Status == "OK")
                                    {
                                        SendDocumentPayload doc = new SendDocumentPayload();
                                        SendDocumentResponse resp = new SendDocumentResponse();

                                        doc.Type = "MOP";
                                        doc.Code = prov.EntCode; //entcode
                                        doc.Name = fileSubirDocResolucion.FileName; //filename
                                        doc.Rut = rutProveedor; //rutproveedor
                                        switch (prov.TipoProveedor)
                                        {
                                            case 18:
                                                doc.ProviderType = "Person";//Tipo Proveedor: NATURAL=PERSON
                                                break;
                                            case 19:
                                                doc.ProviderType = "Entity";//Tipo Proveedor: JURÍDICA - ENTITY
                                                break;
                                        }
                                        doc.ProviderName = prov.RazonSocial; //Razón Social

                                        //Byte[] byteFile = File.ReadAllBytes(rutaArchivoAConvertir);
                                        Byte[] byteFile = System.IO.File.ReadAllBytes(path);
                                        string base64File = Convert.ToBase64String(byteFile);
                                        doc.Content = base64File;

                                        resp = Models.GesintelMethods.RestMethods.SendDocument(token.Token, doc);

                                        if (resp.status == "OK")
                                        {
                                            MOPArchivoPrivado archAceptarSol = new MOPArchivoPrivado();
                                            string tmpFileName = fileSubirDocResolucion.FileName;
                                            int lastDotIndex = tmpFileName.LastIndexOf('.');
                                            string soloNombre = tmpFileName.Substring(0, lastDotIndex);
                                            /////////////////////////////////////////////////////////////
                                            /////////////////////////////////////////////////////////////
                                            archAceptarSol.NomArchivo = soloNombre;
                                            string ext = tmpFileName.Substring(lastDotIndex + 1); //extensión de archivo                                                                                                    
                                            archAceptarSol.Tipo = ext;
                                            archAceptarSol.Fecha = DateTime.Now;
                                            archAceptarSol.Descripcion = "Documento resolución final";
                                            archAceptarSol.Tamano = (decimal)fileSubirDocResolucion.ContentLength;
                                            archAceptarSol.IdArchivo = long.Parse(resp.Folio);
                                            archAceptarSol.UsuarioIngresoReg = Session["SessionNombreUsuario"].ToString();//identityName -- RUT PROVEEDOR
                                            archAceptarSol.FechaIngresoReg = DateTime.Now;
                                            archAceptarSol.UsuarioModReg = Session["SessionNombreUsuario"].ToString();
                                            archAceptarSol.FechaModReg = DateTime.Now;
                                            archAceptarSol.ParentId = prov.Id;//idProveedor
                                            archAceptarSol.IdInstitucion = 21; //idMandanteMOP
                                            archAceptarSol.FechaInicioVigencia = DateTime.Now;
                                            archAceptarSol.FechaFinVigencia = DateTime.Now.AddYears(1);
                                            archAceptarSol.IdDocSolicitado = 0; //0
                                            archAceptarSol.RutProveedor = rutProveedor;
                                            archAceptarSol.TipoDocumento = "NOTA";
                                            //SE INSERTA REGISTRO EN DB
                                            int idArchivoPrivado = SQLMethods.InsertarMOPArchivoPrivado(archAceptarSol);

                                            //NOTAS DEL ARCHIVO SUBIDO
                                            if (idArchivoPrivado > 0)
                                            {
                                                MOPNotasSolicitud nota = new MOPNotasSolicitud();
                                                nota.IdSol = numSolicitud;
                                                nota.Comentario = comentariosAnalista;
                                                nota.FechaNota = DateTime.Now;
                                                nota.CodEstado = "RES";
                                                nota.CodAreaAnalisis = codAreaAnalisisTmp;
                                                nota.IdDoc = idArchivoPrivado;
                                                nota.Tipo = "INT";
                                                //SE REGISTRA EN DB
                                                SQLSolicitudesMOP.GuardarNotasSolicitud(nota);
                                            }
                                            ///////////////////////////////////////////////////////////
                                            ///////////////////////////////////

                                            //NOTAS DE LA SOLICITUD
                                            MOPNotasSolicitud notaResolucionAceptada = new MOPNotasSolicitud();
                                            notaResolucionAceptada.IdSol = numSolicitud;
                                            notaResolucionAceptada.Comentario = "Resolución solicitud respondida";
                                            notaResolucionAceptada.FechaNota = DateTime.Now;
                                            notaResolucionAceptada.CodEstado = "REOBS";
                                            notaResolucionAceptada.CodAreaAnalisis = codAreaAnalisisTmp;
                                            notaResolucionAceptada.IdDoc = 0;
                                            notaResolucionAceptada.Tipo = "PRO";
                                            //SE REGISTRA EN DB
                                            SQLSolicitudesMOP.GuardarNotasSolicitud(notaResolucionAceptada);

                                            //////////////////////////////////////////////////////////
                                            //////////////////////////////////////////////////////////
                                            //ACTUALIZACIÓN DE ESTADO DE SOLICITUD                                            
                                            MOPSolicitud solAceptada = new MOPSolicitud();
                                            solAceptada.ID = numSolicitud;
                                            solAceptada.CodEstado = (accion == "ACE") ? "ACE" : "CAL";
                                            solAceptada.CodEstadoCHP = (accion == "ACE") ? "RES" : "CAL";
                                            solAceptada.FechaRespuesta = DateTime.Now;
                                            //solAceptada.FechaIngreso = DateTime.Now;//SE ENVIA PARA NO ENVIAR NULL
                                            solAceptada.SolPadreRespondida = (accion == "ACE") ? "S" : "N";
                                            //solAceptada.CodEstado = "ACE";
                                            //solAceptada.CodEstadoCHP = "RES";
                                            //solAceptada.FechaRespuesta = DateTime.Now;
                                            //solAceptada.SolPadreRespondida = "S";

                                            SQLSolicitudesMOP.ActualizarEstadoSolicitudFinal(solAceptada);

                                            bool solicitudRelacionada = SQLSolicitudesMOP.ValidarSolicitudRelacionada(codTipoSolicitud, numSolicitud);
                                            if (accion == "ACE" && (codTipoSolicitud == "INS" || solicitudRelacionada == true))
                                            {
                                                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                                //Ya no se hace validación de Proveedor Existente, puesto que éste se inserta al momento de entrar desde Mercado Público por primera vez
                                                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                                //GUARDAR LOG
                                                MOPLog logAceptada = new MOPLog();
                                                logAceptada.Fecha = DateTime.Now;
                                                logAceptada.RutProveedor = rutProveedor;
                                                logAceptada.Usuario = Session["SessionNombreUsuario"].ToString();
                                                logAceptada.idSol = numSolicitud;
                                                logAceptada.idDoc = idArchivoPrivado;
                                                logAceptada.CodTipo = "LSOL";
                                                logAceptada.CodEstado = accion;
                                                logAceptada.Comentario_1 = "Analista de Registro [Acepta] Solicitud de Registro";
                                                logAceptada.Comentario_2 = "Analista de Registro responde [Aceptando] la solicitud de registro [" + numSolicitud.ToString() + "]";
                                                logAceptada.Comentario_3 = comentariosAnalista;
                                                SQLSolicitudesMOP.GuardarMOPLog(logAceptada, funcionarioMOP);
                                            }
                                            else
                                            {
                                                //GUARDAR LOG
                                                MOPLog logAceptada = new MOPLog();
                                                logAceptada.Fecha = DateTime.Now;
                                                logAceptada.RutProveedor = rutProveedor;
                                                logAceptada.Usuario = Session["SessionNombreUsuario"].ToString();
                                                logAceptada.idSol = numSolicitud;
                                                logAceptada.idDoc = idArchivoPrivado;
                                                logAceptada.CodTipo = "LSOL";
                                                logAceptada.CodEstado = accion; //accion
                                                logAceptada.Comentario_1 = "Analista de Registro responde solicitud";
                                                //string c1 = "Analista de Registro responde [" + accion == "ACE" ? "Aceptando" : "Con alcances] la solicitud [" + numSolicitud.ToString() + "]";                                            
                                                string c1 = "Analista de Registro responde [";
                                                string c2 = accion == "ACE" ? "Aceptando" : "Con alcances";
                                                string c3 = "] la solicitud [" + numSolicitud.ToString() + "]";
                                                logAceptada.Comentario_2 = c1 + c2 + c3;
                                                //logAceptada.Comentario_2 = "Analista de Registro responde [Aceptando] la solicitud [" + numSolicitud.ToString() + "]";
                                                logAceptada.Comentario_3 = comentariosAnalista;
                                                SQLSolicitudesMOP.GuardarMOPLog(logAceptada, funcionarioMOP);
                                            }

                                            if (accion == "ACE" && numSolicitudPadre > 0)
                                            {
                                                //Actualizar Solicitud Padre respondida
                                                SQLSolicitudesMOP.ActualizarSolicitudPadreRespondida(numSolicitudPadre);
                                            }

                                            //ENVIO DE CORREO
                                            //string emailProv = "";
                                            string emailProv = hiddenEmailProveedor;
                                            respCorreo = EnvioCorreosMOP.EnviarNotificacion_ResolucionFinalProveedor(numSolicitud, emailProv, funcionarioMOP);

                                            //AL FINAL, SE ELIMINA EL ARCHIVO DE LA RUTA TEMP
                                            if (System.IO.File.Exists(path))
                                            {
                                                //System.IO.File.Delete(path);
                                                System.IO.Directory.Delete(folderPath, true);//con true borra el directorio y todo su contenido
                                            }
                                        }
                                        else
                                        {
                                            return Json(new { success = false, message = "Error al conectar a Repositorio Gesintel - Error de subida: " + resp.status });
                                        }
                                    }
                                    else
                                    {
                                        return Json(new { success = false, message = "Error al conectar a Repositorio Gesintel - Token error: " + token.Status });
                                    }
                                }

                            }
                            else
                            {
                                string msg2 = "Para esta operación debe adjuntar un documento de resolución";
                                return Json(new { success = false, message = msg2 });
                            }

                            #endregion
                            break;

                        #region (SIN USO) RECHAZO y CON ALCANCES VAN EN ACEPTAR
                        //case "REC"://RECHAZAR SOLICITUD VA SIN DOC ///////////////// NO SE ESTÁ USANDO RECHAZAR SOLICITUD
                        //case "CAL"://SOLICITUD CON ALCANCES VA SIN DOC
                        //    msgRespuesta = "Solicitud devuelta con alcances";

                        //    #region ENVIAR CON ALCANCES AL PROVEEDOR (PENDIENTE ENVIO CORREO)


                        //    //NOTAS DE LA SOLICITUD
                        //    MOPNotasSolicitud notaResolucionRechazadaOAlcances = new MOPNotasSolicitud();
                        //    notaResolucionRechazadaOAlcances.IdSol = numSolicitud;
                        //    notaResolucionRechazadaOAlcances.Comentario = "Resolución solicitud respondida";
                        //    notaResolucionRechazadaOAlcances.FechaNota = DateTime.Now;
                        //    notaResolucionRechazadaOAlcances.CodEstado = "REOBS";
                        //    notaResolucionRechazadaOAlcances.CodAreaAnalisis = codPerfil;
                        //    notaResolucionRechazadaOAlcances.IdDoc = 0;
                        //    notaResolucionRechazadaOAlcances.Tipo = "PRO";
                        //    //SE REGISTRA EN DB
                        //    SQLSolicitudesMOP.GuardarNotasSolicitud(notaResolucionRechazadaOAlcances);

                        //    //////////////////////////////////////////////////////////
                        //    //////////////////////////////////////////////////////////
                        //    //ACTUALIZACIÓN DE ESTADO DE SOLICITUD                                            
                        //    MOPSolicitud sol = new MOPSolicitud();
                        //    //sol.CodEstado = (accion == "ACE") ? "ACE" : "CAL";
                        //    //sol.CodEstadoCHP = (accion == "ACE") ? "RES" : "CAL";
                        //    //sol.FechaRespuesta = DateTime.Now;
                        //    //sol.SolPadreRespondida = (accion == "ACE") ? "S" : "N";
                        //    if (accion == "CAL")
                        //    {
                        //        sol.CodEstado = "CAL";
                        //        sol.CodEstadoCHP = "CAL";
                        //    }
                        //    else
                        //    {
                        //        sol.CodEstado = "REC";
                        //        sol.CodEstadoCHP = "REC";
                        //    }


                        //    sol.FechaRespuesta = DateTime.Now;
                        //    sol.SolPadreRespondida = "S";//???????????????????????

                        //    SQLSolicitudesMOP.ActualizarEstadoSolicitud(sol);


                        //    //GUARDAR LOG
                        //    MOPLog log = new MOPLog();
                        //    log.Fecha = DateTime.Now;
                        //    log.RutProveedor = rutProveedor;
                        //    log.Usuario = Session["SessionNombreUsuario"].ToString();
                        //    log.idSol = numSolicitud;
                        //    log.CodTipo = "LSOL";
                        //    log.CodEstado = "RES";
                        //    log.Comentario_1 = "Analista de Registro responde solicitud";
                        //    log.Comentario_2 = "Analista de Registro responde [Con alcances] la solicitud [" + numSolicitud.ToString() + "]";
                        //    log.Comentario_3 = comentariosAnalista;

                        //    SQLSolicitudesMOP.GuardarMOPLog(log, funcionarioMOP);


                        //    //ENVÍO DE CORREO DE NOTIFICACIÓN AL PROVEEDOR






                        //    #endregion

                        //    break;
                        #endregion

                        case "EAN"://ENVIAR A ÁREAS DE ANÁLISIS VA SIN DOC 
                            #region ENVIAR A ANÁLISIS 

                            msgRespuesta = "Solicitud enviada a área de análisis";

                            MOPSolicitud solEan = new MOPSolicitud();
                            solEan.ID = numSolicitud;
                            solEan.CodEstado = "EAN";
                            solEan.CodEstadoCHP = "EAN";
                            solEan.FechaIngreso = DateTime.Now;
                            solEan.SolPadreRespondida = "";//en WebMOP no mandaba nada, pero deberíamos mandar vacío para q no quede null
                            SQLSolicitudesMOP.ActualizarEstadoSolicitud(solEan);

                            List<ArchivosAdjuntosEnviarAnalisis> adj = JsonConvert.DeserializeObject<List<ArchivosAdjuntosEnviarAnalisis>>(hiddenListDocs);
                            foreach (var item in adj)
                            {
                                MOPDetalleSolicitud detEan = new MOPDetalleSolicitud();
                                detEan.ID = item.IdDetalleSolicitud;
                                detEan.CodAreaAnalisis = item.areaDeAnalisis;
                                detEan.CodEstado = "EAN";
                                detEan.Comentario = "";
                                SQLSolicitudesMOP.ActualizarEstadoDetalleSolicitud(detEan);

                                //ACÁ DEBERÍAMOS CREAR UN NUEVO LOG, DE QUÉ ARCHIVOS SE ENVIAN A ÁREAS DE ANÁLISIS, PARA TENER SU HISTORIAL
                            }

                            //GUARDAR LOG
                            MOPLog logEnvioAnalisis = new MOPLog();
                            logEnvioAnalisis.Fecha = DateTime.Now;
                            logEnvioAnalisis.RutProveedor = rutProveedor;
                            logEnvioAnalisis.Usuario = Session["SessionNombreUsuario"].ToString();
                            logEnvioAnalisis.idSol = numSolicitud;
                            logEnvioAnalisis.idDoc = 0;
                            logEnvioAnalisis.CodTipo = "LSOL";
                            logEnvioAnalisis.CodEstado = "EAN";
                            logEnvioAnalisis.Comentario_1 = "Solicitud enviada a Análisis";
                            logEnvioAnalisis.Comentario_2 = "La solicitud [" + numSolicitud.ToString() + "] es enviada a Análisis";
                            logEnvioAnalisis.Comentario_3 = comentariosAnalista;
                            SQLSolicitudesMOP.GuardarMOPLog(logEnvioAnalisis, funcionarioMOP);

                            #endregion
                            break;
                        case "SCO"://SOLICITAR CORRECCIONES VA SIN DOC 
                            #region SOLICITAR CORRECCIONES (OK)
                            msgRespuesta = "Se solicitan correcciones, la solicitud ha sido enviada de vuelta al proveedor";
                            //COMENTARIOS DE ARCHIVOS
                            //POR AHORA NO SE USA
                            MOPDetalleSolicitud det = new MOPDetalleSolicitud();

                            MOPNotasSolicitud notaSolicitarCorrecciones = new MOPNotasSolicitud();
                            notaSolicitarCorrecciones.Comentario = comentariosAnalista;
                            notaSolicitarCorrecciones.CodAreaAnalisis = "ADM";
                            notaSolicitarCorrecciones.CodEstado = "OBS";
                            notaSolicitarCorrecciones.IdSol = numSolicitud;
                            notaSolicitarCorrecciones.FechaNota = DateTime.Now;
                            notaSolicitarCorrecciones.IdDoc = 0;
                            notaSolicitarCorrecciones.Tipo = "PRO"; //también se registra otro igual pero tipo INT (NO SE POR QUÉ) ?????????????
                            SQLSolicitudesMOP.GuardarNotasSolicitud(notaSolicitarCorrecciones);

                            MOPSolicitud solSolicitaCorrecciones = new MOPSolicitud();
                            solSolicitaCorrecciones.ID = numSolicitud;
                            solSolicitaCorrecciones.CodEstado = "SCO";
                            solSolicitaCorrecciones.CodEstadoCHP = "SCO";
                            solSolicitaCorrecciones.FechaIngreso = DateTime.Now; //Se manda pero no se actualiza en esta etapa de SCO
                            solSolicitaCorrecciones.SolPadreRespondida = "N";
                            SQLSolicitudesMOP.ActualizarEstadoSolicitud(solSolicitaCorrecciones);

                            MOPLog logSolicitaCorrecciones = new MOPLog();
                            logSolicitaCorrecciones.Fecha = DateTime.Now;
                            logSolicitaCorrecciones.RutProveedor = rutProveedor;
                            logSolicitaCorrecciones.Usuario = rutProveedor;
                            logSolicitaCorrecciones.idSol = numSolicitud;
                            logSolicitaCorrecciones.idDoc = 0;
                            logSolicitaCorrecciones.CodTipo = "LSOL";
                            logSolicitaCorrecciones.CodEstado = "SCO";
                            logSolicitaCorrecciones.Comentario_1 = "Se solicitan correcciones";
                            logSolicitaCorrecciones.Comentario_2 = "Se solicitan correcciones a la solicitud  [" + numSolicitud.ToString() + "]";
                            logSolicitaCorrecciones.Comentario_3 = comentariosAnalista;
                            SQLSolicitudesMOP.GuardarMOPLog(logSolicitaCorrecciones, funcionarioMOP);

                            string emailProveedor = hiddenEmailProveedor;
                            //ENVIAR CORREO
                            //string funcionarioMOP = Session["SessionNombreUsuario"].ToString();
                            respCorreo = EnvioCorreosMOP.EnviarNotificacion_SolicitarCorreccionesProveedor(numSolicitud, emailProveedor, funcionarioMOP);

                            #endregion
                            break;
                        case "VEA"://VOLVER AL ESTADO ANTERIOR VA SIN DOC 
                            #region VOLVER AL ESTADO ANTERIOR 

                            msgRespuesta = "La solicitud ha sido devuelta a su estado anterior";
                            //DEPENDIENDO DEL ESTADO ACTUAL, LAS ACCIONES A SEGUIR
                            switch (codEstadoSolicitud)
                            {
                                #region SCO => ABT (ESTA VUELTA NO SE REALIZA YA QUE LA SOLICITUD ESTÁ EN MANOS DEL PROVEEDOR)
                                case "SCO":


                                    //ACTUALIZACIÓN MOPDetalleSolicitud                                    
                                    SQLSolicitudesMOP.VolverEstadoAnterior_ActualizarDetalleSolicitud(numSolicitud, codEstadoSolicitud);

                                    /////////////////////////////////////////////////////////////////
                                    ////EVALUAR NO ELIMINAR NOTAS DE SOLICITUD
                                    //Se eliminan las notas de solicitud
                                    //MOPNotasSolicitud nota_VEA_SCO = new MOPNotasSolicitud();
                                    //nota_VEA_SCO.Tipo = "OBS";
                                    //nota_VEA_SCO.IdSol = numSolicitud;
                                    //SQLSolicitudesMOP.VolverEstadoAnterior_EliminarNotasSolicitud(nota_VEA_SCO);


                                    //MOPSolicitud solSco = new MOPSolicitud();
                                    //solSco.CodEstado = "ABT";
                                    //solSco.CodEstadoCHP = "ENV";
                                    /////////////////////////////////////////////////////////////////
                                    ///
                                    SQLSolicitudesMOP.VolverEstadoAnterior_ActualizarSolicitud(numSolicitud, codEstadoSolicitud);


                                    MOPLog logSco = new MOPLog();
                                    logSco.Fecha = DateTime.Now;
                                    logSco.RutProveedor = rutProveedor;
                                    logSco.Usuario = "";//////////////////////////////////////////////////////////
                                    logSco.idSol = numSolicitud;
                                    logSco.idDoc = 0;
                                    logSco.CodTipo = "LSOL";
                                    logSco.CodEstado = "ABT";
                                    logSco.Comentario_1 = "Se vuelve al estado anterior";
                                    logSco.Comentario_2 = "La Solicitud [" + numSolicitud.ToString() + "] es devuelta a su estado anterior, de [Solicitan Correcciones] a [Abierta]";
                                    logSco.Comentario_3 = comentariosAnalista;
                                    SQLSolicitudesMOP.GuardarMOPLog(logSco, funcionarioMOP);

                                    break;
                                #endregion


                                #region EAN => ABT

                                case "EAN":
                                    //MOPDetalleSolicitud detEan = new MOPDetalleSolicitud();
                                    //detEan.Comentario = "";
                                    //detEan.CodAreaAnalisis = "";
                                    //detEan.CodEstado = "";
                                    //Se eliminan las notas de solicitud
                                    MOPNotasSolicitud nota_VEA_EAN = new MOPNotasSolicitud();
                                    nota_VEA_EAN.IdSol = numSolicitud;
                                    SQLSolicitudesMOP.VolverEstadoAnterior_EliminarNotasSolicitud(nota_VEA_EAN, codEstadoSolicitud);

                                    //ACTUALIZACIÓN MOPDetalleSolicitud                                    
                                    SQLSolicitudesMOP.VolverEstadoAnterior_ActualizarDetalleSolicitud(numSolicitud, codEstadoSolicitud);


                                    //MOPSolicitud solEan = new MOPSolicitud();
                                    //solEan.CodEstado = "ABT";
                                    //solEan.CodEstadoCHP = "ENV";

                                    //ACTUALIZACIÓN MOPSolicitud
                                    SQLSolicitudesMOP.VolverEstadoAnterior_ActualizarSolicitud(numSolicitud, codEstadoSolicitud);



                                    MOPLog logEan = new MOPLog();
                                    logEan.Fecha = DateTime.Now;
                                    logEan.RutProveedor = rutProveedor;
                                    logEan.Usuario = "";//////////////////////////////////////////////////////////
                                    logEan.idSol = numSolicitud;
                                    logEan.idDoc = 0;
                                    logEan.CodTipo = "LSOL";
                                    logEan.CodEstado = "ABT";
                                    logEan.Comentario_1 = "Se vuelve al estado anterior";
                                    logEan.Comentario_2 = "La Solicitud [" + numSolicitud.ToString() + "] es devuelta a su estado anterior, de [Enviada a Análisis] a [Abierta]";
                                    logEan.Comentario_3 = comentariosAnalista;
                                    SQLSolicitudesMOP.GuardarMOPLog(logEan, funcionarioMOP);



                                    break;


                                #endregion


                                #region RAN => EAN 

                                case "RAN":
                                    //MOPDetalleSolicitud detRan = new MOPDetalleSolicitud();
                                    //detRan.CodEstado = "EAN";

                                    //ACTUALIZACIÓN MOPDetalleSolicitud     
                                    SQLSolicitudesMOP.VolverEstadoAnterior_ActualizarDetalleSolicitud(numSolicitud, codEstadoSolicitud);

                                    MOPNotasSolicitud nota_VEA_RAN = new MOPNotasSolicitud();
                                    nota_VEA_RAN.IdSol = numSolicitud;
                                    SQLSolicitudesMOP.VolverEstadoAnterior_EliminarNotasSolicitud(nota_VEA_RAN, codEstadoSolicitud);


                                    //MOPSolicitud solRan = new MOPSolicitud();
                                    //solRan.CodEstado = "EAN";
                                    //solRan.CodEstadoCHP = "EAN";
                                    //ACTUALIZACIÓN MOPSolicitud
                                    SQLSolicitudesMOP.VolverEstadoAnterior_ActualizarSolicitud(numSolicitud, codEstadoSolicitud);

                                    MOPLog logRan = new MOPLog();
                                    logRan.Fecha = DateTime.Now;
                                    logRan.RutProveedor = rutProveedor;
                                    logRan.Usuario = "";//////////////////////////////////////////////////////////
                                    logRan.idSol = numSolicitud;
                                    logRan.idDoc = 0;
                                    logRan.CodTipo = "LSOL";
                                    logRan.CodEstado = "EAN";
                                    logRan.Comentario_1 = "Se vuelve al estado anterior";
                                    logRan.Comentario_2 = "La Solicitud [" + numSolicitud.ToString() + "] es devuelta a su estado anterior, de [Resultado del Análisis] a [Enviada a Análisis]";
                                    logRan.Comentario_3 = comentariosAnalista;
                                    SQLSolicitudesMOP.GuardarMOPLog(logRan, funcionarioMOP);
                                    break;
                                #endregion

                                #region ACE, REC, CAL => RAN

                                case "ACE":
                                case "REC":
                                case "CAL":


                                    MOPNotasSolicitud nota_VEA_ACE = new MOPNotasSolicitud();
                                    nota_VEA_ACE.IdSol = numSolicitud;
                                    SQLSolicitudesMOP.VolverEstadoAnterior_EliminarNotasSolicitud(nota_VEA_ACE, codEstadoSolicitud);


                                    //MOPSolicitud solAce = new MOPSolicitud();
                                    //solAce.CodEstado = "RAN";
                                    //solAce.CodEstadoCHP = "EAN";
                                    //ACTUALIZACIÓN MOPSolicitud
                                    SQLSolicitudesMOP.VolverEstadoAnterior_ActualizarSolicitud(numSolicitud, codEstadoSolicitud);



                                    MOPLog logAce = new MOPLog();
                                    logAce.Fecha = DateTime.Now;
                                    logAce.RutProveedor = rutProveedor;
                                    logAce.Usuario = "";//////////////////////////////////////////////////////////
                                    logAce.idSol = numSolicitud;
                                    logAce.idDoc = 0;
                                    logAce.CodTipo = "LSOL";
                                    logAce.CodEstado = "RAN";
                                    logAce.Comentario_1 = "Se vuelve al estado anterior";
                                    //logAce.Comentario_2 = "La Solicitud [" + numSolicitud.ToString() + "] es devuelta a su estado anterior, de ["+ codEstadoSolicitud == "ACE" ? "Aceptada" : "Rechazada"   +"] a [Resultado del Análisis]";
                                    string c2a = "La Solicitud [" + numSolicitud.ToString() + "] es devuelta a su estado anterior, de [";
                                    string c2b = codEstadoSolicitud == "ACE" ? "Aceptada" : "Rechazada";
                                    string c2c = "] a [Resultado del Análisis]";
                                    logAce.Comentario_2 = c2a + c2b + c2c;
                                    logAce.Comentario_3 = comentariosAnalista;
                                    SQLSolicitudesMOP.GuardarMOPLog(logAce, funcionarioMOP);


                                    break;




                                #endregion

                                default:
                                    break;
                            }

                            #endregion

                            break;
                        //case "CAA"://CAMBIAR ÁREA DE ANÁLISIS VA SIN DOC 
                        //    msgRespuesta = "La solicitud ha sido devuelta a su estado anterior";
                        //    break;
                        case "ELI"://VA SIN DOC                            
                            #region ELIMINAR SOLICITUD 

                            msgRespuesta = "SOLICITUD ELIMINADA";

                            MOPSolicitud solElim = new MOPSolicitud();
                            solElim.ID = numSolicitud;
                            solElim.IdSolPadre = numSolicitudPadre;
                            solElim.RutProveedor = rutProveedor;
                            solElim.FechaCreacion = Convert.ToDateTime(hiddenFechaRecepcion);
                            //solElim.FechaCreacion = hiddenFechaRecepcion;
                            solElim.CodTipoSol = codTipoSolicitud;
                            solElim.CodTipoReg = hiddenCodTipoRegistro;
                            solElim.CodEstado = codEstadoSolicitud;
                            solElim.IdRegion = hiddenCodRegion;
                            string solEliminada = SQLSolicitudesMOP.EliminarSolicitud(solElim, funcionarioMOP, comentariosAnalista);
                            //if (solEliminada.Equals("OK"))
                            //{
                            //    msgRespuesta = "DEL_OK";
                            //}
                            //else {
                            //    msgRespuesta = "DEL_FAIL";
                            //}

                            #endregion

                            break;

                        default:
                            break;
                    }
                    return Json(new { success = true, message = msgRespuesta });
                }
                else
                {
                    string msg = "No AJAX Request";
                    return Json(new { success = false, message = msg });
                }
            }
            catch (Exception x)
            {
                //throw;
                return Json(new { success = false, message = "File upload failed!!: " + x.Message.ToString() });
            }
        }






        #endregion









    }
}