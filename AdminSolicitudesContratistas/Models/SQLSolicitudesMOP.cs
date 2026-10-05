using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Web;

namespace AdminSolicitudesContratistas.Models
{
    public class SQLSolicitudesMOP
    {


        
        public static string VolverEstadoAnterior_ActualizarDetalleSolicitud(int numSolicitud, string codEstadoAnterior)
        {
            string resp = "OK";
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    db.SP_MOP_SOL_VEA_ACTUALIZAR_DET_SOLICITUD(numSolicitud, codEstadoAnterior);
                }
            }
            catch (Exception x)
            {
                resp = "Error al intentar actualizar el detalle de Solicitud: " + x.InnerException.Message;
            }
            return resp;
        }

        public static string VolverEstadoAnterior_ActualizarSolicitud(int numSolicitud, string codEstadoAnterior)
        {
            string resp = "OK";
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    db.SP_MOP_SOL_VEA_ACTUALIZAR_SOLICITUD(numSolicitud, codEstadoAnterior);
                }
            }
            catch (Exception x)
            {
                resp = "Error al intentar actualizar el detalle de Solicitud: " + x.InnerException.Message;
            }
            return resp;
        }



        public static string VolverEstadoAnterior_EliminarNotasSolicitud(MOPNotasSolicitud nota, string codEstadoSolicitud)
        {
            string resp = "OK";
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    db.SP_MOP_SOL_VEA_ELIMINAR_NOTAS_SOLICITUD(nota.IdSol, codEstadoSolicitud);
                }
            }
            catch (Exception x)
            {
                resp = "Error al intentar actualizar el detalle de Solicitud: " + x.InnerException.Message;
            }
            return resp;
        }





        public static string ActualizarEstadoDetalleSolicitud(MOPDetalleSolicitud det)
        {
            string resp = "";
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    db.SP_MOP_SOL_ACTUALIZAR_ESTADO_DETALLE_SOLICITUD(det.ID, det.CodAreaAnalisis, det.CodEstado , det.Comentario);
                }
            }
            catch (Exception x)
            {
                resp = "Error al intentar actualizar estado de solicitud: " + x.InnerException.Message;
            }
            return resp;
        }

        public static string ActualizarEstadoSolicitud(MOPSolicitud sol)
        {
            string resp = "";
            
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    var respuesta = db.SP_MOP_SOL_ACTUALIZAR_ESTADO_SOLICITUD(sol.ID, sol.CodEstado, sol.CodEstadoCHP, sol.FechaIngreso, sol.SolPadreRespondida);
                }
            }
            catch (Exception x)
            {
                resp = "Error al intentar actualizar estado de solicitud: " + x.InnerException.Message;
            }
            return resp;
        }
        public static string ActualizarEstadoSolicitudFinal(MOPSolicitud sol)
        {
            string resp = "";
            
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    var respuesta = db.SP_MOP_SOL_ACTUALIZAR_ESTADO_SOLICITUD_FINAL(sol.ID, sol.CodEstado, sol.CodEstadoCHP, sol.FechaRespuesta, sol.SolPadreRespondida);
                }
            }
            catch (Exception x)
            {
                resp = "Error al intentar actualizar estado de solicitud: " + x.InnerException.Message;
            }
            return resp;
        }
        





        public static string ActualizarSolicitudPadreRespondida(int numSolPadre)
        {
            string resp = "";
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    db.SP_MOP_SOL_ACTUALIZAR_SOL_PADRE_RESPONDIDA(numSolPadre);
                }
            }
            catch (Exception x)
            {
                resp = "Error al intentar actualizar estado de solicitud: " + x.InnerException.Message;
            }
            return resp;
        }

        public static string EliminarSolicitud(MOPSolicitud sol, string funcionarioResponsable, string motivoEliminacion)
        {
            string resp = "";
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    var outputParameter = new ObjectParameter("RESPUESTA", typeof(Int32));
                    db.SP_MOP_SOL_ELIMINAR_SOLICITUD_RESPALDO_LOG(sol.ID, sol.IdSolPadre, sol.RutProveedor, sol.FechaCreacion, sol.CodTipoSol, sol.CodTipoReg, sol.CodEstado, sol.IdRegion, funcionarioResponsable, motivoEliminacion, outputParameter);
                    

                    int ret = (int)outputParameter.Value;
                    if (ret == 1) {
                        resp = "OK";
                    }
                    else
                    {
                        resp = "FAIL";
                    }


                }
            }
            catch (Exception x)
            {
                resp = "Error al intentar eliminar solicitud: " + x.Message;
            }
            return resp;
        }



        public static bool ValidarSolicitudRelacionada(string codTipoSolicitud, int numSolicitudAsociada)
        {
            //MOPSolicitud sol = new MOPSolicitud();
            bool SolRelacionada = false;
            try
            {
                if (codTipoSolicitud == "MDO" && numSolicitudAsociada > 0)
                {
                    using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                    {
                        var s = db.SP_MOP_SOL_BUSCA_SOL_ORIGEN_DESDE_RELACIONADA(numSolicitudAsociada).ToList();
                        if (s != null)
                        {
                            foreach (var item in s)
                            {
                                if (item.CodTipoSol == "INS")
                                {
                                    SolRelacionada = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return SolRelacionada;
        }




        public static string GuardarMOPLog(MOPLog log, string funcionario)
        {
            string resp = "OK";
            try
            {
                //string usuario = Session["SessionNombreUsuario"].ToString();
                SQLMethods.GuardarMOPLog(log.RutProveedor, int.Parse(log.idSol.ToString()), int.Parse(log.idDoc.ToString()), log.CodEstado, log.CodTipo, log.Comentario_1, log.Comentario_2, log.Comentario_3, funcionario);
            }
            catch (Exception x)
            {
                resp = "Error al intentar actualizar el detalle de Solicitud: " + x.InnerException.Message;
            }
            return resp;
        }



        public static string GuardarNotasSolicitud(MOPNotasSolicitud nota)
        {
            string resp = "OK";
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    //string usuarioResp = Session["SessionNombreUsuario"].ToString();
                    db.SP_MOP_SOL_GUARDAR_NOTAS_SOLICITUD(nota.IdSol, nota.Comentario, nota.FechaNota, nota.CodEstado, nota.CodAreaAnalisis, nota.IdDoc, nota.Tipo);
                }
            }
            catch (Exception x)
            {
                resp = "Error al intentar actualizar el detalle de Solicitud: " + x.InnerException.Message;
            }
            return resp;
        }


    }
}