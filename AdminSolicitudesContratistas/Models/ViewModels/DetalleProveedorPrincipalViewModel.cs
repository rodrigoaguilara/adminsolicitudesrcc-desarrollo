using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminSolicitudesContratistas.Models.ViewModels
{
    public class DetalleProveedorPrincipalViewModel
    {
        public string Rut { get; set; }
        public string RazonSocial { get; set; }
        public string NombreFantasia { get; set; }
        public string Email { get; set; }
        // Añade cualquier otro campo que devuelva este SP y necesites en la cabecera
    }
}