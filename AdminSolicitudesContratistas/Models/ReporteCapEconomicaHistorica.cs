using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminSolicitudesContratistas.Models
{
    public class ReporteCapEconomicaHistoricaExcel
    {
        public string RutProveedor { get; set; }
        public string RazonSocial { get; set; }
        public int CapacidadEconomica { get; set; }
        public string FechaEstadoFinanciero { get; set; }
        public string EstadoProveedor { get; set; }
        public string FechaRenovacion { get; set; }
        public string Vigencia { get; set; }
        public string TipoRegistro { get; set; }
    }

}