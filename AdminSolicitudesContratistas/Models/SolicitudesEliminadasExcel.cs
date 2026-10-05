using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminSolicitudesContratistas.Models
{
    public class SolicitudesEliminadasExcel
    {
        public int IdSol { get; set; }
        public string IdSolPadre { get; set; }
        public string RutProveedor { get; set; }
        public string RazonSocial { get; set; }
        public string Estado { get; set; }
        public string TipoSol { get; set; }
        public string TipoReg { get; set; }
        public string Region { get; set; }
        public string FechaCreacion { get; set; }
        public string FechaEliminacion { get; set; }
        public string FuncionarioResponsable { get; set; }
        public string MotivoEliminacion { get; set; }

    }
}