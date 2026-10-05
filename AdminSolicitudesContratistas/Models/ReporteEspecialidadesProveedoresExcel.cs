using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminSolicitudesContratistas.Models
{
    public class ReporteEspecialidadesProveedoresExcel
    {
        public string Rut { get; set; }
        public string RazonSocialProveedor { get; set; }
        public string TipoRegistro { get; set; }
        public string CodTipoEspecialidad { get; set; }
        public string TipoEspecialidad { get; set; }
        public string TipoCategoria { get; set; }
        public string CapacidadEconomica { get; set; }
        public string EstadoProveedor { get; set; }
        public string FechaIngresoReg { get; set; }
        public string FechaSuspension { get; set; }
        public string NombreMesRenovacion { get; set; }
        public string FechaEstadoFinanciero { get; set; }
        public string Region { get; set; }
    }
}