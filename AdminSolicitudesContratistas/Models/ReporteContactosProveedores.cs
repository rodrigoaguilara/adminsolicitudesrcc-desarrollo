using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminSolicitudesContratistas.Models
{
    public class ReporteContactosProveedores
    {
        public string Rut { get; set; }
        public string RazonSocial { get; set; }
        public string EstadoProveedor { get; set; }
        public string TipoRegistro { get; set; }
        public string Email { get; set; }
        public string TelefonoContacto { get; set; }
        public string CelularContacto { get; set; }
        public string Direccion { get; set; }
        public string Comuna { get; set; }
        public string Ciudad { get; set; }
        public string Region { get; set; }
    }
}