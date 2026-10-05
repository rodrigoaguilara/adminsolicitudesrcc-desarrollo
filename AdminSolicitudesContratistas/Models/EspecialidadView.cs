using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminSolicitudesContratistas.Models
{
    public class EspecialidadView
    {
        public int Id { get; set; }
        public string TipoEspecialidad { get; set; }
        public string CodTipoEspecialidad { get; set; }
        public string CodTipoCategoria { get; set; }
        public string CodTipoRegistro { get; set; }
        public List<CategoriaView> ListCategorias { get; set; }
    }


    public class CategoriaView
    {
        public string CodTipoCategoria { get; set; }
        public string TipoCategoria { get; set; }
    }
}