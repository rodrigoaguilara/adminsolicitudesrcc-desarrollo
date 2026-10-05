
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AdminSolicitudesContratistas.Models;
using AdminSolicitudesContratistas.Models.GesintelModel;

namespace AdminSolicitudesContratistas.Controllers
{
    public class DocumentosDelProveedorController : Controller
    {
        // GET: DocumentosDelProveedor
        [CheckAuthorization]
        public ActionResult Index()
        {
            return View();
        }


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
            try
            {
                if (Request.IsAjaxRequest())
                {
                    if (fileSubirDocumentoProveedor != null)
                    {
                        //VALIDACIÓN EXTENSIÓN                    
                        if (fileSubirDocumentoProveedor.FileName.EndsWith("pdf"))
                        {
                            //VALIDACION DESCRIPCION
                            if (txtDescripcionSubirDocumentoProveedor.Length > 0)
                            {
                                //VAIDACION TIPO DOCUMENTO
                                if (cmbTipoDocSubirDocumentoProveedor != null)
                                {
                                    //VALIDACION AÑO
                                    //if (txtAñoDocSubirDocumentoProveedor.ToString().Length > 0) {                               }
                                    //VALIDACIÓN PALABRAS CLAVE
                                    if (txtPalabrasClaveDocumentoProveedor.Length > 0)
                                    {

                                        ProveedorView prov = new ProveedorView();
                                        string rutProveedor = hiddenRutProveedorSubirDoc;
                                        prov = SQLMethods.TraerDatosProveedor(rutProveedor);

                                        string fechaTmp = DateTime.Now.ToString("ddMMyyyyHHmmss");
                                        string fileName = Path.GetFileName(fileSubirDocumentoProveedor.FileName);
                                        string folder = rutProveedor.Replace("-", "_") + "_" + fechaTmp;
                                        string folderPath = Path.Combine(Server.MapPath("~/Content/TempUploadedFiles/"), folder);
                                        string path = Path.Combine(Server.MapPath("~/Content/TempUploadedFiles/"), folder, fileName);

                                        Directory.CreateDirectory(folderPath);

                                        string descripcion = txtDescripcionSubirDocumentoProveedor;
                                        string areaDoc = cmbAreaSubirDocumentoProveedor;
                                        string tipoDoc = cmbTipoDocSubirDocumentoProveedor;
                                        int año = txtAñoDocSubirDocumentoProveedor;
                                        string visibleAlProv = cmbVisibleAlProveedorSubirDocProv; //S o N
                                        string palabrasClave = txtPalabrasClaveDocumentoProveedor;

                                        //SE GUARDA ARCHIVO EN RUTA TMP PARA LUEGO SUBIRLO A GESINTEL, Y LUEGO SE ELIMINA
                                        fileSubirDocumentoProveedor.SaveAs(path);

                                        //SUBIDA A GESINTEL
                                        AuthenticateResponse token = Models.GesintelMethods.Token.gesintelLoginToken();
                                        if (token.Status == "OK")
                                        {
                                            SendDocumentPayload doc = new SendDocumentPayload();
                                            SendDocumentResponse resp = new SendDocumentResponse();

                                            doc.Type = "MOP";
                                            doc.Code = prov.EntCode; //entcode
                                            doc.Name = fileSubirDocumentoProveedor.FileName; //filename
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
                                                MOPArchivoPrivado arch = new MOPArchivoPrivado();
                                                string tmpFileName = fileSubirDocumentoProveedor.FileName;
                                                int lastDotIndex = tmpFileName.LastIndexOf('.');
                                                string soloNombre = tmpFileName.Substring(0, lastDotIndex);
                                                /////////////////////////////////////////////////////////////
                                                /////////////////////////////////////////////////////////////
                                                arch.NomArchivo = soloNombre;
                                                //string ext = tmpFileName.Substring(lastDotIndex, tmpFileName.Length); //extensión de archivo
                                                string ext = tmpFileName.Substring(lastDotIndex + 1); //extensión de archivo                                                                                                    
                                                arch.Tipo = ext;
                                                arch.Fecha = DateTime.Now;
                                                arch.Descripcion = descripcion;
                                                arch.Tamano = (decimal)fileSubirDocumentoProveedor.ContentLength;
                                                arch.IdArchivo = long.Parse(resp.Folio);
                                                arch.UsuarioIngresoReg = Session["SessionNombreUsuario"].ToString();//identityName -- RUT PROVEEDOR
                                                arch.FechaIngresoReg = DateTime.Now;
                                                arch.UsuarioModReg = Session["SessionNombreUsuario"].ToString();
                                                arch.FechaModReg = DateTime.Now;
                                                arch.ParentId = prov.Id;//idProveedor
                                                arch.IdInstitucion = 21; //idMandanteMOP
                                                arch.FechaInicioVigencia = DateTime.Now;
                                                arch.FechaFinVigencia = DateTime.Now.AddYears(1);
                                                arch.IdDocSolicitado = 0; //0
                                                arch.RutProveedor = rutProveedor;
                                                arch.TipoDocumento = "INT";

                                                //SE INSERTA REGISTRO EN DB
                                                int idArchivoPrivado = SQLMethods.InsertarMOPArchivoPrivado(arch);

                                                MOPMetaDatosDocumentos mmd = new MOPMetaDatosDocumentos();
                                                mmd.IdDoc = idArchivoPrivado;
                                                mmd.VisibleProv = visibleAlProv; //S o N
                                                mmd.PalabrasClave = palabrasClave;
                                                mmd.Anio = año;
                                                mmd.CodTipoDocumento = tipoDoc;
                                                mmd.CodAreaAnalisis = areaDoc;
                                                mmd.FechaIngresoReg = DateTime.Now;
                                                mmd.UsuarioIngresoReg = Session["SessionNombreUsuario"].ToString();

                                                //SE REGISTRA EN DB
                                                SQLMethods.InsertarMetadatosDocumento(mmd);


                                                //AL FINAL, SE ELIMINA EL ARCHIVO DE LA RUTA TEMP
                                                if (System.IO.File.Exists(path))
                                                {
                                                    //System.IO.File.Delete(path);
                                                    System.IO.Directory.Delete(folderPath, true);//con true borra el directorio y todo su contenido
                                                }


                                            }
                                            else
                                            {
                                                //ViewBag.MsgSubirArchivo = "Error al conectar a Repositorio Gesintel - Error de subida: " + resp.status;
                                                return Json(new { success = false, message = "Error al conectar a Repositorio Gesintel - Error de subida: " + resp.status });
                                            }
                                        }
                                        else
                                        {
                                            //ViewBag.MsgSubirArchivo = "Error al conectar a Repositorio Gesintel - Token error: " + token.Status;
                                            return Json(new { success = false, message = "Error al conectar a Repositorio Gesintel - Token error: " + token.Status });
                                        }

                                    }
                                    else
                                    {
                                        //ViewBag.MsgSubirArchivo = "Debe indicar las palabras clave";
                                        return Json(new { success = false, message = "Debe indicar las palabras clave" });
                                    }
                                }
                                else
                                {
                                    //ViewBag.MsgSubirArchivo = "Debe indicar un tipo de documento";
                                    //return View("Index");
                                    return Json(new { success = false, message = "Debe indicar un tipo de documento" });
                                }
                            }
                            else
                            {
                                //ViewBag.MsgSubirArchivo = "Debe indicar una descripción";                                
                                return Json(new { success = false, message = "Debe indicar una descripción" });
                            }

                        }
                        else
                        {
                            //ViewBag.MsgSubirArchivo = "La extensión del archivo debe ser PDF";
                            //return View("Index");                            
                            return Json(new { success = false, message = "La extensión del archivo debe ser PDF" });
                        }


                    }
                    else
                    {
                        //ViewBag.MsgSubirArchivo = "Debe adjuntar un archivo";                        
                        return Json(new { success = false, message = "Debe adjuntar un archivo" });
                    }


                    string msg = "Archivo Subido Correctamente";
                    return Json(new { success = false, message = msg });
                }
                else
                {
                    string msg = "No AJAX Request";
                    return Json(new { success = false, message = msg });
                }





            }
            catch (Exception x)
            {
                //ViewBag.MsgSubirArchivo = "File upload failed!!: " + x.InnerException.Message.ToString();                
                return Json(new { success = false, message = "File upload failed!!: " + x.InnerException.Message.ToString() });
            }
        }




    }
}