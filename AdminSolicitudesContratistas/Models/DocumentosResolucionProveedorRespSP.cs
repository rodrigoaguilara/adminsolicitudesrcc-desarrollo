using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminSolicitudesContratistas.Models
{
    public class DocumentosResolucionProveedorRespSP
    {
        public int ID { get; set; }
        public string Comentario { get; set; }
        public System.DateTime FechaNota { get; set; }
        public string CodEstado { get; set; }
        public string Estado { get; set; }
        public string CodAreaAnalisis { get; set; }
        public string AreaAnalisis { get; set; }
        public int IdDoc { get; set; }
        public Nullable<long> IdArchivoGesintel { get; set; }
        public string NomArchivo { get; set; }
        public string Descripcion { get; set; }
        public System.DateTime FechaDoc { get; set; }
        public long IdArchivo { get; set; }
        public string UsuarioIngresoReg { get; set; }

        public int DisponibleEnGesintel { get; set; }
    }
}