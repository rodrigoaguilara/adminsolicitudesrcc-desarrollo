using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminSolicitudesContratistas.Models
{
    public class ProveedoresExcel
    {
        public string Rut { get; set; }
        public string RazonSocial { get; set; }
        public string Region { get; set; }
        public string EstadoMOP { get; set; }
        public string TipoRegistro { get; set; }
        public string MesRenovacion { get; set; }
        public string FechaIngresoReg { get; set; }
    }
}