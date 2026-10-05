using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Web;

namespace AdminSolicitudesContratistas.Models
{
    public class SQLMethods
    {

        public static List<Mes> TraerListadoMeses() {
            try
            {
                List<Mes> listMeses = new List<Mes>();
                for (int i = 1; i <= 12; i++)
                {
                    Mes m = new Mes();
                    m.IdMes = i;
                    switch (i)
                    {
                        case 1: m.NombreMes = "Enero"; break;
                        case 2: m.NombreMes = "Febrero"; break;
                        case 3: m.NombreMes = "Marzo"; break;
                        case 4: m.NombreMes = "Abril"; break;
                        case 5: m.NombreMes = "Mayo"; break;
                        case 6: m.NombreMes = "Junio"; break;
                        case 7: m.NombreMes = "Julio"; break;
                        case 8: m.NombreMes = "Agosto"; break;
                        case 9: m.NombreMes = "Septiembre"; break;
                        case 10: m.NombreMes = "Octubre"; break;
                        case 11: m.NombreMes = "Noviembre"; break;
                        case 12: m.NombreMes = "Diciembre"; break;
                    }
                    listMeses.Add(m);                    
                }
                return listMeses;
            }
            catch (Exception x)
            {
                throw;
            }
        }



        public static ProveedorView TraerDatosProveedor(string rut)
        {
            ProveedorView prov = new ProveedorView();
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {

                    var p = db.SP_MOP_TRAER_PROVEEDOR_POR_RUT(rut);

                    foreach (var item in p)
                    {
                        prov.Id = item.Id;
                        prov.Rut = item.Rut;
                        prov.RazonSocial = item.RazonSocial;
                        prov.TipoProveedor = item.TipoProveedor;
                        prov.TipoEmpresa = item.TipoEmpresa;
                        prov.EntCode = item.Entcode;
                        prov.OrgCode = item.OrgCode;
                    }
                    //return prov;
                }
            }
            catch (Exception x)
            {
                //MOPLogRegistroErroresServicioSubidaDocumentos errorLog = new MOPLogRegistroErroresServicioSubidaDocumentos();
                //errorLog.NumSolicitud = 0;
                //errorLog.TipoError = "ERROR AL TRAER DATOS PROVEEDOR";
                //errorLog.ErrorMsg = "Excepción: " + x.Message.ToString();
                //ConsultasSQL.GuardarLogErroresSubida(errorLog);
                throw x;

            }

            return prov;
        }

        public static int InsertarMOPArchivoPrivado(MOPArchivoPrivado a)
        {
            int idArchivoPrivado = 0;
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    var outputParameter = new ObjectParameter("ID_ARCHIVO_PRIVADO", typeof(Int32));
                    db.SP_MOP_INSERTAR_ARCHIVO_PRIVADO(a.NomArchivo, a.Tipo, a.Tamano, a.IdArchivo, a.Fecha, a.Descripcion, a.UsuarioIngresoReg, a.FechaIngresoReg, a.UsuarioModReg,
                        a.FechaModReg, a.ParentId, a.IdInstitucion, a.FechaInicioVigencia, a.FechaFinVigencia, a.IdDocSolicitado, a.RutProveedor, a.TipoDocumento, outputParameter);
                    idArchivoPrivado = (int)outputParameter.Value;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return idArchivoPrivado;
        }

        public static void InsertarMetadatosDocumento(MOPMetaDatosDocumentos mmd)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    var OK = db.SP_MOP_INSERTAR_METADATOS_DOCUMENTOS(mmd.IdDoc,mmd.Anio, mmd.CodAreaAnalisis, mmd.CodTipoDocumento, mmd.PalabrasClave, mmd.VisibleProv, mmd.UsuarioIngresoReg);                    
                }
            }
            catch (Exception)
            {
                throw;
            }            
        }



        public static int GuardarMOPLog(string rutProveedor, int idSol, int idDoc, string codEstado, string codTipoOperacion, string comentario1,
                                                string comentario2, string comentario3, string usuario) {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    int idProveedor = 0;
                    int OK = db.SP_MOP_INSERTAR_MOP_LOG(rutProveedor, idSol, idDoc, codEstado, codTipoOperacion, comentario1, comentario2, comentario3, usuario, idProveedor);
                    return OK;
                }
            }
            catch (Exception x)
            {
                throw x;
            }
        
        }




        public static UsuarioMOPView TraerDatosUsuarioMOP(string usernameAD) {
            UsuarioMOPView usr = new UsuarioMOPView();
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    

                    var u = db.SP_MOP_ADM_TRAER_DATOS_USUARIO_MOP(usernameAD);

                    //foreach (var item in u.ToList())
                    foreach (var item in u)
                    {
                        usr.Nombre = item.Nombre;
                        usr.Email = item.Email;
                        usr.SuperAdmin= item.Superadmin;
                        usr.Perfil = item.DescPerfil;
                        usr.CodPerfil = item.CodPerfil;
                        usr.UserTipoRegistro = item.CodTipoReg;
                        usr.UserTipoSolicitud = item.CodTipoSol;
                        usr.UserRegion = item.CodRegion;

                        
                    }
                    
                }
            }
            catch (Exception x)
            {
                throw x;
            }

            return usr;
        
        }



    }
}