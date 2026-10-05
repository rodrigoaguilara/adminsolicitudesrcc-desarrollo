using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminSolicitudesContratistas.Models.ViewModels
{
    public class HistorialLogViewModel
    {
        public DateTime FECHA_LOG { get; set; }
        public string COMENTARIO_1 { get; set; }
        public string COMENTARIO_2 { get; set; }
        public string COMENTARIO_3 { get; set; }
        public string USUARIO_RESPONSABLE { get; set; }
    }
}