using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminSolicitudesContratistas.Models.ViewModels
{
    public class CategoriaDisponibleViewModel
    {
        public string CodTipoCategoria { get; set; }
        public string TipoCategoria { get; set; }
    }

    // Representa una fila completa de la tabla de especialidades
    public class EspecialidadCompletaViewModel
    {
        public int Id { get; set; } // Id del registro MOP_PROVEEDOR_ESPECIALIDAD
        public string TipoEspecialidad { get; set; }
        public string CodTipoEspecialidad { get; set; }
        public string CodTipoCategoriaSeleccionada { get; set; } // La categoría que tiene actualmente
        public List<CategoriaDisponibleViewModel> CategoriasDisponibles { get; set; } // La lista de radios a mostrar
    }

}