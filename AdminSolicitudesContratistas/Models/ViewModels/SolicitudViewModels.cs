using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminSolicitudesContratistas.Models.ViewModels
{
    //public class SolicitudViewModels
    //{
    //}

    // Para la cabecera y datos principales de la solicitud
    public class SolicitudPrincipalViewModel
    {
        public int IdSolPadre { get; set; }
        public string RazonSocial { get; set; }
        public string RutProveedor { get; set; }
        public string EmailProveedor { get; set; }
        public string Comentario { get; set; }
        public string FechaIngreso { get; set; }
        public string FechaRecepcion { get; set; }
        public string Estado { get; set; }
        public string CodEstado { get; set; }
        public string TipoReg { get; set; }
        public string TipoSol { get; set; }
        public string CodTipoSol { get; set; }
        public string CodTipoReg { get; set; }
        public int IdRegion { get; set; }
        public string EstadoProveedor { get; set; } // Añadimos el estado del proveedor aquí
    }

    // Para la lista de documentos adjuntos
    public class DocumentoAdjuntoViewModel
    {
        public int idDet { get; set; }
        public long? NumArchivo { get; set; }
        public string NomDoc { get; set; }
        public string Comentario { get; set; }
        public long? IdArchivoGesintel { get; set; }
        public string CodAreaAnalisis { get; set; }
        public string AreaAnalisis { get; set; }
        public string Estado { get; set; }
        public string CodEstado { get; set; }
    }

    // Para el log de la solicitud
    public class LogSolicitudViewModel
    {
        public string Estado { get; set; }
        public DateTime? Fecha { get; set; }
        public string Comentario_1 { get; set; }
        public string Comentario_2 { get; set; }
        public string Comentario_3 { get; set; }
        public string CuentaUsuario { get; set; }
    }

    // Para los documentos de resolución
    public class DocumentoResolucionViewModel
    {
        public DateTime? FechaNota { get; set; }
        public string AreaAnalisis { get; set; }
        public string Comentario { get; set; }
        public string NomArchivo { get; set; }
        public long? IdArchivoGesintel { get; set; }
        public int DisponibleEnGesintel { get; set; }
    }

    // Para las observaciones
    public class ObservacionViewModel
    {
        public string AreaAnalisis { get; set; }
        public DateTime? FechaNota { get; set; }
        public string Comentario { get; set; }
    }

    // Para las solicitudes relacionadas
    public class SolicitudRelacionadaViewModel
    {
        public int ID { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public string TipoSolRel { get; set; }
    }

    // El ViewModel principal que une todo
    public class SolicitudDetalleCompletoViewModel
    {
        public SolicitudPrincipalViewModel Principal { get; set; }
        public List<DocumentoAdjuntoViewModel> DocumentosAdjuntos { get; set; }
        public List<LogSolicitudViewModel> LogSolicitud { get; set; }
        public List<DocumentoResolucionViewModel> DocumentosResolucion { get; set; }
        public List<ObservacionViewModel> Observaciones { get; set; }
        public List<SolicitudRelacionadaViewModel> SolicitudesRelacionadas { get; set; }

        public SolicitudDetalleCompletoViewModel()
        {
            DocumentosAdjuntos = new List<DocumentoAdjuntoViewModel>();
            LogSolicitud = new List<LogSolicitudViewModel>();
            DocumentosResolucion = new List<DocumentoResolucionViewModel>();
            Observaciones = new List<ObservacionViewModel>();
            SolicitudesRelacionadas = new List<SolicitudRelacionadaViewModel>();
        }
    }


}