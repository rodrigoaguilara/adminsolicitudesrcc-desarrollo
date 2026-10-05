using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminSolicitudesContratistas.Models
{
    public class UsuarioMOPView
    {
        public string Nombre { get; set; }
        public string Email { get; set; }
        public bool SuperAdmin { get; set; }
        public string Perfil { get; set; }

        public string CodPerfil { get; set; }


        public string UserTipoRegistro { get; set; }
        public string UserTipoSolicitud { get; set; }
        public int UserRegion { get; set; }



    }
}