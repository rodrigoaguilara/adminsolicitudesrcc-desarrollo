using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminSolicitudesContratistas.Models.ViewModels
{
    public class OpcionMenuViewModel
    {
        public string DescOpcion { get; set; }
        public string Controlador { get; set; }
    }

    // Representa un item principal del menú (ej. "Proveedores") y contiene sus sub-items
    public class MenuItemViewModel
    {
        public string CodMenu { get; set; }
        public string DescMenu { get; set; }
        public List<OpcionMenuViewModel> Opciones { get; set; }

        public MenuItemViewModel()
        {
            Opciones = new List<OpcionMenuViewModel>();
        }
    }
}