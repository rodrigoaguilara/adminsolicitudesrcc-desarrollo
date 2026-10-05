using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminSolicitudesContratistas.Models.ViewModels
{
    
    // Para la lista principal de perfiles
    public class PerfilInfoViewModel
    {
        public string CodPerfil { get; set; }
        public string DescPerfil { get; set; }
    }

    // Para la lista de opciones de menú en el modal
    public class OpcionMenuPerfilViewModel
    {
        public int IdOpcionMenuPerfil { get; set; } // Será 0 si no está asignado
        public int OpcionMenuID { get; set; }
        public string DescOpcion { get; set; }
        public string DescMenu { get; set; }
        public bool Asignado { get; set; } // true o false
    }

    // Para recibir los datos guardados desde el cliente
    public class GuardarPermisosViewModel
    {
        public string CodPerfil { get; set; }
        public List<OpcionMenuPerfilViewModel> Permisos { get; set; }
    }
}