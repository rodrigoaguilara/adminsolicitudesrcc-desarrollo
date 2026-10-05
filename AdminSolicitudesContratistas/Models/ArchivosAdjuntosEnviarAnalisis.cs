using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminSolicitudesContratistas.Models
{
    public class ArchivosAdjuntosEnviarAnalisis
    {
        public int IdDetalleSolicitud { get; set; }
        //public string NombreArchivo { get; set; }
        public string areaDeAnalisis { get; set; }
        public string estado { get; set; }
    }
}