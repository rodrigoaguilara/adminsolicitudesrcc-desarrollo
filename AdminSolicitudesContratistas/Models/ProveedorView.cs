using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminSolicitudesContratistas.Models
{
    public class ProveedorView
    {
        public int Id { get; set; }
        public string Rut { get; set; }
        public string RazonSocial { get; set; }
        public int TipoProveedor { get; set; }
        public int TipoEmpresa { get; set; }
        public string Email { get; set; }
        public int ContratoVigente { get; set; }
        public int ResolucionFavorable { get; set; }
        public int NoSuspendido { get; set; }

        public string EntCode { get; set; }
        public string OrgCode { get; set; }







    }
}